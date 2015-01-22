using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using khahn.common.entity;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;
using khahn.common.utils;

namespace khahn.common.sqlBuilding
{
    public interface IConditionalExpression
    {
        void appendTo(StringBuilder sb, IEntityDefinitionProvider definitionProvider);
    }

    public static class ConditionalExpressionExtensions
    {
        public static string ToSql(this IConditionalExpression conditionalExpression,
            IEntityDefinitionProvider definitionProvider)
        {
            var sb = new StringBuilder();
            conditionalExpression.appendTo(sb, definitionProvider);
            return sb.ToString();
        }
    }

    public class ConditionalExpressionParser : IConditionalExpression
    {
        private ReflectedEntityKey _baseEntityKey;
        private string sep = string.Empty;
        private IEntityDefinition _entityDefinition;

        public ConditionalExpressionParser(ReflectedEntityKey baseEntityKey)
        {
            _baseEntityKey = baseEntityKey;
        }

        public void appendTo(StringBuilder sb, IEntityDefinitionProvider definitionProvider)
        {
            _entityDefinition = definitionProvider.GetEntityDefinition(_baseEntityKey);
            sb.Append(Visit(_expression));
        }

        private Expression _expression;

        public void SetExpression<T>(Expression<Func<T, bool>> expression) where T : IEntity
        {
            _expression = expression;
        }

        #region visit

        protected internal virtual object Visit(Expression exp)
        {
            if (exp == null) return string.Empty;
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    //return "(" + VisitBinary(exp as BinaryExpression) + ")";
                    return VisitBinary(exp as BinaryExpression);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit(exp as MemberInitExpression);
                default:
                    return exp.ToString();
            }
        }

        protected virtual object VisitLambda(LambdaExpression lambda)
        {
            return Visit(lambda.Body);
        }

