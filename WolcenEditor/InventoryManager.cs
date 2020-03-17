using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class InventoryManager
    {
        private static Size defaultGridSize = new Size(50, 50);
        private static PictureBox sourceBox;
        private static bool isValid = false;

        public static Dictionary<string, int> charMap = new Dictionary<string, int>
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

        public static Dictionary<string, int> equipMap = new Dictionary<string, int>
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

        public enum typeMap : int
        {
            Weapon = 3,
            Offhand = 3,
            Armor = 2,
            Accessory = 2,
            Gem = 6,
            Potion = 4
        }

        public static void LoadCharacterInventory(object sender)
        {
            //TestingSemantics();
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
                        pictureBox.Image = GetInventoryBitmap(charMap[pictureBox.Name], pictureBox);
                        //pictureBox.Click += LoadItemData;
                        pictureBox.AllowDrop = true;
                        pictureBox.MouseDown += Pb_MouseDown;
                        pictureBox.DragEnter += Pb_DragEnter;
                        pictureBox.DragDrop += Pb_DragDrop;
                        pictureBox.MouseMove += Pb_MouseMove;
                        pictureBox.MouseLeave += Pb_MouseLeave;
                        pictureBox.GiveFeedback += Pb_GiveFeedBack;
                        //pictureBox.ContextMenu = new ContextMenu();
                    }
                }
            }

            LoadBeltInventory((sender as TabPage).Controls["beltConfig"] as GroupBox);
            LoadRandomInventory((sender as TabPage).Controls["charRandomInv"] as Panel);
        }

        private static void Pb_GiveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;

            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Bitmap _bmp = new Bitmap(sourceBox.Width, sourceBox.Height);
                sourceBox.DrawToBitmap(_bmp, new Rectangle(Point.Empty, _bmp.Size));
                _bmp.MakeTransparent(Color.White);
                Cursor cur = Form1.CreateCursorNoResize(_bmp, sourceBox.Width / 2, 10);
                Cursor.Current = cur;
            }
            else
            {
                Cursor.Current = Cursors.NoMove2D;
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

        private static void LoadBeltInventory(GroupBox beltConfigBox)
        {
            PictureBox charBelt1 = beltConfigBox.Controls["charBelt1"] as PictureBox;
            PictureBox charBelt2 = beltConfigBox.Controls["charBelt2"] as PictureBox;
            charBelt1.MouseDown += Pb_MouseDown;
            charBelt1.DragEnter += Pb_DragEnter;
            charBelt1.DragDrop += Pb_DragDrop;
            charBelt1.MouseMove += Pb_MouseMove;
            charBelt1.MouseLeave += Pb_MouseLeave;
            charBelt1.AllowDrop = true;
            charBelt1.GiveFeedback += Pb_GiveFeedBack;
            charBelt2.MouseDown += Pb_MouseDown;
            charBelt2.DragEnter += Pb_DragEnter;
            charBelt2.DragDrop += Pb_DragDrop;
            charBelt2.MouseMove += Pb_MouseMove;
            charBelt2.MouseLeave += Pb_MouseLeave;
            charBelt2.AllowDrop = true;
            charBelt2.GiveFeedback += Pb_GiveFeedBack;

            if (cData.Character.BeltConfig[0].Locked == 1) LockBeltSlot(charBelt1);
            else UnlockBeltSlot(charBelt1);
            if (cData.Character.BeltConfig[1].Locked == 1) LockBeltSlot(charBelt2);
            else UnlockBeltSlot(charBelt2);

            LoadBeltInventoryBitmaps(beltConfigBox);
        }

        private static void LoadBeltInventoryBitmaps(GroupBox beltConfigBox)
        {
            foreach (InventoryBelt iv in cData.Character.InventoryBelt)
            {
                if (iv.BeltSlot == 0)
                {
                    (beltConfigBox.Controls["charBelt1"] as PictureBox).Image = GetInventoryBitmap(0, (beltConfigBox.Controls["charBelt1"] as PictureBox));
                }
                if (iv.BeltSlot == 1)
                {
                    (beltConfigBox.Controls["charBelt2"] as PictureBox).Image = GetInventoryBitmap(0, (beltConfigBox.Controls["charBelt2"] as PictureBox));
                }
            }
        }

        private static void LockBeltSlot(object sender)
        {
            (sender as PictureBox).BackgroundImage = new Bitmap((Image)Properties.Resources.c_beltSlot, 49, 49);
            (sender as PictureBox).BackgroundImage.Tag = "1";
        }

        private static void UnlockBeltSlot(object sender)
        {
            (sender as PictureBox).BackgroundImage = Properties.Resources.e_beltSlot;
            (sender as PictureBox).BackgroundImage.Tag = "0";
        }

        private static void LoadRandomInventory(object sender)
        {
            Panel charRandomInv = (sender as Panel);
            charRandomInv.Controls.Clear();

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
                    pb.MouseMove += Pb_MouseMove;
                    pb.MouseLeave += Pb_MouseLeave;
                    pb.DragEnter += Pb_DragEnter;
                    pb.DragDrop += Pb_DragDrop;
                    pb.GiveFeedback += Pb_GiveFeedBack;
                    pb.ContextMenu = new ContextMenu();
                    charRandomInv.Controls.Add(pb);
                }
            }

            LoadInventoryBitmaps(charRandomInv);
        }

        private static void LoadInventoryBitmaps(Panel charRandomInv)
        {
            IList<InventoryGrid> invGrid = cData.Character.InventoryGrid;
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

        private static void ReloadEquipBitmap(TabPage tabPage, PictureBox DestinationPb)
        {
            PictureBox pb = null;
            int index = tabPage.Controls.IndexOfKey(DestinationPb.Name);
            pb = tabPage.Controls[index] as PictureBox;

            pb.Image = GetInventoryBitmap(charMap[DestinationPb.Name], DestinationPb);
        }

        public static void ReloadInventoryBitmap(Panel charRandomInv, int dx, int dy)
        {
            IList<InventoryGrid> invGrid = cData.Character.InventoryGrid;
            foreach (PictureBox pb in (charRandomInv.Controls))
            {
                int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                int y = Convert.ToInt32(pb.Name.Split('|')[1]);

                if (dx == x && dy == y)
                {
                    pb.Image = GetInventoryBitmap(0, pb);
                }
            }
        }

        public static void ReloadInventoryBitmap(Panel charRandomInv, PictureBox DestinationPb)
        {
            IList<InventoryGrid> invGrid = cData.Character.InventoryGrid;
            foreach (PictureBox pb in (charRandomInv.Controls))
            {
                int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                int y = Convert.ToInt32(pb.Name.Split('|')[1]);
                int dx = Convert.ToInt32(DestinationPb.Name.Split('|')[0]);
                int dy = Convert.ToInt32(DestinationPb.Name.Split('|')[1]);

                if (dx == x && dy == y)
                {
                    pb.Image = GetInventoryBitmap(0, pb);
                }
            }
        }

        private static void Pb_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox Destination = (sender as PictureBox);

            if (Destination.Name == sourceBox.Name)
            {
                int index = Destination.Parent.Controls.IndexOfKey(sourceBox.Name);
                (Destination.Parent.Controls[index] as PictureBox).Image = sourceBox.Image;
                isValid = false;
                return;
            }
            if (Destination.Image != null)
            {
                isValid = false;
                return;
            }
            if (Destination.Name == "charBelt1" || Destination.Name == "charBelt2")
            {
                if (charMap.ContainsKey(sourceBox.Name)) return;
                int x = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
                foreach (var d in cData.Character.InventoryGrid)
                {
                    if (d.InventoryX == x && d.InventoryY == y)
                    {
                        if (d.Potion != null)
                        {
                            isValid = true;
                            ConfirmMove(Destination);
                            LoadBeltInventoryBitmaps(Destination.Parent as GroupBox);
                            return;
                        }
                        else
                        {
                            isValid = false;
                            return;
                        }
                    }
                }
            }
            if (!charMap.ContainsKey(Destination.Name))
            {
                int x = Convert.ToInt32(Destination.Name.Split('|')[0]);
                int y = Convert.ToInt32(Destination.Name.Split('|')[1]);
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
                    if (Destination.Parent.Controls.ContainsKey(Convert.ToString(x + "|" + (y + 1))))
                    {
                        if ((Destination.Parent.Controls[Convert.ToString(x + "|" + (y + 1))] as PictureBox).Image != null)
                        {
                            isValid = false;
                            return;
                        }
                    }
                }
            }
            Bitmap bmp = (e.Data.GetData(DataFormats.Bitmap) as Bitmap);
            if (charMap.ContainsKey(sourceBox.Name))
            {
                if (!charMap.ContainsKey(Destination.Name))
                {
                    if (sourceBox.Name == "charBelt" || sourceBox.Name == "charRRing" || sourceBox.Name == "charLRing" || sourceBox.Name == "charNeck")
                    {
                        Destination.Image = new Bitmap(bmp, 50, 50);
                        Destination.MaximumSize = new Size(50, 50);
                        Destination.Size = new Size(50, 50);
                    }
                    else
                    {
                        Destination.Image = new Bitmap(bmp, 50, 100);
                        Destination.MaximumSize = new Size(50, 100);
                        Destination.Size = new Size(50, 100);
                    }
                }
                else
                {
                    if (isDestinationOK(Destination) == false)
                    {
                        isValid = false;
                        return;
                    }
                }
            }
            else if (charMap.ContainsKey(Destination.Name))
            {
                if (isDestinationOK(Destination) == false) { isValid = false; return; }
                Destination.Image = bmp;
            }
            else
            {
                Destination.Image = bmp;
                Destination.MaximumSize = sourceBox.MaximumSize;
                Destination.Size = sourceBox.Size;
            }
            sourceBox.Image = null;
            isValid = true;
            if (charMap.ContainsKey(sourceBox.Name))
            {
                if (!charMap.ContainsKey(Destination.Name))
                {
                    ConfirmMoveToInventory(Destination);
                    ReloadInventoryBitmap((sender as PictureBox).Parent as Panel, Destination);
                    return;
                }
            }
            else
            {
                if (ConfirmMove(Destination))
                {
                    if (charMap.ContainsKey(Destination.Name))
                    {
                        ReloadEquipBitmap(Destination.Parent as TabPage, Destination);
                    }
                }
                return;
            }

            //if (charMap.ContainsKey(Destination.Name))
            //{
            //    ConfirmMove(Destination);
            //    Destination.Size = sourceBox.Size;
            //    Destination.MaximumSize = sourceBox.Size;
            //    ReloadEquipBitmap(Destination.Parent as TabPage, Destination);
            //}
        }

        private static bool isDestinationOK(PictureBox destination)
        {
            if (destination.Name == "charRWeapon" || destination.Name == "charLWeapon")
            {
                foreach (var item in cData.Character.InventoryEquipped)
                {
                    if (item.BodyPart == charMap["charLWeapon"])
                    {
                        if (ItemDataDisplay.ParseItemNameForType(item.Weapon.Name).Contains("2H")
                            || ItemDataDisplay.ParseItemNameForType(item.Weapon.Name).Contains("Bow")
                            || ItemDataDisplay.ParseItemNameForType(item.Weapon.Name).Contains("Staff"))
                        {
                            return false;
                        }
                    }
                }
            }

            string sourceItemType = null;
            if (charMap.ContainsKey(destination.Name) && charMap.ContainsKey(sourceBox.Name))
            {
                foreach (var equip in cData.Character.InventoryEquipped)
                {
                    if (charMap[sourceBox.Name] == equip.BodyPart)
                    {
                        sourceItemType = equip.ItemType;
                        break;
                    }
                }
            }
            else
            {
                foreach (var iGrid in cData.Character.InventoryGrid)
                {
                    int x = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
                    int y = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
                    if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                    {
                        sourceItemType = iGrid.ItemType;
                        break;
                    }
                }
            }

            if (sourceItemType == "Shoulder" || sourceItemType == "Arm Armor" || sourceItemType == "Ring"
                || sourceItemType == "Sword1H" || sourceItemType == "Axe1H" || sourceItemType == "Dagger"
                || sourceItemType == "Gun" || sourceItemType == "Mace1H")
            {
                if (charMap[destination.Name] == equipMap[sourceItemType] - 1)
                {
                    return true;
                }
            }
            if (charMap[destination.Name] == equipMap[sourceItemType])
            {
                return true;
            }
            return false;
        }

        private static void ConfirmMoveToInventory(PictureBox destination)
        {
            int x = Convert.ToInt32(destination.Name.Split('|')[0]);
            int y = Convert.ToInt32(destination.Name.Split('|')[1]);

            InventoryEquipped _tmp = new InventoryEquipped();
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                int bodyPart = charMap[sourceBox.Name];
                if (equip.BodyPart == bodyPart)
                {
                    _tmp = equip;
                    break;
                }
            }

            cData.Character.InventoryEquipped.Remove(_tmp);

            InventoryGrid _tmpNew = new InventoryGrid();
            _tmpNew.Armor = _tmp.Armor;
            _tmpNew.Weapon = _tmp.Weapon;
            _tmpNew.InventoryX = x;
            _tmpNew.InventoryY = y;
            _tmpNew.Rarity = _tmp.Rarity;
            _tmpNew.Quality = _tmp.Quality;
            _tmpNew.Type = _tmp.Type;
            _tmpNew.ItemType = _tmp.ItemType;
            _tmpNew.Value = _tmp.Value;
            _tmpNew.Level = _tmp.Level;
            _tmpNew.Sockets = _tmp.Sockets;
            _tmpNew.MagicEffects = _tmp.MagicEffects;

            cData.Character.InventoryGrid.Add(_tmpNew);
        }

        private static bool ConfirmMove(PictureBox pictureBox)
        {
            if (charMap.ContainsKey(pictureBox.Name))
            {
                int bodyPart = charMap[pictureBox.Name];
                InventoryGrid _tmp = new InventoryGrid();
                InventoryEquipped _tmpE = new InventoryEquipped();
                if (!charMap.ContainsKey(sourceBox.Name))
                {
                    int x = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
                    int y = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
                    foreach (var iGrid in cData.Character.InventoryGrid)
                    {
                        if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                        {
                            _tmp = iGrid;
                            break;
                        }
                    }

                    cData.Character.InventoryGrid.Remove(_tmp);

                    InventoryEquipped _tmpNew = new InventoryEquipped();
                    _tmpNew.BodyPart = bodyPart;
                    _tmpNew.Rarity = _tmp.Rarity;
                    _tmpNew.Quality = _tmp.Quality;
                    _tmpNew.Type = _tmp.Type;
                    _tmpNew.ItemType = _tmp.ItemType;
                    _tmpNew.Value = _tmp.Value;
                    _tmpNew.Level = _tmp.Level;
                    _tmpNew.Armor = _tmp.Armor;
                    _tmpNew.Weapon = _tmp.Weapon;
                    _tmpNew.Sockets = _tmp.Sockets;
                    _tmpNew.MagicEffects = _tmp.MagicEffects;

                    cData.Character.InventoryEquipped.Add(_tmpNew);
                }
                else
                {
                    foreach (var equip in cData.Character.InventoryEquipped)
                    {
                        int _oldBodyPart = charMap[sourceBox.Name];
                        if (equip.BodyPart == _oldBodyPart)
                        {
                            _tmpE = equip;
                            break;
                        }
                    }

                    cData.Character.InventoryEquipped.Remove(_tmpE);

                    InventoryEquipped _tmpNew = new InventoryEquipped();
                    _tmpNew.BodyPart = bodyPart;
                    _tmpNew.Rarity = _tmpE.Rarity;
                    _tmpNew.Quality = _tmpE.Quality;
                    _tmpNew.Type = _tmpE.Type;
                    _tmpNew.ItemType = _tmpE.ItemType;
                    _tmpNew.Value = _tmpE.Value;
                    _tmpNew.Level = _tmpE.Level;
                    _tmpNew.Armor = _tmpE.Armor;
                    _tmpNew.Weapon = _tmpE.Weapon;
                    _tmpNew.MagicEffects = _tmpE.MagicEffects;

                    cData.Character.InventoryEquipped.Add(_tmpNew);
                }
            }
            else if (pictureBox.Name == "charBelt1" || pictureBox.Name == "charBelt2")
            {
                InventoryGrid gridItem = null;
                int x = Convert.ToInt32(sourceBox.Name.Split('|')[0]);
                int y = Convert.ToInt32(sourceBox.Name.Split('|')[1]);
                foreach (var d in cData.Character.InventoryGrid)
                {
                    if (d.InventoryX == x && d.InventoryY == y)
                    {
                        gridItem = d;
                        break;
                    }
                }

                cData.Character.InventoryGrid.Remove(gridItem);

                InventoryBelt _newBeltItem = new InventoryBelt();
                _newBeltItem.Rarity = gridItem.Rarity;
                _newBeltItem.Quality = gridItem.Quality;
                _newBeltItem.Type = gridItem.Type;
                _newBeltItem.ItemType = gridItem.ItemType;
                _newBeltItem.Value = gridItem.Value;
                _newBeltItem.Level = gridItem.Level;
                _newBeltItem.Potion = gridItem.Potion;
                _newBeltItem.BeltSlot = pictureBox.Name == "charBelt1" ? 0 : 1;

                cData.Character.InventoryBelt.Add(_newBeltItem);

                ReloadInventoryBitmap((sourceBox.Parent as Panel), x, y);
            }
            else if (sourceBox.Name == "charBelt1" || sourceBox.Name == "charBelt2")
            {
                InventoryBelt beltItem = null;
                foreach (var d in cData.Character.InventoryBelt)
                {
                    if (d.BeltSlot == 0 && sourceBox.Name == "charBelt1")
                    {
                        beltItem = d;
                        break;
                    }
                    if (d.BeltSlot == 1 && sourceBox.Name == "charBelt2")
                    {
                        beltItem = d;
                        break;
                    }
                }

                cData.Character.InventoryBelt.Remove(beltItem);

                InventoryGrid _newInvItem = new InventoryGrid();
                _newInvItem.Rarity = beltItem.Rarity;
                _newInvItem.Quality = beltItem.Quality;
                _newInvItem.Type = beltItem.Type;
                _newInvItem.ItemType = beltItem.ItemType;
                _newInvItem.Value = beltItem.Value;
                _newInvItem.Level = beltItem.Level;
                _newInvItem.Potion = beltItem.Potion;
                _newInvItem.InventoryX = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                _newInvItem.InventoryY = Convert.ToInt32(pictureBox.Name.Split('|')[1]);

                cData.Character.InventoryGrid.Add(_newInvItem);
            }
            else
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
            return true;
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
            if (e.Button == MouseButtons.Left)
            {
                if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2")
                {
                    if ((sender as PictureBox).Image != null)
                    {
                        Bitmap bmp = new Bitmap((sender as PictureBox).Image);
                        sourceBox = (sender as PictureBox);
                        if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Copy) == DragDropEffects.Copy && isValid)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if ((sender as PictureBox).BackgroundImage.Tag == null) (sender as PictureBox).BackgroundImage.Tag = "0";
                        if (((sender as PictureBox).BackgroundImage.Tag as string) == "1") UnlockBeltSlot(sender);
                        else LockBeltSlot(sender);

                        if (((sender as PictureBox).BackgroundImage.Tag as string) == "1")   // Closed
                        {
                            if ((sender as PictureBox).Name == "charBelt1") cData.Character.BeltConfig[0].Locked = 1;
                            if ((sender as PictureBox).Name == "charBelt2") cData.Character.BeltConfig[1].Locked = 1;
                        }
                        else if (((sender as PictureBox).BackgroundImage.Tag as string) == "0")    // Open
                        {
                            if ((sender as PictureBox).Name == "charBelt1") cData.Character.BeltConfig[0].Locked = 0;
                            if ((sender as PictureBox).Name == "charBelt2") cData.Character.BeltConfig[1].Locked = 0;
                        }
                        return;
                    }
                    ItemDataDisplay.LoadItemData(sender,
                        ((((sender as PictureBox).Parent as GroupBox).Parent as TabPage).Controls["itemStatDisplay"] as Panel), cData.Character, "InventoryBelt");
                }
                else
                {
                    if ((sender as PictureBox).Image == null) return;
                    Bitmap bmp = new Bitmap((sender as PictureBox).Image);
                    sourceBox = (sender as PictureBox);

                    if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Copy) == DragDropEffects.Copy && isValid)
                    {
                        if (!charMap.ContainsKey((sender as PictureBox).Name))
                        {
                            (sender as PictureBox).Size = defaultGridSize;
                        }
                    }
                    if (charMap.ContainsKey((sender as PictureBox).Name)) ItemDataDisplay.LoadItemData(sender, (sender as PictureBox).Parent.Controls["itemStatDisplay"] as Panel, cData.Character, "InventoryEquipped");
                    else ItemDataDisplay.LoadItemData(sender, (sender as PictureBox).Parent.Parent.Controls["itemStatDisplay"] as Panel, cData.Character, "InventoryGrid");
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2") return;
                if (charMap.ContainsKey((sender as PictureBox).Name)) return;
                //if ((sender as PictureBox).Image == null && charMap.ContainsKey((sender as PictureBox).Name)) return;
                InventoryContextMenu.ShowContextMenu((sender as PictureBox), e.Location, cData.Character, "InventoryGrid");
            }
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

        //private static Bitmap GetInventoryEquippedBitmap(int bodyPart, bool flip = false)
        private static Bitmap GetInventoryBitmap(int bodyPart = 0, PictureBox pb = null)
        {
            if (pb.Name == "charBelt1")
            {
                string dirPath = @".\UIResources\";
                foreach (InventoryBelt iv in cData.Character.InventoryBelt)
                {
                    if (iv.Potion != null && iv.BeltSlot == 0)
                    {
                        string[] pName = iv.Potion.Name.Split('_');
                        int itemRarity = iv.Rarity;
                        string l_itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";

                        if (File.Exists(dirPath + "Items\\" + l_itemName))
                        {
                            return CombineGridBitmaps(dirPath, l_itemName, itemRarity, iv.ItemType, pb);
                        }
                    }
                }
            }
            else if (pb.Name == "charBelt2")
            {
                string dirPath = @".\UIResources\";
                foreach (InventoryBelt iv in cData.Character.InventoryBelt)
                {
                    if (iv.Potion != null && iv.BeltSlot == 1)
                    {
                        string[] pName = iv.Potion.Name.Split('_');
                        int itemRarity = iv.Rarity;
                        string l_itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";

                        if (File.Exists(dirPath + "Items\\" + l_itemName))
                        {
                            return CombineGridBitmaps(dirPath, l_itemName, itemRarity, iv.ItemType, pb);
                        }
                    }
                }
            }
            else if (bodyPart == 0)
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
                                pb.Size = new Size(50, 50);
                                pb.MaximumSize = new Size(50, 50);
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

                        if (i.MagicEffects != null)
                        {
                            if (i.MagicEffects.RolledAffixes != null)
                            {
                                foreach (var effect in i.MagicEffects.RolledAffixes)
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
                            else if (i.MagicEffects.Default != null)
                            {
                                foreach (var effect in i.MagicEffects.Default)
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
                    string dirPath = @".\UIResources\";
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
            if (!charMap.ContainsKey(pb.Name))
            {
                pb.Size = new Size(50, 50);
                pb.MaximumSize = new Size(50, 50);
            }
            return null;
        }

        public static Image getImageFromPath(string v, Size destionationSize, int itemWidth, int itemHeight)
        {
            Bitmap finalImage = new Bitmap(destionationSize.Width, destionationSize.Height);
            Bitmap itemImage = new Bitmap(Image.FromFile(v), itemWidth, itemHeight);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(itemImage, (finalImage.Width / 2) - (itemImage.Width / 2), (finalImage.Height / 2) - (itemImage.Height / 2), itemWidth, itemHeight);
            }
            return finalImage;
        }
    }
}
