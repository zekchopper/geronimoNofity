using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class IzletEqTests
    {
        [TestMethod()]
        public void NoviIzlet()
        {
            //Arrange
            var stari = new List<Izlet>() 
            { 
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "10" }
            };

            var novi = stari.ToList().Append(new Izlet { id = "3", limitation = "20", registered = "0" });

            //act
            var diff = novi.Except(stari, new IzletEq()).ToList();

            //assert
            //samo taj id 3 je u diffu
            Assert.AreEqual(1, diff.Count());
            Assert.AreEqual("3", diff.First().id);
        }

        [TestMethod()]
        public void OslobodiloSeMjestoNetkoSeOdjavio()
        {
            //Arrange
            var stari = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "10" }
            };
            
            var novi = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "9" }
            };

            //act
            var diff = novi.Except(stari, new IzletEq()).ToList();

            //assert
            //treba biti taj jedan s jednim mjestom
            Assert.AreEqual(1, diff.Count());
            Assert.AreEqual(1, diff.First().preostalo);
            Assert.AreEqual("2", diff.First().id);
        }

        [TestMethod()]
        public void OslobodiloSeMjestoNetkoSeOdjavioAliBiloJeMjesta()
        {
            //Arrange
            var stari = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "5" }
            };

            var novi = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "4" }
            };

            //act
            var diff = novi.Except(stari, new IzletEq()).ToList();

            //assert            
            Assert.AreEqual(0, diff.Count());
        }

        [TestMethod()]
        public void OslobodiloSeMjestoNetkoSeOdjavioAliIDaljeNemaMjesta()
        {
            //Arrange
            var stari = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "12" }
            };

            var novi = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "10" }
            };

            //act
            var diff = novi.Except(stari, new IzletEq()).ToList();

            //assert            
            Assert.AreEqual(0, diff.Count());
        }

        [TestMethod()]
        public void StariIzletMaknut()
        {
            //Arrange
            var stari = new List<Izlet>()
            {
                new Izlet { id = "1", limitation = "5", registered = "5" },
                new Izlet { id = "2", limitation = "10", registered = "10" }
            };

            var novi = new List<Izlet>()
            {                
                new Izlet { id = "2", limitation = "10", registered = "10" }
            };

            //act
            var diff = novi.Except(stari, new IzletEq()).ToList();

            //assert
            //0 razlike
            Assert.AreEqual(0, diff.Count());            
        }

    }
}