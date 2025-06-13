using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using AppMonitoreo.Models;

namespace AppMonitoreo.Services
{
    public class FirebaseServices
    {
        private readonly FirebaseClient firebaseClient;

        public FirebaseServices()
        {
            firebaseClient = new FirebaseClient("https://appmgl-5b338-default-rtdb.firebaseio.com/");
        }

        // Método para agregar una ubicación con el UID del usuario
        public async Task AddUbicacion(string uid, Ubicacion ubicacion)
        {
            try
            {
                var result = await firebaseClient
                    .Child("Ubicaciones")
                    .Child(uid) // Guardar bajo el UID del usuario
                    .PostAsync(ubicacion);

                ubicacion.Id = result.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar ubicación: {ex.Message}");
            }
        }

        // Método para obtener la última ubicación del usuario por UID
        public async Task<Ubicacion> ObtenerUltimaUbicacion(string uid)
        {
            try
            {
                var result = await firebaseClient
                    .Child("Ubicaciones")
                    .Child(uid)
                    .OnceAsync<Ubicacion>();

                var ultima = result.LastOrDefault();
                return ultima?.Object;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ubicación: {ex.Message}");
                return null;
            }
        }

        public async Task<Ubicacion> ObtenerUbicacionDeUsuario(string uid)
        {
            try
            {
                var ubicacion = await firebaseClient
                    .Child("Ubicaciones")
                    .Child(uid)
                    .OnceSingleAsync<Ubicacion>();

                return ubicacion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la ubicación del usuario {uid}: {ex.Message}");
                return null;
            }
        }

        public async Task SetUbicacionActual(string uid, Ubicacion ubicacion)
        {
            try
            {
                await firebaseClient
                    .Child("Ubicaciones")
                    .Child(uid) // Nodo del usuario
                    .PutAsync(ubicacion); // Sobrescribe la ubicación actual del usuario
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ubicación del usuario {uid}: {ex.Message}");
            }
        }

    }
}
