namespace WolcenEditor
{
    public class InventoryBelt
    {
        public int Rarity { get; set; }
        public int Quality { get; set; }
        public int Type { get; set; }
        public string ItemType { get; set; }
        public string ItemVersion { get; set; }
        public int Value { get; set; }
        public int Level { get; set; }
        public Potion Potion { get; set; }
        public int BeltSlot { get; set; }

    }

    public class Potion
    {
        public string Name { get; set; }
        public int Charge { get; set; }
        public int ImmediateHP { get; set; }
        public int ImmediateMana { get; set; }
        public int ImmediateStamina { get; set; }

    }
}