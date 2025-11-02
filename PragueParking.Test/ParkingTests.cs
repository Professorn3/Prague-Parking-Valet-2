using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParking.Core;

namespace PragueParking.Tests
{
    [TestClass]
    public class ParkingTests
    {
        [TestMethod]
        public void ParkCar_OnEmptySpot_ShouldSucceed()
        {
            // Här kommer AAA-principen in i bilden: Arrange, Act, Assert nedanför 
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var car = new Car("TEST-01");

            
            bool result = spot.Park(car);

            
            Assert.IsTrue(result);
            Assert.AreEqual(1, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }

        [TestMethod]
        public void ParkTwoMCs_OnEmptySpot_ShouldSucceed()
        {

         
            
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var mc1 = new Motorcycle("MC-001"); 
            var mc2 = new Motorcycle("MC-002"); 
            bool result1 = spot.Park(mc1);
            bool result2 = spot.Park(mc2);

          
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(2, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }

        [TestMethod]
        public void ParkCarAndMC_OnEmptySpot_ShouldFail()
        {
         
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var car = new Car("CAR-01"); 
            var mc = new Motorcycle("MC-01"); 
            bool resultCar = spot.Park(car);
            bool resultMC = spot.Park(mc); 
            Assert.IsTrue(resultCar);
            Assert.IsFalse(resultMC);
            Assert.AreEqual(1, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }
    }
}