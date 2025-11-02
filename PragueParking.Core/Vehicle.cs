using System;

namespace PragueParking.Core
{
    // VIKTIGT: Vi tar bort [JsonConverter(typeof(VehicleConverter))] härifrån.
    // Det var källan till de flesta JSON-felen.
    public abstract class Vehicle : IVehicle
    {
        // ==========================================================
        // HÄR ÄR FIXEN:
        // Vi ändrar tillbaka till "public set".
        // Detta är den enklaste lösningen för att låta
        // Newtonsoft.Json ställa in värdena vid omladdning.
        // ==========================================================
        public string RegNumber { get; set; }
        public DateTime ArrivalTime { get; set; }

        public abstract string VehicleType { get; }
        public abstract int Size { get; }

        protected Vehicle(string regNumber)
        {
            RegNumber = regNumber;
            ArrivalTime = DateTime.Now;
        }

        protected Vehicle()
        {
            // Vi ändrar "!!!" till string.Empty (tom sträng).
            RegNumber = string.Empty;
            ArrivalTime = DateTime.Now;
        }
    }

    // Hela VehicleConverter-klassen är BORTTAGEN.
    // Den behövs inte och orsakade bara problem.
}

