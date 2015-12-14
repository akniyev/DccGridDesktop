using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DccGridDesktop.Models
{
    [Serializable]
    public class Club
    {
        public string name;
        public string description;

        public string Encode()
        {
            return name + "%#" + description;
        }

        public void loadFromString(string s)
        {
            var r = s.Split(new string[] { "%#" }, StringSplitOptions.None);
            if (r.Length != 2) throw new Exception();

            name = r[0];
            description = r[1];
        }

        static public Club Decode(string s)
        {
            var c = new Club();

            c.loadFromString(s);

            return c;
        }
    }
}
