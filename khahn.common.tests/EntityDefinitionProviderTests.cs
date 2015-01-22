using System;
using System.Linq;
using khahn.common.entity.interfaces;
using khahn.common.entity.JsonProviders;
using khahn.common.entity.ReflectionProviders;
using Moq;
using NUnit.Framework;

namespace khahn.common.tests
{
    [TestFixture]
    public class EntityDefinitionProviderTests
    {
        private IEntityDefinitionProvider _entityDefinitionProvider;

        [SetUp]
        public void SetupTest()
        {
            _entityDefinitionProvider = new ReflectionEntityDefinitionProvider();
        }

        [Test]
        public void TestNameAndFields()
        {
            var entityDef = _entityDefinitionProvider.GetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>());
            Assert.AreEqual("TestClass", entityDef.Name);
            string[] fields = { "stringField", "longField", "objectField", "dateField" };
            CollectionAssert.AreEquivalent(fields, entityDef.FieldDefinitions.Select(definition => definition.Name));
        }

        [Test]
        public void TestTryGet()
        {
            IEntityDefinition entityDefinition;
            Assert.IsFalse(_entityDefinitionProvider.TryGetEntityDefinition(new NamedKey(""), out entityDefinition));
            Assert.IsNull(entityDefinition);
            Assert.IsTrue(_entityDefinitionProvider.TryGetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>(),
                out entityDefinition));
            Assert.AreEqual(_entityDefinitionProvider.GetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>()),
                entityDefinition);
        }

        [Test]
        public void TestGenerate()
        {
            var entityDef = _entityDefinitionProvider.GetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>());
            var entity = entityDef.GenerateEntity();
            Assert.AreSame(entityDef, entity.EntityDefinition);
            Assert.IsAssignableFrom<TestClass>(entity);
            var testClass = (TestClass) entity;
            Assert.IsNotNull(testClass.longField);
            Assert.IsNotNull(testClass.stringField);
            Assert.IsNotNull(testClass.objectField);
            Field[] fields = {testClass.longField, testClass.stringField, testClass.dateField, testClass.objectField};
            CollectionAssert.AreEquivalent(fields,
                entityDef.FieldDefinitions.Select(definition => definition.GetField(entity)));
            CollectionAssert.AreEquivalent(fields.Select(field => field.FieldDefinition), entityDef.FieldDefinitions);
        }

        [Test]
        public void TestFieldTypes()
        {
            var entityDef = _entityDefinitionProvider.GetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>());
            var fields = entityDef.FieldDefinitions.ToDictionary(definition => definition.Name, definition => definition);
            IFieldDefinition stringField = fields["stringField"];
            IFieldDefinition longField = fields["longField"];
            IFieldDefinition objectField = fields["objectField"];
            Assert.AreEqual(typeof (string), stringField.FieldType);
            Assert.AreEqual(typeof (long), longField.FieldType);
            Assert.AreEqual(typeof (object), objectField.FieldType);
        }

        [Test]
        public void TestFieldGenerate()
        {
            var entityDef = _entityDefinitionProvider.GetEntityDefinition(ReflectedEntityKey.EntityKey<TestClass>());
            var fields = entityDef.FieldDefinitions.ToDictionary(definition => definition.Name, definition => definition);
            IFieldDefinition stringField = fields["stringField"];
            IFieldDefinition longField = fields["longField"];
            IFieldDefinition objectField = fields["objectField"];
            Assert.IsAssignableFrom<Field<string>>(stringField.GenerateField());
            Assert.IsAssignableFrom<Field<long>>(longField.GenerateField());
            Assert.IsAssignableFrom<Field<object>>(objectField.GenerateField());
        }
    }
}