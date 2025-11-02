using System;

namespace PragueParking.Core
{
    public interface IVehicle
    {
        string RegNumber { get; }
        DateTime ArrivalTime { get; }
        string VehicleType { get; }
        int Size { get; }
    }
}

