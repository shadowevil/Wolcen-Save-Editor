using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class ItemDataDisplay
    {
        private static int posY = 0;
        private static PictureBox accessablePictureBox;
        private static InventoryGrid iGridItem;
        private static InventoryBelt iBeltItem;
        private static InventoryEquipped iEquippedItem;
        private static int globalItemIndex = -1;
        private struct coords
        {
            public static int x = -1;
            public static int y = -1;
            public static int panelID = -1;
            public static int BodyPart = -1;
        };

        public static void LoadItemData(object sender, Panel panel, object type, string select)
        {
            coords.x = -1;
            coords.y = -1;
            coords.panelID = -1;
            coords.BodyPart = -1;
            iGridItem = null;
            iBeltItem = null;
            iEquippedItem = null;

            PictureBox pb = (sender as PictureBox);
            PictureBox pictureBox = (sender as PictureBox);
            if (pb == null) return;

            accessablePictureBox = pb;
            if (select != "InventoryBelt" && select != "InventoryEquipped")
            {
                coords.x = Convert.ToInt32(pb.Name.Split('|')[0]);
                coords.y = Convert.ToInt32(pb.Name.Split('|')[1]);
                if (pb.Name.Split('|').Length == 3)
                {
                    coords.panelID = Convert.ToInt32(pb.Name.Split('|')[2]);
                    type = (type as IList)[coords.panelID];
                }
            }
            else if (select == "InventoryEquipped")
            {
                coords.BodyPart = InventoryManager.charMap[pb.Name];
            }
            setGridItem(type, select, pb.Name);

            Panel itemStatDisplay = panel;

            UnloadItemData(itemStatDisplay);

            string itemName = null;
            if (itemName == null && iBeltItem != null) itemName = iBeltItem.Potion.Name;
            if (itemName == null) itemName = getItemStat("Armor", "Name");
            if (itemName == null) itemName = getItemStat("Weapon", "Name");
            if (itemName == null) itemName = getItemStat("Potion", "Name");
            if (itemName == null) itemName = getItemStat("Gem", "Name");
            if (itemName == null) itemName = getItemStat("Reagent", "Name");
            if (itemName == null) itemName = getItemStat("Enneract", "Name");
            if (itemName == null) itemName = getItemStat("NPC2Consumable", "Name");
            if (itemName == null) return;

            string l_itemName = null;
            WolcenStaticData.ItemLocalizedNames.TryGetValue(itemName, out l_itemName);
            if (l_itemName == null) return;

            Color itemRarity = getItemColorRarity();
            
            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, l_itemName, itemStatDisplay, 13, itemRarity));
            string itemType = ParseItemNameForType(itemName);

            string itemStat = null;

            itemStat = getItemStat("Armor", "Health");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Armor", "Armor");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Force Shield: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Armor", "Resistance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "All Resistance: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat("Weapon", "DamageMin");
            string itemStat2 = getItemStat("Weapon", "DamageMax");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Weapon", "ResourceGeneration");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat("Weapon", "ShieldResistance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resistance: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Weapon", "ShieldBlockChance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Chance: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Weapon", "ShieldBlockEfficiency");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Efficiency: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat("Potion", "Charge");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Charge: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat("Potion", "ImmediateMana");
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
            itemStat = getItemStat("Potion", "ImmediateHP");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat("Potion", "ImmediateStamina");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Stamina Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            if (itemType != "Potions")
            {
                List<Effect> defaultEffects = getItemMagicEffect("Default");
                if (defaultEffects != null)
                {
                    foreach (Effect effect in defaultEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                        if (s_Effect.Contains("%1") || s_Effect.Contains("%2"))
                        {
                            if (!effect.Parameters[0].semantic.Contains("TriggeredSkillSelector"))
                            {
                                if (effect.EffectId.Contains("percent")
                                    || effect.Parameters[0].semantic.Contains("ChanceFlatFloat")
                                    || effect.Parameters[0].semantic.Contains("PossibilityInt"))
                                {
                                    s_Effect = s_Effect.Replace("%1", "%1%");
                                }

                                if (effect.Parameters.Count <= 0)
                                {
                                    LogMe.WriteLog("Error: effectID(" + effect.EffectId + "), effectName(" + effect.EffectName + ") missing semantics!");
                                    return;
                                }

                                s_Effect = s_Effect.Replace("%1", "+" + effect.Parameters[0].value.ToString());
                                if (s_Effect.Contains("%2") && effect.Parameters.Count > 1)
                                {
                                    s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                                }
                            }
                            else
                            {
                                if (effect.EffectId.Contains("percent")
                                    || effect.Parameters[1].semantic.Contains("ChanceFlatFloat")
                                    || effect.Parameters[1].semantic.Contains("PossibilityInt"))
                                {
                                    s_Effect = s_Effect.Replace("%1", "%1%");
                                }

                                if (effect.Parameters.Count <= 0)
                                {
                                    LogMe.WriteLog("Error: effectID(" + effect.EffectId + "), effectName(" + effect.EffectName + ") missing semantics!");
                                    return;
                                }

                                s_Effect = s_Effect.Replace("%1", "+" + effect.Parameters[1].value.ToString());
                            }
                        }
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }

            if (itemName.Contains("Reagent"))
            {
                itemStat = getItemStat("Reagent", "StackSize");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Amount in stack: " + itemStat, itemStatDisplay, 9, Color.White));

                switch (Convert.ToInt32(itemName.Substring(8, 1)))
                {
                    case 1:
                        itemStat = "Rerolls all magic effects when used on an item. It will consume all socketed gems to guarantee a magic effect whose power depends on the gems' level, and whose type is chosen randomly among all the types of gems on the item.";
                        break;
                    case 2:
                        itemStat = "Removes a random magic effect and adds another when used on an item. It will consume all socketed gems to guarantee a new magic effect whose power depends on the gems' level, and whose type is chosen randomly among all the types of gems on the item.";
                        break;
                    case 3:
                        itemStat = "Adds a new random magic effect when used on an item. It will consume all socketed gems to guarantee a new magic effect whose power depends on the gems' level, and whose type is chosen randomly among all the types of gems on the item.";
                        break;
                    case 4:
                        itemStat = "Removes a random magic effect when used on an item. It will consume all socketed gems to guarantee keeping a magic effect whose type is chosen randomly among all the types of gems on the item. Greater gems power allow the keeping of higher level magic effects.";
                        break;
                }
                itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, itemStat, itemStatDisplay, 9, Color.White));
                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                if (itemName.Contains("Legendary")) itemStat = "This item can only be applied on legendary items.";
                else itemStat = "This item cannot be applied on legendary and unique items.";
                itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, itemStat, itemStatDisplay, 9, Color.White));
                return;
            }

            itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

            itemStat = getItemStat("Gem", "StackSize");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Amount in stack: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat("Gem", "Name");
            if (itemStat != null)
            {
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

                foreach (int[] d in GemOrder)
                {
                    foreach (int i in d)
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
                return;
            }
            
            List<Socket> Sockets = getSockets();
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
            itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));
            
            List<Effect> magicEffects = getItemMagicEffect("RolledAffixes");
            if (magicEffects != null)
            {
                foreach (Effect effect in magicEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                    if (s_Effect.Contains("%1") || s_Effect.Contains("%2"))
                    {
                        if (!effect.Parameters[0].semantic.Contains("TriggeredSkillSelector"))
                        {
                            if (effect.EffectId.Contains("percent")
                                || effect.Parameters[0].semantic.Contains("ChanceFlatFloat"))
                            {
                                s_Effect = s_Effect.Replace("%1", "%1%");
                            }

                            if (effect.Parameters.Count <= 0)
                            {
                                LogMe.WriteLog("Error: effectID(" + effect.EffectId + "), effectName(" + effect.EffectName + ") missing semantics!");
                                return;
                            }

                            s_Effect = s_Effect.Replace("%1", "+" + effect.Parameters[0].value.ToString());
                            if (s_Effect.Contains("%2") && effect.Parameters.Count > 1)
                            {
                                s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                            }
                        }
                        else
                        {
                            if (effect.EffectId.Contains("percent")
                                || effect.Parameters[1].semantic.Contains("ChanceFlatFloat")
                                || effect.Parameters[1].semantic.Contains("PossibilityInt"))
                            {
                                s_Effect = s_Effect.Replace("%1", "%1%");
                            }

                            if (effect.Parameters.Count <= 0)
                            {
                                LogMe.WriteLog("Error: effectID(" + effect.EffectId + "), effectName(" + effect.EffectName + ") missing semantics!");
                                return;
                            }

                            s_Effect = s_Effect.Replace("%1", "+" + effect.Parameters[1].value.ToString());
                        }
                    }
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Effect, itemStatDisplay, 9, Color.White));
                }
            }
        }

        private static void UnloadItemData(Panel itemStatDisplay)
        {
            posY = 0;
            itemStatDisplay.Controls.Clear();
        }

        private static void setGridItem(object type, string select, string pbName)
        {
            int i = 0;
            if (select == "InventoryBelt")
            {
                foreach (var iGrid in (GetPropertyValue(type, select) as IList<InventoryBelt>))
                {
                    if (iGrid.BeltSlot == 0 && pbName == "charBelt1") iBeltItem = iGrid;
                    else if (iGrid.BeltSlot == 1 && pbName == "charBelt2") iBeltItem = iGrid;
                }
            } else if(select == "InventoryEquipped"){
                foreach (var iGrid in (GetPropertyValue(type, select) as IList<InventoryEquipped>))
                {
                    if (iGrid.BodyPart == coords.BodyPart)
                    {
                        iEquippedItem = iGrid;
                    }
                }
            } else {
                foreach (var iGrid in (GetPropertyValue(type, select) as IList<InventoryGrid>))
                {
                    if (iGrid.InventoryX == coords.x && iGrid.InventoryY == coords.y)
                    {
                        iGridItem = ObjectExtensions.Copy(iGrid);
                        globalItemIndex = i;
                        return;
                    }
                    i++;
                }
                iGridItem = null;
            }
        }

        private static Color getItemColorRarity()
        {
            Color valueReturn = Color.White;
            if (iGridItem != null && coords.BodyPart == -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        valueReturn = ColorTranslator.FromHtml(WolcenStaticData.rarityColorBank[item.Rarity]);
                    }
                }
            }
            else if (iEquippedItem != null && coords.BodyPart != -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryEquipped)
                {
                    if (item.BodyPart == coords.BodyPart)
                    {
                        valueReturn = ColorTranslator.FromHtml(WolcenStaticData.rarityColorBank[item.Rarity]);
                    }
                }
            }
            else if (iGridItem != null && coords.BodyPart == -1 && coords.panelID != -1)
            {
                foreach (var item in cData.PlayerChest.Panels[coords.panelID].InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        valueReturn = ColorTranslator.FromHtml(WolcenStaticData.rarityColorBank[item.Rarity]);
                    }
                }
            }

            return valueReturn;
        }

        private static int posX = -1;

        private static Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
        {
            Label lb = new Label();
            lb.Name = "s_lbl" + name;
            lb.Text = text;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, fontSize, FontStyle.Regular);
            lb.ForeColor = fontColor;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Width = panel.Width - 10;
            SizeF textSize = new SizeF();
            using (Bitmap bmp = new Bitmap(panel.Width, panel.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    textSize = g.MeasureString(text, lb.Font, lb.Width);
                }
            }
            lb.Height = (int)textSize.Height + 1;
            if (lb.Height >= 25) lb.Height += 10;
            lb.MaximumSize = new Size(lb.Width, lb.Height);
            if (posX == -1) posX = (panel.Width / 2) - lb.Width / 2;
            lb.Location = new Point(posX, posY);
            posY += lb.Height;
            lb.Parent = panel;
            lb.AutoSize = false;
            return lb;
        }

        private static Label createLabelLineBreak(Panel panel)
        {
            Label lb = new Label();
            lb.Name = "s_lbl_LineBreak";
            lb.Text = "__________________________________________________";
            lb.AutoSize = false;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, 5, FontStyle.Regular);
            lb.ForeColor = Color.LightGray;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Parent = panel;
            lb.Width = panel.Width - 10;
            lb.Height = 10;

            if (posX == -1) posX = (panel.Width / 2) - lb.Width / 2;
            lb.Location = new Point(posX, posY);
            posY += lb.Height;
            return lb;
        }

        private static List<Socket> getSockets()
        {
            List<Socket> valueReturn = null;
            if (iGridItem != null && coords.BodyPart == -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (GetPropertyValue(item, "Sockets") != null)
                        {
                            return GetPropertyValue(item, "Sockets") as List<Socket>;
                        }
                    }
                }
            }
            else if (iEquippedItem != null && coords.BodyPart != -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryEquipped)
                {
                    if (item.BodyPart == coords.BodyPart)
                    {
                        if (GetPropertyValue(item, "Sockets") != null)
                        {
                            return GetPropertyValue(item, "Sockets") as List<Socket>;
                        }
                    }
                }
            }
            else if (iGridItem != null && coords.BodyPart == -1 && coords.panelID != -1)
            {
                foreach (var item in cData.PlayerChest.Panels[coords.panelID].InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (GetPropertyValue(item, "Sockets") != null)
                        {
                            return GetPropertyValue(item, "Sockets") as List<Socket>;
                        }
                    }
                }
            }

            return valueReturn;
        }

        private static List<Effect> getItemMagicEffect(string stat)
        {
            List<Effect> valueReturn = null;
            if (iGridItem != null && coords.BodyPart == -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (GetPropertyValue(item.MagicEffects, stat) != null)
                        {
                            valueReturn = GetPropertyValue(item.MagicEffects, stat) as List<Effect>;
                            break;
                        }
                    }
                }
            }
            else if (iEquippedItem != null && coords.BodyPart != -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryEquipped)
                {
                    if (item.BodyPart == coords.BodyPart)
                    {
                        if (GetPropertyValue(item.MagicEffects, stat) != null)
                        {
                            valueReturn = GetPropertyValue(item.MagicEffects, stat) as List<Effect>;
                            break;
                        }
                    }
                }
            }
            else if (iGridItem != null && coords.BodyPart == -1 && coords.panelID != -1)
            {
                foreach (var item in cData.PlayerChest.Panels[coords.panelID].InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (GetPropertyValue(item.MagicEffects, stat) != null)
                        {
                            valueReturn = GetPropertyValue(item.MagicEffects, stat) as List<Effect>;
                            break;
                        }
                    }
                }
            }

            return valueReturn;
        }

        private static string getItemStat(string type, string stat)
        {
            string valueReturn = null;
            if (iGridItem != null && coords.BodyPart == -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (iGridItem.ItemType == "Shield" && GetPropertyValue(item, stat) == null && type != "Gem")
                        {
                            type = "Weapon";
                            if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                            {
                                valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                break;
                            }
                            else valueReturn = null;
                        }
                        else
                        {
                            if (GetPropertyValue(item, stat) != null && stat != "Armor")
                            {
                                valueReturn = GetPropertyValue(item, stat).ToString();
                                break;
                            }
                            else
                            {
                                if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                                {
                                    valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (iEquippedItem != null && coords.BodyPart != -1 && coords.panelID == -1)
            {
                foreach (var item in cData.Character.InventoryEquipped)
                {
                    if (item.BodyPart == coords.BodyPart)
                    {
                        if (iEquippedItem.ItemType == "Shield" && GetPropertyValue(item, stat) == null && type != "Gem")
                        {
                            type = "Weapon";
                            if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                            {
                                valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                break;
                            }
                            else valueReturn = null;
                        }
                        else
                        {
                            if (GetPropertyValue(item, stat) != null && stat != "Armor")
                            {
                                valueReturn = GetPropertyValue(item, stat).ToString();
                                break;
                            }
                            else
                            {
                                if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                                {
                                    valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (iGridItem != null && coords.BodyPart == -1 && coords.panelID != -1)
            {
                foreach (var item in cData.PlayerChest.Panels[coords.panelID].InventoryGrid)
                {
                    if (iGridItem.InventoryX == item.InventoryX && iGridItem.InventoryY == item.InventoryY)
                    {
                        if (iGridItem.ItemType == "Shield" && GetPropertyValue(item, stat) == null && type != "Gem")
                        {
                            type = "Weapon";
                            if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                            {
                                valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                break;
                            }
                            else valueReturn = null;
                        }
                        else
                        {
                            if (GetPropertyValue(item, stat) != null && stat != "Armor")
                            {
                                valueReturn = GetPropertyValue(item, stat).ToString();
                                break;
                            }
                            else
                            {
                                if (GetPropertyValue(GetPropertyValue(item, type), stat) != null)
                                {
                                    valueReturn = GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (type == "Gem") return null;
                foreach (var item in cData.Character.InventoryBelt)
                {
                    if (iBeltItem == null) return null;
                    if (item.BeltSlot == iBeltItem.BeltSlot)
                    {
                        if (GetPropertyValue(item, "Potion") != null)
                        {
                            if (GetPropertyValue(GetPropertyValue(item, "Potion"), stat) != null)
                            {
                                valueReturn = GetPropertyValue(GetPropertyValue(item, "Potion"), stat).ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return valueReturn;
        }

        public static string getGemStats(string gemName, int gemEffect)
        {
            foreach (var gem in WolcenStaticData.GemAffixesWithValues)
            {
                if (gem.Key == gemName)
                {
                    if (gemName.Contains("physical_Gem") && gemEffect == 8) gemEffect = 6;
                    string returnStr = WolcenStaticData.MagicLocalized[gem.Value.ElementAt(gemEffect).Key];
                    if (gem.Value.ElementAt(gemEffect).Key.Contains("percent")) returnStr = returnStr.Replace("%1", "%1%");
                    returnStr = "+" + returnStr.Replace("%1", gem.Value.ElementAt(gemEffect).Value);
                    if (returnStr.Contains("%2")) returnStr = returnStr.Replace("%2", gem.Value.ElementAt(gemEffect).Value);
                    return returnStr;
                }
            }
            return "[NOT FOUND]";
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            if (obj.GetType() == null) return null;
            var objType = obj.GetType();
            var prop = objType.GetProperty(propertyName);

            if (prop == null) return null;
            else return prop.GetValue(obj, null);
        }

        public static string ParseItemNameForType(string name)
        {
            if (name.ToLower().Contains("speical")) return "Gem";
            if (name.ToLower().Contains("1h"))
            {
                if (name.ToLower().Contains("sword")) return "Sword1H";
                if (name.ToLower().Contains("axe")) return "Axe1H";
                if (name.ToLower().Contains("mace") || name.ToLower().Contains("hammer")) return "Mace1H";
            }
            if (name.ToLower().Contains("2h"))
            {
                if (name.ToLower().Contains("sword")) return "Sword2H";
                if (name.ToLower().Contains("mace") || name.ToLower().Contains("hammer")) return "Mace2H";
                if (name.ToLower().Contains("axe")) return "Axe2H";
            }
            if (name.ToLower().Contains("bow")) return "Bow";
            if (name.ToLower().Contains("amulet") || name.ToLower().Contains("unique_glass_canon")) return "Amulet";
            if (name.ToLower().Contains("helmet")) return "Helmet";
            if (name.ToLower().Contains("chest") || name.ToLower().Contains("torso")) return "Chest Armor";
            if (name.ToLower().Contains("boots")) return "Foot Armor";
            if (name.ToLower().Contains("pants")) return "Leg Armor";
            if (name.ToLower().Contains("pauldron") || name.ToLower().Contains("shoulder")) return "Shoulder";
            if (name.ToLower().Contains("glove")) return "Arm Armor";
            if (name.ToLower().Contains("belt") || name.ToLower().Contains("sash") || name.ToLower().Contains("waistband")) return "Belt";
            if (name.ToLower().Contains("ring")) return "Ring";
            if (name.ToLower().Contains("catalyst") || name.ToLower().Contains("1h_offhand")) return "Trinket";
            if (name.ToLower().Contains("shield")) return "Shield";
            if (name.ToLower().Contains("staff")) return "Staff";
            if (name.ToLower().Contains("dagger")) return "Dagger";
            if (name.ToLower().Contains("gun") || name.ToLower().Contains("pistol")) return "Gun";
            if (name.ToLower().Contains("gem")) return "Gem";
            if (name.ToLower().Contains("potion")) return "Potions";
            if (name.ToLower().Contains("unique_fire_sword")
                || name.ToLower().Contains("unique_vanity_sword")
                || name.ToLower().Contains("unique_envy_sword"))
                return "Sword1H";
            if (name.ToLower().Contains("unique_deliverance_axe")) return "Axe2H";
            if (name.ToLower().Contains("unique_cc_mace")) return "Mace1H";
            if (name.ToLower().Contains("unique_stone_sword")) return "Sword2H";
            if (name.ToLower().Contains("unique_lightning_sword")) return "Sword2H";
            if (name.ToLower().Contains("unique_mace_sleeper")) return "Mace2H";
            return null;
        }
    }
}
