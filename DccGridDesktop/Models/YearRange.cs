using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DccGridDesktop.Models
{
    [Serializable]
    public class YearRange
    {
        public int startYear;
        public int endYear;

        public string Encode()
        {
            return startYear + ";" + endYear;
        }

        public void LoadFromString(string s)
        {
            try
            {
                var rs = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var start = int.Parse(rs[0]);
                var end = int.Parse(rs[1]);

                startYear = start;
                endYear = end;
            }
            catch
            {

            }
        }

        public static YearRange Decode(string s)
        {
            var y = new YearRange();
            y.LoadFromString(s);
            return y;
        }
    }
}
