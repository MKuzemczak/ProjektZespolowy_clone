using NUnit.Framework;
using System;
using TestDl.Services;

namespace TestDl.Service.Test
{
    public class CalculatorTest
    {
        private ICalculator Sut;

        [SetUp]
        public void ClassInitialize()
        {
            Sut = new Calculator();
        }

        [Test]
        public void Add_5Plus5_Expect10()
        {
            //Assign
            int a = 5, b = 5;
            var expectedResult = 10;

            //Act
            var result = Sut.Add(a, b);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Add_Minus5Plus5_Expect0()
        {
            //Assign
            int a = -5, b = 5;
            var expectedResult = 0;

            //Act
            var result = Sut.Add(a, b);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Add_MaxIntPlus1_ExpectExeption()
        {
            //Assign
            int a = int.MaxValue, b = 1;
            TestDelegate actFunction = () => Sut.Add(a, b);

            //Act
            //Assert
            Assert.Throws<OverflowException>(actFunction);
        }

        [Test]
        public void Sub_5Minus5_Expect0()
        {
            //Assign
            int a = 5, b = 5;
            var expectedResult = 0;

            //Act
            var result = Sut.Sub(a, b);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Sub_5MinusNegative5_Expect10()
        {
            //Assign
            int a = 5, b = -5;
            var expectedResult = 10;

            //Act
            var result = Sut.Sub(a, b);

            //Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Sub_MinIntMinus1_ExpectExeption()
        {
            //Assign
            int a = int.MinValue, b = 1;
            TestDelegate actFunction = () => Sut.Sub(a, b);

            //Act
            //Assert
            Assert.Throws<OverflowException>(actFunction);
        }
    }
}
