using System.Collections.Generic;

namespace WolcenEditor
{
    public class ApocalypticData
    {
        public string ChosenType { get; set; }
        public IList<UnlockedTypes> UnlockedTypes { get; set; }
    }

    public class UnlockedTypes
    {
        public string Type { get; set; }
    }
}