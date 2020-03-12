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
    public static class InventoryManager
    {

        private static Size defaultGridSize = new Size(50, 50);
        private static PictureBox sourceBox;
        private static bool isValid = false;
        private static Form createItemForm;
        private static Form editItemForm;
        private static int posY = 0;
        private static ContextMenu accessableContextMenu = null;
        private static int ItemQuality = 1;
        private static InventoryGrid CopiedItem = null;

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
                Cursor cur = new Cursor(_bmp.GetHicon());
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

        //private static void TestingSemantics()
        //{
        //    foreach (var d in WolcenStaticData.Testing)
        //    {
        //        string[] someString = null;
        //        WolcenStaticData.Semantics.TryGetValue(d.Value, out someString);
        //        if (someString == null)
        //            LogMe.WriteLog(d.Key + "[" + d.Value + "] - \"" + WolcenStaticData.MagicLocalized[d.Value] + "\"");
        //    }
        //}

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

        private static void ReloadInventoryBitmap(Panel charRandomInv, int dx, int dy)
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

        private static void ReloadInventoryBitmap(Panel charRandomInv, PictureBox DestinationPb)
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
                ConfirmMove(Destination);
                if (charMap.ContainsKey(Destination.Name))
                {
                    ReloadEquipBitmap(Destination.Parent as TabPage, Destination);
                }
                return;
            }

            if (charMap.ContainsKey(Destination.Name))
            {
                ConfirmMove(Destination);
                Destination.Size = sourceBox.Size;
                Destination.MaximumSize = sourceBox.Size;
                ReloadEquipBitmap(Destination.Parent as TabPage, Destination);
            }
        }

        private static bool isDestinationOK(PictureBox destination)
        {
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

        private static void ConfirmMove(PictureBox pictureBox)
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
                    LoadItemGridData(sender, e);
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
                    if (!charMap.ContainsKey((sender as PictureBox).Name)) LoadItemGridData(sender, e);
                    else LoadItemData(sender, e);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2") return;
                if (charMap.ContainsKey((sender as PictureBox).Name)) return;
                if ((sender as PictureBox).Image != null)
                {
                    LoadInventoryContextMenu((sender as PictureBox).ContextMenu, true);
                }
                else LoadInventoryContextMenu((sender as PictureBox).ContextMenu);
                (sender as PictureBox).ContextMenu.Show((sender as PictureBox), e.Location);
            }
        }
        
        private static void LoadInventoryContextMenu(ContextMenu contextMenu, bool _editItem = false)
        {
            if(contextMenu.MenuItems.Count > 0) contextMenu.MenuItems.Clear();
            if (_editItem == false)
            {
                MenuItem createItem = new MenuItem();
                createItem.Text = "Create item";
                createItem.Name = "CreateItem";
                createItem.Click += CreateItem_Click;

                MenuItem pasteItem = new MenuItem()
                {
                    Text = "Paste",
                    Name = "PasteItem"
                };
                pasteItem.Click += PasteItem_Click;

                contextMenu.MenuItems.Add(pasteItem);
                contextMenu.MenuItems.Add(createItem);
            }
            else
            {
                MenuItem editItem = new MenuItem()
                {
                    Text = "Edit item",
                    Name = "EditItem"
                };

                MenuItem copyItem = new MenuItem()
                {
                    Text = "Copy",
                    Name = "CopyItem"
                };

                MenuItem deleteItem = new MenuItem()
                {
                    Text = "Delete item",
                    Name = "DeleteItem"
                };

                deleteItem.Click += DeleteItem_Click;
                copyItem.Click += CopyItem_Click;
                editItem.Click += EditItem_Click;
                contextMenu.MenuItems.Add(editItem);
                contextMenu.MenuItems.Add(copyItem);
                contextMenu.MenuItems.Add(deleteItem);
            }

            accessableContextMenu = contextMenu;
        }

        private static void CopyItem_Click(object sender, EventArgs e)
        {
            string selectedItemCoords = (accessableContextMenu.SourceControl as PictureBox).Name;
            int x = Convert.ToInt32(selectedItemCoords.Split('|')[0]);
            int y = Convert.ToInt32(selectedItemCoords.Split('|')[1]);

            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    //((sender as MenuItem).Parent as ContextMenu).MenuItems["PasteItem"].Enabled = true;
                    CopiedItem = iGrid;
                    return;
                }
            }
        }

        private static void PasteItem_Click(object sender, EventArgs e)
        {
            if (CopiedItem == null) return;
            string selectedItemCoords = (accessableContextMenu.SourceControl as PictureBox).Name;
            int x = Convert.ToInt32(selectedItemCoords.Split('|')[0]);
            int y = Convert.ToInt32(selectedItemCoords.Split('|')[1]);

            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    return;
                }
            }

            CopiedItem.InventoryX = x;
            CopiedItem.InventoryY = y;

            cData.Character.InventoryGrid.Add(CopiedItem);
            ReloadInventoryBitmap(((accessableContextMenu.SourceControl as PictureBox).Parent as Panel), x, y);
        }

        private static void EditItem_Click(object sender, EventArgs e)
        {
            if (editItemForm == null)
            {
                editItemForm = new Form()
                {
                    Width = 700,
                    Height = 400,
                    MaximumSize = new Size(700, 400),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    TopMost = false,
                    Text = "Edit items!",
                    BackgroundImage = WolcenEditor.Properties.Resources.bg,
                    BackgroundImageLayout = ImageLayout.Center
                };

                ListView itemsInInventoryGrid = new ListView()
                {
                    Name = "itemsGrid",
                    Size = new Size(200, 190),
                    Location = new Point(10, 10),
                    Visible = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Parent = editItemForm,
                    BackColor = ColorTranslator.FromHtml("#1d1d1d"),
                    ForeColor = Color.White,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = false,
                    Sorting = SortOrder.Ascending,
                    HideSelection = false
                };
                itemsInInventoryGrid.ItemSelectionChanged += ItemsInInventoryGrid_ItemSelectionChanged;
                itemsInInventoryGrid.Columns.Add("X", 20, HorizontalAlignment.Left);
                itemsInInventoryGrid.Columns.Add("Y", 20, HorizontalAlignment.Left);
                itemsInInventoryGrid.Columns.Add("Name", 205, HorizontalAlignment.Left);

                PictureBox displayItemView = new PictureBox()
                {
                    Name = "displayItemView",
                    Size = new Size(125, 150),
                    MaximumSize = new Size(125, 150),
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(10, itemsInInventoryGrid.Height + 15),
                    Visible = true,
                    Parent = editItemForm,
                    BackColor = Color.Transparent
                };
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, displayItemView, new object[] { true });

                TreeView statEditView = new TreeView()
                {
                    Name = "statEditView",
                    Size = new Size(250, 350),
                    MaximumSize = new Size(250, 345),
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(itemsInInventoryGrid.Width + 20, 10),
                    Visible = true,
                    Parent = editItemForm,
                    BackColor = ColorTranslator.FromHtml("#1d1d1d"),
                    ForeColor = Color.White,
                    HideSelection = false
                };

                statEditView.AfterSelect += StatEditView_AfterSelect;

                Button addSelectedStat = new Button()
                {
                    Name = "addSelectedStat",
                    Text = "Add Selected Affix",
                    Size = new Size(100, 25),
                    Location = new Point(475, 330),
                    Visible = true,
                    FlatStyle = FlatStyle.Standard,
                    Enabled = true,
                    Parent = editItemForm
                };
                addSelectedStat.Click += AddSelectedStat_Click;

                Button deleteAffix = new Button()
                {
                    Name = "deleteAffix",
                    Text = "Delete Selected Affix",
                    Size = new Size(100, 25),
                    Location = new Point(475 + 105, 330),
                    Visible = true,
                    FlatStyle = FlatStyle.Standard,
                    Enabled = false,
                    Parent = editItemForm
                };
                deleteAffix.Click += DeleteAffix_Click;
            }
            LoadItemsInInventoryGrid(editItemForm.Controls["itemsGrid"] as ListView);
            editItemForm.ShowDialog();
        }

        private static void DeleteAffix_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = ((sender as Button).Parent.Controls["statEditView"] as TreeView).SelectedNode;
            string effectName = selectedNode.Name;
            string effectId = selectedNode.ImageKey;
            
            ListViewItem selectedItem = ((sender as Button).Parent.Controls["itemsGrid"] as ListView).SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            string l_itemName = selectedItem.SubItems[2].Name;

            for (int i = 0; i < cData.Character.InventoryGrid.Count; i++)
            {
                if (cData.Character.InventoryGrid[i].InventoryX == x && cData.Character.InventoryGrid[i].InventoryY == y)
                {
                    if (selectedNode.FullPath.Contains("Default Affixes"))
                    {
                        for (int s = 0; s < cData.Character.InventoryGrid[i].MagicEffects.Default.Count; s++)
                        {
                            if (cData.Character.InventoryGrid[i].MagicEffects.Default[s].EffectId == effectId
                                && cData.Character.InventoryGrid[i].MagicEffects.Default[s].EffectName == effectName)
                            {
                                cData.Character.InventoryGrid[i].MagicEffects.Default.RemoveAt(s);
                            }
                        }
                    }
                    else
                    {
                        for (int s = 0; s < cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes.Count; s++)
                        {
                            if (cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].EffectId == effectId
                                && cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].EffectName == effectName)
                            {
                                cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes.RemoveAt(s);
                            }
                        }
                    }
                }
            }

            RemoveItemEditControls();

            (editItemForm.Controls["deleteAffix"] as Button).Enabled = false;
            LoadCurrentAffixes(((sender as Button).Parent.Controls["statEditView"] as TreeView).Nodes["CurrentAffixes"]);
            ReloadInventoryBitmap(((accessableContextMenu.SourceControl as PictureBox).Parent as Panel), x, y);
            LoadItemGridData((accessableContextMenu.SourceControl as PictureBox), null);
        }

        private static void RemoveItemEditControls()
        {
            for (int i = 0; i < 6; i++)
            {
                editItemForm.Controls.RemoveByKey("star" + i.ToString());
                editItemForm.Controls.RemoveByKey("socketGroup" + i.ToString());
                editItemForm.Controls.RemoveByKey("tickAmount");
                editItemForm.Controls.RemoveByKey("socketAmount");
                editItemForm.Controls.RemoveByKey("cboRarity");
                editItemForm.Controls.RemoveByKey("defaultAffix");
                editItemForm.Controls.RemoveByKey("lblStat" + i.ToString());
                editItemForm.Controls.RemoveByKey("txtStat" + i.ToString());
            }
        }

        private static void StatEditView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = (sender as TreeView).SelectedNode;
            if (selectedNode.Nodes.Count > 0) return;
            string[] parameters = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] parameterValues = selectedNode.SelectedImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (!selectedNode.FullPath.Contains("Current Affixes"))
            {
                (editItemForm.Controls["addSelectedStat"] as Button).Text = "Add Selected Affix";
                if (parameterValues.Count() < 1)
                {
                    if (parameters.Count() == 2)
                    {
                        parameterValues = new string[] { "0", "0" };
                    }
                    else
                    {
                        parameterValues = new string[] { "0" };
                    }
                }
            }
            else
            {
                (editItemForm.Controls["deleteAffix"] as Button).Enabled = (selectedNode.FullPath.Contains("Default Affixes") || selectedNode.FullPath.Contains("Rolled Affixes") ? true : false);
                (editItemForm.Controls["addSelectedStat"] as Button).Text = "Update Selected Affix";
            }

            RemoveItemEditControls();

            CheckBox defaultAffix = new CheckBox()
            {
                Name = "defaultAffix",
                Text = "Default Affix?",
                AutoSize = true,
                Location = new Point(480, 80),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Parent = editItemForm,
                Visible = true
            };

            if (selectedNode.FullPath == "Current Affixes\\Rarity")
            {
                Label title = new Label()
                {
                    Name = "lblStat0",
                    Text = "Rarity of item:",
                    Location = new Point(480, 100),
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Parent = editItemForm,
                    Visible = true
                };
                ComboBox rarityBox = new ComboBox()
                {
                    Name = "cboRarity",
                    Location = new Point(480, 115),
                    Size = new Size(150, 20),
                    Parent = editItemForm,
                    Visible = true,
                    DisplayMember = "Key",
                    ValueMember = "Value",
                };
                foreach (var d in WolcenStaticData.Rarity)
                {
                    KeyValuePair<string, int> rarityKeys = new KeyValuePair<string, int>(d.Key, d.Value);
                    rarityBox.Items.Add(rarityKeys);
                }
                rarityBox.SelectedIndex = WolcenStaticData.Rarity.ElementAt(Convert.ToInt32(parameterValues[0])).Value;
            }
            else if (selectedNode.FullPath == "Current Affixes\\Number of Sockets")
            {
                Label title = new Label()
                {
                    Name = "lblStat0",
                    Text = "Number of Sockets:",
                    Location = new Point(480, 15),
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Parent = editItemForm,
                    Visible = true
                };

                ListViewItem selectedItem = ((sender as TreeView).Parent.Controls["itemsGrid"] as ListView).SelectedItems[0];
                int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
                int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
                TrackBar numberOfSockets = new TrackBar()
                {
                    Name = "socketAmount",
                    Location = new Point(480, 30),
                    Size = new Size(195, 35),
                    BackColor = ColorTranslator.FromHtml("#1d1d1d"),
                    Parent = editItemForm,
                    Visible = true,
                    Minimum = 0,
                };
                int maxSockets = 0;
                WolcenStaticData.MaxSocketsByType.TryGetValue(getItemTypeFromGrid(x, y), out maxSockets);
                numberOfSockets.Maximum = maxSockets;
                numberOfSockets.Value = Convert.ToInt32(parameterValues[0]);
                numberOfSockets.ValueChanged += NumberOfSockets_ValueChanged;

                Label tickAmount = new Label()
                {
                    Name = "tickAmount",
                    Text = numberOfSockets.Value.ToString(),
                    Location = new Point(numberOfSockets.Location.X + numberOfSockets.Width - 20, numberOfSockets.Location.Y + numberOfSockets.Height + 5),
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Parent = editItemForm,
                    Visible = true
                };

                NumberOfSockets_ValueChanged(sender, e);
            }
            else if (selectedNode.FullPath == "Current Affixes\\Quality")
            {
                Label label = new Label()
                {
                    Name = "lblStat0",
                    Text = "Quality of item:",
                    Location = new Point(480, 100),
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Parent = editItemForm,
                    Visible = true
                };

                for (int i = 1; i <= 5; i++)
                {
                    CheckBox stars = new CheckBox()
                    {
                        Name = "star" + i.ToString(),
                        Text = "",
                        Appearance = Appearance.Button,
                        BackColor = Color.Transparent,
                        AutoSize = false,
                        Size = new Size(40, 40),
                        BackgroundImage = new Bitmap(Image.FromFile(@".\UIResources\Inventory\quality_star_empty.png"), 40, 40),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        FlatStyle = FlatStyle.Flat,
                        Parent = editItemForm,
                        Location = new Point(430 + (i * 42), 115)
                    };
                    stars.FlatAppearance.BorderSize = 0;
                    stars.FlatAppearance.CheckedBackColor = Color.Transparent;
                    stars.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    stars.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    stars.Click += Stars_Click;
                }
                (editItemForm.Controls["star" + parameterValues[0]] as CheckBox).Checked = true;
                Stars_Click(editItemForm.Controls["star" + parameterValues[0]] as CheckBox, null);
            }
            else
            {
                for (int i = 0; i < parameterValues.Count(); i++)
                {
                    Label title = new Label()
                    {
                        Name = "lblStat" + i,
                        Text = "Stat value " + (i + 1) + ":",
                        Location = new Point(480, 100 + (50 * i)),
                        AutoSize = true,
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Parent = editItemForm,
                        Visible = true
                    };
                    TextBox valueBox = new TextBox();
                    valueBox.Name = "txtStat" + i.ToString();
                    valueBox.Parent = editItemForm;
                    valueBox.Location = new Point(480, 115 + (50 * i));
                    valueBox.Size = new Size(150, 20);
                    valueBox.Visible = true;
                    if (selectedNode.ImageKey != "default") valueBox.Text = parameterValues[i];
                    else valueBox.Text = parameterValues[0];
                    valueBox.KeyPress += numberOnly_KeyPress;
                }
            }

            if (selectedNode.FullPath.Contains("Current Affixes")) (editItemForm.Controls["defaultAffix"] as CheckBox).Visible = false;
            else (editItemForm.Controls["defaultAffix"] as CheckBox).Visible = true;
        }

        private static void Stars_Click(object sender, EventArgs e)
        {
            int checkAmount = Convert.ToInt32((sender as CheckBox).Name.Substring(4, 1));
            if ((sender as CheckBox).Checked == false)
            {
                for (int i = 5; i > checkAmount; i--)
                {
                    (editItemForm.Controls["star" + i.ToString()] as CheckBox).Checked = false;
                    starCheckChanged((editItemForm.Controls["star" + i.ToString()] as CheckBox), e);
                }
            }
            else
            {
                for (int i = 1; i <= checkAmount; i++)
                {
                    (editItemForm.Controls["star" + i.ToString()] as CheckBox).Checked = true;
                    starCheckChanged((editItemForm.Controls["star" + i.ToString()] as CheckBox), e);
                }
            }
            ItemQuality = checkAmount;
        }

        private static void starCheckChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked == true)
            {
                (sender as CheckBox).BackgroundImage = new Bitmap(Image.FromFile(@".\UIResources\Inventory\quality_star_full.png"), 40, 40);
            }
            else
            {
                (sender as CheckBox).BackgroundImage = new Bitmap(Image.FromFile(@".\UIResources\Inventory\quality_star_empty.png"), 40, 40);
            }
        }

        private static void NumberOfSockets_ValueChanged(object sender, EventArgs e)
        {
            TreeNode selectedNode = (editItemForm.Controls["statEditView"] as TreeView).SelectedNode;
            string[] socketEffect = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] GemName = { "" };
            if (selectedNode.Tag != null) GemName = (selectedNode.Tag as string).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            (editItemForm.Controls["tickAmount"] as Label).Text = (editItemForm.Controls["socketAmount"] as TrackBar).Value.ToString();
            int socketsAvailable = (editItemForm.Controls["socketAmount"] as TrackBar).Value;

            for (int i = 0; i < 4; i++)
            {
                editItemForm.Controls.RemoveByKey("socketGroup" + i.ToString());
            }

            for (int i = 0; i < socketsAvailable; i++)
            {
                GroupBox socketGroup = new GroupBox()
                {
                    Name = "socketGroup" + i.ToString(),
                    Text = "Socket " + (i + 1).ToString(),
                    Size = new Size(200, 75),
                    Parent = editItemForm,
                    Location = new Point(475, 90 + (i * 80)),
                    Visible = true,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White
                };
                
                PictureBox gemDisplay = new PictureBox()
                {
                    Name = "gemDisplay",
                    Size = new Size(50, 50),
                    Location = new Point(140, 15),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Parent = editItemForm.Controls["socketGroup" + i.ToString()],
                    BackgroundImageLayout = ImageLayout.Center
                };

                ComboBox cboSocket = new ComboBox()
                {
                    Name = "socketType",
                    Size = new Size(130, 15),
                    Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Regular),
                    Parent = editItemForm.Controls["socketGroup" + i.ToString()],
                    Location = new Point(5, 16),
                    Visible = true,
                    DisplayMember = "Key",
                    ValueMember = "Value",
                };
                cboSocket.SelectedIndexChanged += CboSocket_SelectedIndexChanged;
                foreach (var d in WolcenStaticData.SocketType)
                {
                    KeyValuePair<string, int> socketPair = new KeyValuePair<string, int>(d.Value, d.Key);
                    cboSocket.Items.Add(socketPair);
                }
                if (i <= socketEffect.Count())
                {
                    cboSocket.SelectedIndex = 0;
                    socketEffect = new string[i + 1];
                    for (int c = 0; c < socketEffect.Count(); c++)
                    {
                        socketEffect[c] = "0";
                    }
                }
                else cboSocket.SelectedIndex = WolcenStaticData.SocketType.ElementAt(Convert.ToInt32(socketEffect[i])).Key;

                ComboBox cboSocketed = new ComboBox()
                {
                    Name = "socketedGem",
                    Size = new Size(130, 15),
                    Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Regular),
                    Parent = editItemForm.Controls["socketGroup" + i.ToString()],
                    Location = new Point(5, 16 + cboSocket.Height + 5),
                    Visible = true,
                    DisplayMember = "Key",
                    ValueMember = "Value",
                };
                cboSocketed.SelectedIndexChanged += CboSocketed_SelectedIndexChanged;
                cboSocketed.Items.Add(new KeyValuePair<string, string>("[EMPTY]", "NULL"));

                foreach (var d in WolcenStaticData.GemAffixesWithValues)
                {
                    KeyValuePair<string, string> socketedGem = new KeyValuePair<string, string>(WolcenStaticData.ItemLocalizedNames[d.Key], d.Key);
                    cboSocketed.Items.Add(socketedGem);
                }
                if (i >= GemName.Count()) cboSocketed.SelectedIndex = 0;
                else if (GemName[i] != "") cboSocketed.SelectedIndex = cboSocketed.FindStringExact(WolcenStaticData.ItemLocalizedNames[GemName[i]]);
                else cboSocketed.SelectedIndex = 0;

                gemDisplay.BackgroundImage = new Bitmap(Image.FromFile(@".\UIResources\Inventory\" + WolcenStaticData.SocketImageLocation[Convert.ToInt32(socketEffect[i])]), 35, 35);
                if (i < GemName.Count())
                {
                    if (GemName[i] != "")
                    {
                        gemDisplay.Image = new Bitmap(Image.FromFile(@".\UIResources\Items\" + GemName[i] + ".png"), 50, 50);
                    }
                }
            }
        }

        private static void CboSocket_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            (comboBox.Parent.Controls["gemDisplay"] as PictureBox).BackgroundImage = new Bitmap(Image.FromFile(@".\UIResources\Inventory\" + WolcenStaticData.SocketImageLocation[((KeyValuePair<string, int>)comboBox.SelectedItem).Value]), 35, 35);
        }

        private static void CboSocketed_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            if (((KeyValuePair<string, string>)comboBox.SelectedItem).Value == "NULL") return;
            (comboBox.Parent.Controls["gemDisplay"] as PictureBox).Image = new Bitmap(Image.FromFile(@".\UIResources\Items\" + ((KeyValuePair<string, string>)comboBox.SelectedItem).Value + ".png"), 50, 50);
        }

        private static void numberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.') && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)
                || (e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }

        private static void AddSelectedStat_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = ((sender as Button).Parent.Controls["statEditView"] as TreeView).SelectedNode;
            string effectName = selectedNode.Name;
            string effectId = selectedNode.ImageKey;
            string[] semantics = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] semanticValues = selectedNode.SelectedImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            ListViewItem selectedItem = ((sender as Button).Parent.Controls["itemsGrid"] as ListView).SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            string l_itemName = selectedItem.SubItems[2].Name;

            string Mode = (sender as Button).Text.Contains("Update") ? "update" : "add";

            if (selectedNode.ImageKey != "default")
            {
                if (Mode == "update")
                {
                    for (int i = 0; i < cData.Character.InventoryGrid.Count; i++)
                    {
                        if (cData.Character.InventoryGrid[i].InventoryX == x && cData.Character.InventoryGrid[i].InventoryY == y)
                        {
                            if (selectedNode.FullPath.Contains("Default Affixes"))
                            {
                                for (int s = 0; s < cData.Character.InventoryGrid[i].MagicEffects.Default.Count; s++)
                                {
                                    if (cData.Character.InventoryGrid[i].MagicEffects.Default[s].EffectId == effectId
                                        && cData.Character.InventoryGrid[i].MagicEffects.Default[s].EffectName == effectName)
                                    {
                                        for (int d = 0; d < cData.Character.InventoryGrid[i].MagicEffects.Default[s].Parameters.Count; d++)
                                        {
                                            if (cData.Character.InventoryGrid[i].MagicEffects.Default[s].Parameters[d].semantic == semantics[d])
                                            {
                                                cData.Character.InventoryGrid[i].MagicEffects.Default[s].Parameters[d].value = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat" + d.ToString()] as TextBox).Text);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int s = 0; s < cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes.Count; s++)
                                {
                                    if (cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].EffectId == effectId
                                        && cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].EffectName == effectName)
                                    {
                                        for (int d = 0; d < cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters.Count; d++)
                                        {
                                            if (cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters[d].semantic == semantics[d])
                                            {
                                                cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters[d].value = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat" + d.ToString()] as TextBox).Text);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    RemoveItemEditControls();
                }
                else if (Mode == "add")
                {
                    string l_statName = WolcenStaticData.MagicLocalized[effectId];
                    //((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text
                    if (semantics.Count() == 2)
                    {
                        if (((sender as Button).Parent.Controls["txtStat1"] as TextBox).Text == "0")
                        {
                            MessageBox.Show("Value 2 cannot be 0", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        semanticValues = new string[] { ((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text, ((sender as Button).Parent.Controls["txtStat1"] as TextBox).Text };
                    }
                    else
                    {
                        if (((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text == "0")
                        {
                            MessageBox.Show("Value 1 cannot be 0", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        semanticValues = new string[] { ((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text };
                    }
                    InventoryGrid itemEditing = null;
                    InventoryGrid oldItem = null;

                    foreach (var iGrid in cData.Character.InventoryGrid)
                    {
                        if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                        {
                            oldItem = iGrid;
                            itemEditing = iGrid;
                            break;
                        }
                    }
                    if (itemEditing == null) return;

                    ItemMagicEffects magicEffects = itemEditing.MagicEffects;
                    Effect effect = new Effect() { EffectName = effectName, EffectId = effectId };

                    if (!selectedNode.FullPath.Contains("Current Affixes"))
                    {
                        if (magicEffects == null) magicEffects = new ItemMagicEffects();

                        if ((editItemForm.Controls["defaultAffix"] as CheckBox).Checked)
                        {
                            if (magicEffects.Default != null)
                            {
                                foreach (var t in magicEffects.Default)
                                {
                                    if (effect.EffectName == t.EffectName)
                                    {
                                        MessageBox.Show("Cannot have the same default affix on this item. Pleas eedit the current one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                magicEffects.Default = new List<Effect>();
                            }
                        }
                        else
                        {
                            if (magicEffects.RolledAffixes != null)
                            {
                                foreach (var t in magicEffects.RolledAffixes)
                                {
                                    if (effect.EffectName == t.EffectName)
                                    {
                                        MessageBox.Show("Cannot have the same affix on this item, please edit the current one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                magicEffects.RolledAffixes = new List<Effect>();
                            }
                        }
                    }

                    if (effect.Parameters == null) effect.Parameters = new List<EffectParams>();
                    for (int i = 0; i < semantics.Count(); i++)
                    {
                        EffectParams ep = new EffectParams();
                        ep.semantic = semantics[i];
                        ep.value = Convert.ToDouble(semanticValues[i]);
                        effect.Parameters.Add(ep);
                    }
                    if (selectedNode.FullPath.Contains("Current Affixes"))
                    {
                        if (selectedNode.FullPath.Contains("Default Affixes"))
                        {
                            for (int i = 0; i < itemEditing.MagicEffects.Default.Count(); i++)
                            {
                                if (itemEditing.MagicEffects.Default[i].EffectName == effect.EffectName)
                                {
                                    itemEditing.MagicEffects.Default.RemoveAt(i);
                                    break;
                                }
                            }
                            magicEffects.Default.Add(effect);
                        }
                        else
                        {
                            for (int i = 0; i < itemEditing.MagicEffects.RolledAffixes.Count(); i++)
                            {
                                if (itemEditing.MagicEffects.RolledAffixes[i].EffectName == effect.EffectName)
                                {
                                    itemEditing.MagicEffects.RolledAffixes.RemoveAt(i);
                                    break;
                                }
                            }
                            magicEffects.RolledAffixes.Add(effect);
                        }
                    }
                    else
                    {
                        if ((editItemForm.Controls["defaultAffix"] as CheckBox).Checked)
                        {
                            magicEffects.Default.Add(effect);
                        }
                        else
                        {
                            magicEffects.RolledAffixes.Add(effect);
                        }
                    }
                    itemEditing.MagicEffects = magicEffects;

                    cData.Character.InventoryGrid.Remove(oldItem);
                    cData.Character.InventoryGrid.Add(itemEditing);
                }
            }
            else
            {
                for (int i = 0; i < cData.Character.InventoryGrid.Count; i++)
                {
                    if (cData.Character.InventoryGrid[i].InventoryX == x && cData.Character.InventoryGrid[i].InventoryY == y)
                    {
                        if (selectedNode.Name == "Rarity") cData.Character.InventoryGrid[i].Rarity = ((sender as Button).Parent.Controls["cboRarity"] as ComboBox).SelectedIndex;
                        if (selectedNode.Name == "Quality") cData.Character.InventoryGrid[i].Quality = ItemQuality;
                        if (selectedNode.Name == "Sockets")
                        {
                            List<Socket> currentSockets = new List<Socket>();
                            int setNumSockets = (editItemForm.Controls["socketAmount"] as TrackBar).Value;
                            for (int s = 0; s < setNumSockets; s++)
                            {
                                Socket sock = new Socket();
                                sock.Effect = ((KeyValuePair<string, int>)((editItemForm.Controls["socketGroup" + s] as GroupBox).Controls["socketType"] as ComboBox).SelectedItem).Value;
                                if (((KeyValuePair<string, string>)((editItemForm.Controls["socketGroup" + s] as GroupBox).Controls["socketedGem"] as ComboBox).SelectedItem).Key != "[EMPTY]")
                                {
                                    sock.Gem = new Gem()
                                    {
                                        Name = ((KeyValuePair<string, string>)((editItemForm.Controls["socketGroup" + s] as GroupBox).Controls["socketedGem"] as ComboBox).SelectedItem).Value
                                    };
                                }
                                currentSockets.Add(sock);
                            }
                            cData.Character.InventoryGrid[i].Sockets = currentSockets;
                        }
                        if (selectedNode.Name == "Value") cData.Character.InventoryGrid[i].Value = ((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text;
                        if (selectedNode.Name == "Level") cData.Character.InventoryGrid[i].Level = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                        switch (cData.Character.InventoryGrid[i].Type)
                        {
                            case (int)typeMap.Weapon:
                                if (selectedNode.Name == "DamageMin") cData.Character.InventoryGrid[i].Weapon.DamageMin = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "DamageMax") cData.Character.InventoryGrid[i].Weapon.DamageMax = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ResourceGeneration") cData.Character.InventoryGrid[i].Weapon.ResourceGeneration = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                break;
                            case (int)typeMap.Armor:
                                if (selectedNode.Name == "Armor") cData.Character.InventoryGrid[i].Armor.Armor = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "Health") cData.Character.InventoryGrid[i].Armor.Health = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "Resistance") cData.Character.InventoryGrid[i].Armor.Resistance = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                break;
                            case (int)typeMap.Potion:
                                if (selectedNode.Name == "Charge") cData.Character.InventoryGrid[i].Potion.Charge = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateHP") cData.Character.InventoryGrid[i].Potion.ImmediateHP = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateMana") cData.Character.InventoryGrid[i].Potion.ImmediateMana = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateStamina") cData.Character.InventoryGrid[i].Potion.ImmediateStamina = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                break;
                        }
                    }
                }
            }
            if((editItemForm.Controls["deleteAffix"] as Button) != null) (editItemForm.Controls["deleteAffix"] as Button).Enabled = false;
            if((editItemForm.Controls["defaultAffix"] as CheckBox) != null) (editItemForm.Controls["defaultAffix"] as CheckBox).Checked = false;
            LoadCurrentAffixes(((sender as Button).Parent.Controls["statEditView"] as TreeView).Nodes["CurrentAffixes"]);
            ReloadInventoryBitmap(((accessableContextMenu.SourceControl as PictureBox).Parent as Panel), x, y);
            LoadItemGridData((accessableContextMenu.SourceControl as PictureBox), null);
            RemoveItemEditControls();
        }

        private static void ItemsInInventoryGrid_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            RemoveItemEditControls();
            ListView listView = (sender as ListView);
            if (listView.SelectedItems.Count <= 0) return;
            PictureBox displayBox = ((sender as ListView).Parent as Form).Controls["displayItemView"] as PictureBox;
            ListViewItem selectedItem = listView.SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            string l_itemName = selectedItem.SubItems[2].Name;
            string itemName = null;
            string dirPath = @".\UIResources\Items\";

            int itemWidth = 60;
            int itemHeight = 150;
            WolcenStaticData.ItemWeapon.TryGetValue(l_itemName, out itemName);
            if (itemName == null)
            {
                WolcenStaticData.ItemArmor.TryGetValue(l_itemName, out itemName);
                itemWidth = 75;
                itemHeight = 135;
            }
            if (itemName == null)
            {
                WolcenStaticData.ItemAccessories.TryGetValue(l_itemName, out itemName);
                itemWidth = 85;
                itemHeight = 85;
            }
            if (itemName == null)
            {
                // Try Potions
                if (l_itemName.ToLower().Contains("potion"))
                {
                    string[] pName = l_itemName.Split('_');
                    if (File.Exists(dirPath + pName[0] + "_" + pName[1] + "_" + pName[2] + ".png"))
                    {
                        itemWidth = 100;
                        itemHeight = 130;
                        itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";
                    }
                }

                // Try Gems
                if (l_itemName.ToLower().Contains("gem"))
                {
                    if (File.Exists(dirPath + l_itemName + ".png"))
                    {
                        itemWidth = 90;
                        itemHeight = 100;
                        itemName = l_itemName + ".png";
                    }
                }

                if (itemName == null) return;
            }

            LoadMagicProperties(((sender as ListView).Parent.Controls["statEditView"] as TreeView));

            LoadCurrentAffixes(((sender as ListView).Parent.Controls["statEditView"] as TreeView).Nodes["CurrentAffixes"]);

            displayBox.Image = getImageFromPath(dirPath + itemName, displayBox.Size, itemWidth, itemHeight);
        }

        private static void LoadMagicProperties(TreeView treeView)
        {
            if (treeView.Nodes.Count <= 0)
            {
                treeView.Nodes.Clear();

                TreeNode Implicit = new TreeNode() { Name = "AffixesImplicit", Text = "Implicit Affixes" };
                TreeNode MastersSlavesMagicWeapons = new TreeNode() { Name = "AffixesMastersSlavesMagicWeapons", Text = "Master Slave Magic Weapon Affixes" };
                TreeNode MastersSlavesPhysicalWeapons = new TreeNode() { Name = "AffixesMastersSlavesPhysicalWeapons", Text = "Master Slave Physical Weapon Affixes" };
                TreeNode Accessories = new TreeNode() { Name = "AffixesAccessories", Text = "Accessory Affixes" };
                TreeNode Armors = new TreeNode() { Name = "AffixesArmors", Text = "Armor Affixes" };
                TreeNode Uniques = new TreeNode() { Name = "AffixesUniques", Text = "Unique Affixes" };
                TreeNode UniquesMax = new TreeNode() { Name = "AffixesUniquesMax", Text = "Mid Tier Unique Affixes" };
                TreeNode UniquesMaxMax = new TreeNode() { Name = "AffixesUniquesMaxMax", Text = "End Game Affixes" };
                TreeNode Weapons = new TreeNode() { Name = "AffixesWeapons", Text = "Weapon Affixes" };

                TreeNode currentlyAdded = new TreeNode() { Name = "CurrentAffixes", Text = "Current Affixes" };

                treeView.Nodes.Add(currentlyAdded);
                treeView.Nodes.Add(Implicit);
                treeView.Nodes.Add(MastersSlavesMagicWeapons);
                treeView.Nodes.Add(MastersSlavesPhysicalWeapons);
                treeView.Nodes.Add(Accessories);
                treeView.Nodes.Add(Armors);
                treeView.Nodes.Add(Uniques);
                treeView.Nodes.Add(UniquesMax);
                treeView.Nodes.Add(UniquesMaxMax);
                treeView.Nodes.Add(Weapons);

                AddNodes(treeView.Nodes["AffixesImplicit"], WolcenStaticData.AffixesImplicit);
                AddNodes(treeView.Nodes["AffixesMastersSlavesMagicWeapons"], WolcenStaticData.AffixesMastersSlavesMagicWeapons);
                AddNodes(treeView.Nodes["AffixesMastersSlavesPhysicalWeapons"], WolcenStaticData.AffixesMastersSlavesPhysicalWeapons);
                AddNodes(treeView.Nodes["AffixesAccessories"], WolcenStaticData.AffixesAccessories);
                AddNodes(treeView.Nodes["AffixesArmors"], WolcenStaticData.AffixesArmors);
                AddNodes(treeView.Nodes["AffixesUniques"], WolcenStaticData.AffixesUniques);
                AddNodes(treeView.Nodes["AffixesUniquesMax"], WolcenStaticData.AffixesUniquesMax);
                AddNodes(treeView.Nodes["AffixesUniquesMaxMax"], WolcenStaticData.AffixesUniquesMaxMax);
                AddNodes(treeView.Nodes["AffixesWeapons"], WolcenStaticData.AffixesWeapons);
            }
        }

        private static void LoadCurrentAffixes(TreeNode treeNode)
        {
            if (treeNode == null) return;
            treeNode.Nodes.Clear();
            //int x = Convert.ToInt32((accessableContextMenu.SourceControl as PictureBox).Name.Split('|')[0]);
            //int y = Convert.ToInt32((accessableContextMenu.SourceControl as PictureBox).Name.Split('|')[1]);
            ListViewItem selectedItem = (treeNode.TreeView.Parent.Controls["itemsGrid"] as ListView).SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);

            TreeNode affixNode = new TreeNode()
            {
                Name = "RolledAffixes",
                Text = "Rolled Affixes"
            };

            TreeNode defaultNode = new TreeNode()
            {
                Name = "DefaultAffixes",
                Text = "Default Affixes"
            };

            // Weapons
            TreeNode damageMin = null;
            TreeNode damageMax = null;
            TreeNode ResourceGeneration = null;
            // Armor
            TreeNode Armor = null;
            TreeNode Health = null;
            TreeNode Resistance = null;
            // Potions
            TreeNode Charge = null;
            TreeNode ImmediateHP = null;
            TreeNode ImmediateMana = null;
            TreeNode ImmediateStamina = null;

            TreeNode Rarity = null;
            TreeNode Quality = null;
            TreeNode Sockets = null;
            TreeNode Value = null;
            TreeNode Level = null;

            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    Rarity = new TreeNode()
                    {
                        Name = "Rarity",
                        Text = "Rarity",
                        ImageKey = "default",
                        SelectedImageKey = iGrid.Rarity.ToString()
                    };
                    Quality = new TreeNode()
                    {
                        Name = "Quality",
                        Text = "Quality",
                        ImageKey = "default",
                        SelectedImageKey = iGrid.Quality.ToString()
                    };
                    string sockets = "0";
                    if (iGrid.Sockets != null) sockets = iGrid.Sockets.Count().ToString();
                    Sockets = new TreeNode()
                    {
                        Name = "Sockets",
                        Text = "Number of Sockets",
                        ImageKey = "default",
                        SelectedImageKey = sockets
                    };
                    if (iGrid.Sockets != null)
                    {
                        foreach (var sock in iGrid.Sockets)
                        {
                            Sockets.StateImageKey += sock.Effect + "|";
                            if (sock.Gem != null)
                            {
                                Sockets.Tag += sock.Gem.Name + "|";
                            }
                        }
                    }
                    Value = new TreeNode()
                    {
                        Name = "Value",
                        Text = "Value",
                        ImageKey = "default",
                        SelectedImageKey = iGrid.Value.ToString()
                    };
                    Level = new TreeNode()
                    {
                        Name = "Level",
                        Text = "Item Level",
                        ImageKey = "default",
                        SelectedImageKey = iGrid.Level.ToString()
                    };

                    treeNode.Nodes.Add(Rarity);
                    treeNode.Nodes.Add(Quality);
                    treeNode.Nodes.Add(Sockets);
                    treeNode.Nodes.Add(Value);
                    treeNode.Nodes.Add(Level);

                    if (iGrid.Type == (int)typeMap.Weapon)    // Weapons & offhands
                    {
                        damageMin = new TreeNode()
                        {
                            Name = "DamageMin",
                            Text = "Damage Min",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Weapon.DamageMin.ToString()
                        };
                        damageMax = new TreeNode()
                        {
                            Name = "DamageMax",
                            Text = "Damage Max",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Weapon.DamageMax.ToString()
                        };
                        ResourceGeneration = new TreeNode()
                        {
                            Name = "ResourceGeneration",
                            Text = "Resource Generation",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Weapon.ResourceGeneration.ToString()
                        };
                        treeNode.Nodes.Add(damageMin);
                        treeNode.Nodes.Add(damageMax);
                        treeNode.Nodes.Add(ResourceGeneration);
                    }

                    if (iGrid.Type == (int)typeMap.Armor)
                    {
                        Armor = new TreeNode()
                        {
                            Name = "Armor",
                            Text = "Armor",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Armor.Armor.ToString()
                        };
                        Health = new TreeNode()
                        {
                            Name = "Health",
                            Text = "Health",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Armor.Health.ToString()
                        };
                        Resistance = new TreeNode()
                        {
                            Name = "Resistance",
                            Text = "Resistance",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Armor.Resistance.ToString()
                        };
                        treeNode.Nodes.Add(Armor);
                        treeNode.Nodes.Add(Health);
                        treeNode.Nodes.Add(Resistance);
                    }

                    if (iGrid.Type == (int)typeMap.Potion)
                    {
                        Charge = new TreeNode()
                        {
                            Name = "Charge",
                            Text = "Charges",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Potion.Charge.ToString()
                        };
                        ImmediateHP = new TreeNode()
                        {
                            Name = "ImmediateHP",
                            Text = "Immediate Health",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Potion.ImmediateHP.ToString()
                        };
                        ImmediateMana = new TreeNode()
                        {
                            Name = "ImmediateMana",
                            Text = "Immediate Umbra",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Potion.ImmediateMana.ToString()
                        };
                        ImmediateStamina = new TreeNode()
                        {
                            Name = "ImmediateStamina",
                            Text = "Immediate Stamina",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Potion.ImmediateStamina.ToString()
                        };
                        treeNode.Nodes.Add(Charge);
                        treeNode.Nodes.Add(ImmediateHP);
                        treeNode.Nodes.Add(ImmediateMana);
                        treeNode.Nodes.Add(ImmediateStamina);
                    }

                    if (iGrid.MagicEffects != null)
                    {
                        if (iGrid.MagicEffects != null)
                        {
                            if (iGrid.MagicEffects.Default != null)
                            {
                                foreach (var de in iGrid.MagicEffects.Default)
                                {
                                    TreeNode node = new TreeNode();
                                    node.Name = de.EffectName;
                                    node.Text = WolcenStaticData.MagicLocalized[de.EffectId];
                                    node.ImageKey = de.EffectId;
                                    for (int i = 0; i < de.Parameters.Count(); i++)
                                    {
                                        node.StateImageKey += de.Parameters[i].semantic + "|";
                                        node.SelectedImageKey += de.Parameters[i].value.ToString() + "|";
                                    }
                                    defaultNode.Nodes.Add(node);
                                }
                            }

                            if(iGrid.MagicEffects.RolledAffixes != null)
                            {
                                foreach (var me in iGrid.MagicEffects.RolledAffixes)
                                {
                                    TreeNode node = new TreeNode();
                                    node.StateImageKey = "";
                                    node.SelectedImageKey = "";
                                    node.Name = me.EffectName;
                                    node.Text = WolcenStaticData.MagicLocalized[me.EffectId];
                                    node.ImageKey = me.EffectId;
                                    for (int i = 0; i < me.Parameters.Count(); i++)
                                    {
                                        node.StateImageKey += me.Parameters[i].semantic + "|";
                                        node.SelectedImageKey += me.Parameters[i].value.ToString() + "|";
                                    }
                                    affixNode.Nodes.Add(node);
                                }
                            }
                        }
                    }
                }
            }
            if (affixNode.Nodes.Count != 0) treeNode.Nodes.Add(affixNode);
            if (defaultNode.Nodes.Count != 0) treeNode.Nodes.Add(defaultNode);
        }

        private static void AddNodes(TreeNode treeNode, Dictionary<string, string> dict)
        {
            foreach (var d in dict)
            {
                TreeNode node = null;
                string key = d.Key;
                string value = d.Value;
                string lValue = null;
                string[] semantics;
                WolcenStaticData.MagicLocalized.TryGetValue(value, out lValue);
                WolcenStaticData.Semantics.TryGetValue(value, out semantics);
                if (lValue != null)
                {
                    node = new TreeNode();
                    node.Name = d.Key;
                    node.ImageKey = value;
                    node.Text = lValue;
                    if (semantics != null && semantics.Count() > 0)
                    {
                        for (int i = 0; i < semantics.Count(); i++)
                        {
                            node.StateImageKey += semantics[i] + "|";
                        }
                    }
                    else
                    {
                        LogMe.WriteLog("Error: null semantic find for " + key + "(" + value + ")");
                    }
                    treeNode.Nodes.Add(node);
                }
            }
        }

        private static void LoadItemsInInventoryGrid(ListView listView)
        {
            listView.Items.Clear();
            string selectedItemCoords = (accessableContextMenu.SourceControl as PictureBox).Name;
            int x = Convert.ToInt32(selectedItemCoords.Split('|')[0]);
            int y = Convert.ToInt32(selectedItemCoords.Split('|')[1]);
            foreach (var item in cData.Character.InventoryGrid)
            {
                ListViewItem i = null;
                switch (item.Type)
                {
                    case (int)typeMap.Weapon:       // Also Offhand
                        i = new ListViewItem();
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem() {
                            Name = item.Weapon.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Weapon.Name]
                        });
                        break;
                    case (int)typeMap.Armor:       // Also Accessories
                        i = new ListViewItem();
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem()
                        {
                            Name = item.Armor.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Armor.Name]
                        });
                        break;
                    case (int)typeMap.Gem:
                        //listView.Items.Add(new ListViewItem(new[] { item.InventoryX.ToString(), item.InventoryY.ToString(), WolcenStaticData.ItemLocalizedNames[item.Gem.Name] }));
                        //i.Text = item.InventoryX.ToString();
                        //i.SubItems.Add(item.InventoryY.ToString());
                        //i.SubItems.Add(new ListViewItem.ListViewSubItem()
                        //{
                        //    Name = item.Gem.Name,
                        //    Text = WolcenStaticData.ItemLocalizedNames[item.Gem.Name]
                        //});
                        break;
                    case (int)typeMap.Potion:
                        i = new ListViewItem();
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem()
                        {
                            Name = item.Potion.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Potion.Name]
                        });
                        break;
                }
                if (i != null)
                {
                    if (item.InventoryX == x && item.InventoryY == y) i.Selected = true;
                    listView.Items.Add(i);
                }
            }
        }

        private static void DeleteItem_Click(object sender, EventArgs e)
        {
            PictureBox pb = (((sender as MenuItem).Parent as ContextMenu).SourceControl as PictureBox);
            InventoryGrid itemToDelete = null;
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                int x = Convert.ToInt32(pb.Name.Split('|')[0]);
                int y = Convert.ToInt32(pb.Name.Split('|')[1]);
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    itemToDelete = iGrid;
                    break;
                }
            }
            if (itemToDelete != null)
            {
                cData.Character.InventoryGrid.Remove(itemToDelete);
                pb.Size = defaultGridSize;
                pb.MaximumSize = defaultGridSize;
                ReloadInventoryBitmap(pb.Parent as Panel, pb);
            }
        }

        private static void CreateItem_Click(object sender, EventArgs e)
        {
            if (createItemForm == null)
            {
                createItemForm = new Form();

                createItemForm.Width = 700;
                createItemForm.Height = 400;
                createItemForm.MaximumSize = createItemForm.Size;
                createItemForm.StartPosition = FormStartPosition.CenterParent;
                createItemForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                createItemForm.MaximizeBox = false;
                createItemForm.MinimizeBox = false;
                createItemForm.TopMost = true;
                createItemForm.Text = "Create a new item!";

                TabControl tabControl = new TabControl();
                tabControl.Name = "tabControl";
                tabControl.Location = new Point(0, 0);
                tabControl.Size = new Size(createItemForm.Width - 15, createItemForm.Height - 40);
                tabControl.MaximumSize = new Size(createItemForm.Width - 15, createItemForm.Height - 40);
                tabControl.Appearance = TabAppearance.Normal;
                tabControl.Alignment = TabAlignment.Top;
                tabControl.SizeMode = TabSizeMode.FillToRight;
                tabControl.Visible = true;

                TabPage itemCreatePage = new TabPage();
                itemCreatePage.Name = "itemCreatePage";
                itemCreatePage.Parent = tabControl;
                itemCreatePage.Text = "Create new Item!";
                itemCreatePage.BackgroundImage = WolcenEditor.Properties.Resources.bg;
                itemCreatePage.BackgroundImageLayout = ImageLayout.Center;
                itemCreatePage.Size = tabControl.Size;

                PictureBox displayItemView = new PictureBox();
                displayItemView.Name = "displayItemView";
                displayItemView.Size = new Size(125, 150);
                displayItemView.MaximumSize = new Size(125, 150);
                displayItemView.BorderStyle = BorderStyle.FixedSingle;
                displayItemView.Location = new Point(createItemForm.Width - 30 - displayItemView.Width, 20);
                displayItemView.Visible = true;
                displayItemView.Parent = itemCreatePage;
                displayItemView.BackColor = Color.Transparent;
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, displayItemView, new object[] { true });

                Label label1 = new Label();
                label1.Text = "Item list";
                label1.ForeColor = Color.White;
                label1.Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Bold);
                label1.Visible = true;
                label1.Parent = itemCreatePage;
                label1.Location = new Point(7, 2);
                label1.AutoSize = true;
                label1.BackColor = Color.Transparent;

                TreeView itemListView = new TreeView();
                itemListView.Name = "itemListView";
                itemListView.Size = new Size(175, itemCreatePage.Height);
                itemListView.MaximumSize = new Size(175, itemCreatePage.Height - 60);
                itemListView.Location = new Point(5, 20);
                itemListView.Visible = true;
                itemListView.BorderStyle = BorderStyle.FixedSingle;
                itemListView.BackColor = ColorTranslator.FromHtml("#1d1d1d");
                itemListView.Parent = itemCreatePage;
                itemListView.AfterSelect += ItemListView_AfterSelect;
                itemListView.ForeColor = Color.White;

                Button button = new Button();
                button.Name = "addItem";
                button.Click += Button_Click;
                button.Size = new Size(100, 50);
                button.Text = "Add to Inventory";
                button.Location = new Point(displayItemView.Location.X, displayItemView.Location.Y + displayItemView.Height + 15);
                button.Parent = itemCreatePage;

                Panel itemDescriptionView = new Panel();
                itemDescriptionView.Name = "itemDescriptionView";
                itemDescriptionView.Size = new Size(353, itemCreatePage.Height - 60);
                itemDescriptionView.Visible = true;
                itemDescriptionView.BorderStyle = BorderStyle.FixedSingle;
                itemDescriptionView.BackColor = Color.Transparent;
                itemDescriptionView.Parent = itemCreatePage;
                itemDescriptionView.Location = new Point(185, 20);
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemDescriptionView, new object[] { true });
                
                LoadItemList(itemListView);
                createItemForm.Controls.Add(tabControl);
            }
            createItemForm.ShowDialog();
        }

        private static void Button_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = ((sender as Button).Parent.Controls["itemListView"] as TreeView).SelectedNode;
            if (selectedNode.ImageIndex == 0) return;
            InventoryGrid newGridItem = new InventoryGrid();
            int x = Convert.ToInt32((accessableContextMenu.SourceControl as PictureBox).Name.Split('|')[0]);
            int y = Convert.ToInt32((accessableContextMenu.SourceControl as PictureBox).Name.Split('|')[1]);

            newGridItem.InventoryX = x;
            newGridItem.InventoryY = y;
            newGridItem.Type = selectedNode.ImageIndex;
            newGridItem.Quality = 1;
            newGridItem.Rarity = 1;
            newGridItem.Value = "0";
            if (selectedNode.Name.ToLower().Contains("unique"))
            {
                newGridItem.Rarity = 6;
            }
            newGridItem.ItemType = ParseItemNameForType(selectedNode.Name);
            switch (selectedNode.ImageIndex)
            {
                case (int)typeMap.Armor:        // Also accessory
                    newGridItem.Armor = new ItemArmor();
                    newGridItem.Armor.Name = selectedNode.Name;
                    break;
                case (int)typeMap.Weapon:       // Also offhand
                    newGridItem.Weapon = new ItemWeapon();
                    newGridItem.Weapon.Name = selectedNode.Name;
                    break;
                case (int)typeMap.Potion:
                    newGridItem.Potion = new Potion();
                    newGridItem.Potion.Name = selectedNode.Name;
                    break;
                case (int)typeMap.Gem:
                    newGridItem.Gem = new Gem();
                    newGridItem.Gem.Name = selectedNode.Name;
                    break;
            }
            cData.Character.InventoryGrid.Add(newGridItem);
            ReloadInventoryBitmap(((accessableContextMenu.SourceControl as PictureBox).Parent as Panel), (accessableContextMenu.SourceControl as PictureBox));
            createItemForm.Dispose();
            createItemForm = null;
        }

        private static string ParseItemNameForType(string name)
        {
            if (name.ToLower().Contains("speical"))     return "Gem";
            if (name.ToLower().Contains("1h"))
            {
                if (name.ToLower().Contains("sword"))   return "Sword1H";
                if (name.ToLower().Contains("axe"))     return "Axe1H";
                if (name.ToLower().Contains("mace") || name.ToLower().Contains("hammer"))    return "Mace1H";
            }
            if (name.ToLower().Contains("2h"))
            {
                if (name.ToLower().Contains("sword"))   return "Sword2H";
                if (name.ToLower().Contains("mace") || name.ToLower().Contains("hammer"))    return "Mace2H";
                if (name.ToLower().Contains("axe"))     return "Axe2H";
            }
            if (name.ToLower().Contains("bow"))         return "Bow";
            if (name.ToLower().Contains("amulet"))      return "Amulet";
            if (name.ToLower().Contains("helmet"))      return "Helmet";
            if (name.ToLower().Contains("chest") || name.ToLower().Contains("torso")) return "Chest Armor";
            if (name.ToLower().Contains("boots"))       return "Foot Armor";
            if (name.ToLower().Contains("pants"))       return "Leg Armor";
            if (name.ToLower().Contains("pauldron") || name.ToLower().Contains("shoulder")) return "Shoulder";
            if (name.ToLower().Contains("glove"))       return "Arm Armor";
            if (name.ToLower().Contains("belt"))        return "Belt";
            if (name.ToLower().Contains("ring"))        return "Ring";
            if (name.ToLower().Contains("catalyst"))    return "Trinket";
            if (name.ToLower().Contains("shield"))      return "Shield";
            if (name.ToLower().Contains("staff"))       return "Staff";
            if (name.ToLower().Contains("dagger"))      return "Dagger";
            if (name.ToLower().Contains("gun") || name.ToLower().Contains("pistol")) return "Gun";
            if (name.ToLower().Contains("gem"))         return "Gem";
            if (name.ToLower().Contains("potion"))      return "Potions";
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

        private static void LoadItemList(TreeView itemListView)
        {
            // Main Categories
            TreeNode Weapons = new TreeNode() { Name = "Weapons", Text = "Weapons", Tag = "Weapons" };
            TreeNode Armor = new TreeNode() { Name = "Armor", Text = "Armor", Tag = "Armor" };
            TreeNode Accessories = new TreeNode() { Name = "Accessories", Text = "Accessories", Tag = "Accessories" };
            TreeNode Potions = new TreeNode() { Name = "Potions", Text = "Potions", Tag = "Potions" };
            TreeNode Gems = new TreeNode() { Name = "Gems", Text = "Gems", Tag = "Gems" };

            // Sub Categories
            TreeNode Amulet = new TreeNode() { Name = "Amulet", Text = "Amulets", Tag = "Amulet" };
            TreeNode Helmet = new TreeNode() { Name = "Helmet", Text = "Helmets", Tag = "Helmet" };
            TreeNode ChestArmor = new TreeNode() { Name = "Chest Armor", Text = "Chest Armor", Tag = "ChestArmor" };
            TreeNode FootArmor = new TreeNode() { Name = "Foot Armor", Text = "Boots", Tag = "FootArmor" };
            TreeNode LegArmor = new TreeNode() { Name = "Leg Armor", Text = "Leggings", Tag = "LegArmor" };
            TreeNode Shoulder = new TreeNode() { Name = "Shoulder", Text = "Shoulders", Tag = "Shoulder" };
            TreeNode ArmArmor = new TreeNode() { Name = "Arm Armor", Text = "Gloves", Tag = "ArmArmor" };
            TreeNode Belt = new TreeNode() { Name = "Belt", Text = "Belts", Tag = "Belt" };
            TreeNode Ring = new TreeNode() { Name = "Ring", Text = "Rings", Tag = "Ring" };
            TreeNode Sword1H = new TreeNode() { Name = "Sword1H", Text = "1H Swords", Tag = "Sword1H" };
            TreeNode Shield = new TreeNode() { Name = "Shield", Text = "Shield", Tag = "Shield" };
            TreeNode Trinket = new TreeNode() { Name = "Trinket", Text = "Catalysts", Tag = "Trinket" };
            TreeNode Mace1H = new TreeNode() { Name = "Mace1H", Text = "1H Mace", Tag = "Mace1H" };
            TreeNode Bow = new TreeNode() { Name = "Bow", Text = "Bows", Tag = " Bow" };
            TreeNode Axe1H = new TreeNode() { Name = "Axe1H", Text = "1H Axes", Tag = "Axe1H" };
            TreeNode Staff = new TreeNode() { Name = "Staff", Text = "Staves", Tag = "Staff" };
            TreeNode Axe2H = new TreeNode() { Name = "Axe2H", Text = "2H Axes", Tag = "Axe2H" };
            TreeNode Sword2H = new TreeNode() { Name = "Sword2H", Text = "2H Swords", Tag = "Sword2H" };
            TreeNode Dagger = new TreeNode() { Name = "Dagger", Text = "Daggers", Tag = "Dagger" };
            TreeNode Mace2H = new TreeNode() { Name = "Mace2H", Text = "2H Maces", Tag = "Mace2H" };
            TreeNode Gun = new TreeNode() { Name = "Gun", Text = "Guns", Tag = "Gun" };

            Weapons.Nodes.Add(Sword1H);
            Weapons.Nodes.Add(Mace1H);
            Weapons.Nodes.Add(Bow);
            Weapons.Nodes.Add(Axe1H);
            Weapons.Nodes.Add(Staff);
            Weapons.Nodes.Add(Axe2H);
            Weapons.Nodes.Add(Sword2H);
            Weapons.Nodes.Add(Dagger);
            Weapons.Nodes.Add(Mace2H);
            Weapons.Nodes.Add(Gun);
            Weapons.Nodes.Add(Trinket);
            Armor.Nodes.Add(Helmet);
            Armor.Nodes.Add(ChestArmor);
            Armor.Nodes.Add(FootArmor);
            Armor.Nodes.Add(LegArmor);
            Armor.Nodes.Add(Shoulder);
            Armor.Nodes.Add(ArmArmor);
            Armor.Nodes.Add(Belt);
            Armor.Nodes.Add(Shield);
            Accessories.Nodes.Add(Amulet);
            Accessories.Nodes.Add(Ring);

            itemListView.Nodes.Add(Weapons);
            itemListView.Nodes.Add(Armor);
            itemListView.Nodes.Add(Accessories);
            itemListView.Nodes.Add(Potions);
            itemListView.Nodes.Add(Gems);

            foreach (var item in WolcenStaticData.ItemLocalizedNames)
            {
                if (WolcenStaticData.ItemWeapon.ContainsKey(item.Key) && ParseItemNameForType(item.Key) != "Shield")
                {
                    foreach (var d in equipMap)
                    {
                        string tester = ParseItemNameForType(item.Key);
                        if (tester == d.Key)
                        {
                            itemListView.Nodes["Weapons"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (int)typeMap.Weapon);
                            break;
                        }
                    }
                    //Weapons.Nodes.Add(item.Key, item.Value, (int)typeMap.Weapon);
                }
                else if (WolcenStaticData.ItemArmor.ContainsKey(item.Key) && ParseItemNameForType(item.Key) != "Amulet" && ParseItemNameForType(item.Key) != "Ring" 
                    || ParseItemNameForType(item.Key) == "Shield" || ParseItemNameForType(item.Key) == "Belt")
                {
                    foreach (var d in equipMap)
                    {
                        string tester = ParseItemNameForType(item.Key);
                        if (tester == d.Key)
                        {
                            itemListView.Nodes["Armor"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (tester == "Shield" ? (int)typeMap.Weapon : (int)typeMap.Armor));
                            break;
                        }
                    }
                    //Armor.Nodes.Add(item.Key, item.Value, (int)typeMap.Armor);
                }
                else if (WolcenStaticData.ItemAccessories.ContainsKey(item.Key) && ParseItemNameForType(item.Key) != "Belt" || ParseItemNameForType(item.Key) == "Amulet" || ParseItemNameForType(item.Key) == "Ring")
                {
                    foreach (var d in equipMap)
                    {
                        string tester = ParseItemNameForType(item.Key);
                        if (tester == d.Key)
                        {
                            itemListView.Nodes["Accessories"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (int)typeMap.Accessory);
                            break;
                        }
                    }
                    //Accessories.Nodes.Add(item.Key, item.Value, (int)typeMap.Accessory);
                }
                else if (item.Key.ToLower().Contains("potion"))
                {
                    Potions.Nodes.Add(item.Key, item.Value, (int)typeMap.Potion);
                }
                else if (item.Key.ToLower().Contains("gem"))
                {
                    Gems.Nodes.Add(item.Key, item.Value, (int)typeMap.Gem);
                }
            }
        }

        private static void ItemListView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            posY = 0;
            string dirPath = @".\UIResources\Items\";
            if ((sender as TreeView).SelectedNode.Nodes.Count > 0) return;
            TreeNode selectedNode = (sender as TreeView).SelectedNode;
            PictureBox itemView = ((sender as TreeView).Parent.Controls["displayItemView"] as PictureBox);
            Panel descView = ((sender as TreeView).Parent.Controls["itemDescriptionView"] as Panel);
            descView.Controls.Clear();
            string itemName = null;
            int itemWidth = 60;
            int itemHeight = 150;
            WolcenStaticData.ItemWeapon.TryGetValue(selectedNode.Name, out itemName);
            if (itemName == null)
            {
                WolcenStaticData.ItemArmor.TryGetValue(selectedNode.Name, out itemName);
                itemWidth = 75;
                itemHeight = 135;
            }
            if (itemName == null)
            {
                WolcenStaticData.ItemAccessories.TryGetValue(selectedNode.Name, out itemName);
                itemWidth = 85;
                itemHeight = 85;
            }
            if (itemName == null)
            {
                // Try Potions
                if (selectedNode.Name.ToLower().Contains("potion"))
                {
                    string[] pName = selectedNode.Name.Split('_');
                    if (File.Exists(dirPath + pName[0] + "_" + pName[1] + "_" + pName[2] + ".png"))
                    {
                        itemWidth = 100;
                        itemHeight = 130;
                        itemName = pName[0] + "_" + pName[1] + "_" + pName[2] + ".png";
                    }
                }

                // Try Gems
                if(selectedNode.Name.ToLower().Contains("gem"))
                {
                    if (File.Exists(dirPath + selectedNode.Name + ".png"))
                    {
                        itemWidth = 90;
                        itemHeight = 100;
                        itemName = selectedNode.Name + ".png";
                    }
                }

                if (itemName == null)
                {
                    //string writeToFile = "Missing: " + selectedNode.Name + ", " + WolcenStaticData.ItemLocalizedNames[selectedNode.Name];
                    //using (StreamWriter sw = File.AppendText(".\\missingItems.txt"))
                    //{
                    //    sw.WriteLine(writeToFile);
                    //}
                    return;
                }
            }

            descView.Controls.Add(createLabel(selectedNode.Name, WolcenStaticData.ItemLocalizedNames[selectedNode.Name], descView, 13, Color.White));
            itemView.Image = getImageFromPath(dirPath + itemName, itemView.Size, itemWidth, itemHeight);
        }

        private static Image getImageFromPath(string v, Size destionationSize, int itemWidth, int itemHeight)
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

        private static void LoadItemGridData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = null;
            if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2")
                itemStatDisplay = (((sender as PictureBox).Parent as GroupBox).Parent.Controls["itemStatDisplay"] as Panel);
            else itemStatDisplay = (((sender as PictureBox).Parent as Panel).Parent.Controls["itemStatDisplay"] as Panel);

            UnloadItemData(itemStatDisplay);
            PictureBox pictureBox = (sender as PictureBox);

            string itemName = null;
            if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2") itemName = getItemNameFromBelt(pictureBox);
            else itemName = getItemNameFromGrid(pictureBox);
            if (itemName == null) return;

            string l_itemName = null;
            WolcenStaticData.ItemLocalizedNames.TryGetValue(itemName, out l_itemName);
            if (l_itemName == null) return;

            Color itemRarity = Color.White;
            if ((sender as PictureBox).Name == "charBelt1" || (sender as PictureBox).Name == "charBelt2") itemRarity = getItemBeltColorRarity(pictureBox);
            else itemRarity = getItemGridColorRarity(pictureBox);

            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, l_itemName, itemStatDisplay, 13, itemRarity));
            string itemType = ParseItemNameForType(itemName);

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
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
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

        private static Color getItemBeltColorRarity(PictureBox pictureBox)
        {
            foreach (var iBelt in cData.Character.InventoryBelt)
            {
                if (iBelt.BeltSlot == 0 && pictureBox.Name == "charBelt1")
                {
                    return ColorTranslator.FromHtml(WolcenStaticData.rarityColorBank[iBelt.Rarity]);
                }
                if (iBelt.BeltSlot == 1 && pictureBox.Name == "charBelt2")
                {
                    return ColorTranslator.FromHtml(WolcenStaticData.rarityColorBank[iBelt.Rarity]);
                }
            }
            return Color.White;
        }

        private static string getItemNameFromBelt(PictureBox pictureBox)
        {
            foreach (var iBelt in cData.Character.InventoryBelt)
            {
                if (iBelt.BeltSlot == 0 && pictureBox.Name == "charBelt1")
                {
                    return iBelt.Potion.Name;
                }
                if (iBelt.BeltSlot == 1 && pictureBox.Name == "charBelt2")
                {
                    return iBelt.Potion.Name;
                }
            }
            return null;
        }

        private static string getItemTypeFromGrid(int x, int y)
        {
            foreach (var iGrid in cData.Character.InventoryGrid)
            {
                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                {
                    return iGrid.ItemType;
                }
            }
            return null;
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
                    string hexColor = WolcenStaticData.rarityColorBank[1];
                    WolcenStaticData.rarityColorBank.TryGetValue(item.Rarity, out hexColor);
                    return ColorTranslator.FromHtml(hexColor);
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

                List<Effect> defaultEffects = getItemMagicEffect(bodyPart, "Default");
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
                
                List<Effect> defaultEffects = getItemMagicEffect(bodyPart, "Default");
                foreach (Effect effect in defaultEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                    if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                }

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
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        
                    }
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
            string itemStat = null;
            if (pictureBox.Name == "charBelt1" || pictureBox.Name == "charBelt2")
            {
                foreach (var iBelt in cData.Character.InventoryBelt)
                {
                    if (pictureBox.Name == "charBelt1" && iBelt.BeltSlot == 0)
                    {
                        if (iBelt.Potion != null)
                        {
                            itemStat = iBelt.Potion.GetType().GetProperty(stat).GetValue(iBelt.Potion, null).ToString();
                            return itemStat;
                        }
                    }
                    
                    if (pictureBox.Name == "charBelt2" && iBelt.BeltSlot == 1)
                    {
                        if (iBelt.Potion != null)
                        {
                            itemStat = iBelt.Potion.GetType().GetProperty(stat).GetValue(iBelt.Potion, null).ToString();
                            return itemStat;
                        }
                    }
                }
            }
            else
            {
                foreach (var iGrid in cData.Character.InventoryGrid)
                {
                    int x = Convert.ToInt32(pictureBox.Name.Split('|')[0]);
                    int y = Convert.ToInt32(pictureBox.Name.Split('|')[1]);
                    if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                    {
                        itemStat = null;
                        if (iGrid.Armor != null)
                        {
                            if (iGrid.Armor.GetType().GetProperty(stat) != null)
                            {
                                itemStat = iGrid.Armor.GetType().GetProperty(stat).GetValue(iGrid.Armor, null).ToString();
                                return itemStat;
                            }
                        }
                        if (iGrid.Weapon != null)
                        {
                            if (iGrid.Weapon.GetType().GetProperty(stat) != null)
                            {
                                itemStat = iGrid.Weapon.GetType().GetProperty(stat).GetValue(iGrid.Weapon, null).ToString();
                                return itemStat;
                            }
                        }
                        if (iGrid.Potion != null)
                        {
                            if (iGrid.Potion.GetType().GetProperty(stat) != null)
                            {
                                itemStat = iGrid.Potion.GetType().GetProperty(stat).GetValue(iGrid.Potion, null).ToString();
                                return itemStat;
                            }
                        }
                        if (iGrid.Gem != null)
                        {
                            if (iGrid.Gem.GetType().GetProperty(stat) != null)
                            {
                                itemStat = iGrid.Gem.GetType().GetProperty(stat).GetValue(iGrid.Gem, null).ToString();
                                return itemStat;
                            }
                        }
                    }
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
            lb.Parent = panel;
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

                    itemName += WolcenStaticData.rarityColorBank[equip.Rarity];

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
            return null;
        }
    }
}
