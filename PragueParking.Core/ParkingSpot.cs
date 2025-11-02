using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json; // <-- Lägg till denna "using"

namespace PragueParking.Core
{
    public class ParkingSpot : IParkingSpot
    {
        public int SpotNumber { get; set; }
        public int Capacity { get; set; }

        // ==========================================================
        // HÄR ÄR FIXEN:
        // Denna rad talar om för JSON-biblioteket att
        // spara typ-information (Car, Bus, etc.) i listan.
        // Detta ersätter hela VehicleConverter-klassen.
        // ==========================================================
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<IVehicle> ParkedVehicles { get; set; }

        public ParkingSpot(int number, int capacity)
        {
            SpotNumber = number;
            Capacity = capacity;
            ParkedVehicles = new List<IVehicle>();
        }

        public ParkingSpot()
        {
            ParkedVehicles = new List<IVehicle>();
        }

        public int GetCurrentFill()
        {
            return ParkedVehicles.Sum(v => v.Size);
        }

        public bool CanPark(IVehicle vehicle)
        {
            return (GetCurrentFill() + vehicle.Size) <= Capacity;
        }

        public bool Park(IVehicle vehicle)
        {
            if (CanPark(vehicle))
            {
                ParkedVehicles.Add(vehicle);
                return true;
            }
            return false;
        }

        public IVehicle? Unpark(string regNummer)
        {
            var vehicle = ParkedVehicles.FirstOrDefault(v =>
                v.RegNumber.Equals(regNummer, StringComparison.OrdinalIgnoreCase));

            if (vehicle != null)
            {
                ParkedVehicles.Remove(vehicle);
                return vehicle;
            }
            return null;
        }
    }
}

