using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors; // Para Geolocation

namespace AppMonitoreo.Pages;

public partial class NewPage1 : ContentPage
{
    const string apiKey = "AIzaSyDnhXdrM8NZCsLTBk9Ge8fn3ITq3qxgkbE"; // Reemplaza con tu API Key real

    public NewPage1()
    {
        InitializeComponent();
        MostrarRuta();

    }
    private async void MostrarRuta()
    {
        try
        {
            // 1. Obtener ubicación actual
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
                location = await Geolocation.GetLocationAsync();

            double latOrigen = location.Latitude;
            double lonOrigen = location.Longitude;

            // 2. Establece destino manualmente (puedes hacerlo dinámico luego)
            double latDestino = -13.6590185;
            double lonDestino = -73.3517575;

            // 3. Construir URL del iframe
            string mapsUrl = $"https://www.google.com/maps/embed/v1/directions?key={apiKey}&origin={latOrigen},{lonOrigen}&destination={latDestino},{lonDestino}&mode=driving";

            string html = $@"
                <html>
                    <head>
                        <meta name='viewport' content='initial-scale=1.0, user-scalable=no' />
                        <meta charset='utf-8'>
                    </head>
                    <body style='margin:0;padding:0;'>
                        <iframe
                            width='100%' height='100%'
                            frameborder='0' style='border:0'
                            src='{mapsUrl}' allowfullscreen>
                        </iframe>
                    </body>
                </html>";

            // 4. Mostrar HTML en el WebView
            MyWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo obtener la ubicación: {ex.Message}", "OK");
        }

    }
}