        protected virtual object VisitBinary(BinaryExpression b)
        {
            object left, right;
            var operand = BindOperant(b.NodeType); //sep= " " ??
            if (operand == "AND" ||
                operand == "OR")
            {
//                var m = b.Left as MemberExpression;
//                if (m != null &&
//                    m.Expression != null
//                    &&
//                    m.Expression.NodeType == ExpressionType.Parameter)
//                    left = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
//                else
                left = Visit(b.Left);

//                m = b.Right as MemberExpression;
//                if (m != null &&
//                    m.Expression != null
//                    &&
//                    m.Expression.NodeType == ExpressionType.Parameter)
//                    right = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
//                else
                right = Visit(b.Right);

                if (left as PartialSqlString == null &&
                    right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return new PartialSqlString(QuoteValue(result, result.GetType()));
                }

                if (left as PartialSqlString == null)
                    left = ((bool) left) ? GetTrueExpression() : GetFalseExpression();
                if (right as PartialSqlString == null)
                    right = ((bool) right) ? GetTrueExpression() : GetFalseExpression();
            }
            else
            {
                left = Visit(b.Left);
                right = Visit(b.Right);

                if (left as EnumMemberAccess != null &&
                    right as PartialSqlString == null)
                {
                    var enumType = ((EnumMemberAccess) left).EnumType;

                    //enum value was returned by Visit(b.Right)
                    long numvericVal;
                    if (Int64.TryParse(right.ToString(), out numvericVal))
                        right =
                            QuoteValue(
                                Enum.ToObject(enumType, numvericVal).ToString(),
                                typeof (string));
                    else
                        right = QuoteValue(right, right.GetType());
                }
                else if (right as EnumMemberAccess != null &&
                         left as PartialSqlString == null)
                {
                    var enumType = ((EnumMemberAccess) right).EnumType;

                    //enum value was returned by Visit(b.Left)
                    long numvericVal;
                    if (Int64.TryParse(left.ToString(), out numvericVal))
                        left =
                            QuoteValue(
                                Enum.ToObject(enumType, numvericVal).ToString(),
                                typeof (string));
                    else
                        left = QuoteValue(left, left.GetType());
                }
                else if (left as PartialSqlString == null &&
                         right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return result;
                }
                else if (left as PartialSqlString == null)
                    left = QuoteValue(left, left != null ? left.GetType() : null);
                else if (right as PartialSqlString == null)
                    right = QuoteValue(right, right != null ? right.GetType() : null);
            }

            if (operand == "=" &&
                right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase)) operand = "is";
            else if (operand == "<>" &&
                     right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase)) operand = "is not";

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString(string.Format("{0}({1},{2})", operand, left, right));
                default:
                    return new PartialSqlString("(" + left + sep + operand + sep + right + ")");
            }
        }

        protected virtual object VisitMemberAccess(MemberExpression m)
        {
            m = reduceFieldValueAccess(m);
            if (m.Expression.NodeType == ExpressionType.Parameter ||
                m.Expression.NodeType == ExpressionType.Convert)
            {
                var propertyInfo = m.Member as PropertyInfo;

                if (propertyInfo.PropertyType.IsEnum)
                    return new EnumMemberAccess(m.Member.Name, propertyInfo.PropertyType);

                return new PartialSqlString(m.Member.Name);
            }

            var member = Expression.Convert(m, typeof (object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        private MemberExpression reduceFieldValueAccess(MemberExpression m)
        {
            if (m == null) return null;
            var mEx = m.Expression as MemberExpression;
            if (mEx != null)
            {
                var propertyInfo = (PropertyInfo) mEx.Member;
                if (ReflectionUtils.IsTypeField(propertyInfo.PropertyType))
                {
                    return mEx;
                }
            }
            return m;
        }

        protected virtual object VisitMemberInit(MemberInitExpression exp)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke();
        }

        protected virtual object VisitNew(NewExpression nex)
        {
            // TODO : check !
            var member = Expression.Convert(nex, typeof (object));
            var lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                return getter();
            }
            catch (System.InvalidOperationException)
            {
                // FieldName ?
                List<Object> exprs = VisitExpressionList(nex.Arguments);
                StringBuilder r = new StringBuilder();
                foreach (Object e in exprs)
                {
                    r.AppendFormat("{0}{1}",
                        r.Length > 0 ? "," : "",
                        e);
                }
                return r.ToString();
            }
        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
            return p.Name;
        }

        public bool IsParameterized { get; set; }
        public Dictionary<string, object> Params = new Dictionary<string, object>();
        private int paramCounter = 0;
        private const string ParameterIdentifier = ":";

        protected virtual object VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
                return new PartialSqlString("null");

            if (!IsParameterized)
            {
                return c.Value;
            }
            else
            {
                string paramPlaceholder = ParameterIdentifier + paramCounter++;
                Params.Add(paramPlaceholder, c.Value);
                return new PartialSqlString(paramPlaceholder);
            }
        }

        protected virtual object VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    var o = Visit(u.Operand);

                    if (o as PartialSqlString == null)
                        return !((bool) o);
                    //todo fix whatever this does
