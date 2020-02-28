using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolcenEditor
{
    public class CharacterData
    {
        public string Name { get; set; }
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public int DifficultyMode { get; set; }
        public int League { get; set; }
        public string UpdatedAt { get; set; }

        public CharacterCustomization CharacterCustomization { get; set; }

        public Stats Stats { get; set; }

        public IList<UnlockedSkill> UnlockedSkills { get; set; }

        public IList<SkillBar> SkillBar { get; set; }

        public IList<string> PassiveSkills { get; set; }

        public IList<BeltConfig> BeltConfig { get; set; }

        public Progression Progression { get; set; }

        public Telemetry Telemetry { get; set; }

        public Versions Versions { get; set; }

        public CharacterCosmeticInventory CharacterCosmeticInventory { get; set; }

        public IList<InventoryEquipped> InventoryEquipped { get; set; }

        public IList<InventoryGrid> InventoryGrid { get; set; }

        public IList<InventoryBelt> InventoryBelt { get; set; }

        public IList<PSTConfig> PSTConfig { get; set; }

        public ApocalypticData ApocalypticData { get; set; }

        public IList<Tutorials> Tutorials { get; set; }

        public IList<Sequences> Sequences { get; set; }

        public LastGameParameters LastGameParameters { get; set; }

    }

    public static class CharacterIO
    {
        public static CharacterData ReadCharacter(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);

            CharacterData character = JsonConvert.DeserializeObject<CharacterData>(jsonData);
            return character;
        }

        public static void WriteCharacter(string outputPath, CharacterData characterData)
        {
            if (File.Exists($"{outputPath}") && !File.Exists($"{outputPath}.bak"))
            {
                string oldJsonFile = File.ReadAllText(outputPath);
                File.WriteAllText($"{outputPath}.bak", oldJsonFile);
            }
            string newJsonFile = JsonConvert.SerializeObject(characterData, Formatting.Indented);
            File.WriteAllText(outputPath, newJsonFile);
        }
    }

    public static class WolcenStaticData
    {
        public static readonly Dictionary<string, List<int>> QuestSelections = new Dictionary<string, List<int>>
        {
            { "INTRO_Quest1", new List<int> { 1} },

            { "ACT1_Quest1", new List<int> { 1 } },
            { "ACT1_Quest2", new List<int> { 1, 2, 3, 7, 10, 12, 13, 14 } },
            { "ACT1_Quest3", new List<int> { 1,3,5,6,8,9,12} },
            { "ACT1_Quest4", new List<int> { 1,3,5,6,7,9,11 } },
            { "ACT1_Quest5", new List<int> { 1,2,3,4,5,6,7,8,10 } },

            { "ACT2_Quest1", new List<int> { 1,3 } },
            { "ACT2_Quest2", new List<int> { 1,2,3,4 } },
            { "ACT2_Quest3", new List<int> { 1,2,5 } },
            { "ACT2_Quest4", new List<int> { 1,2,3,4 } },
            { "ACT2_Quest5", new List<int> { 1,2,3,4,5,6,7,9,10 } },
            { "ACT2_Quest6", new List<int> { 1,3,4,5,6,7,8 } },

            { "ACT3_Quest1", new List<int> { 1,2,3,4,6,7,8 } },
            { "ACT3_Quest2", new List<int> { 1,3,5,7 } },
            { "ACT3_Quest3", new List<int> { 1,3,4,5,7 } },
            { "ACT3_Quest4", new List<int> { 1,2,4,5,6,8 } },
        };

        public static readonly Dictionary<int, string> HairColorBank = new Dictionary<int, string>
        {
            {1, "#3B2215" },
            {2, "#5B391F" },
            {3, "#120E0B" },
            {4, "#965F35" },
            {5, "#684D41" },
            {6, "#FC9432" },
            {7, "#824C1A" },
            {8, "#FCFCFC" },
            {9, "#828282" },
            {51, "#424242" },
            {10, "#DBAC73" },
            {11, "#FCCA32" },
            {12, "#FCD96F" },
            {13, "#FFD9AE" },
            {14, "#FC603D" },
            {15, "#82311F" },
            {16, "#2E110B" },
            {17, "#F46816" },
            {18, "#863505" },
            {19, "#9C5927" },
            {50, "#FF8000" },
            {20, "#822424" },
            {22, "#970A05" },
            {23, "#410402" },
            {24, "#FE1111" },
            {26, "#35B3FC" },
            {27, "#1B5C82" },
            {28, "#4454FC" },
            {29, "#232B82" },
            {30, "#0C0F2E" },
            {31, "#C244FC" },
            {32, "#642382" },
            {34, "#822F73" },
            {35, "#2E1129" },
            {36, "#FF399F" },
            {37, "#FBB1E2" },
            {38, "#71054D" },
            {39, "#FF399F" },
            {40, "#35FC49" },
            {41, "#FF399F" },
        };
    }
}
