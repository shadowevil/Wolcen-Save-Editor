using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace WolcenEditor
{
    public class PlayerData
    {
        public bool EulaAgreement { get; set; }
        public string[] NewsReadIds { get; set; }
        public AccountCosmeticInventory AccountCosmeticInventory { get; set; }
        public SoftcoreNormal SoftcoreNormal { get; set; }
    }

    public static class PlayerDataIO
    {
        public static readonly string ColorsUnlockedBitmask = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        public static readonly string WeaponsUnlockedBitmask = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        public static readonly string ArmorsUnlockedBitmask = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";

        public static PlayerData ReadPlayerData(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
            return playerData;
        }

        public static void WritePlayerData(string outputPath, PlayerData playerData)
        {
            if (File.Exists($"{outputPath}") && !File.Exists($"{outputPath}.bak"))
            {
                File.Copy(outputPath, outputPath + ".bak");
            }

            string newJsonFile = JsonConvert.SerializeObject(playerData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            try
            {
                File.WriteAllText(outputPath, newJsonFile);
                MessageBox.Show("Successfully saved player data!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class SoftcoreNormal
    {
        public bool CompletedStory { get; set; }
        public int ExpeditionsMaxLevelReached { get; set; }
        public CityBuilding CityBuilding { get; set; }
    }

    public class CityBuilding
    {
        public int Version { get; set; }
        public int FinishedTurns { get; set; }
        public IList<FinishedProjects> FinishedProjects { get; set; }
        public IList<OngoingProjects> OngoingProjects { get; set; }
        public IList<PlaceHolder> OngoingEventProjects { get; set; }
        public IList<PlaceHolder> PendingRewards { get; set; }
        public IList<PlaceHolder> RewardInstances { get; set; }
        public IList<PlaceHolder> RolledProjects { get; set; }
        public IList<PlaceHolder> PendingEvents { get; set; }
        public int TurnsUntilNextNarrativeEventRoll { get; set; }
        public IList<PlaceHolder> TimedProductionFactors { get; set; }
        public IList<PlaceHolder> NETypeCooldowns { get; set; }
        public IList<PlaceHolder> NECooldowns { get; set; }
        public IList<PlaceHolder> CECooldowns { get; set; }
        public int TurnsUntilNextCourierEventRoll { get; set; }
    }

    public class PlaceHolder
    { }

    public class OngoingProjects
    {
        public string Name { get; set; }
        public int CurrentProduction { get; set; }
        public int PlayerLevel { get; set; }
    }

    public class FinishedProjects
    {
        public string Name { get; set; }
    }

    public class AccountCosmeticInventory
    {
        public CosmeticColorsUnlocked CosmeticColorsUnlocked { get; set; }
        public CosmeticWeaponsUnlocked CosmeticWeaponsUnlocked { get; set; }
        public CosmeticArmorsUnlocked CosmeticArmorsUnlocked { get; set; }
    }

    public class CosmeticColorsUnlocked
    {
        public string version { get; set; }
        public string bitmask { get; set; }
    }

    public class CosmeticWeaponsUnlocked
    {
        public string version { get; set; }
        public string bitmask { get; set; }
    }

    public class CosmeticArmorsUnlocked
    {
        public string version { get; set; }
        public string bitmask { get; set; }
    }
}
