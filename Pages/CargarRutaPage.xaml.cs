using System.Text.Json;
using AppMonitoreo.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace AppMonitoreo.Pages;

public partial class CargarRutaPage : ContentPage
{
	public CargarRutaPage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var permiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (permiso != PermissionStatus.Granted)
        {
            permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (permiso == PermissionStatus.Granted)
        {
            try
            {
                var solicitud = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var ubicacion = await Geolocation.GetLocationAsync(solicitud);

                if (ubicacion != null)
                {
                    lblCoordenadas.Text = $"Coordenadas: Lat={ubicacion.Latitude}, Lon={ubicacion.Longitude}";

                    var posicion = new Location(ubicacion.Latitude, ubicacion.Longitude);
                    var region = MapSpan.FromCenterAndRadius(posicion, Distance.FromKilometers(1));
                    map.MoveToRegion(region);
                    // Instancia del servicio Firebase
                    var firebase = new FirebaseServices();

                    // Crear objeto de ubicación
                    var nuevaUbicacion = new Ubicacion
                    {
                        Latitud = ubicacion.Latitude,
                        Longitud = ubicacion.Longitude,
                    };

                    // Enviar a Firebase
                    await firebase.AddUbicacion(nuevaUbicacion);
                    await DisplayAlert("Ubicación", "Ubicacion enviada.", "OK");

                    var pin = new Pin
                    {
                        Label = "Mi ubicación",
                        Location = posicion,
                        Type = PinType.Place
                    };

                    map.Pins.Clear();
                    map.Pins.Add(pin);
                }
                else
                {
                    await DisplayAlert("Ubicación", "No se pudo obtener la ubicación.", "OK");
                }

                // Aquí cargamos y dibujamos la ruta
                var ruta = await CargarRutaDesdeGeoJsonAsync();
                if (ruta.Count > 1)
                {
                    var polyline = new Polyline
                    {
                        StrokeColor = Colors.Blue,
                        StrokeWidth = 5
                    };

                    foreach (var punto in ruta)
                    {
                        polyline.Geopath.Add(punto);
                    }

                    map.MapElements.Add(polyline);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Permisos", "No se concedieron los permisos de ubicación.", "OK");
        }
    }

    private async Task<List<Location>> CargarRutaDesdeGeoJsonAsync()
    {
        var ubicaciones = new List<Location>();

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("ruta.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            using var document = JsonDocument.Parse(json);

            var features = document.RootElement.GetProperty("features");

            foreach (var feature in features.EnumerateArray())
            {
                var geometry = feature.GetProperty("geometry");
                var type = geometry.GetProperty("type").GetString();

                if (type == "LineString")
                {
                    var coordinates = geometry.GetProperty("coordinates");

                    foreach (var coord in coordinates.EnumerateArray())
                    {
                        // GeoJSON: [longitude, latitude, altitude]
                        double lon = coord[0].GetDouble();
                        double lat = coord[1].GetDouble();

                        ubicaciones.Add(new Location(lat, lon));
                    }

                    break; // Solo usamos la primera línea
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al procesar ruta.json: {ex.Message}", "OK");
        }

        return ubicaciones;
    }

}