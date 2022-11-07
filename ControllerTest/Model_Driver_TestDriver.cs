using Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Model {
    [TestFixture]
    public class Model_Driver {

        Driver d1, d2;

        [SetUp]
        public void Setup() {
            d1 = new Driver("Driver1", new Car(0,0,0, false), IParticipant.TeamColors.Red);
            d2 = new Driver("Driver2", new Car(0,0,0, false), IParticipant.TeamColors.Blue);
        }

        [Test]
        public void DriverToString() {
            string expected = "Driver Driver1";
            string actual = d1.ToString();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AreEqual() {
            Assert.AreNotEqual(d1, d2);
        }

        [Test]
        public void Equal_IsNotDriver() {
            bool expected = false;
            Car c = new Car(0,0,0, false);
            bool actual = d1.Equals(c);
            Assert.AreEqual(expected, actual);
        }
    }
}