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
                        pictureBox.ContextMenu = new ContextMenu();
                    }
                }
            }

            LoadRandomInventory((sender as TabPage).Controls["charRandomInv"] as Panel);
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
                    pb.DragEnter += Pb_DragEnter;
                    pb.DragDrop += Pb_DragDrop;
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
                if ((sender as PictureBox).Image == null) return;
                Bitmap bmp = new Bitmap((sender as PictureBox).Image);
                sourceBox = (sender as PictureBox);
                if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Copy) == DragDropEffects.Copy && isValid)
                {
                    if (!charMap.ContainsKey((sender as PictureBox).Name))
                    {
                        (sender as PictureBox).Size = defaultGridSize;
                    }
                    return;
                }
                if (!charMap.ContainsKey((sender as PictureBox).Name)) LoadItemGridData(sender, e);
                else LoadItemData(sender, e);
            }
            else if (e.Button == MouseButtons.Right)
            {
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
                contextMenu.MenuItems.Add(createItem);
            }
            else
            {
                MenuItem editItem = new MenuItem()
                {
                    Text = "Edit item",
                    Name = "EditItem"
                };
                MenuItem deleteItem = new MenuItem()
                {
                    Text = "Delete item",
                    Name = "DeleteItem"
                };
                deleteItem.Click += DeleteItem_Click;
                editItem.Click += EditItem_Click;
                contextMenu.MenuItems.Add(editItem);
                contextMenu.MenuItems.Add(deleteItem);
            }

            accessableContextMenu = contextMenu;
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
                    TopMost = true,
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
                    Sorting = SortOrder.Ascending
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
                    ForeColor = Color.White
                };
            }
            LoadItemsInInventoryGrid(editItemForm.Controls["itemsGrid"] as ListView);
            editItemForm.ShowDialog();
        }

        private static void ItemsInInventoryGrid_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
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
            displayBox.Image = getImageFromPath(dirPath + itemName, displayBox.Size, itemWidth, itemHeight);

        }

        private static void LoadItemsInInventoryGrid(ListView listView)
        {
            listView.Items.Clear();
            foreach (var item in cData.Character.InventoryGrid)
            {
                ListViewItem i = new ListViewItem();
                switch (item.Type)
                {
                    case (int)typeMap.Weapon:       // Also Offhand
                        //listView.Items.Add(new ListViewItem( new[] { item.InventoryX.ToString(), item.InventoryY.ToString(), WolcenStaticData.ItemLocalizedNames[item.Weapon.Name] }));
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem() {
                            Name = item.Weapon.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Weapon.Name]
                        });
                        break;
                    case (int)typeMap.Armor:       // Also Accessories
                        //listView.Items.Add(new ListViewItem(new[] { item.InventoryX.ToString(), item.InventoryY.ToString(), WolcenStaticData.ItemLocalizedNames[item.Armor.Name] }));
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
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem()
                        {
                            Name = item.Gem.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Gem.Name]
                        });
                        break;
                    case (int)typeMap.Potion:
                        //listView.Items.Add(new ListViewItem(new[] { item.InventoryX.ToString(), item.InventoryY.ToString(), WolcenStaticData.ItemLocalizedNames[item.Potion.Name] }));
                        i.Text = item.InventoryX.ToString();
                        i.SubItems.Add(item.InventoryY.ToString());
                        i.SubItems.Add(new ListViewItem.ListViewSubItem()
                        {
                            Name = item.Potion.Name,
                            Text = WolcenStaticData.ItemLocalizedNames[item.Potion.Name]
                        });
                        break;
                }
                listView.Items.Add(i);
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

                List<Effect> defaultEffects = getItemMagicEffect(bodyPart, "Default");
                foreach (Effect effect in defaultEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                    if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
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
