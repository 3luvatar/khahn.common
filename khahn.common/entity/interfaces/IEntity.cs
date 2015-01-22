using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khahn.common.entity.interfaces
{
    public interface IEntity
    {
        IEntityDefinition EntityDefinition { get; }
    }
}
