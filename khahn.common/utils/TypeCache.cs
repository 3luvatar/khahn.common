using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khahn.common.utils
{
    public static class TypeCache<T_KEY, T_VAL>
    {
        public static T_VAL Value { get; set; }
    }
}