using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;
using khahn.common.mapper;
using khahn.common.tests.utils;
using Moq;
using NUnit.Framework;

namespace khahn.common.tests.mapperTests
{
    [TestFixture]
    public class ConverterBuilderTests
    {
        private ConverterBuilder _converterBuilder;

        [SetUp]
        public void Setup()
        {
            _converterBuilder = new ConverterBuilder();
        }

        [Test]
        public void TestCache()
        {
            var fieldDef = new Mock<IFieldDefinition>();
            fieldDef.SetupGet(definition => definition.FieldType).Returns(typeof (int));
            var converterExpected = _converterBuilder.BuilderConverterFor(fieldDef.Object);
            var converterActual = _converterBuilder.BuilderConverterFor(fieldDef.Object);
            Assert.AreSame(converterExpected, converterActual);
            converterActual = _converterBuilder.BuildConverter(typeof (int?));
            Assert.AreNotSame(converterExpected, converterActual, "should produce different converter for nullable types");
        }

        private class TestToString
        {
            private string val;

            public TestToString(string val)
            {
                this.val = val;
            }

            public override string ToString()
            {
                return val;
            }
        }

        [Test(Description = "string converter test")]
        public void TestString()
        {
            var converter = _converterBuilder.BuildConverter(typeof (string));
            const string expectedValue = "thisIsATest";
            var valueToConvert = new TestToString(expectedValue);
            object convertedValue = converter(valueToConvert);
            Assert.IsInstanceOf<string>(convertedValue);
            Assert.AreEqual(expectedValue, convertedValue);
            Assert.IsNull(converter(null));
        }

        [Test]
        public void TestInt()
        {
            var converter = _converterBuilder.BuildConverter(typeof (int));
            const decimal valueToConvert = 4.95m;
            int expectedValue = Convert.ToInt32(valueToConvert);
            Assert.AreEqual(expectedValue, converter(valueToConvert));
            Assert.AreEqual(0, converter(""));
            Assert.AreEqual(0, converter(null));
            Assert.AreEqual(1337, converter("1337"));
            Assert.Throws<FormatException>(() => converter("fail"));
            converter = _converterBuilder.BuildConverter(typeof (int?));
            Assert.AreEqual(expectedValue, converter(valueToConvert));
            Assert.AreEqual(null, converter(""));
            Assert.AreEqual(null, converter(null));
            Assert.Throws<FormatException>(() => converter("fail"));
        }

        [Test]
        public void TestDateTime()
        {
            var converter = _converterBuilder.BuildConverter(typeof (DateTime));
            DateTime dateTime = DateTime.Today;
            string convertValue = dateTime.ToString();
            Assert.AreEqual(dateTime, converter(convertValue));
            Assert.AreEqual(new DateTime(), converter(""));
            Assert.AreEqual(new DateTime(), converter(null));
            Assert.Throws<FormatException>(() => converter("fail"));
            converter = _converterBuilder.BuildConverter(typeof (DateTime?));
            Assert.AreEqual(dateTime, converter(convertValue));
            Assert.AreEqual(null, converter(""));
            Assert.AreEqual(null, converter(null));
            Assert.Throws<FormatException>(() => converter("fail"));
        }

        [Test]
        public void TestBool()
        {
            var converter = _converterBuilder.BuildConverter(typeof (bool));
            Assert.IsTrue((bool) converter("y"));
            Assert.IsFalse((bool) converter("n"));
            Assert.IsTrue((bool) converter("Y"));
            Assert.IsFalse((bool) converter("N"));
            Assert.IsFalse((bool) converter(""));
            Assert.IsFalse((bool) converter(null));
            Assert.IsTrue((bool) converter(1));
            Assert.IsFalse((bool) converter(0));
            converter = _converterBuilder.BuildConverter(typeof(bool?));
            Assert.IsTrue((bool)converter("y"));
            Assert.IsFalse((bool)converter("n"));
            Assert.IsTrue((bool)converter("Y"));
            Assert.IsFalse((bool)converter("N"));
            Assert.IsNull(converter(""));
            Assert.IsNull(converter(null));
            Assert.IsTrue((bool)converter(1));
            Assert.IsFalse((bool)converter(0));
        }
    }
}