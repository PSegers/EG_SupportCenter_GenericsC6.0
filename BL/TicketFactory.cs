using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC.DAL;

namespace SC.BL
{
    public class TicketFactory<T, K> where T : K, new()
    {
        public static K Create()
        {
            K objK;
            objK = new T();
            return objK;
        }
    }
}
