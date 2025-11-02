using System.Collections.Generic;

namespace PragueParking.Core
{

    // mall för en parkeringsplats
    public interface IParkingSpot
    {
        int SpotNumber { get; }
        int Capacity { get; }
        List<IVehicle> ParkedVehicles { get; }

        int GetCurrentFill();
        bool CanPark(IVehicle vehicle);


        bool Park(IVehicle vehicle);

        IVehicle? Unpark(string regNummer);
    }
}

