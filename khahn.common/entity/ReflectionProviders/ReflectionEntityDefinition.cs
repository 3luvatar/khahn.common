using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using khahn.common.entity.interfaces;
using khahn.common.utils;

namespace khahn.common.entity.ReflectionProviders
{
    internal class ReflectionEntityDefinition : IEntityDefinition
    {
        public ReflectionEntityDefinition(ReflectedEntityKey entityKey)
        {
            _entityKey = entityKey;
            _constructorInvoker =
                new Lazy<ConstructorInvoker>(() => EntityType.Constructor().DelegateForCreateInstance());
        }

        private readonly ReflectedEntityKey _entityKey;
        private readonly List<ReflectionFieldDefinition> _fieldDefinitions = new List<ReflectionFieldDefinition>();
        private readonly Lazy<ConstructorInvoker> _constructorInvoker;
        private string _name;

        public Type EntityType
        {
            get { return _entityKey.EntityType; }
        }

        public IEntityKey EntityKey
        {
            get { return _entityKey; }
        }

        IEnumerable<IFieldDefinition> IEntityDefinition.FieldDefinitions
        {
            get { return _fieldDefinitions; }
        }

        public List<ReflectionFieldDefinition> FieldDefinitions
        {
            get { return _fieldDefinitions; }
        }

        public IEntity GenerateEntity()
        {
            var entity = (ClassEntity) _constructorInvoker.Value();
            entity.EntityDefinition = this;
            foreach (var fieldDefinition in FieldDefinitions)
            {
                fieldDefinition.SetField(entity, fieldDefinition.GenerateField());
            }
            return entity;
        }

        public string Name
        {
            get { return EntityType.Name; }
        }
    }

    internal class ReflectionFieldDefinition : IFieldDefinition
    {
        public ReflectionFieldDefinition(PropertyInfo propertyInfo, IEntityDefinition entityDefinition)
        {
            _propertyInfo = propertyInfo;
            _entityDefinition = entityDefinition;
            _fieldConstructor =
                new Lazy<ConstructorInvoker>(() => _propertyInfo.PropertyType.Constructor().DelegateForCreateInstance());
            _fieldType = new Lazy<Type>(() => ResolveFieldBaseType(_propertyInfo.PropertyType));
            _fieldGetter = new Lazy<MemberGetter>(() => _propertyInfo.DelegateForGetPropertyValue());
            _fieldSetter = new Lazy<MemberSetter>(() => _propertyInfo.DelegateForSetPropertyValue());
        }

        private static Type ResolveFieldBaseType(Type t)
        {
            Type genericTypeInstance = ReflectionUtils.GetGenericTypeInstance(t, typeof (Field<>));
            return genericTypeInstance.GetGenericArguments().First();
        }

        private readonly Lazy<ConstructorInvoker> _fieldConstructor;
        private readonly PropertyInfo _propertyInfo;
        private readonly Lazy<Type> _fieldType;
        private readonly Lazy<MemberGetter> _fieldGetter;
        private readonly Lazy<MemberSetter> _fieldSetter;
        private readonly IEntityDefinition _entityDefinition;
        private string _sqlName;

        public IEntityDefinition EntityDefinition
        {
            get { return _entityDefinition; }
        }

        public string Name
        {
            get { return _propertyInfo.Name; }
        }

        public string SqlName
        {
            get { return _sqlName ?? Name; }
        }

        public PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        public IField GenerateField()
        {
            Field field = (Field) _fieldConstructor.Value();
            field.FieldDefinition = this;
            return field;
        }

        IField IFieldDefinition.GetField(IEntity entity)
        {
            return GetField(entity);
        }

        public Field GetField(IEntity entity)
        {
            return (Field) _fieldGetter.Value(entity);
        }

        public void SetField(IEntity entity, IField field)
        {
            _fieldSetter.Value(entity, field);
        }

        public Type FieldType
        {
            get { return _fieldType.Value; }
        }
    }
}