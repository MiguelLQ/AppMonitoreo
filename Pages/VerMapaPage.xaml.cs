using System.Text.Json;
using AppMonitoreo.Models;
using AppMonitoreo.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps; // Para Geolocation
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AppMonitoreo.Pages;

public partial class VerMapaPage : ContentPage
{
    private readonly string usuarioUID = "WGKj9WUDTygspeKS7DPlZydPQDc2"; // UID real del usuario de prueba
    private readonly FirebaseServices firebase;
    private CancellationTokenSource cts;
    private bool rutaDibujada = false;

    public VerMapaPage()
    {
        InitializeComponent();
        firebase = new FirebaseServices();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        cts = new CancellationTokenSource();
        StartTracking(cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        cts.Cancel(); // Cancelar rastreo al salir
    }

    private async void StartTracking(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Obtener ubicación propia (celular)
                var solicitud = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var ubicacionPropia = await Geolocation.GetLocationAsync(solicitud);

                // Obtener ubicación del usuario de prueba (Firebase)
                var ubicacionUsuario = await firebase.ObtenerUbicacionDeUsuario(usuarioUID);

                if (ubicacionPropia != null && ubicacionUsuario != null)
                {
                    var posicionPropia = new Location(ubicacionPropia.Latitude, ubicacionPropia.Longitude);
                    var posicionUsuario = new Location(ubicacionUsuario.Latitud, ubicacionUsuario.Longitud);

                    lblCoordenadas.Text = $"Tu posición:\nLat: {posicionPropia.Latitude:F6} | Lon: {posicionPropia.Longitude:F6}";

                    map.Pins.Clear();

                    // Pin propio
                    var pinPropio = new Pin
                    {
                        Label = "Tú",
                        Location = posicionPropia,
                        Type = PinType.Generic
                    };
                    map.Pins.Add(pinPropio);

                    // Pin del usuario de prueba
                    var pinUsuario = new Pin
                    {
                        Label = "Usuario de prueba",
                        Location = posicionUsuario,
                        Type = PinType.Place
                    };
                    map.Pins.Add(pinUsuario);

                    // Centrar mapa entre los dos
                    var centerLatitude = (posicionPropia.Latitude + posicionUsuario.Latitude) / 2;
                    var centerLongitude = (posicionPropia.Longitude + posicionUsuario.Longitude) / 2;
                    var centerPosition = new Location(centerLatitude, centerLongitude);
                    var region = MapSpan.FromCenterAndRadius(centerPosition, Distance.FromKilometers(1));
                    map.MoveToRegion(region);

                    // Cargar ruta desde ruta.json SOLO una vez
                    if (!rutaDibujada)
                    {
                        var ruta = await CargarRutaDesdeGeoJsonAsync();
                        if (ruta.Count > 1)
                        {
                            var polyline = new Polyline
                            {
                                StrokeColor = Colors.BlueViolet,
                                StrokeWidth = 35
                            };

                            foreach (var punto in ruta)
                            {
                                polyline.Geopath.Add(punto);
                            }

                            map.MapElements.Add(polyline);
                            rutaDibujada = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error obteniendo ubicación: {ex.Message}", "OK");
            }

            await Task.Delay(5000); // actualiza cada 5 segundos
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

                    break; // Solo toma la primera LineString
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar ruta.json: {ex.Message}", "OK");
        }

        return ubicaciones;
    }
}
