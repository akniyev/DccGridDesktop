using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DccGridDesktop.Models
{
    [Serializable]
    public enum ParticipantStatus { None, Win, Transfer }

    [Serializable]
    public class Participant
    {
        public string name;
        public string surname;
        public string patronymic;
        public int birthYear;
        public double weight;
        public string clubName;
        public ParticipantStatus status = ParticipantStatus.None;

        public override bool Equals(object obj)
        {
            if (!(obj is Participant)) return false;

            var o = obj as Participant;

            return this.name == o.name && this.surname == o.surname && this.patronymic == o.patronymic && this.birthYear == o.birthYear
                && this.weight == o.weight && this.clubName == o.clubName && this.status == o.status;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool isInWeightRange(WeightRange wr)
        {
            return weight > wr.minWeight && weight <= wr.maxWeight;
        }


        public bool isInYearRange(YearRange yr)
        {
            return birthYear >= yr.startYear && birthYear <= yr.endYear;
        }

        public override string ToString()
        {
            string statusStr = "";
            if (status == ParticipantStatus.Win)
                statusStr = "+";
            else if (status == ParticipantStatus.Transfer)
                statusStr = "->";
            return String.Format("{0} {1} {2}, {3}кг, {4}г ({5})" + statusStr,
                name, surname, patronymic, weight, birthYear, clubName);
        }

        public string Encode()
        {
            return String.Format("{0};{1};{2};{3};{4};{5};{6}", name, surname, patronymic, weight, birthYear, clubName, status);
        }

        public void loadFromString(string s)
        {
            try
            {
                var rs = s.Split(new char[] { ';' }, StringSplitOptions.None);
                var nname = rs[0];
                var nsurname = rs[1];
                var npatronymic = rs[2];
                var nweight = double.Parse(rs[3]);
                var nyear = int.Parse(rs[4]);
                var nclubname = rs[5];

                ParticipantStatus nstatus;
                if (rs.Length > 6)
                {
                    nstatus = (ParticipantStatus)Enum.Parse(typeof(ParticipantStatus), rs[6]);
                }
                else
                {
                    nstatus = ParticipantStatus.None;
                }

                name = nname;
                surname = nsurname;
                patronymic = npatronymic;
                weight = nweight;
                birthYear = nyear;
                clubName = nclubname;
                status = nstatus;
            }
            catch { }
        }

        public static Participant Decode(string s)
        {
            var p = new Participant();

            p.loadFromString(s);

            return p;
        }

        public Participant() { }

        public Participant(string l)
        {
            loadFromString(l);
        }

        public Participant getCopy()
        {
            var c = new Participant();
            c.name = this.name;
            c.surname = this.surname;
            c.patronymic = this.patronymic;
            c.birthYear = this.birthYear;
            c.clubName = this.clubName;
            c.weight = this.weight;
            c.status = this.status;

            return c;
        }
    }
}
