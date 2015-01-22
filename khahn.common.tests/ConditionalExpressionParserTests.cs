using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;
using khahn.common.sqlBuilding;
using NUnit.Framework;

namespace khahn.common.tests
{
    [TestFixture]
    public class ConditionalExpressionParserTests
    {
        private ConditionalExpressionParser _conditionalExpression;
        private IEntityDefinitionProvider _entityDefinitionProvider;

        [SetUp]
        public void Setup()
        {
            _conditionalExpression = new ConditionalExpressionParser(ReflectedEntityKey.EntityKey<TestClass>());
            _entityDefinitionProvider = new ReflectionEntityDefinitionProvider();
        }

        private string TestExpressionEquals<T>(Expression<Func<T, bool>> expression, string expectedSql)
            where T : IEntity
        {
            _conditionalExpression.SetExpression(expression);
            string actualSql = _conditionalExpression.ToSql(_entityDefinitionProvider);
            Assert.AreEqual(expectedSql, actualSql);
            return actualSql;
        }

        [Test]
        public void TestStringEquals()
        {
            TestExpressionEquals<TestClass>(test => test.stringField.Value == "test", "(stringField='test')");
            TestExpressionEquals<TestClass>(test => test.stringField == "test", "(stringField='test')");
        }

        [Test]
        public void TestStringStartsWith()
        {
            TestExpressionEquals<TestClass>(test => test.stringField.Value.StartsWith("test"),
                "upper(stringField) like 'TEST%' ");
        }

        [Test]
        public void TestAndCondition()
        {
            TestExpressionEquals<TestClass>(test => test.longField == 5 && test.stringField == "rawr",
                "((longField=5)AND(stringField='rawr'))");
        }

        [Test]
        public void TestOrCondition()
        {
            TestExpressionEquals<TestClass>(test => test.longField == 5 || test.stringField == "rawr",
                "((longField=5)OR(stringField='rawr'))");
        }

        [Test]
        public void TestComplexBinaryConditions()
        {
            TestExpressionEquals<TestClass>(
                test =>
                    (test.longField == 5 || test.stringField == "rawr") &&
                    (test.longField > 10 || test.dateField == new DateTime(1992, 5, 5)),
                "(((longField=5)OR(stringField='rawr'))AND((longField>10)OR(dateField='5/5/1992 12:00:00 AM')))");
        }

        [Test]
        public void TestDateCompare()
        {
            TestExpressionEquals<TestClass>(test => test.dateField < new DateTime(1992, 5, 5),
                "(dateField<'5/5/1992 12:00:00 AM')");
        }
    }
}