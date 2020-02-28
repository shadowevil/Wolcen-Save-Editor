using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolcenEditor
{
    public class Stats
    {
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Constitution { get; set; }
        public int Power { get; set; }
        public int Level { get; set; }
        public int PassiveSkillPoints { get; set; }
        public string CurrentXP { get; set; }
        public int RemainingStatsPoints { get; set; }
        public string Gold { get; set; }
        public string PrimordialAffinity { get; set; }
        public int IsAutoDashAvailable { get; set; }
        public int DashStatusActivation { get; set; }

    }
}
