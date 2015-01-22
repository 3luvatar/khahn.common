using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;

namespace khahn.common.tests.utils
{
    public static class MoqExtensions
    {
        public static ISetup<IToStringable, string> SetupToString<TMock>(this Mock<TMock> mock) where TMock : class
        {
            return mock.As<IToStringable>().Setup(m => m.ToString());
        }

        //Our dummy nested interface.
        public interface IToStringable
        {
            /// <summary>
            /// ToString.
            /// </summary>
            /// <returns></returns>
            string ToString();
        }
    }
}