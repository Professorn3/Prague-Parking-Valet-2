namespace PragueParking.Core
{
    // REPARERAD: Ärver från Vehicle och anropar base()
    public class Bus : Vehicle
    {
        public override string VehicleType => "Bus";
        public override int Size => 16;

        public Bus(string regNumber) : base(regNumber) { }
        public Bus() : base() { }
    }
}

