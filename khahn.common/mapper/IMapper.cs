using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity;
using khahn.common.entity.fetchExpressions;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;

namespace khahn.common.mapper
{
    public interface IMapper<T_KEY>
        where T_KEY : IEntityKey
    {
        T MapItem<T>(T_KEY key, EntityFetchExpression fetchExpression, IDataReader dataReader)
            where T : IEntity;
    }

    public static class MapperExensions
    {
        public static T MapItem<T>(this IMapper<ReflectedEntityKey> mapper,
            EntityFetchExpression fetchExpression,
            IDataReader dataReader)
            where T : ClassEntity
        {
            return mapper.MapItem<T>(ReflectedEntityKey.EntityKey<T>(), fetchExpression, dataReader);
        }

        public static IEnumerable<T> MapItems<T>(this IMapper<ReflectedEntityKey> mapper,
            EntityFetchExpression fetchExpression,
            IDataReader dataReader)
            where T : ClassEntity
        {
            while (dataReader.Read())
            {
                yield return mapper.MapItem<T>(fetchExpression, dataReader);
            }
        }
    }

    public class ReflectionMapper : IMapper<ReflectedEntityKey>
    {
        private IEntityDefinitionProvider<ReflectedEntityKey> _entityDefinitionProvider;

        public ReflectionMapper(IEntityDefinitionProvider<ReflectedEntityKey> entityDefinitionProvider)
        {
            _entityDefinitionProvider = entityDefinitionProvider;
        }

        public T MapItem<T>(ReflectedEntityKey key, EntityFetchExpression fetchExpression, IDataReader dataReader)
            where T : IEntity
        {
            IEntityDefinition definition = _entityDefinitionProvider.GetEntityDefinition(key);
            T entity = (T) definition.GenerateEntity();
            IReadOnlyList<IFieldDefinition> fields = fetchExpression.Fields(_entityDefinitionProvider);
            for (var i = 0; i < fields.Count; i++)
            {
                ReflectionFieldDefinition fieldDefinition = (ReflectionFieldDefinition) fields[i];
                Field field = fieldDefinition.GetField(entity);
                var valueConverter = Converter(fieldDefinition);
                field.Value = valueConverter(dataReader.GetValue(i));
            }
            return default(T);
        }

        private Func<object, object> Converter(ReflectionFieldDefinition reflectionFieldDefinition)
        {
            //will build Lambda Expression to convert value and compile and cache on field defintion
            Delegate del;

            Type t = reflectionFieldDefinition.FieldType;
            if (t == typeof (string))
            {
                return o =>
                {
                    if (o == null ||
                        o == DBNull.Value)
                    {
                        return null;
                    }
                    return o.ToString();
                };
            }
            if (t == typeof (int))
            {
                return o => { return Convert.ToInt32(o); };
            }
            return null;
        }
    }
}