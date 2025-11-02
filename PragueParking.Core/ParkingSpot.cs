using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace PragueParking.Core
{
    public class ParkingSpot : IParkingSpot
    {
        public int SpotNumber { get; set; }
        public int Capacity { get; set; }

        //Om denna ruta blockeras av en buss, lagras bussens regnr här.
        public string? OccupiedByBusReg { get; set; } = null;

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

        // Kollar nu om den är blockerad av en buss
        public int GetCurrentFill()
        {
            if (OccupiedByBusReg != null)
            {
                return this.Capacity;
            }
            if (ParkedVehicles.Any(v => v is Bus))
            {
                return this.Capacity;
            }
            // Annars, räkna för MC/Cyklar 
            return ParkedVehicles.Sum(v => v.Size);
        }

        public bool CanPark(IVehicle vehicle)
        {

            // Kan inte parkera om den är blockerad av en buss, bussar hanteras annorlunda
            if (OccupiedByBusReg != null)
            {
                return false;
            }

            // Normal regel för alla andra fordon (Bil, MC, Cykel)
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
                v.RegNumber.Equals(regNummer, System.StringComparison.OrdinalIgnoreCase));

            if (vehicle != null)
            {
                ParkedVehicles.Remove(vehicle);
                return vehicle;
            }
            return null;
        }
    }
}

