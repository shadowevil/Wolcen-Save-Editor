using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace WolcenEditor
{
    public static class StashManager
    {
        private static int posY = 0;
        private static Dictionary<string, int> equipMap = new Dictionary<string, int>
        {
            { "Amulet", 14 },
            { "Helmet", 3 },
            { "Chest Armor", 1 },
            { "Foot Armor", 17 },
            { "Leg Armor", 11 },
            { "Shoulder", 6 },      // Also 5
            { "Arm Armor", 10 },    // Also 9
            { "Belt", 19 },
            { "Ring", 22 },         // Also 21
            { "Sword1H", 16 },      // Also 15
            { "Shield", 16 },
            { "Trinket", 16 },
            { "Mace1H", 16 },       // Also 15
            { "Bow", 15 },
            { "Axe1H", 16 },        // Also 15
            { "Staff", 15 },
            { "Axe2H", 15 },
            { "Sword2H", 15 },
            { "Dagger", 16 },       // Also 15
            { "Mace2H", 15 },
            { "Gun", 16 },          // Also 15
        };

        private enum typeMap : int
        {
            Weapon = 3,
            Offhand = 3,
            Armor = 2,
            Accessory = 2,
            Gem = 6,
            Potion = 4
        }

        public static void LoadPlayerStash(object sender)
        {
            Panel stashPanelGrid = (sender as TabPage).Controls["stashPanelGrid"] as Panel;
            stashPanelGrid.Controls.Clear();

            foreach (var _panel in cData.PlayerChest.Panels)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        PictureBox pb = new PictureBox();
                        pb.Name = x.ToString() + "|" + y.ToString() + "|" + _panel.ID.ToString();
                        pb.BackgroundImage = WolcenEditor.Properties.Resources.inventorySlot;
                        pb.Location = new Point(x * 50 + 7, y * 50 + 11);
                        pb.Size = new Size(50, 50);
                        pb.MaximumSize = pb.Size;
                        pb.SizeMode = PictureBoxSizeMode.AutoSize;
                        pb.BackgroundImageLayout = ImageLayout.Stretch;
                        //pb.AllowDrop = true;
                        pb.MouseDown += Pb_MouseDown;
                        //pb.MouseMove += Pb_MouseMove;
                        //pb.MouseLeave += Pb_MouseLeave;
                        //pb.DragEnter += Pb_DragEnter;
                        //pb.DragDrop += Pb_DragDrop;
                        //pb.GiveFeedback += Pb_GiveFeedBack;
                        //pb.ContextMenu = new ContextMenu();
                        stashPanelGrid.Controls.Add(pb);
                    }
                }
            }

            LoadStashBitmaps(stashPanelGrid, 0);
        }

        private static void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            if((sender as PictureBox).Image != null) LoadItemData(sender, e);
        }

        private static void LoadItemData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = (((sender as PictureBox).Parent as Panel).Parent as TabPage).Controls["itemStashStatDisplay"] as Panel;

            UnloadItemData(itemStatDisplay);
            PictureBox pictureBox = (sender as PictureBox);

            string itemName = getItemNameFromGrid(pictureBox);

            string l_itemName = null;
            WolcenStaticData.ItemLocalizedNames.TryGetValue(itemName, out l_itemName);
            if (l_itemName == null) return;

            Color itemRarity = getItemGridColorRarity(pictureBox);

            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, l_itemName, itemStatDisplay, 13, itemRarity));
            string itemType = InventoryManager.ParseItemNameForType(itemName);

            string itemStat = null;
            if (itemType != "Potions")
            {
                itemStat = getItemStat(pictureBox, "Health");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(pictureBox, "Armor");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Force Shield: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(pictureBox, "Resistance");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "All Resistance: " + itemStat, itemStatDisplay, 9, Color.White));

                itemStat = getItemStat(pictureBox, "DamageMin");
                string itemStat2 = getItemStat(pictureBox, "DamageMax");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(pictureBox, "ResourceGeneration");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));
            }
            itemStat = getItemStat(pictureBox, "Charge");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Charge: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(pictureBox, "ImmediateMana");
            if (itemStat != null)
            {
                if (itemStat.Contains("-"))
                {
                    if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Rage Generation: " + itemStat.Substring(1, itemStat.Length - 1), itemStatDisplay, 9, Color.White));
                }
                else
                {
                    if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Umbra Generation: " + itemStat, itemStatDisplay, 9, Color.White));
                }
            }
            itemStat = getItemStat(pictureBox, "ImmediateHP");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat(pictureBox, "ImmediateStamina");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Stamina Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            if (itemType != "Potions")
            {
                List<Effect> defaultEffects = getItemMagicEffect(pictureBox, "Default");
                if (defaultEffects != null)
                {
                    foreach (Effect effect in defaultEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                        if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }

            itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

            if (itemType == "Gem")
            {
                itemStat = getItemStat(pictureBox, "Name");
                string socketType = null;
                string localizedEffect = null;
                var GemAffixes = WolcenStaticData.GemAffixesWithValues[itemStat];
                List<int[]> GemOrder = new List<int[]>
                    {
                        new int[] { 0, 3, 6 },
                        new int[] { 1, 4, 7 },
                        new int[] { 2, 5, 8 }
                    };

                if (itemStat.Contains("physical_Gem"))
                {
                    GemOrder.RemoveAt(2);
                    GemOrder.Add(new int[] { 2, 5, 6 });
                }

                foreach (int[] x in GemOrder)
                {
                    foreach (int i in x)
                    {
                        socketType = WolcenStaticData.SocketType[i];
                        localizedEffect = WolcenStaticData.MagicLocalized[GemAffixes.ElementAt(i).Key];
                        if (GemAffixes.ElementAt(i).Key.Contains("percent")) localizedEffect = localizedEffect.Replace("%1", "%1%");
                        if (localizedEffect.Contains("%2")) localizedEffect = localizedEffect.Replace("%2", GemAffixes.ElementAt(i).Value);
                        localizedEffect = localizedEffect.Replace("%1", "+" + GemAffixes.ElementAt(i).Value);
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, socketType, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[i])));
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, localizedEffect, itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[i])));
                    }
                }
            }

            if (itemType != "Potions")
            {
                List<Socket> Sockets = getSockets(pictureBox);
                if (Sockets != null)
                {
                    foreach (Socket socket in Sockets)
                    {
                        string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                        if (socket.Gem == null) s_Socket += " [empty]";
                        else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, InventoryManager.getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    }
                }
            }
            itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

            if (itemType != "Potions")
            {
                List<Effect> magicEffects = getItemMagicEffect(pictureBox, "RolledAffixes");
                if (magicEffects != null)
                {
                    foreach (Effect effect in magicEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                        if (s_Effect.Contains("%1") || s_Effect.Contains("%2"))
                        {
                            if (effect.EffectId.Contains("percent")) s_Effect = s_Effect.Replace("%1", "%1%");
                            s_Effect = s_Effect.Replace("%1", "+" + effect.Parameters[0].value.ToString());
                            if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        }
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }
        }

        private static Label createLabelLineBreak(Panel panel)
        {
            Label lb = new Label();
            lb.Name = "s_lbl_LineBreak";
            lb.Text = "__________________________________________________";
            lb.Font = new Font(Form1.DefaultFont.FontFamily, 5, FontStyle.Regular);
            lb.ForeColor = Color.LightGray;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Size = new Size(panel.Width - 20, 5 + 5);
            lb.Location = new Point(0, posY - 5);
            posY += 5;
            return lb;
        }

        private static List<Socket> getSockets(PictureBox pictureBox)
        {
            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            return (item.Sockets as List<Socket>);
                        }
                    }
                }
            }
            return null;
        }

        private static List<Effect> getItemMagicEffect(PictureBox pictureBox, string stat)
        {
            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            return (item.MagicEffects.GetType().GetProperty(stat).GetValue(item.MagicEffects, null) as List<Effect>);
                        }
                    }
                }
            }
            return null;
        }

        private static string getItemStat(PictureBox pictureBox, string stat)
        {
            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            string itemStat = null;
                            if (item.Armor != null)
                            {
                                if (item.Armor.GetType().GetProperty(stat) != null)
                                {
                                    itemStat = item.Armor.GetType().GetProperty(stat).GetValue(item.Armor, null).ToString();
                                    return itemStat;
                                }
                            }
                            if (item.Weapon != null)
                            {
                                if (item.Weapon.GetType().GetProperty(stat) != null)
                                {
                                    itemStat = item.Weapon.GetType().GetProperty(stat).GetValue(item.Weapon, null).ToString();
                                    return itemStat;
                                }
                            }
                            if (item.Potion != null)
                            {
                                if (item.Potion.GetType().GetProperty(stat) != null)
                                {
                                    itemStat = item.Potion.GetType().GetProperty(stat).GetValue(item.Potion, null).ToString();
                                    return itemStat;
                                }
                            }
                            if (item.Gem != null)
                            {
                                if (item.Gem.GetType().GetProperty(stat) != null)
                                {
                                    itemStat = item.Gem.GetType().GetProperty(stat).GetValue(item.Gem, null).ToString();
                                    return itemStat;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
        {
            Label lb = new Label();
            lb.Name = "s_lbl" + name;
            lb.Text = text;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, fontSize, FontStyle.Regular);
            lb.ForeColor = fontColor;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Size = new Size(panel.Width - 10, fontSize + 10);
            lb.Location = new Point(0, posY);
            lb.Parent = panel;
            posY += fontSize + 10;
            return lb;
        }

        private static Color getItemGridColorRarity(PictureBox pictureBox)
        {
            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            string hexColor = WolcenStaticData.rarityColorBank[1];
                            WolcenStaticData.rarityColorBank.TryGetValue(item.Rarity, out hexColor);
                            return ColorTranslator.FromHtml(hexColor);
                        }
                    }
                }
            }
            return Color.White;
        }

        private static string getItemNameFromGrid(PictureBox pictureBox)
        {
            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            if (item.Armor != null)
                            {
                                return item.Armor.Name;
                            }
                            if (item.Weapon != null)
                            {
                                return item.Weapon.Name;
                            }
                            if (item.Gem != null)
                            {
                                return item.Gem.Name;
                            }
                            if (item.Potion != null)
                            {
                                return item.Potion.Name;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static void UnloadItemData(Panel itemStatDisplay)
        {
            posY = 0;
            itemStatDisplay.Controls.Clear();
        }

        private static void LoadStashBitmaps(Panel stashPanelGrid, int selectedPanel = 0)
        {
            foreach (var _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == selectedPanel)
                {
                    foreach (var iGrid in _panel.InventoryGrid)
                    {
                        foreach (PictureBox pb in stashPanelGrid.Controls)
                        {
                            int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                            int y = Convert.ToInt32(pb.Name.Split('|')[1]);

                            if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                            {
                                pb.Image = GetStashBitmap(x, y, selectedPanel, pb, iGrid);
                            }
                        }
                    }
                }
            }
        }

        private static Image GetStashBitmap(int x, int y, int selectedPanel, PictureBox pb, InventoryGrid iGrid)
        {
            string dirPath = @".\UIResources\";
            string itemName = "";
            string l_itemName = null;
            int itemRarity = 0;
            if (iGrid.Armor != null)
            {
                itemName = iGrid.Armor.Name;
                itemRarity = iGrid.Rarity;
                WolcenStaticData.ItemArmor.TryGetValue(itemName, out l_itemName);
                if (l_itemName == null)
                {
                    WolcenStaticData.ItemAccessories.TryGetValue(itemName, out l_itemName);
                }
                else
                {
                    if (iGrid.ItemType != "Belt")
                    {
                        pb.MaximumSize = new Size(50, 100);
                        pb.Size = new Size(50, 100);
                    }
                }
            }
            if (iGrid.Weapon != null)
            {
                itemName = iGrid.Weapon.Name;
                itemRarity = iGrid.Rarity;
                WolcenStaticData.ItemWeapon.TryGetValue(itemName, out l_itemName);
                pb.MaximumSize = new Size(50, 100);
                pb.Size = new Size(50, 100);
            }
            if (iGrid.Potion != null)
            {
                string[] pName = iGrid.Potion.Name.Split('_');
                itemRarity = iGrid.Rarity;
                l_itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";
            }
            if (iGrid.Gem != null)
            {
                itemName = iGrid.Gem.Name;
                itemRarity = iGrid.Rarity;
                l_itemName = itemName + ".png";
            }

            if (iGrid.MagicEffects != null)
            {
                if (iGrid.MagicEffects.RolledAffixes != null)
                {
                    foreach (var effect in iGrid.MagicEffects.RolledAffixes)
                    {
                        string effectId = effect.EffectId;
                        if (effect.Parameters != null)
                        {
                            int effectParamCount = effect.Parameters.Count();
                            List<string> aSem = new List<string>();
                            for (int z = 0; z < effect.Parameters.Count(); z++)
                            {
                                aSem.Add(effect.Parameters[z].semantic);
                            }
                            string[] actualSemantics = aSem.ToArray();
                            if (actualSemantics != null)
                            {
                                string[] semantics = null;
                                WolcenStaticData.Semantics.TryGetValue(effectId, out semantics);
                                if (semantics != null)
                                {
                                    if (semantics.Count() != actualSemantics.Count())
                                    {
                                        LogMe.WriteLog("Error: Wrong Semantic count (" + semantics.Count() + ")->(" + actualSemantics.Count() + ")");
                                        for (int z = 0; z < actualSemantics.Count(); z++)
                                        {
                                            LogMe.WriteLog("Error-Cont: Actual Parameters for EffectId: " + effectId + "(" + actualSemantics[z] + ")");
                                        }
                                    }
                                    else if (semantics.Count() == actualSemantics.Count())
                                    {
                                        for (int z = 0; z < actualSemantics.Count(); z++)
                                        {
                                            if (semantics[z] != actualSemantics[z])
                                            {
                                                LogMe.WriteLog("Error: semantic miss-match " + effectId + "(" + semantics[z] + ")->(" + actualSemantics[z] + ")");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int z = 0; z < actualSemantics.Count(); z++)
                                    {
                                        LogMe.WriteLog("Error: Semantic doesn't exist for EffectID: " + effectId + "(" + actualSemantics[z] + ")");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (File.Exists(dirPath + "Items\\" + l_itemName))
            {
                return CombineGridBitmaps(dirPath, l_itemName, itemRarity, iGrid.ItemType, pb);
            }
            else
            {
                if (iGrid.Armor != null)
                {
                    return new Bitmap(Image.FromFile(dirPath + "Items\\" + "unknown_armor.png"));
                }
                if (iGrid.Weapon != null)
                {
                    return new Bitmap(Image.FromFile(dirPath + "Items\\" + "unknown_weapon.png"));
                }
            }
            return null;
        }

        private static Bitmap CombineGridBitmaps(string dirPath, string itemName, int quality, string itemType, PictureBox pb = null)
        {
            Bitmap Background = new Bitmap(Image.FromFile(dirPath + "ItemBorders\\" + quality + ".png"), pb.Width, pb.Height);
            Bitmap ItemImage = new Bitmap(Image.FromFile(dirPath + "Items\\" + itemName));
            Bitmap FinalImage = new Bitmap(Background.Width, Background.Height);

            using (Graphics g = Graphics.FromImage(FinalImage))
            {
                g.Clear(Color.Black);
                g.DrawImage(Background, 0, 0);
                int width = Background.Width - 10;
                int height = Background.Height - 10;
                if (itemType == "Shoulder")
                {
                    width = 50;
                    height = 55;
                }
                if (itemType == "Belt")
                {
                    width = 40;
                    height = 25;
                }
                if (itemType == "Ring" || itemType == "Amulet")
                {
                    width = 35;
                    height = 30;
                }
                if (itemType == "Helmet")
                {
                    height = 65;
                    width = 50;
                }
                if (itemType == "Potions")
                {
                    height = 40;
                    width = 20;
                }
                if (itemType == "Leg Armor")
                {
                    height = 75;
                    width = 45;
                }
                int x = Background.Width / 2 - (width / 2);
                int y = Background.Height / 2 - (height / 2);
                g.DrawImage(ItemImage, x, y, width, height);
            }

            return FinalImage;
        }
    }
}
