using System.Collections.Generic;
using System.Linq;

namespace PragueParking.Core
{
    public class ParkingGarage
    {
        public List<IParkingSpot> Spots { get; set; }
        private Configuration config; // Behåll en referens till config

        public ParkingGarage(Configuration config)
        {
            this.config = config; // Spara config
            Spots = new List<IParkingSpot>();
            Initialize(config);
        }

        public ParkingGarage()
        {
            Spots = new List<IParkingSpot>();
            this.config = new Configuration(); // Skapa en default config
        }

        public void Initialize(Configuration config)
        {
            this.config = config; // Uppdatera config
            Spots = new List<IParkingSpot>();
            for (int i = 1; i <= config.TotalSpots; i++)
            {
                Spots.Add(new ParkingSpot(i, config.SpotSize));
            }
        }

        public bool ParkVehicle(IVehicle vehicle, int spotNumber)
        {
            var spot = Spots.FirstOrDefault(s => s.SpotNumber == spotNumber);

            // FIX: Använd BusSpotLimit från den sparade config-referensen
            if (vehicle is Bus && spotNumber > this.config.BusSpotLimit)
            {
                return false;
            }

            if (spot != null && spot.CanPark(vehicle))
            {
                return spot.Park(vehicle);
            }
            return false;
        }

        public bool ParkVehicle(IVehicle vehicle, out int parkedAtSpot)
        {
            int startSpot = 1;
            // FIX: Använd BusSpotLimit från den sparade config-referensen
            int endSpot = (vehicle is Bus) ? this.config.BusSpotLimit : Spots.Count;

            for (int i = startSpot - 1; i < endSpot; i++)
            {
                if (Spots[i].CanPark(vehicle))
                {
                    Spots[i].Park(vehicle);
                    parkedAtSpot = Spots[i].SpotNumber;
                    return true;
                }
            }

            parkedAtSpot = -1;
            return false;
        }

        public IVehicle? UnparkVehicle(string regNummer, out int spotNumber)
        {
            foreach (var spot in Spots)
            {
                var vehicle = spot.Unpark(regNummer);
                if (vehicle != null)
                {
                    spotNumber = spot.SpotNumber;
                    return vehicle;
                }
            }
            spotNumber = -1;
            return null;
        }

        public IParkingSpot? FindVehicle(string regNummer)
        {
            return Spots.FirstOrDefault(s =>
                s.ParkedVehicles.Any(v =>
                    v.RegNumber.Equals(regNummer, StringComparison.OrdinalIgnoreCase)));
        }

        public bool CanUpdateConfiguration(Configuration newConfig)
        {
            if (newConfig.TotalSpots < this.Spots.Count)
            {
                for (int i = newConfig.TotalSpots; i < this.Spots.Count; i++)
                {
                    if (this.Spots[i].GetCurrentFill() > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

