namespace PragueParking.Core
{
    // ärver från Vehicle och sätter Size till 2

    public class Motorcycle : Vehicle
	{
		public override string VehicleType => "Motorcycle";
		public override int Size => 2;

		public Motorcycle(string regNumber) : base(regNumber) { }
		public Motorcycle() : base() { }
	}
}
