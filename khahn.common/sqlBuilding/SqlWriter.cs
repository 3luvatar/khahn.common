using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;

namespace khahn.common.sqlBuilding
{
    public interface ISqlWriter
    {
        void StartSelect();
        void EndSelect();
        ICriteriaWriter Where();
    }

    public interface ICriteriaWriter : IDisposable
    {
        void WriteAnd(Clause clauseLeft, Clause clauseRight);
        void WriteOr(Clause clauseLeft, Clause clauseRight);
    }

    public class Clause
    {
        protected virtual void WriteTo(StringBuilder sb)
        {
        }

        public string SqlValue(object o)
        {
            if (o == null)
            {
                return "NULL";
            }
            if (o is IField)
            {
                IField field = (IField) o;
                return field.FieldDefinition.Name;
            }
            if (o is string)
            {
                return (string) o;
            }
            if (o is IEnumerable)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var val in (IEnumerable) o)
                {
                    sb.Append(SqlValue(val));
                }
                return sb.ToString();
            }
            return o.ToString();

        }
    }

    public class BinaryOperator : Clause
    {
        private const string equalsOper = "=";
        private readonly object _left;
        private readonly object _right;

        public BinaryOperator(string @operator, object left, object right)
        {
            Operator = @operator;
            _left = left;
            _right = right;
        }

        protected override void WriteTo(StringBuilder sb)
        {
            if (equalsOper.Equals(Operator) &&
                (_left == null || _right == null))
            {
                Operator = "IN";
            }
            sb.AppendFormat("{0} {1} {2}", SqlValue(_left), Operator, SqlValue(_right));
        }

        protected virtual string Operator { get; private set; }
    }

    public class SqlWriter
    {
    }
}