namespace PragueParking.Core
{
    // ärver helt korrekt från Vehicle och sätter Size till 1
    public class Bicycle : Vehicle
    {
        public override string VehicleType => "Bicycle";
        public override int Size => 1; 

        public Bicycle(string regNumber) : base(regNumber) { }
        public Bicycle() : base() { }
    }
}
