using System.Collections.Generic;

namespace WolcenEditor
{
    public class Progression
    {
        public LastPlayed LastPlayed { get; set; }
        public IList<QuestProgression> QuestProgression { get; set; }
    }

    public class QuestProgression
    {
        public string Name { get; set; }
        public int Step { get; set; }
        public string QuestId { get; set; }
        public int StepId { get; set; }
        public string state { get; set; }
    }

    public class LastPlayed
    {
        public string QuestId { get; set; }
        public int StepId { get; set; }
    }
}