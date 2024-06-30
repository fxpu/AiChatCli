using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxPu.UtilityLib
{
    internal class Test
    {
        public void TestX()
        {
            ThrowUtility.ThrowIfIsEmpty<ArgumentException>("");
            IQueryable<string> q = null!; ;
            ThrowUtility.ThrowIfIsEmpty<ArgumentException>(q);


        }
    }
}