//                    if (IsFieldName(o))
//                        o = o + "=" + GetQuotedTrueValue();

                    return new PartialSqlString("NOT (" + o + ")");
                case ExpressionType.Convert:
                    if (u.Method != null && !u.Method.DeclaringType.Implements<IField>())
                        return Expression.Lambda(u).Compile().DynamicInvoke();
                    break;
            }

            return Visit(u.Operand);
        }

        private bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object != null &&
                m.Object as MethodCallExpression != null)
                return IsColumnAccess(m.Object as MethodCallExpression);

            var exp = m.Object as MemberExpression;
            exp = reduceFieldValueAccess(exp);
            return exp != null
                   && exp.Expression != null
                   && exp.Expression.Type == _baseEntityKey.EntityType
                   && exp.Expression.NodeType == ExpressionType.Parameter;
        }

        protected virtual object VisitMethodCall(MethodCallExpression m)
        {
//            if (m.Method.DeclaringType == typeof (Sql))
//                return VisitSqlMethodCall(m);

            if (IsArrayMethod(m))
                throw new NotImplementedException();
//                return VisitArrayMethodCall(m);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m);

            return Expression.Lambda(m).Compile().DynamicInvoke();
        }

        protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Object> list = new List<Object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                    original[i].NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                    list.Add(Visit(original[i]));
            }
            return list;
        }

        protected virtual object VisitNewArray(NewArrayExpression na)
        {
            List<Object> exprs = VisitExpressionList(na.Expressions);
            StringBuilder r = new StringBuilder();
            foreach (Object e in exprs)
            {
                r.Append(r.Length > 0 ? "," + e : e);
            }

            return r.ToString();
        }

        protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {
            List<Object> exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }

        protected virtual string BindOperant(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        private bool IsArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null &&
                m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 2)
                    return true;
            }

            return false;
        }

        /* VisitArrayMethodCall
        protected virtual object VisitArrayMethodCall(MethodCallExpression m)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    List<Object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args[1];

                    var memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression);

                    var member = Expression.Convert(memberExpr, typeof (object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = Sql.Flatten(getter() as IEnumerable);

                    StringBuilder sIn = new StringBuilder();

                    if (inArgs.Any())
                    {
                        foreach (Object e in inArgs)
                        {
                            sIn.AppendFormat("{0}'{1}'",
                                sIn.Length > 0 ? "," : "",
                                e);
                        }
                    }
                    else
                    {
                        // The collection is empty, so avoid generating invalid SQL syntax of "ColumnName IN ()".
                        // Instead, just select from the null set via "ColumnName IN (NULL)"
                        sIn.Append("NULL");
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }
         */
        /* visit sql method
        protected virtual object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":

                    var member = Expression.Convert(m.Arguments[1], typeof (object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = Sql.Flatten(getter() as IEnumerable);

                    StringBuilder sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (!typeof (ICollection).IsAssignableFrom(e.GetType()))
                        {
                            sIn.AppendFormat("{0}{1}",
                                sIn.Length > 0 ? "," : "",
                                OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
                        }
                        else
                        {
                            var listArgs = e as ICollection;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                    sIn.Length > 0 ? "," : "",
                                    OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()));
                            }
                        }
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn.ToString());
                    break;
                case "Desc":
                    statement = string.Format("{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format("{0} As {1}",
                        quotedColName,
                        OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
                    break;
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                    statement = string.Format("{0}({1}{2})",
                        m.Method.Name,
                        quotedColName,
                        args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }
        */

        protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format("ltrim(rtrim({0}))", quotedColName);
                    break;
                case "LTrim":
                    statement = string.Format("ltrim({0})", quotedColName);
                    break;
                case "RTrim":
                    statement = string.Format("rtrim({0})", quotedColName);
                    break;
                case "ToUpper":
                    statement = string.Format("upper({0})", quotedColName);
                    break;
                case "ToLower":
                    statement = string.Format("lower({0})", quotedColName);
                    break;
                case "StartsWith":
                    statement = string.Format("upper({0}) like '{1}%' ",
                        quotedColName,
                        args[0].ToString().ToUpper());
                    break;
                case "EndsWith":
                    statement = string.Format("upper({0}) like '%{1}'",
                        quotedColName,
                        args[0].ToString().ToUpper());
                    break;
                case "Contains":
                    statement = string.Format("upper({0}) like '%{1}%'",
                        quotedColName,
                        args[0].ToString().ToUpper());
                    break;
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        statement = string.Format("substring({0} from {1} for {2})",
                            quotedColName,
                            startIndex,
                            length);
                    }
                    else
                        statement = string.Format("substring({0} from {1})",
                            quotedColName,
                            startIndex);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        private Type[] skipQuoteTypes = new[] {typeof (int), typeof (long), typeof (decimal), typeof (bool)};

        protected string QuoteValue(object val, Type type = null)
        {
            if (val == null) return "NULL";
            type = type ?? val.GetType();

            if (skipQuoteTypes.Contains(type))
            {
                return val.ToString();
            }
            else
            {
                return '\'' + val.ToString() + '\'';
            }
        }

        protected object GetTrueExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedTrueValue()));
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedFalseValue()));
        }

        protected static object GetQuotedTrueValue()
        {
            return new PartialSqlString("Y");
        }

        protected static object GetQuotedFalseValue()
        {
            return new PartialSqlString("N");
        }
    }

    #endregion visit

    public class PartialSqlString
    {
        public PartialSqlString(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class EnumMemberAccess : PartialSqlString
    {
        public EnumMemberAccess(string text, Type enumType)
            : base(text)
        {
            if (!enumType.IsEnum) throw new ArgumentException("Type not valid", "enumType");

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
    }
}