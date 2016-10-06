using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSIcon
{
    class AlleyUtils
    {
        public static byte[] StringToByte(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            return bytes;
        }

        public static byte[] StringToCMPString(string data)
        {
            string str = data + "\u0000";
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            return bytes;
        }

    }
}


   