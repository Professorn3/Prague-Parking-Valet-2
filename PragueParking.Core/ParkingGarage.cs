using PragueParking.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParking.Core
{
    public class ParkingGarage
    {
        public List<IParkingSpot> Spots { get; set; }
        private Configuration config;

        // da

        public ParkingGarage(Configuration config)
        {
            this.config = config;
            Spots = new List<IParkingSpot>();
            Initialize(config);
        }

        public ParkingGarage()
        {
            Spots = new List<IParkingSpot>();
            this.config = new Configuration();
        }

        public void Initialize(Configuration config)
        {
            this.config = config;

            if (Spots == null)
            {
                Spots = new List<IParkingSpot>();
            }

            // Synkronisera antalet platser
            if (Spots.Count < config.TotalSpots)
            {
                for (int i = Spots.Count + 1; i <= config.TotalSpots; i++)
                {
                    Spots.Add(new ParkingSpot(i, config.SpotSize));
                }
            }
            else if (Spots.Count > config.TotalSpots)
            {
                for (int i = Spots.Count - 1; i >= config.TotalSpots; i--)
                {
                    if (i < Spots.Count && Spots[i].GetCurrentFill() == 0)
                    {
                        Spots.RemoveAt(i);
                    }
                }
            }

            foreach (var spotInterface in Spots)
            {
                if (spotInterface is ParkingSpot spot)
                {
                    spot.Capacity = config.SpotSize;
                }
            }
        }


        public bool ParkVehicle(IVehicle vehicle, int spotNumber)
        {
            if (vehicle is Bus bus)
            {
                return ParkBus(bus, spotNumber);
            }

            // Normal parkering för Bil, MC, Cykel
            var spot = Spots.ElementAtOrDefault(spotNumber - 1); // Hitta plats
            if (spot == null)
            {
                return false; // Platsen existerar inte
            }

            // Använder ParkingSpot.Park() som kollar CanPark()
            return spot.Park(vehicle);
        }


        public bool ParkVehicle(IVehicle vehicle, out int parkedAtSpot)
        {
            if (vehicle is Bus bus)
            {
                return ParkBus(bus, out parkedAtSpot);
            }

            // Normal parkering för Bil, MC, Cykel
            for (int i = 0; i < Spots.Count; i++)
            {
                if (Spots[i].Park(vehicle))
                {
                    parkedAtSpot = i + 1;
                    return true;
                }
            }

            parkedAtSpot = -1;
            return false; 
        }

        private bool ParkBus(Bus bus, out int parkedAtSpot)
        {
            for (int i = 0; i <= this.config.BusSpotLimit - 4; i++)
            {
                if (AreSpotsAvailableForBus(i))
                {
                    ParkBus(bus, i + 1);
                    parkedAtSpot = i + 1;
                    return true;
                }
            }

            parkedAtSpot = -1;
            return false; 
        }

        private bool ParkBus(Bus bus, int startSpotNumber)
        {
            int startIndex = startSpotNumber - 1;

            if (startIndex < 0 || (startIndex + 3) >= Spots.Count) return false;
            if (startSpotNumber > this.config.BusSpotLimit) return false;

            if (!AreSpotsAvailableForBus(startIndex))
            {
                return false;
            }

            Spots[startIndex].ParkedVehicles.Add(bus);

            
            if (Spots[startIndex + 1] is ParkingSpot spot2) spot2.OccupiedByBusReg = bus.RegNumber;
            if (Spots[startIndex + 2] is ParkingSpot spot3) spot3.OccupiedByBusReg = bus.RegNumber;
            if (Spots[startIndex + 3] is ParkingSpot spot4) spot4.OccupiedByBusReg = bus.RegNumber;

            return true;
        }

        
        private bool AreSpotsAvailableForBus(int startIndex)
        {
            if (startIndex < 0 || (startIndex + 3) >= Spots.Count) return false;
            if ((startIndex + 1) > this.config.BusSpotLimit) return false;

            // Kontrollera alla fyra platser inviduellt 
            return Spots[startIndex].GetCurrentFill() == 0 &&
                   Spots[startIndex + 1].GetCurrentFill() == 0 &&
                   Spots[startIndex + 2].GetCurrentFill() == 0 &&
                   Spots[startIndex + 3].GetCurrentFill() == 0;
        }

        public IVehicle? UnparkVehicle(string regNummer, out int spotNumber)
        {
            // Hitta platsen där fordonet är parkerat
            var spot = Spots.FirstOrDefault(s =>
                s.ParkedVehicles.Any(v =>
                    v.RegNumber.Equals(regNummer, StringComparison.OrdinalIgnoreCase)));

            if (spot == null)
            {
                spotNumber = -1;
                return null;
            }

            // Vi hittade fordonet, hämta ut det
            var vehicle = spot.Unpark(regNummer);
            spotNumber = spot.SpotNumber;

            if (vehicle is Bus)
            {
                // bussen ska ta sin plats + 3 extra rutor (16 storlek)
                int startIndex = spotNumber - 1; // 0-baserat index
                if (startIndex + 3 < Spots.Count) // Säkerhetskoll
                {
                    if (Spots[startIndex + 1] is ParkingSpot spot2) spot2.OccupiedByBusReg = null;
                    if (Spots[startIndex + 2] is ParkingSpot spot3) spot3.OccupiedByBusReg = null;
                    if (Spots[startIndex + 3] is ParkingSpot spot4) spot4.OccupiedByBusReg = null;
                }
            }

            return vehicle;
        }

        public IParkingSpot? FindVehicle(string regNummer)
        {
            return Spots.FirstOrDefault(s =>
                s.ParkedVehicles.Any(v => v.RegNumber.Equals(regNummer, StringComparison.OrdinalIgnoreCase)) ||
                (s is ParkingSpot ps && ps.OccupiedByBusReg == regNummer)
            );
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

            if (newConfig.BusSpotLimit < this.config.BusSpotLimit)
            {
                for (int i = newConfig.BusSpotLimit; i < this.config.BusSpotLimit; i++)
                {
                    if (i < this.Spots.Count && this.Spots[i].ParkedVehicles.Any(v => v is Bus))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

