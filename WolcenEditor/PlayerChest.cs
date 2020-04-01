using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using Newtonsoft.Json.Serialization;

namespace WolcenEditor
{
    public class PlayerChest
    {
        public string Version { get; set; }
        public string InventoryVersion { get; set; }
        public string ItemsVersion { get; set; }
        public List<Panels> Panels { get; set; }
    }

    public class Panels
    {
        public int ID { get; set; }
        public bool Locked { get; set; }
        public IList<InventoryGrid> InventoryGrid { get; set; }
    }

    public static class PlayerChestIO
    {
        public static PlayerChest ReadPlayerStash(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            PlayerChest playerChest = JsonConvert.DeserializeObject<PlayerChest>(jsonData);
            return playerChest;
        }

        public static void WritePlayerChest(string outputPath, PlayerChest playerChest, bool backup = true)
        {
            if (backup)
            {
                if (File.Exists($"{outputPath}") && !File.Exists($"{outputPath}.bak"))
                {
                    File.Copy(outputPath, outputPath + ".bak");
                }
            }
            string newJsonFile = JsonConvert.SerializeObject(playerChest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputPath, newJsonFile);
        }
    }
}
