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

        public static readonly Dictionary<int, string> Sexes = new Dictionary<int, string>
        {
            {0, "Male" },
            {1, "Female" },

        };

        public static readonly Dictionary<int, string> EyeColor = new Dictionary<int, string>
        {
            {1, "1.png" },
            {2, "2.png" },
            {3, "3.png" },
            {4, "4.png" },
            {5, "5.png" },
            {6, "6.png" },
            {7, "7.png" },
            {8, "8.png" },
            {9, "9.png" },
            {10, "10.png" },
            {11, "11.png" },
            {12, "12.png" },
            {13, "13.png" }
        };

        public static readonly Dictionary<int, string> Face = new Dictionary<int, string>
        {
            {1, "1.png" },
            {2, "2.png" },
            {3, "3.png" },
            {4, "4.png" },
            {5, "5.png" },
            {6, "6.png" },
            {7, "7.png" },
            {8, "8.png" }
        };

        public static readonly Dictionary<int, string> Beard = new Dictionary<int, string>
        {
            {1, "1.png" },
            {2, "2.png" },
            {3, "3.png" },
            {4, "4.png" },
            {5, "5.png" },
            {6, "6.png" },
            {7, "7.png" },
            {8, "8.png" },
            {9, "9.png" }
        };

        public static readonly Dictionary<int, string> HairStyle = new Dictionary<int, string>
        {
            {1, "1.png" },
            {2, "2.png" },
            {3, "3.png" },
            {4, "4.png" },
            {5, "5.png" },
            {6, "6.png" },
            {100, "100.png" },
            {101, "101.png" },
            {102, "102.png" },
            {103, "103.png" },
            {104, "104.png" },
            {105, "105.png" },
            {106, "106.png" },
            {107, "107.png" },
            {108, "108.png" },
            {109, "109.png" },
            {110, "110.png" }
        };

        public static readonly Dictionary<int, string> HeadStyle = new Dictionary<int, string>
        {
            {1, "Head_male_01.png" },
            {2, "Head_male_02.png" },
            {3, "Head_male_03.png" },
            {4, "Head_male_04.png" },
            {5, "Head_female_01.png" },
            {6, "Head_female_02.png" },
            {7, "Head_female_03.png" },
            {8, "Head_female_04.png" }
        };

        public static readonly Dictionary<int, string> SkinColor = new Dictionary<int, string>
        {
            { 100, "#d8c3a8" },
            { 101, "#a9877d" },
            { 102, "#9c7767" },
            { 103, "#795e4f" },
            { 104, "#64483a" },
            { 105, "#533c36" },
            { 106, "#2c2a25" },
            { 107, "#896f56" },
            { 108, "#856858" },
            { 109, "#847878" },
            { 110, "#807461" },
            { 111, "#181412" },
            { 200, "#d8c3a8" },
            { 201, "#a9877d" },
            { 202, "#9c7767" },
            { 203, "#795e4f" },
            { 204, "#64483a" },
            { 205, "#533c36" },
            { 206, "#2c2a25" },
            { 207, "#896f56" },
            { 208, "#856858" },
            { 209, "#847878" },
            { 210, "#807461" },
            { 211, "#181412" },
            { 300, "#d8c3a8" },
            { 301, "#a9877d" },
            { 302, "#9c7767" },
            { 303, "#795e4f" },
            { 304, "#64483a" },
            { 305, "#533c36" },
            { 306, "#2c2a25" },
            { 307, "#896f56" },
            { 308, "#856858" },
            { 309, "#847878" },
            { 310, "#807461" },
            { 311, "#181412" },
            { 400, "#d8c3a8" },
            { 401, "#a9877d" },
            { 402, "#9c7767" },
            { 403, "#795e4f" },
            { 404, "#64483a" },
            { 405, "#533c36" },
            { 406, "#2c2a25" },
            { 407, "#896f56" },
            { 408, "#856858" },
            { 409, "#847878" },
            { 410, "#807461" },
            { 411, "#181412" },
            { 512, "#F7DFC1" },
            { 500, "#d8c3a8" },
            { 501, "#a9877d" },
            { 502, "#9c7767" },
            { 503, "#795e4f" },
            { 504, "#64483a" },
            { 505, "#533c36" },
            { 506, "#2c2a25" },
            { 507, "#896f56" },
            { 508, "#856858" },
            { 509, "#847878" },
            { 510, "#807461" },
            { 511, "#181412" },
            { 612, "#F7DFC1" },
            { 600, "#d8c3a8" },
            { 601, "#a9877d" },
            { 602, "#9c7767" },
            { 603, "#795e4f" },
            { 604, "#64483a" },
            { 605, "#533c36" },
            { 606, "#2c2a25" },
            { 607, "#896f56" },
            { 608, "#856858" },
            { 609, "#847878" },
            { 610, "#807461" },
            { 611, "#181412" },
            { 712, "#F7DFC1" },
            { 700, "#d8c3a8" },
            { 701, "#a9877d" },
            { 702, "#9c7767" },
            { 703, "#795e4f" },
            { 704, "#64483a" },
            { 705, "#533c36" },
            { 706, "#2c2a25" },
            { 707, "#896f56" },
            { 708, "#856858" },
            { 709, "#847878" },
            { 710, "#807461" },
            { 711, "#181412" },
            { 812, "#F7DFC1" },
            { 800, "#d8c3a8" },
            { 801, "#a9877d" },
            { 802, "#9c7767" },
            { 803, "#795e4f" },
            { 804, "#64483a" },
            { 805, "#533c36" },
            { 806, "#2c2a25" },
            { 807, "#896f56" },
            { 808, "#856858" },
            { 809, "#847878" },
            { 810, "#807461" },
            { 811, "#181412" }
        };

    }
}
