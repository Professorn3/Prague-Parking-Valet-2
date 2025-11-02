using System;

// Det säkerställer att oavsett fordon, så måste alla ha ett registreringsnummer, en ankomsttid, en typ och en storlek.

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

