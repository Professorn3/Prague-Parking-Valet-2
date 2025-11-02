namespace PragueParking.Core
{
	public class Motorcycle : Vehicle
	{
		public override string VehicleType => "Motorcycle";
		public override int Size => 2; // Enligt VG-spec

		public Motorcycle(string regNumber) : base(regNumber) { }
		public Motorcycle() : base() { }
	}
}
