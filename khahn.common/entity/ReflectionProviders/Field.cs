using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;

namespace khahn.common.entity.ReflectionProviders
{
    public abstract class Field : IField
    {
        public IFieldDefinition FieldDefinition { get; internal set; }

        public object Value
        {
            //crazy vodo don't touch
            get { return ((IField) this).Value; }
            set { ((IField) this).Value = value; }
        }

        public bool Provided { get; internal set; }
    }

    public class Field<T> : Field, IField
    {
        object IField.Value
        {
            get { return Value; }
            set { Value = (T) value; }
        }

        private T _value;

        public new T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public static implicit operator T(Field<T> d)
        {
            return d.Value;
        }
    }
}