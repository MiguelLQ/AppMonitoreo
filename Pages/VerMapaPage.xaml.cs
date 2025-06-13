using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps; // Para Geolocation

namespace AppMonitoreo.Pages;

public partial class VerMapaPage : ContentPage
{
    private readonly string usuarioUID = "FfnTBoXRCShQzIF5cb2OdgFYdlA2"; // 👈 reemplaza por el UID real del usuario de prueba

    private readonly FirebaseServices firebase;
    private CancellationTokenSource cts;

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
                var ubicacion = await firebase.ObtenerUbicacionDeUsuario(usuarioUID);

                if (ubicacion != null)
                {
                    var posicion = new Location(ubicacion.Latitud, ubicacion.Longitud);

                    lblCoordenadas.Text = $"Coordenadas: Lat = {posicion.Latitude}, Lon = {posicion.Longitude}";

                    var pin = new Pin
                    {
                        Label = "Ubicación del Usuario",
                        Location = posicion,
                        Type = PinType.Place
                    };

                    map.Pins.Clear();
                    map.Pins.Add(pin);

                    var region = MapSpan.FromCenterAndRadius(posicion, Distance.FromKilometers(0.5));
                    map.MoveToRegion(region);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error obteniendo ubicación: {ex.Message}", "OK");
            }

            await Task.Delay(5000); // cada 5 segundos
        }
    }

}