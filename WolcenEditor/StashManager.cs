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
        private static int currentPanel = 0;
        private static PictureBox sourceBox;
        private static bool isValid = false;

        public static void LoadPlayerStash(object sender)
        {
            Panel stashPanelGrid = (sender as TabPage).Controls["stashPanelGrid"] as Panel;

            (stashPanelGrid.Parent.Controls["button1"] as Button).Click += StashManager_Click;
            (stashPanelGrid.Parent.Controls["button2"] as Button).Click += StashManager_Click;
            (stashPanelGrid.Parent.Controls["button3"] as Button).Click += StashManager_Click;
            (stashPanelGrid.Parent.Controls["button4"] as Button).Click += StashManager_Click;
            (stashPanelGrid.Parent.Controls["button5"] as Button).Click += StashManager_Click;


            LoadGrid(stashPanelGrid);
        }

        private static void LoadGrid(Panel stashPanelGrid)
        {
            stashPanelGrid.Controls.Clear();
            foreach (var _panel in cData.PlayerChest.Panels)
            {
                if (_panel.InventoryGrid == null)
                {
                    _panel.InventoryGrid = new List<InventoryGrid>();
                }
                if (currentPanel == _panel.ID)
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
                            pb.AllowDrop = true;
                            pb.MouseDown += Pb_MouseDown;
                            pb.MouseMove += Pb_MouseMove;
                            pb.MouseLeave += Pb_MouseLeave;
                            pb.DragEnter += Pb_DragEnter;
                            pb.DragDrop += Pb_DragDrop;
                            pb.GiveFeedback += Pb_GiveFeedBack;
                            //pb.ContextMenu = InventoryContextMenu.LoadContextMenu(pb);
                            stashPanelGrid.Controls.Add(pb);
                        }
                    }
                }
            }
            LoadStashBitmaps(stashPanelGrid);
        }

        private static void Pb_GiveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;

            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Bitmap _bmp = new Bitmap(sourceBox.Width, sourceBox.Height);
                sourceBox.DrawToBitmap(_bmp, new Rectangle(Point.Empty, _bmp.Size));
                _bmp.MakeTransparent(Color.White);
                Cursor cur = new Cursor(_bmp.GetHicon());
                Cursor.Current = cur;
            }
            else
            {
                Cursor.Current = Cursors.NoMove2D;
            }
        }

        private static void Pb_DragDrop(object sender, DragEventArgs e)
        {
            Bitmap bmp = (e.Data.GetData(DataFormats.Bitmap) as Bitmap);
            PictureBox Destination = (sender as PictureBox);
            int dx = Convert.ToInt32(Destination.Name.Split('|')[0]);
            int dy = Convert.ToInt32(Destination.Name.Split('|')[1]);
            int dpanelID = Convert.ToInt32(Destination.Name.Split('|')[2]);
            int sx = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
            int sy = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
            int spanelID = Convert.ToInt32(sourceBox.Name.Split('|')[2]);

            if (Destination.Image != null) return;

            if (Destination.Name == sourceBox.Name)
            {
                int index = Destination.Parent.Controls.IndexOfKey(sourceBox.Name);
                (Destination.Parent.Controls[index] as PictureBox).Image = sourceBox.Image;
                isValid = false;
                return;
            }

            if (sourceBox.MaximumSize.Height == 100)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (dx == i && dy == 9)
                    {
                        isValid = false;
                        return;
                    }
                }
                if (Destination.Parent.Controls.ContainsKey(Convert.ToString(dx + "|" + (dy + 1) + "|" + dpanelID)))
                {
                    if ((Destination.Parent.Controls[Convert.ToString(dx + "|" + (dy + 1) + "|" + dpanelID)] as PictureBox).Image != null)
                    {
                        isValid = false;
                        return;
                    }
                }
            }

            isValid = true;
            Destination.Image = bmp;
            Destination.MaximumSize = sourceBox.MaximumSize;
            Destination.Size = sourceBox.Size;
            sourceBox.Image = null;
            sourceBox.Size = new Size(50, 50);
            sourceBox.MaximumSize = new Size(50, 50);
            ConfirmMove(dx, dy, dpanelID);
            ReloadGridBitmap(Destination.Parent as Panel, dx, dy, dpanelID);
        }

        public static void ReloadGridBitmap(Panel panel, int dx, int dy, int dpanelID)
        {
            foreach (PictureBox pb in panel.Controls)
            {
                int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                int y = Convert.ToInt32(pb.Name.Split('|')[1]);
                int panelid = Convert.ToInt32(pb.Name.Split('|')[2]);
                if (dx == x && dy == y) pb.Image = GetStashBitmap(x, y, panelid, pb, getGridByLocation(x, y, panelid));
            }
        }

        private static InventoryGrid getGridByLocation(int x, int y, int panelid)
        {
            foreach (var i in cData.PlayerChest.Panels[panelid].InventoryGrid)
            {
                if (i.InventoryX == x && i.InventoryY == y)
                {
                    return i;
                }
            }
            return null;
        }

        private static void ConfirmMove(int dx, int dy, int dpanelID)
        {
            int sx = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
            int sy = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
            int sPanelID = Convert.ToInt32(sourceBox.Name.Split('|')[2]);

            if (dpanelID != sPanelID) return;

            for (int p = 0; p < cData.PlayerChest.Panels.Count; p++)
            {
                if (cData.PlayerChest.Panels[p].ID == dpanelID)
                {
                    for (int i = 0; i < cData.PlayerChest.Panels[p].InventoryGrid.Count; i++)
                    {
                        if (cData.PlayerChest.Panels[p].InventoryGrid[i].InventoryX == sx &&
                            cData.PlayerChest.Panels[p].InventoryGrid[i].InventoryY == sy)
                        {
                            cData.PlayerChest.Panels[p].InventoryGrid[i].InventoryX = dx;
                            cData.PlayerChest.Panels[p].InventoryGrid[i].InventoryY = dy;
                        }
                    }
                }
            }
        }

        private static void Pb_MouseLeave(object sender, EventArgs e)
        {
            if (Form1.ActiveForm == null) return;
            Form1.ActiveForm.Cursor = Cursors.Arrow;
        }

        private static void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            if (Form1.ActiveForm == null) return;
            if (e.Button == MouseButtons.Left) return;
            if ((sender as PictureBox).Image != null)
            {
                Form1.ActiveForm.Cursor = Cursors.Hand;
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

        private static void StashManager_Click(object sender, EventArgs e)
        {
            currentPanel = Convert.ToInt32((sender as Button).Name.Substring(6, 1)) - 1;
            LoadGrid((sender as Button).Parent.Controls["stashPanelGrid"] as Panel);
        }

        private static void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if ((sender as PictureBox).Image != null)
                {
                    Bitmap bmp = new Bitmap((sender as PictureBox).Image);
                    sourceBox = (sender as PictureBox);
                    if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Copy) == DragDropEffects.Copy && isValid)
                    {
                        isValid = false;
                        return;
                    }
                    else LoadItemData(sender, e);
                }
            }
            else if(e.Button == MouseButtons.Right)
            {
                InventoryContextMenu.ShowContextMenu((sender as PictureBox), e.Location, cData.PlayerChest.Panels, "InventoryGrid");
            }
        }

        public static void LoadItemData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = (((sender as PictureBox).Parent as Panel).Parent as TabPage).Controls["itemStashStatDisplay"] as Panel;

            UnloadItemData(itemStatDisplay);
            PictureBox pictureBox = (sender as PictureBox);

            int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
            int panelID = Convert.ToInt32(pictureBox.Name.Split('|')[2]);

            string itemName = getItemNameFromGrid(x, y, panelID, "Armor");
            if (itemName == null) itemName = getItemNameFromGrid(x, y, panelID, "Weapon");
            if (itemName == null) itemName = getItemNameFromGrid(x, y, panelID, "Potion");
            if (itemName == null) itemName = getItemNameFromGrid(x, y, panelID, "Gem");
            if (itemName == null) return;

            string l_itemName = null;
            WolcenStaticData.ItemLocalizedNames.TryGetValue(itemName, out l_itemName);
            if (l_itemName == null) return;

            Color itemRarity = getItemGridColorRarity(x, y, panelID);

            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, l_itemName, itemStatDisplay, 13, itemRarity));
            string itemType = InventoryManager.ParseItemNameForType(itemName);

            string itemStat = null;
            
            itemStat = getItemStat(x, y, panelID, "Armor", "Health");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Armor", "Armor");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Force Shield: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Armor", "Resistance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "All Resistance: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat(x, y, panelID, "Weapon", "DamageMin");
            string itemStat2 = getItemStat(x, y, panelID, "Weapon", "DamageMax");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Weapon", "ResourceGeneration");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat(x, y, panelID, "Weapon", "ShieldResistance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resistance: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Weapon", "ShieldBlockChance");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Chance: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Weapon", "ShieldBlockEfficiency");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Efficiency: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat(x, y, panelID, "Potion", "Charge");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Charge: " + itemStat, itemStatDisplay, 9, Color.White));
            itemStat = getItemStat(x, y, panelID, "Potion", "ImmediateMana");
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
            itemStat = getItemStat(x, y, panelID, "Potion", "ImmediateHP");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            itemStat = getItemStat(x, y, panelID, "Potion", "ImmediateStamina");
            if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Stamina Generation: " + itemStat, itemStatDisplay, 9, Color.White));

            if (itemType != "Potions")
            {
                List<Effect> defaultEffects = getItemMagicEffect(x, y, panelID, "MagicEffects", "Default");
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
                itemStat = getItemStat(x, y, panelID, "Gem", "Name");
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
                List<Effect> magicEffects = getItemMagicEffect(x, y, panelID, "MagicEffects", "RolledAffixes");
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

        private static List<Effect> getItemMagicEffect(int x, int y, int panel, string type, string stat)
        {
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panel)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            return (GetPropertyValue(GetPropertyValue(item, type), stat) as List<Effect>);
                        }
                    }
                }
            }
            return null;
        }

        private static string getItemStat(int x, int y, int panel, string type, string stat)
        {
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (panel == _panel.ID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            if (GetPropertyValue(item, type) != null)
                            {
                                return GetPropertyValue(GetPropertyValue(item, type), stat).ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj.GetType() == null) return null;
            var objType = obj.GetType();
            var prop = objType.GetProperty(propertyName);

            return prop.GetValue(obj, null);
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

        private static Color getItemGridColorRarity(int x, int y, int panelID)
        {
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

        private static string getItemNameFromGrid(int x, int y, int panelID, string type)
        {
            foreach (Panels _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == panelID)
                {
                    foreach (var item in _panel.InventoryGrid)
                    {
                        if (item.InventoryX == x && item.InventoryY == y)
                        {
                            if (GetPropertyValue(item, type) != null)
                            {
                                return GetPropertyValue(GetPropertyValue(item, type), "Name").ToString();
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

        private static void LoadStashBitmaps(Panel stashPanelGrid)
        {
            foreach (var _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == currentPanel && _panel.InventoryGrid != null)
                {
                    foreach (var iGrid in _panel.InventoryGrid)
                    {
                        foreach (PictureBox pb in stashPanelGrid.Controls)
                        {
                            int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                            int y = Convert.ToInt32(pb.Name.Split('|')[1]);

                            if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                            {
                                pb.Image = GetStashBitmap(x, y, currentPanel, pb, iGrid);
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
            pb.Size = new Size(50, 50);
            pb.MaximumSize = new Size(50, 50);
            if (iGrid == null) return null;
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
