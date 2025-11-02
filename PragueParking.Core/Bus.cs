namespace PragueParking.Core
{
    // ärver också från Vehicle och sätter Size till 16

    public class Bus : Vehicle
    {
        public override string VehicleType => "Bus";
        public override int Size => 16;

        public Bus(string regNumber) : base(regNumber) { }
        public Bus() : base() { }
    }
}

