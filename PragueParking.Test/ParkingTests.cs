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
            // Arrange
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var car = new Car("TEST-01"); // Size = 4

            // Act
            bool result = spot.Park(car);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }

        [TestMethod]
        public void ParkTwoMCs_OnEmptySpot_ShouldSucceed()
        {
            // Arrange
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var mc1 = new Motorcycle("MC-001"); // Size = 2
            var mc2 = new Motorcycle("MC-002"); // Size = 2

            // Act
            bool result1 = spot.Park(mc1);
            bool result2 = spot.Park(mc2);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(2, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }

        [TestMethod]
        public void ParkCarAndMC_OnEmptySpot_ShouldFail()
        {
            // Arrange
            var spot = new ParkingSpot(number: 1, capacity: 4);
            var car = new Car("CAR-01"); // Size = 4
            var mc = new Motorcycle("MC-01"); // Size = 2

            // Act
            bool resultCar = spot.Park(car);
            bool resultMC = spot.Park(mc); // Denna ska misslyckas

            // Assert
            Assert.IsTrue(resultCar);
            Assert.IsFalse(resultMC);
            Assert.AreEqual(1, spot.ParkedVehicles.Count);
            Assert.AreEqual(4, spot.GetCurrentFill());
        }
    }
}

