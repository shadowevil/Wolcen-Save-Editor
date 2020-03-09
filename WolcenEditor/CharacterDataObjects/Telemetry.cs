using System.Collections.Generic;
using System.ComponentModel;

namespace WolcenEditor
{
    public class Telemetry
    {
        public string Version { get; set; }

        public Count PlayTime { get; set; }
        public Count PlayTimeOutTown { get; set; }
        public IList<TypeCount> KillCountPerBossrank { get; set; }
        public IList<TypeCount> KillCountPerMobRankType { get; set; }
        public Count MinLevelKilled { get; set; }
        public Count MaxLevelKilled { get; set; }
        public Count DeathCount { get; set; }
        public IList<TypeCount> DeathCountPerBossrank { get; set; }
        public Count XpFromQuest { get; set; }
        public Count XpFromKill { get; set; }
        public Count GoldDropped { get; set; }
        public Count GoldGainedQuests { get; set; }
        public Count GoldGainedMerchant { get; set; }
        public Count GoldPicked { get; set; }
        public Count GoldSpent { get; set; }
        public Count GoldSpentMerchant { get; set; }
        public Count GoldSpentJewelerUnsocketItem { get; set; }
        public Count PrimordialAffinitySpent { get; set; }
        public Count PrimordialAffinitySpentSkillLevelUp { get; set; }
        public Count PrimordialAffinityGained { get; set; }
        public IList<TypeCount> ItemsDropped { get; set; }
        public IList<TypeCount> ItemsPicked { get; set; }
        public IList<TypeCount> ItemsBought { get; set; }
        public IList<TypeCount> ItemsSold { get; set; }
        public IList<TypeCount> TimeSpentPerZone { get; set; }
        public IList<TypeCount> SoloReviveTokenUsedPerZone { get; set; }
        public IList<TypeCount> SoloDeathPerZone { get; set; }
        public IList<TypeCount> MultiRevivePerZone { get; set; }
        public IList<TypeCount> SkillUsage { get; set; }
        public Count QuestAttempt_NPC1 { get; set; }
        public Count QuestAttempt_NPC2 { get; set; }
        public Count QuestSuccess_NPC1 { get; set; }
        public Count QuestSuccess_NPC2 { get; set; }
        public Count QuestFailed_NPC1 { get; set; }
        public Count QuestFailed_NPC2 { get; set; }
        public Count QuestMaxFloorReached_NPC2 { get; set; }
        public Count UnlockChestCount { get; set; }
        public Count ResetPSTCount { get; set; }
        public Count ResetCharacterAttributesCount { get; set; }
    }

    public class TypeCount
    {
        public string Type { get; set; }
        public string Total { get; set; }
        public string PerLevel { get; set; }
    }

    public class Count
    {
        public string Total { get; set; }
        public string PerLevel { get; set; }
    }
}