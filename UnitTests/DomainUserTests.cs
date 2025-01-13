using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Infrastructure.Services;

namespace UnitTests
{
    public class DomainUserTests
    {
        [Fact]
        public void ChangingStatusOfMachineUnavailable_Correct()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 1, true);

            //Act
            var result = testCar.SetCarAvailability();

            //Assert
            Assert.Equal(1, (int)result);
        }
        [Fact]
        public void ChangingStatusOfMachineUnavailable_Incorrect()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 0, true);

            //Act
            var result = testCar.SetCarAvailability();

            //Assert
            Assert.Equal(0, (int)result);
        }

        [Fact]
        public void BuyCarTest_Correct()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 1, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");

            var testManager1 = new User();
            testManager1.Create("Misha", "mixalev703@mail.ru", "Manager", "Az100Az.");
            var testManager2 = new User();
            testManager2.Create("Ira", "mixalev723@mail.ru", "Manager", "Az100Az.");
            var testManagers = new List<User>();
            testManagers.Add(testManager1);
            testManagers.Add(testManager2);

            //Act
            var result = testCar.BuyCar(testManagers, testClient);

            //Assert
            Assert.Equal(BuyCarActionResult.SendBuyMessage, result.Status);
            Assert.NotNull(result.Client);
            Assert.NotNull(result.Manager);
        }

        [Fact]
        public void BuyCarTest_InCorrectCarStockCount()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 0, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");

            var testManager1 = new User();
            testManager1.Create("Misha", "mixalev703@mail.ru", "Manager", "Az100Az.");
            var testManager2 = new User();
            testManager2.Create("Ira", "mixalev723@mail.ru", "Manager", "Az100Az.");
            var testManagers = new List<User>();
            testManagers.Add(testManager1);
            testManagers.Add(testManager2);

            //Act
            var result = testCar.BuyCar(testManagers, testClient);

            //Assert
            Assert.Equal(BuyCarActionResult.SendErrorMessageNoHaveCar, result.Status);
            Assert.NotNull(result.Client);
            Assert.Null(result.Manager);
        }

        [Fact]
        public void BuyCarTest_InCorrectManagers()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 1, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");
            var testManagers = new List<User>();


            //Act
            var result = testCar.BuyCar(testManagers, testClient);

            //Assert
            Assert.Equal(BuyCarActionResult.SendErrorMessageNoHaveManagers, result.Status);
            Assert.NotNull(result.Client);
            Assert.Null(result.Manager);
        }

        [Fact]
        public void InfoCarTest_Correct()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 1, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");

            var testManager1 = new User();
            testManager1.Create("Misha", "mixalev703@mail.ru", "Manager", "Az100Az.");
            var testManager2 = new User();
            testManager2.Create("Ira", "mixalev723@mail.ru", "Manager", "Az100Az.");
            var testManagers = new List<User>();
            testManagers.Add(testManager1);
            testManagers.Add(testManager2);

            //Act
            var result = testCar.InfoCar(testManagers, testClient);

            //Assert
            Assert.Equal(InfoCarActionResult.SendInfoMessage, result.Status);
            Assert.NotNull(result.Client);
            Assert.NotNull(result.Manager);
        }

        [Fact]
        public void InfoCarTest_InCorrectCarStockCount()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 0, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");

            var testManager1 = new User();
            testManager1.Create("Misha", "mixalev703@mail.ru", "Manager", "Az100Az.");
            var testManager2 = new User();
            testManager2.Create("Ira", "mixalev723@mail.ru", "Manager", "Az100Az.");
            var testManagers = new List<User>();
            testManagers.Add(testManager1);
            testManagers.Add(testManager2);

            //Act
            var result = testCar.InfoCar(testManagers, testClient);

            //Assert
            Assert.Equal(InfoCarActionResult.SendErrorMessageNoHaveCar, result.Status);
            Assert.NotNull(result.Client);
            Assert.Null(result.Manager);
        }

        [Fact]
        public void InfoCarTest_InCorrectManagers()
        {
            //Arrange
            var testCar = new Car();
            testCar.Create("BMW", "7", "green", 1, true);

            var testClient = new User();
            testClient.Create("Masha", "mixalev702@mail.ru", "null", "Az100Az.");
            var testManagers = new List<User>();


            //Act
            var result = testCar.InfoCar(testManagers, testClient);

            //Assert
            Assert.Equal(InfoCarActionResult.SendErrorMessageNoHaveManagers, result.Status);
            Assert.NotNull(result.Client);
            Assert.Null(result.Manager);
        }
    }
}
