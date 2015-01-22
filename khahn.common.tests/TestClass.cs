using System;
using khahn.common.entity.ReflectionProviders;

namespace khahn.common.tests
{
    public class TestClass : ClassEntity
    {
        public Field<string> stringField { get; set; }
        public Field<long> longField { get; set; }
        public Field<DateTime> dateField { get; set; }
        public Field<object> objectField { get; set; }
    }
}