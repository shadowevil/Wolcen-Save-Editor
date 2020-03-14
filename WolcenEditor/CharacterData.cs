using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
                File.Copy(outputPath, $"{outputPath}.bak");
            }
            string newJsonFile = JsonConvert.SerializeObject(characterData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputPath, newJsonFile);
        }
    }
}