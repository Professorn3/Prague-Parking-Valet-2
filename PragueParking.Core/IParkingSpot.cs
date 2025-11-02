using System.Collections.Generic;

namespace PragueParking.Core
{
    public interface IParkingSpot
    {
        int SpotNumber { get; }
        int Capacity { get; }
        List<IVehicle> ParkedVehicles { get; }

        int GetCurrentFill();
        bool CanPark(IVehicle vehicle);

        // FIX: Denna metod lades till för att lösa 'does not contain definition for Park'
        bool Park(IVehicle vehicle);

        IVehicle? Unpark(string regNummer);
    }
}

