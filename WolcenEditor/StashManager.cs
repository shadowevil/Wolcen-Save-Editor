using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class StashManager
    {
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
                if (_panel.Locked == true)
                {
                    Button isLocked = new Button()
                    {
                        Name = "isLocked|" + _panel.ID,
                        Text = "Click to unlock",
                        AutoSize = false,
                        Size = new Size(100, 25),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Black,
                        Parent = stashPanelGrid,
                        Location = new Point((stashPanelGrid.Width / 2) - 50, (stashPanelGrid.Height / 2) - 12)
                    };
                    isLocked.Click += IsLocked_Click;
                }
                else
                {
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
            }
            LoadStashBitmaps(stashPanelGrid);
        }

        private static void IsLocked_Click(object sender, EventArgs e)
        {
            int panelID = Convert.ToInt32((sender as Button).Name.Split('|')[1]);

            cData.PlayerChest.Panels[panelID].Locked = false;
            cData.PlayerChest.Panels[panelID].InventoryGrid = new List<InventoryGrid>();
            LoadGrid((sender as Button).Parent as Panel);
        }

        private static void Pb_GiveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;

            if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                Bitmap bmp = new Bitmap(sourceBox.Image);
                Form1.SetCursor(bmp, sourceBox.Width / 2, 10);
                bmp.Dispose();
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
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                if (!(panel.Controls[i].Name.Contains("isLocked")))
                {
                    PictureBox pb = panel.Controls[i] as PictureBox;
                    int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                    int y = Convert.ToInt32(pb.Name.Split('|')[1]);
                    int panelid = Convert.ToInt32(pb.Name.Split('|')[2]);
                    if (dx == x && dy == y) pb.Image = GetStashBitmap(x, y, panelid, pb, getGridByLocation(x, y, panelid));
                }
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
                e.Effect = DragDropEffects.Move;
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
                    if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Move) == DragDropEffects.Move && isValid)
                    {
                        isValid = false;
                        return;
                    }
                    else ItemDataDisplay.LoadItemData(sender, (((sender as PictureBox).Parent as Panel).Parent as TabPage).Controls["itemStashStatDisplay"] as Panel, cData.PlayerChest.Panels, "InventoryGrid");
                }
            }
            else if(e.Button == MouseButtons.Right)
            {
                InventoryContextMenu.ShowContextMenu((sender as PictureBox), e.Location, cData.PlayerChest.Panels, "InventoryGrid");
            }
        }

        private static void LoadStashBitmaps(Panel stashPanelGrid)
        {
            foreach (var _panel in cData.PlayerChest.Panels)
            {
                if (_panel.ID == currentPanel && _panel.InventoryGrid != null)
                {
                    foreach (var iGrid in _panel.InventoryGrid)
                    {
                        for (int i = 0; i < stashPanelGrid.Controls.Count; i++)
                        {
                            PictureBox pb = stashPanelGrid.Controls[i] as PictureBox;
                            if (pb != null)
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
        }

        private static Image GetStashBitmap(int x, int y, int selectedPanel, PictureBox pb, InventoryGrid iGrid)
        {
            string dirPath = @".\UIResources\";
            string itemName = "";
            string l_itemName = null;
            string stackSize = null;
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
                if(iGrid.Gem.StackSize > 0) stackSize = iGrid.Gem.StackSize.ToString();
            }
            if (iGrid.Reagent != null)
            {
                itemName = iGrid.Reagent.Name;
                itemRarity = iGrid.Rarity;
                WolcenStaticData.ItemReagent.TryGetValue(itemName, out l_itemName);
                pb.MaximumSize = new Size(50, 50);
                pb.Size = new Size(50, 50);
                if(iGrid.Reagent.StackSize > 0) stackSize = iGrid.Reagent.StackSize.ToString();
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

                if (iGrid.MagicEffects.Default != null)
                {
                    foreach (var effect in iGrid.MagicEffects.Default)
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
                return CombineGridBitmaps(dirPath, l_itemName, itemRarity, iGrid.ItemType, pb, stackSize);
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

        private static Bitmap CombineGridBitmaps(string dirPath, string itemName, int quality, string itemType, PictureBox pb = null, string stackSize = null)
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
                int xOffset = 0;
                int yOffset = 0;

                switch (itemType)
                {
                    case "Shoulder":
                        width = 50; height = 55;
                        break;
                    case "Belt":
                        width = 40; height = 25;
                        break;
                    case "Amulet":
                    case "Ring":
                        width = 35; height = 30;
                        break;
                    case "Helmet":
                        width = 50; height = 60;
                        break;
                    case "Potions":
                        width = 20; height = 40;
                        break;
                    case "Leg Armor":
                        width = 50; height = 95;
                        break;
                    case "Foot Armor":
                        width = 45; height = 75;
                        break;
                    case "Chest Armor":
                        width = 45; height = 95;
                        break;
                }

                int x = Background.Width / 2 - (width / 2) + xOffset;
                int y = Background.Height / 2 - (height / 2) + yOffset;
                g.DrawImage(ItemImage, x, y, width, height);
                if (stackSize != null)
                {
                    g.DrawString(stackSize, Form1.DefaultFont, Brushes.White, 3, 3);
                }
            }

            Background.Dispose();
            ItemImage.Dispose();

            return FinalImage;
        }
    }
}
