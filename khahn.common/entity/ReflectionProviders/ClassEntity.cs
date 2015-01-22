using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;

namespace khahn.common.entity.ReflectionProviders
{
    public abstract class ClassEntity : IEntity
    {
        private IEntityDefinition _entityDefinition;

        [IgnoreField]
        public IEntityDefinition EntityDefinition
        {
            get { return _entityDefinition; }
            internal set { _entityDefinition = value; }
        }
    }
}