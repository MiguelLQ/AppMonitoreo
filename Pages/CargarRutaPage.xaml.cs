using System.Text.Json;
using AppMonitoreo.Models;
using AppMonitoreo.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace AppMonitoreo.Pages;

public partial class CargarRutaPage : ContentPage
{
    private bool _rutaDibujada = false;
    private string _uid = "FfnTBoXRCShQzIF5cb2OdgFYdlA2"; // UID fijo (reemplaza si usas autenticación)

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
            // Inicia el temporizador que actualiza ubicación cada 10 segundos
            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                _ = ActualizarUbicacionAsync(); // Llamada asíncrona
                return true; // Repetir
            });
        }
        else
        {
            await DisplayAlert("Permisos", "No se concedieron los permisos de ubicación.", "OK");
        }
    }

    private async Task ActualizarUbicacionAsync()
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

                var firebase = new FirebaseServices();

                var nuevaUbicacion = new Ubicacion
                {
                    Latitud = ubicacion.Latitude,
                    Longitud = ubicacion.Longitude,
                };

                await firebase.SetUbicacionActual(_uid, nuevaUbicacion);

                // Actualiza el pin actual
                var pin = new Pin
                {
                    Label = "Mi ubicación",
                    Location = posicion,
                    Type = PinType.Place
                };

                map.Pins.Clear();
                map.Pins.Add(pin);

                // Cargar ruta solo la primera vez
                if (!_rutaDibujada)
                {
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
                        _rutaDibujada = true;
                    }
                }
            }
            else
            {
                await DisplayAlert("Ubicación", "No se pudo obtener la ubicación.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al obtener ubicación: {ex.Message}", "OK");
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
                        double lon = coord[0].GetDouble();
                        double lat = coord[1].GetDouble();
                        ubicaciones.Add(new Location(lat, lon));
                    }

                    break; // Solo tomamos la primera línea
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
