using System.Collections.Generic;

namespace WolcenEditor
{
    public class CharacterCosmeticInventory
    {
        public CosmeticColors CosmeticColors { get; set; }
        public CosmeticSkinT CosmeticSkinT { get; set; }
        public CosmeticWeaponT CosmeticWeaponT { get; set; }
    }

    public class CosmeticWeaponT
    {
        public Weapons WeaponSkinTransfers { get; set; }


        public class Weapons
        {
            public Weapon WeaponLeft { get; set; }
            public Weapon WeaponRight { get; set; }
        }

        public class Weapon
        {
            public Ids Ids { get; set; }

        }

        public class Ids
        {
            public string Shield { get; set; }
            public string Dagger { get; set; }
            public string Mace1H { get; set; }
            public string Mace2H { get; set; }
            public string Sword1H { get; set; }
            public string Sword2H { get; set; }
            public string Axe1H { get; set; }
            public string Axe2H { get; set; }
            public string Staff { get; set; }
            public string Trinket { get; set; }
            public string Bow { get; set; }
            public string Gun { get; set; }

        }

    }

    public class CosmeticSkinT
    {
        public Equipped Equip { get; set; }

        public class Equipped
        {
            public EquipSlot ArmL { get; set; }
            public EquipSlot ArmR { get; set; }
            public EquipSlot Boots { get; set; }
            public EquipSlot Chest { get; set; }
            public EquipSlot Helmet { get; set; }
            public EquipSlot Pant { get; set; }
            public EquipSlot ShoulderL { get; set; }
            public EquipSlot ShoulderR { get; set; }
        }
        public class EquipSlot
        {
            public string SkinName { get; set; }
        }
    }

    public class CosmeticColors
    {
        public BodySlot ArmL { get; set; }
        public BodySlot ArmR { get; set; }
        public BodySlot Boots { get; set; }
        public BodySlot Chest { get; set; }
        public BodySlot Helmet { get; set; }
        public BodySlot Pant { get; set; }
        public BodySlot ShoulderL { get; set; }
        public BodySlot ShoulderR { get; set; }

        public class BodySlot
        {
            public IList<ColorSlots> ColorSlots { get; set; }
        }

        public class ColorSlots
        {
            public string slot { get; set; }
            public string name { get; set; }
        }
    }


}