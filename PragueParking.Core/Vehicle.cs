using System;

namespace PragueParking.Core
{
    
    
    public abstract class Vehicle : IVehicle
    {

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
            
            RegNumber = string.Empty;
            ArrivalTime = DateTime.Now;
        }
    }

    
    
}

