using Newtonsoft.Json;
using System.Collections.Generic;

namespace WolcenEditor
{
    public class InventoryGrid
    {
        public int InventoryX { get; set; }
        public int InventoryY { get; set; }
        public int Rarity { get; set; }
        public int Quality { get; set; }
        public int Type { get; set; }
        public string ItemType { get; set; }
        public string Value { get; set; }
        public int Level { get; set; }
        public ItemArmor Armor { get; set; }
        public ItemWeapon Weapon { get; set; }
        public IList<Socket> Sockets { get; set; }
        public Gem Gem { get; set; }
        public Potion Potion { get; set; }
        public Reagent Reagent { get; set; }
        public ItemMagicEffects MagicEffects { get; set; }

        [JsonIgnore]
        public InventoryGrid copy
        {
            get
            {
                return (InventoryGrid)this.MemberwiseClone();
            }
        }
    }
}