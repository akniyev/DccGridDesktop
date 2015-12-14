using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DccGridDesktop.Models
{
    [Serializable]
    class ParticipantGroup
    {
        public WeightRange weigths;
        public YearRange years;
        public List<Participant> participants;
        public int RoundId { get; set; }
        Heap<Participant> _baseDistribution;
        public Heap<Participant> BaseDistribution
        {
            get
            {
                return _baseDistribution;
            }
            set
            {
                _baseDistribution = value;
                Rounds = new List<Heap<Participant>>();
            }
        }

        public void AutoDistribute()
        {
            if (this.RoundId != -1) return;
            if (this.participants.Count < 2) return;
            if (this.BaseDistribution.Length < 2)
            {
                if (this.BaseDistribution.Length == 1 && this.BaseDistribution[0].Length >= this.participants.Count)
                {
                    for (int i = 0; i < this.participants.Count; i++)
                    {
                        this.BaseDistribution[0][i] = this.participants[i];
                    }
                    this.participants.Clear();
                }
                return;
            }


            var count1 = 2;
            for (int i = 0; i < this.BaseDistribution[0].Length; i++)
            {
                if (count1 / 2 + (participants.Count - count1) == this.BaseDistribution[1].Length)
                {
                    break;
                }
                else
                {
                    count1++;
                }
            }

            var count2 = participants.Count - count1;

            for (int i = 0; i < count1; i++)
            {
                this.BaseDistribution[0][i] = participants[0];
                participants.RemoveAt(0);
            }

            for (int i = 0; i < count2; i++)
            {
                this.BaseDistribution[1][i] = participants[0];
                participants.RemoveAt(0);
            }
        }

        public List<Heap<Participant>> Rounds { get; set; }

        int ParticipantsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < BaseDistribution.Length; i++)
                {
                    for (int j = 0; j < BaseDistribution[i].Length; j++)
                    {
                        if (BaseDistribution[i][j] != null) count++;
                    }
                }
                return count + participants.Count;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} / {1}-{2} ({3})", weigths.name, years.startYear, years.endYear, ParticipantsCount);
        }

        public List<string> isBaseDistributionCorrect()
        {
            var bd = this.BaseDistribution;
            var group = this;
            //Проверка, что все распределены
            var result = new List<string>();
            if (group.participants.Count > 0)
            {
                result.Add("Остались нераспределенные игроки!");
            }

            //Проверка, что разбивка по числу правильная
            if (group.BaseDistribution.Length > 1)
            {
                int count1 = 0, count2 = 0;
                count1 = group.BaseDistribution[0].Count(x => x != null);
                count2 = group.BaseDistribution[1].Count(x => x != null);

                if (!(count1 % 2 == 0 && (count1 / 2 + count2) == group.BaseDistribution[1].Length))
                {
                    result.Add("Количество игроков по раундам некорректно!");
                }

                for (int i = 0; i < group.BaseDistribution[0].Length / 2; i++)
                {
                    if (bd[0][2 * i] == null ^ bd[0][2 * i + 1] == null)
                    {
                        result.Add("Некорректное распределение по парам!");
                        break;
                    }
                }
            }
            return result;
        }

        public List<string> isRoundDistributionCorrect()
        {
            var group = this;
            var bd = group.Rounds[group.RoundId];
            var bdr = bd[group.RoundId];
            //Проверка, что все распределены
            var result = new List<string>();

            //Проверка, что разбивка по числу правильная
            if (bd.Length > 1)
            {
                int count = 0;
                for (int i = 0; i < bd[group.RoundId].Length; i++)
                {
                    if (bd[group.RoundId][i] != null) count++;
                }

                if (count % 2 != 0)
                {
                    result.Add("Количество игроков в раунде некорректно!");
                }

                for (int i = 0; i < bd[group.RoundId].Length / 2; i++)
                {
                    if (bdr[2 * i] != null && bdr[2 * i + 1] != null)
                    {
                        if (!(bdr[2 * i].status != ParticipantStatus.Win ^ bdr[2 * i + 1].status != ParticipantStatus.Win))
                        {
                            result.Add("Некорректное распределение выигрышей!");
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public string Serialize()
        {
            var json = new JavaScriptSerializer().Serialize(this);
            return json;
        }
    }
}
