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
    public static partial class InventoryManager
    {

        private static Size defaultGridSize = new Size(50, 50);
        private static PictureBox sourceBox;
        private static bool isValid = false;
        private static int posY = 0;

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
                InventoryContextMenu.ShowContextMenu((sender as PictureBox), e.Location, cData.Character, "InventoryGrid");
            }
        }

        public static string ParseItemNameForType(string name)
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

        public static void LoadItemGridData(object sender, EventArgs e)
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
                
                itemStat = getItemStat(pictureBox, "ShieldResistance");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resistance: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(pictureBox, "ShieldBlockChance");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Chance: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(pictureBox, "ShieldBlockEfficiency");
                if (itemStat != null && itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Block Efficiency: " + itemStat, itemStatDisplay, 9, Color.White));
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

        public static string getItemTypeFromGrid(int x, int y)
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
            else   // Weapons
            {
                string itemStat = getItemStat(bodyPart, "DamageMin");
                string itemStat2 = getItemStat(bodyPart, "DamageMax");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "ResourceGeneration");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));

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

        public static string getGemStats(string gemName, int gemEffect)
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

        public static Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
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
