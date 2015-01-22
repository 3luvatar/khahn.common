using System;
using System.Collections.Generic;
using khahn.common.entity;
using khahn.common.entity.fetchExpressions;
using khahn.common.entity.interfaces;
using khahn.common.entity.JsonProviders;
using khahn.common.entity.ReflectionProviders;
using Moq;
using NUnit.Framework;

namespace khahn.common.tests
{
    [TestFixture]
    public class FetchExpressionTests
    {
        [Test]
        public void TestAllFieldsExpression()
        {
            List<IFieldDefinition> fieldsList = new List<IFieldDefinition>()
            {
                Mock.Of<IFieldDefinition>(),
                Mock.Of<IFieldDefinition>(),
                Mock.Of<IFieldDefinition>(),
                Mock.Of<IFieldDefinition>(),
            };
            IEntityKey entityKey = new NamedKey("test");
            var moqEntityDefinition = new Mock<IEntityDefinition>();
            moqEntityDefinition.SetupGet(definition => definition.FieldDefinitions).Returns(fieldsList);
            var moqEntityProvider = new Mock<IEntityDefinitionProvider>();
            moqEntityProvider.Setup(provider => provider.GetEntityDefinition(It.IsAny<IEntityKey>()))
                .Returns(moqEntityDefinition.Object);
            var fetchExpression = EntityFetchExpression.AllFieldsExpression(entityKey);
            Assert.AreSame(entityKey, fetchExpression.EntityKey);
            CollectionAssert.AreEqual(fieldsList, fetchExpression.Fields(moqEntityProvider.Object));
        }
    }
}