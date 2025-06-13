using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using AppMonitoreo.Models;
using Firebase.Database.Query;
using System;

namespace AppMonitoreo.Services
{
    public class FirebaseServices
    {
        private readonly FirebaseClient firebaseClient;

        public FirebaseServices()
        {
            firebaseClient = new FirebaseClient("https://appmgl-5b338-default-rtdb.firebaseio.com/");
        }

        // Método para agregar una ubicación a Firebase
        public async Task AddUbicacion(Ubicacion ubicacion)
        {
            try
            {
                var result = await firebaseClient
                    .Child("Ubicaciones") // <-- Nodo más apropiado que 
                    .PostAsync(ubicacion);

                ubicacion.Id = result.Key;  // Se puede usar luego si quieres actualizar
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar ubicación: {ex.Message}");
            }
        }
    }
}
