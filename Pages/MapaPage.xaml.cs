using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace AppMonitoreo.Pages;

public partial class MapaPage : ContentPage
{
    Pin userPin;
    Pin truckPin;
    Polyline routeLine;
    public MapaPage()
	{
		InitializeComponent();
        StartTracking();

    }
    async void StartTracking()
    {
        var truckLocation = new Location(-13.609105, -73.337323); // ubicación simulada del camión

        // Crear pin del camión
        truckPin = new Pin
        {
            Label = "Camión de Basura 🛻",
            Address = "Ruta de servicio",
            Location = truckLocation,
            Type = PinType.Place
        };
        map.Pins.Add(truckPin);

        // Crear línea para la ruta
        routeLine = new Polyline
        {
            StrokeColor = Colors.Green,
            StrokeWidth = 5
        };
        map.MapElements.Add(routeLine);

        while (true)
        {
            try
            {
                var userLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

                if (userLocation != null)
                {
                    // Crear o actualizar el pin del usuario
                    if (userPin == null)
                    {
                        userPin = new Pin
                        {
                            Label = "Mi Ubicación 📍",
                            Address = "Aquí estás tú",
                            Location = userLocation,
                            Type = PinType.Place
                        };
                        map.Pins.Add(userPin);
                    }
                    else
                    {
                        userPin.Location = userLocation;
                    }

                    // Calcular distancia
                    double distanceKm = Location.CalculateDistance(userLocation, truckLocation, DistanceUnits.Kilometers);

                    // Actualizar label con formato más elegante
                    distanceLabel.Text = $"🚛 ➡ 📍 Distancia al camión: {distanceKm:F2} km";
                    distanceLabel.TextColor = Colors.BlueViolet;
                    distanceLabel.FontSize = 16;
                    distanceLabel.FontAttributes = FontAttributes.Bold;

                    // Centrar mapa entre el camión y el usuario
                    var centerLocation = new Location(
                        (userLocation.Latitude + truckLocation.Latitude) / 2,
                        (userLocation.Longitude + truckLocation.Longitude) / 2
                    );

                    map.MoveToRegion(MapSpan.FromCenterAndRadius(centerLocation, Distance.FromKilometers(distanceKm + 1)));

                    // Actualizar ruta (polilínea)
                    routeLine.Geopath.Clear();
                    routeLine.Geopath.Add(userLocation);
                    routeLine.Geopath.Add(truckLocation);
                }
            }
            catch (Exception ex)
            {
                distanceLabel.Text = "❌ Error al obtener ubicación: " + ex.Message;
                distanceLabel.TextColor = Colors.Red;
            }

            await Task.Delay(10000); // actualizar cada 10 segundos
        }
    }
}