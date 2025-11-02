namespace PragueParking.Core
{

    public class Bus : Vehicle
    {
        public override string VehicleType => "Bus";
        public override int Size => 16;

        public Bus(string regNumber) : base(regNumber) { }
        public Bus() : base() { }
    }
}

