using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khahn.common.entity.JsonProviders
{
    public class NamedKey : IEntityKey
    {
        private string _name;

        public NamedKey(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Equals(IEntityKey key)
        {
            NamedKey namedKey = key as NamedKey;
            return namedKey != null && string.Equals(_name, namedKey.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}