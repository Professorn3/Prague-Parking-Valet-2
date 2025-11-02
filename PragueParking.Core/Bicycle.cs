namespace PragueParking.Core
{
    public class Bicycle : Vehicle
    {
        public override string VehicleType => "Bicycle";
        public override int Size => 1; // Enligt VG-spec

        public Bicycle(string regNumber) : base(regNumber) { }
        public Bicycle() : base() { }
    }
}
