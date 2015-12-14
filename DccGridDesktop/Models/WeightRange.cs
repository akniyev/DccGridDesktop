using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DccGridDesktop.Models
{
    [Serializable]
    public class WeightRange
    {
        public string name;
        public double minWeight;
        public double maxWeight;

        public void LoadFromString(string s)
        {
            try
            {
                var rs = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var nname = rs[0];
                var nmin = double.Parse(rs[1]);
                var nmax = double.Parse(rs[2]);

                name = nname;
                minWeight = nmin;
                maxWeight = nmax;
            }
            catch
            {

            }
        }
        public string Encode()
        {
            return name + ";" + minWeight + ";" + maxWeight;
        }

        public static WeightRange Decode(string s)
        {
            var w = new WeightRange();
            w.LoadFromString(s);
            return w;
        }
    }
}
