using System.Collections.Generic;
using System.ComponentModel;

namespace WolcenEditor
{
    public class InventoryEquipped
    {
        public int BodyPart { get; set; }
        public int Rarity { get; set; }
        public int Quality { get; set; }
        public int Type { get; set; }
        public string ItemType { get; set; }
        public string ItemVersion { get; set; }
        public string ItemGameplayVersion { get; set; }
        public string Value { get; set; }
        public int Level { get; set; }
        public ItemArmor Armor { get; set; }
        public ItemWeapon Weapon { get; set; }
        public IList<Socket> Sockets { get; set; }
        public ItemMagicEffects MagicEffects { get; set; } 
    }

    public class ItemWeapon
    {
        public string Name { get; set; }
        public double DamageMin { get; set; }
        public double DamageMax { get; set; }
        public double ResourceGeneration { get; set; }
        public double ShieldResistance { get; set; }
        public double ShieldBlockChance { get; set; }
        public double ShieldBlockEfficiency { get; set; }
    }

    public class ItemArmor
    {
        public string Name { get; set; }
        public double Armor { get; set; }
        public double Health { get; set; }
        public double Resistance { get; set; }

    }
    public class Socket
    {
        public Gem Gem { get; set; }

        public int Effect { get; set; }
    }
    public class Gem
    {
        public string Name { get; set; }
        public int StackSize { get; set; }
    }

    public class Enneract
    {
        public string Name { get; set; }
        public string Stats_SkillUID { get; set; }
        public int Stats_SkillLevel { get; set; }
    }

    public class NPC2Consumable
    {
        public string Name { get; set; }
        public int StackSize { get; set; }
    }

    public class ItemMagicEffects
    {
        public List<Effect> Default { get; set; }

        public List<Effect> RolledAffixes { get; set; }

        public List<Effect> FromGems { get; set; }
    }

    public class Effect
    {
        public string EffectId { get; set; }
        public string EffectName { get; set; }
        public int MaxStack { get; set; }
        public int bDefault { get; set; }
        public List<EffectParams> Parameters { get; set; }
    }

    public class EffectParams
    {
        public string semantic { get; set; }
        public double value { get; set; }
    }
}