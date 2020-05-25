using Moq;
using NUnit.Framework;
using TestDl.Services;

namespace TestDl.Service.Test
{
    public class VatCalculatorTests
    {
        private IVatCalculator Sut;
        private Mock<ICalculator> calculator;

        [SetUp]
        public void ClassInitialize()
        {
            calculator = new Mock<ICalculator>();
            calculator.Setup(c => c.Add(It.IsAny<int>(), It.IsAny<int>())).Returns((int a, int b) => a + b);

            Sut = new VatCalculator(calculator.Object);
        }

        [Test]
        public void AddVat_ForInput1000_Expect1023()
        {
            //Assign
            var input = 1000;
            var expectedResult = 1230;

            //Act
            var result = Sut.AddVat(input);


            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void AddVat_ForInput1000_CheckVatValue()
        {
            //Act
            var input = 1000;
            var expectedResult = 230;
            calculator.Setup(c => c.Add(It.IsAny<int>(), It.IsAny<int>())).Returns((int a, int b) => a < b ? a : b);

            //Assign
            var onlyVat = Sut.AddVat(input);

            //Assert
            Assert.AreEqual(expectedResult, onlyVat);
        }
    }
}
