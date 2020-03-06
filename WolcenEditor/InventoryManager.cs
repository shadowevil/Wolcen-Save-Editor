using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace WolcenEditor
{
    public static class InventoryManager
    {
        private static Size defaultGridSize = new Size(50, 50);
        private static PictureBox sourceBox;
        private static bool isValid = false;

        private static Dictionary<string, int> charMap = new Dictionary<string, int>
        {
            {"charHelm", 3 },
            {"charChest", 1},
            {"charLPad", 6 },
            {"charRPad", 5},
            {"charLHand", 10 },
            {"charRHand", 9},
            {"charBelt", 19 },
            {"charPants", 11 },
            {"charNeck", 14},
            {"charBoots", 17 },
            {"charLRing", 22},
            {"charRRing", 21 },
            {"charLWeapon", 15},
            {"charRWeapon", 16},
        };

        public static void LoadCharacterInventory(object sender)
        {
            TabPage tabPage = (sender as TabPage);
            //bool flip = false;

            foreach (Control control in tabPage.Controls)
            {
                PictureBox pictureBox = (control as PictureBox);
                if (pictureBox != null)
                {
                    int bodyPart = 0;
                    charMap.TryGetValue(pictureBox.Name, out bodyPart);
                    if (bodyPart > 0)
                    {
                        pictureBox.Image = GetInventoryBitmap(charMap[pictureBox.Name]);
                        pictureBox.Click += LoadItemData;
                        //pictureBox.MouseDown += Pb_MouseDown;
                        //pictureBox.DragEnter += Pb_DragEnter;
                        //pictureBox.DragDrop += Pb_DragDrop;
                        //pictureBox.BackgroundImage = setRarityBackground(pictureBox);
                    }
                }
            }

            LoadRandomInventory(sender);
        }

        private static void LoadRandomInventory(object sender)
        {
            Panel charRandomInv = ((sender as TabPage).Controls["charRandomInv"] as Panel);
            IList<InventoryGrid> invGrid = cData.Character.InventoryGrid;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    PictureBox pb = new PictureBox();
                    pb.Name = x.ToString() + "|" + y.ToString();
                    pb.BackgroundImage = WolcenEditor.Properties.Resources.inventorySlot;
                    pb.Location = new Point(x * 50 + 5, y * 50 + 5);
                    pb.Size = new Size(50, 50);
                    pb.MaximumSize = pb.Size;
                    pb.SizeMode = PictureBoxSizeMode.AutoSize;
                    pb.BackgroundImageLayout = ImageLayout.Stretch;
                    pb.AllowDrop = true;
                    pb.MouseDown += Pb_MouseDown;
                    pb.DragEnter += Pb_DragEnter;
                    pb.DragDrop += Pb_DragDrop;
                    charRandomInv.Controls.Add(pb);
                }
            }

            foreach (InventoryGrid inv in invGrid)
            {
                foreach (PictureBox pb in (charRandomInv.Controls))
                {
                    int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                    int y = Convert.ToInt32(pb.Name.Split('|')[1]);

                    if (inv.InventoryX == x && inv.InventoryY == y)
                    {
                        pb.Image = GetInventoryBitmap(0, pb);
                    }
                }
            }
        }

        private static void Pb_DragDrop(object sender, DragEventArgs e)
        {
            if ((sender as PictureBox).Name == sourceBox.Name)
            {
                int index = (sender as PictureBox).Parent.Controls.IndexOfKey(sourceBox.Name);
                ((sender as PictureBox).Parent.Controls[index] as PictureBox).Image = sourceBox.Image;
                isValid = false;
                return;
            }
            if ((sender as PictureBox).Image != null)
            {
                isValid = false;
                return;
            }
            int x = Convert.ToInt32((sender as PictureBox).Name.Split('|')[0]);
            int y = Convert.ToInt32((sender as PictureBox).Name.Split('|')[1]);
            if (sourceBox.MaximumSize.Height == 100)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (x == i && y == 5)
                    {
                        isValid = false;
                        return;
                    }
                }
            }
            Bitmap bmp = (e.Data.GetData(DataFormats.Bitmap) as Bitmap);
            (sender as PictureBox).Image = bmp;
            (sender as PictureBox).MaximumSize = sourceBox.MaximumSize;
            (sender as PictureBox).Size = sourceBox.Size;
            sourceBox.Image = null;
            isValid = true;
            ConfirmMove((sender as PictureBox));
        }

        private static void ConfirmMove(PictureBox pictureBox)
        {
            for (int i = 0; i < cData.Character.InventoryGrid.Count; i++)
            {
                int x = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
                if (cData.Character.InventoryGrid[i].InventoryX == x && cData.Character.InventoryGrid[i].InventoryY == y)
                {
                    cData.Character.InventoryGrid[i].InventoryX = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                    cData.Character.InventoryGrid[i].InventoryY = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                }
            }
        }

        private static void Pb_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private static void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as PictureBox).Image == null) return;
            Bitmap bmp = new Bitmap((sender as PictureBox).Image);
            sourceBox = (sender as PictureBox);
            if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Copy) == DragDropEffects.Copy && isValid)
            {
                (sender as PictureBox).Size = defaultGridSize;
                return;
            }
            LoadItemGridData(sender, e);
        }

        private static int posY = 0;

        private static void LoadItemGridData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = (((sender as PictureBox).Parent as Panel).Parent.Controls["itemStatDisplay"] as Panel);
            UnloadItemData(itemStatDisplay);
            PictureBox pictureBox = (sender as PictureBox);

            string itemName = getItemNameFromGrid(pictureBox);
            WolcenStaticData.ItemLocalizedNames.TryGetValue(itemName, out itemName);

            Color itemRarity = getItemGridColorRarity(pictureBox);

            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, itemName, itemStatDisplay, 13, itemRarity));
            string itemType = null;
            WolcenStaticData.ItemArmor.TryGetValue(getItemNameFromGrid(pictureBox), out itemType);
            if (itemType == null)
            {
                WolcenStaticData.ItemAccessories.TryGetValue(getItemNameFromGrid(pictureBox), out itemType);
                if (itemType == null)
                {
                    WolcenStaticData.ItemWeapon.TryGetValue(getItemNameFromGrid(pictureBox), out itemType);
                }
            }
            if(itemType == null) itemType = getItemTypeFromGrid(pictureBox);

            string itemStat = getItemStat(pictureBox, "Health");
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

                // Offensive 1
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

            List<Socket> Sockets = getSockets(pictureBox);
            if (Sockets != null)
            {
                foreach (Socket socket in Sockets)
                {
                    string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                    if (socket.Gem == null) s_Socket += " [empty]";
                    else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                }
            }

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

            itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

            List<Effect> magicEffects = getItemMagicEffect(pictureBox, "RolledAffixes");
            if (magicEffects != null)
            {
                foreach (Effect effect in magicEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                    if (effect.EffectId.Contains("percent")) s_Effect = s_Effect.Replace("%1", "%1%");
                    s_Effect = s_Effect.Replace("%1", effect.Parameters[0].value.ToString());
                    if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                }
            }
        }

        private static string getItemTypeFromGrid(PictureBox pictureBox)
        {
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    return iGrid.ItemType;
                }
            }
            return null;
        }

        private static string getItemNameFromGrid(PictureBox pictureBox)
        {
            foreach (InventoryGrid item in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
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
            return null;
        }

        private static Color getItemGridColorRarity(PictureBox pictureBox)
        {
            foreach (InventoryGrid item in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                if (item.InventoryX == x && item.InventoryY == y)
                {
                    return ColorTranslator.FromHtml(WolcenStaticData.qualityColorBank[item.Rarity]);
                }
            }
            return Color.White;
        }

        private static void LoadItemData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = ((sender as PictureBox).Parent.Controls["itemStatDisplay"] as Panel);
            UnloadItemData(itemStatDisplay);

            PictureBox pictureBox = (sender as PictureBox);
            int bodyPart = charMap[pictureBox.Name];
            string itemName = getItemName_Color(bodyPart);
            if (itemName == "Item not found!") return;
            Color nameColor = ColorTranslator.FromHtml("#" + itemName.Split('#')[1]);
            itemName = itemName.Split('#')[0];
            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, WolcenStaticData.ItemLocalizedNames[itemName], itemStatDisplay, 13, nameColor));
            if (bodyPart != 15 && bodyPart != 16)
            {       // Armor
                string itemStat = getItemStat(bodyPart, "Health");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "Armor");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Force Shield: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "Resistance");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "All Resistance: " + itemStat, itemStatDisplay, 9, Color.White));

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List <Socket> Sockets = getSockets(bodyPart);
                if (Sockets != null)
                {
                    foreach (Socket socket in Sockets)
                    {
                        string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                        if (socket.Gem == null) s_Socket += " [empty]";
                        else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    }
                }

                List<Effect> defaultEffects = getItemMagicEffect(bodyPart, "Default");
                foreach (Effect effect in defaultEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                    if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                }

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Effect> magicEffects = getItemMagicEffect(bodyPart, "RolledAffixes");
                if (magicEffects != null)
                {
                    foreach (Effect effect in magicEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                        if (effect.EffectId.Contains("percent")) s_Effect = s_Effect.Replace("%1", "%1%");
                        s_Effect = s_Effect.Replace("%1", effect.Parameters[0].value.ToString());
                        if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }
            else   // Weapons
            {
                string itemStat = getItemStat(bodyPart, "DamageMin");
                string itemStat2 = getItemStat(bodyPart, "DamageMax");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "ResourceGeneration");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Socket> Sockets = getSockets(bodyPart);
                if (Sockets != null)
                {
                    foreach (Socket socket in Sockets)
                    {
                        string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                        if (socket.Gem == null) s_Socket += " [empty]";
                        else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        if(socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    }
                }

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Effect> magicEffects = getItemMagicEffect(bodyPart, "RolledAffixes");
                if (magicEffects != null)
                {
                    foreach (Effect effect in magicEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                        if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }
        }

        private static void UnloadItemData(Panel itemStatDisplay)
        {
            posY = 0;
            itemStatDisplay.Controls.Clear();
        }

        private static string getGemStats(string gemName, int gemEffect)
        {
            foreach (var gem in WolcenStaticData.GemAffixesWithValues)
            {
                if (gem.Key == gemName)
                {
                    string returnStr = WolcenStaticData.MagicLocalized[gem.Value.ElementAt(gemEffect).Key];
                    if (gemName.Contains("physical_Gem") && gemEffect == 8) gemEffect = 6;
                    if (gem.Value.ElementAt(gemEffect).Key.Contains("percent")) returnStr = returnStr.Replace("%1", "%1%");
                    returnStr = "+" + returnStr.Replace("%1", gem.Value.ElementAt(gemEffect).Value);
                    if (returnStr.Contains("%2")) returnStr = returnStr.Replace("%2", gem.Value.ElementAt(gemEffect).Value);
                    return returnStr;
                }
            }
            return "[NOT FOUND]";
        }

        private static List<Socket> getSockets(PictureBox pictureBox)
        {
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    return (iGrid.Sockets as List<Socket>);
                }
            }
            return null;
        }

        private static List<Socket> getSockets(int bodyPart)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return (equip.GetType().GetProperty("Sockets").GetValue(equip, null) as List<Socket>);
                }
            }
            return null;
        }

        private static List<Effect> getItemMagicEffect(PictureBox pictureBox, string stat)
        {
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    if (iGrid.MagicEffects == null) return null;
                    return (iGrid.MagicEffects.GetType().GetProperty(stat).GetValue(iGrid.MagicEffects, null) as List<Effect>);
                }
            }
            return null;
        }

        private static List<Effect> getItemMagicEffect(int bodyPart, string stat)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return (equip.MagicEffects.GetType().GetProperty(stat).GetValue(equip.MagicEffects, null) as List<Effect>);
                }
            }
            return null;
        }

        private static string getItemStat(PictureBox pictureBox, string stat)
        {
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    string itemStat = null;
                    if (iGrid.Armor != null)
                    {
                        if (iGrid.Armor.GetType().GetProperty(stat) != null)
                        {
                            itemStat = iGrid.Armor.GetType().GetProperty(stat).GetValue(iGrid.Armor, null).ToString();
                        }
                    }
                    if (iGrid.Weapon != null)
                    {
                        if (iGrid.Weapon.GetType().GetProperty(stat) != null)
                        {
                            itemStat = iGrid.Weapon.GetType().GetProperty(stat).GetValue(iGrid.Weapon, null).ToString();
                        }
                    }
                    if (iGrid.Potion != null)
                    {
                        if (iGrid.Potion.GetType().GetProperty(stat) != null)
                        {
                            itemStat = iGrid.Potion.GetType().GetProperty(stat).GetValue(iGrid.Potion, null).ToString();
                        }
                    }
                    if (iGrid.Gem != null)
                    {
                        if (iGrid.Gem.GetType().GetProperty(stat) != null)
                        {
                            itemStat = iGrid.Gem.GetType().GetProperty(stat).GetValue(iGrid.Gem, null).ToString();
                        }
                    }

                    return itemStat;
                }
            }
            return null;
        }

        private static string getItemStat(int bodyPart, string stat)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    string itemStat = "";
                    if (bodyPart == 16 || bodyPart == 15)
                        itemStat = equip.Weapon.GetType().GetProperty(stat).GetValue(equip.Weapon, null).ToString();
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemStat = equip.Armor.GetType().GetProperty(stat).GetValue(equip.Armor, null).ToString();
                    else
                        itemStat = equip.Armor.GetType().GetProperty(stat).GetValue(equip.Armor, null).ToString();

                    return itemStat;
                }
            }
            return "Item stat not found!";
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

        private static Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
        {
            Label lb = new Label();
            lb.Name = "s_lbl" + name;
            lb.Text = text;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, fontSize, FontStyle.Regular);
            lb.ForeColor = fontColor;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Size = new Size(panel.Width - 20, fontSize + 10);
            lb.Location = new Point(0, posY);
            posY += fontSize + 10;
            return lb;
        }

        private static string getItemName_Color(int bodyPart)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    string itemName = "";
                    if (bodyPart == 16 || bodyPart == 15)
                        itemName = equip.Weapon.Name;
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemName = equip.Armor.Name;
                    else
                        itemName = equip.Armor.Name;

                    itemName += WolcenStaticData.qualityColorBank[equip.Rarity];

                    return itemName;
                }
            }
            return "Item not found!";
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
                int x = Background.Width / 2 - (width / 2);
                int y = Background.Height / 2 - (height / 2);
                g.DrawImage(ItemImage, x, y, width, height);
            }

            return FinalImage;
        }

        private static Bitmap GetInventoryBitmap(int bodyPart = 0, PictureBox pb = null)
        {
            if (bodyPart == 0)
            {
                foreach (var i in cData.Character.InventoryGrid)
                {
                    int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                    int y = Convert.ToInt32(pb.Name.Split('|')[1]);
                    if (i.InventoryX == x && i.InventoryY == y)
                    {
                        string dirPath = @".\UIResources\";
                        string itemName = "";
                        string l_itemName = null;
                        int itemRarity = 0;
                        if (i.Armor != null)
                        {
                            itemName = i.Armor.Name;
                            itemRarity = i.Rarity;
                            WolcenStaticData.ItemArmor.TryGetValue(itemName, out l_itemName);
                            if (l_itemName == null)
                            {
                                WolcenStaticData.ItemAccessories.TryGetValue(itemName, out l_itemName);
                            }
                            else
                            {
                                if (i.ItemType != "Belt")
                                {
                                    pb.MaximumSize = new Size(50, 100);
                                    pb.Size = new Size(50, 100);
                                }
                            }
                        }
                        if (i.Weapon != null)
                        {
                            itemName = i.Weapon.Name;
                            itemRarity = i.Rarity;
                            WolcenStaticData.ItemWeapon.TryGetValue(itemName, out l_itemName);
                            pb.MaximumSize = new Size(50, 100);
                            pb.Size = new Size(50, 100);
                        }
                        if (i.Potion != null)
                        {
                            string[] pName = i.Potion.Name.Split('_');
                            itemRarity = i.Rarity;
                            l_itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";
                        }
                        if (i.Gem != null)
                        {
                            itemName = i.Gem.Name;
                            itemRarity = i.Rarity;
                            l_itemName = itemName + ".png";
                        }

                        if (File.Exists(dirPath + "Items\\" + l_itemName))
                        {
                            return CombineGridBitmaps(dirPath, l_itemName, itemRarity, i.ItemType, pb);
                        }
                        else
                        {
                            if (i.Armor != null)
                            {
                                return new Bitmap(Image.FromFile(dirPath + "Items\\" + "unknown_armor.png"));
                            }
                            if (i.Weapon != null)
                            {
                                return new Bitmap(Image.FromFile(dirPath + "Items\\" + "unknown_weapon.png"));
                            }
                        }
                    }
                }
                }
            else
            {
                foreach (var i in cData.Character.InventoryEquipped)
                {
                    string dirPath = @".\UIResources\Items\";
                    string itemName = "";
                    string l_itemName = null;
                    int itemRarity = 0;
                    if (i.BodyPart == bodyPart)
                    {
                        if (i.Armor != null)
                        {
                            itemName = i.Armor.Name;
                            itemRarity = i.Rarity;
                            WolcenStaticData.ItemArmor.TryGetValue(itemName, out l_itemName);
                            if (l_itemName == null)
                            {
                                WolcenStaticData.ItemAccessories.TryGetValue(itemName, out l_itemName);
                            }
                        }
                        if (i.Weapon != null)
                        {
                            itemName = i.Weapon.Name;
                            itemRarity = i.Rarity;
                            WolcenStaticData.ItemWeapon.TryGetValue(itemName, out l_itemName);
                        }

                        if (File.Exists(dirPath + l_itemName))
                        {
                            return ResizeAndCombine(dirPath, itemName, itemRarity);
                        }
                        else
                        {
                            if (i.Armor != null)
                            {
                                return new Bitmap(Image.FromFile(dirPath + "unknown_armor.png"));
                            }
                            if (i.Weapon != null)
                            {
                                return new Bitmap(Image.FromFile(dirPath + "unknown_weapon.png"));
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static Bitmap ResizeAndCombine(string dirPath, string itemName, int quality)
        {
            List<Bitmap> images = new List<Bitmap>();
            var underImage = new Bitmap(Image.FromFile(@".\UIResources\ItemBorders\" + quality + ".png"));

            var finalImage = new Bitmap(underImage.Width, underImage.Height);

            var itemImage = new Bitmap(Image.FromFile(dirPath + itemName + ".png"));

            Bitmap resized = new Bitmap(itemImage, new Size(underImage.Width, underImage.Height));

            images.Add(underImage);
            images.Add(resized);

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Black);
                //go through each image and draw it on the final image (Notice the offset; since I want to overlay the images i won't have any offset between the images in the finalImage)
                int offset = 0;
                foreach (Bitmap image in images)
                {
                    g.DrawImage(image, new Rectangle(offset, 0, image.Width, image.Height));
                }
            }
            return finalImage;
        }
    }
}
