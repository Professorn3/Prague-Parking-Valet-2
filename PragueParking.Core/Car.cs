namespace PragueParking.Core
{
    // REPARERAD: Ärver från Vehicle och anropar base()
    public class Car : Vehicle
    {
        public override string VehicleType => "Car";
        public override int Size => 4;

        public Car(string regNumber) : base(regNumber) { }
        public Car() : base() { }
    }
}

