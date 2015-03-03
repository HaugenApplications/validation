using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HaugenApps.ChangeTracking;
using HaugenApps.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HaugenApps.ValidationTests
{
    [TestClass]
    public class ValidatorTests
    {
        class TestClass
        {
            [Range(1, 19)]
            public int IntegerProperty { get; set; }
            public string StringProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
        }

        [TestMethod]
        public void RequiredTest()
        {
            var propWatch = new PropertyWatcher<TestClass>();

            propWatch.Set(c => c.StringProperty, "Test string!");

            var ret = propWatch.Validate(x => x.Required(c => c.StringProperty, c => c.IntegerProperty)).ToList();

            Assert.AreEqual(1, ret.Count, "Incorrect validation error count!");
            Assert.AreEqual("IntegerProperty", ret[0].Property.Name);
        }

        [TestMethod]
        public void DateRangeTest()
        {
            var propWatch = new PropertyWatcher<TestClass>();

            propWatch.Set(c => c.DateTimeProperty, DateTime.UtcNow.AddDays(-1));

            var ret = propWatch.Validate(x => x.DateRange(null, DateTime.UtcNow.AddDays(-2), c => c.DateTimeProperty)).ToList();

            Assert.AreEqual(1, ret.Count, "Incorrect validation error count!");
            Assert.AreEqual("DateTimeProperty", ret[0].Property.Name);
        }

        [TestMethod]
        public void DataAnnotationTest()
        {
            var propWatch = new PropertyWatcher<TestClass>();

            propWatch.Set(c => c.IntegerProperty, 25);

            var ret = propWatch.Validate(x => x.HonorDataAnnotations()).ToList();

            Assert.AreEqual(1, ret.Count, "Incorrect validation error count!");
            Assert.AreEqual("IntegerProperty", ret[0].Property.Name);

            propWatch.Set(c => c.IntegerProperty, 5);

            ret = propWatch.Validate(x => x.HonorDataAnnotations()).ToList();

            Assert.AreEqual(0, ret.Count, "Validation error occurred.");
        }
    }
}
