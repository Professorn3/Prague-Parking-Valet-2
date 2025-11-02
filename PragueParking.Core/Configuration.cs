using System.Collections.Generic;
using System;

namespace PragueParking.Core
{
    public class Configuration
    {
        public int TotalSpots { get; set; } = 100;
        public int SpotSize { get; set; } = 4;
        public int FreeParkingMinutes { get; set; } = 10;

        // FIX: Här är raden som saknades
        public int BusSpotLimit { get; set; } = 50;

        public Dictionary<string, int> Prices { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VehicleSizes { get; set; } = new Dictionary<string, int>();
        public string ParkingDataFile { get; set; } = "parking_data.json";

        public int CalculateCost(string vehicleType, TimeSpan duration)
        {
            if (duration.TotalMinutes <= FreeParkingMinutes)
                return 0;

            if (!Prices.TryGetValue(vehicleType, out int rate))
            {
                rate = 20;
            }

            int hours = (int)Math.Ceiling(duration.TotalHours);
            return hours * rate;
        }
    }
}

