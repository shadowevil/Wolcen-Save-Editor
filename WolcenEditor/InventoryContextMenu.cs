using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class InventoryContextMenu
    {
        private static PictureBox accessablePictureBox;
        private static InventoryGrid iGridItem;
        private static InventoryGrid iGridCopiedItem;
        private static int globalItemIndex = -1;
        private struct coords
        {
            public static int x;
            public static int y;
            public static int panelID = -1;
        };

        public static ContextMenu LoadContextMenu(PictureBox pb)
        {
            ContextMenu _return = new ContextMenu();

            MenuItem createItem = new MenuItem();
            createItem.Text = "Create item";
            createItem.Name = "CreateItem";
            createItem.Click += CreateItem_Click;
            createItem.Visible = (pb.Image == null ? true : false);

            MenuItem pasteItem = new MenuItem()
            {
                Text = "Paste",
                Name = "PasteItem",
                Enabled = iGridCopiedItem != null ? true : false,
                Visible = (pb.Image == null ? true : false)
            };

            MenuItem editItem = new MenuItem()
            {
                Text = "Edit item",
                Name = "EditItem",
                Visible = (pb.Image == null ? false : true)
            };
            if (iGridItem != null)
            {
                editItem.Enabled = iGridItem.Enneract != null ? false : true;
            }

            MenuItem copyItem = new MenuItem()
            {
                Text = "Copy",
                Name = "CopyItem",
                Visible = (pb.Image == null ? false : true)
            };

            MenuItem deleteItem = new MenuItem()
            {
                Text = "Delete item",
                Name = "DeleteItem",
                Visible = (pb.Image == null ? false : true)
            };

            deleteItem.Click += DeleteItem_Click;
            copyItem.Click += CopyItem_Click;
            editItem.Click += EditItem_Click;
            pasteItem.Click += PasteItem_Click;

            _return.MenuItems.Add(createItem);
            _return.MenuItems.Add(pasteItem);
            _return.MenuItems.Add(editItem);
            _return.MenuItems.Add(copyItem);
            _return.MenuItems.Add(deleteItem);

            return _return;
        }

        public static void ShowContextMenu(PictureBox pb, Point location, object type, string select)
        {
            coords.x = -1;
            coords.y = -1;
            coords.panelID = -1;

            accessablePictureBox = pb;
            coords.x = Convert.ToInt32(pb.Name.Split('|')[0]);
            coords.y = Convert.ToInt32(pb.Name.Split('|')[1]);
            if (pb.Name.Split('|').Length == 3)
            {
                coords.panelID = Convert.ToInt32(pb.Name.Split('|')[2]);
                type = (type as IList)[coords.panelID];
            }
            setGridItem(type, select);
            LoadContextMenu(pb).Show(pb, location);
        }

        private static void setGridItem(object type, string select)
        {
            int i = 0;
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

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj.GetType() == null) return null;
            var objType = obj.GetType();
            var prop = objType.GetProperty(propertyName);

            return prop.GetValue(obj, null);
        }

        private static void EditItem_Click(object sender, EventArgs e)
        {
            using (EditItem form = new EditItem(coords.panelID, iGridItem, accessablePictureBox))
            {

            }
        }

        private static void CopyItem_Click(object sender, EventArgs e)
        {
            iGridCopiedItem = ObjectExtensions.Copy(iGridItem);
        }

        private static void DeleteItem_Click(object sender, EventArgs e)
        {
            if (globalItemIndex != -1)
            {
                if (coords.panelID == -1)
                {
                    cData.Character.InventoryGrid.RemoveAt(globalItemIndex);
                    InventoryManager.ReloadInventoryBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y);
                }
                else
                {
                    cData.PlayerChest.Panels[coords.panelID].InventoryGrid.RemoveAt(globalItemIndex);
                    StashManager.ReloadGridBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y, coords.panelID);
                }
                iGridItem = null;
            }
        }

        private static void PasteItem_Click(object sender, EventArgs e)
        {
            if (iGridCopiedItem != null)
            {
                InventoryGrid item = ObjectExtensions.Copy(iGridCopiedItem);
                item.InventoryX = coords.x;
                item.InventoryY = coords.y;

                PictureBox check = null;

                if (coords.panelID == -1)
                {
                    check = (accessablePictureBox.Parent as Panel).Controls[(coords.x + "|" + (coords.y + 1)).ToString()] as PictureBox;
                }
                else
                {
                    check = (accessablePictureBox.Parent as Panel).Controls[(coords.x + "|" + (coords.y + 1) + "|" + coords.panelID).ToString()] as PictureBox;
                }

                if (check == null) return;
                if (check.Image != null) return;

                if (coords.panelID == -1)
                {
                    if (cData.Character.InventoryGrid.Count <= 0) cData.Character.InventoryGrid = new List<InventoryGrid>();
                    cData.Character.InventoryGrid.Add(item);
                    InventoryManager.ReloadInventoryBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y);
                }
                else
                {
                    if (cData.PlayerChest.Panels[coords.panelID].InventoryGrid.Count <= 0) cData.PlayerChest.Panels[coords.panelID].InventoryGrid = new List<InventoryGrid>();
                    cData.PlayerChest.Panels[coords.panelID].InventoryGrid.Add(item);
                    StashManager.ReloadGridBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y, coords.panelID);
                }
            }
        }

        private static void CreateItem_Click(object sender, EventArgs e)
        {
            using (CreateItem form = new CreateItem(coords.panelID, iGridItem, accessablePictureBox))
            {

            }
        }
    }

    public class CreateItem : Form
    {
        public Form accessableForm;
        public int panelID = -1;
        public InventoryGrid selectedGridItem;
        public TreeView magicNodes;
        private PictureBox accessablePictureBox;
        private int posY = 0;

        public CreateItem(int panelid, InventoryGrid itemSelected, PictureBox accPB)
        {
            accessablePictureBox = accPB;
            selectedGridItem = itemSelected;
            panelID = panelid;
            accessableForm = this;
            Width = 700;
            Height = 400;
            MaximumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = false;
            Text = "Create a new item!";
            BackgroundImage = WolcenEditor.Properties.Resources.bg;
            BackgroundImageLayout = ImageLayout.Center;

            InitFormControls();
            this.ShowDialog();
        }

        private void InitFormControls()
        {
            PictureBox displayItemView = new PictureBox();
            displayItemView.Name = "displayItemView";
            displayItemView.Size = new Size(125, 150);
            displayItemView.MaximumSize = new Size(125, 150);
            displayItemView.BorderStyle = BorderStyle.FixedSingle;
            displayItemView.Location = new Point(this.Width - 30 - displayItemView.Width, 20);
            displayItemView.Visible = true;
            displayItemView.Parent = this;
            displayItemView.BackColor = Color.Transparent;
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, displayItemView, new object[] { true });

            Label label1 = new Label();
            label1.Text = "Item list";
            label1.ForeColor = Color.White;
            label1.Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Bold);
            label1.Visible = true;
            label1.Parent = this;
            label1.Location = new Point(7, 2);
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;

            TreeView itemListView = new TreeView();
            itemListView.Name = "itemListView";
            itemListView.Size = new Size(175, this.Height - 85);
            itemListView.MaximumSize = new Size(175, this.Height - 85);
            itemListView.Location = new Point(5, 45);
            itemListView.Visible = true;
            itemListView.BorderStyle = BorderStyle.FixedSingle;
            itemListView.BackColor = ColorTranslator.FromHtml("#1d1d1d");
            itemListView.Parent = this;
            itemListView.AfterSelect += ItemListView_AfterSelect;
            itemListView.ForeColor = Color.White;

            Button button = new Button();
            button.Name = "addItem";
            button.Click += Button_Click;
            button.Size = new Size(100, 50);
            button.Text = "Add to Inventory";
            button.Location = new Point(displayItemView.Location.X, displayItemView.Location.Y + displayItemView.Height + 15);
            button.Parent = this;

            TextBox itemSearchTextBox = new TextBox();
            itemSearchTextBox.Name = "searchItem";
            itemSearchTextBox.Size = new Size(itemListView.Width, 20);
            itemSearchTextBox.TextChanged += ItemSearchTextBox_TextChanged;
            itemSearchTextBox.GotFocus += ItemSearchTextBox_GotFocus;
            itemSearchTextBox.LostFocus += ItemSearchTextBox_LostFocus;
            itemSearchTextBox.Text = "Search items...";
            itemSearchTextBox.Location = new Point(itemListView.Location.X, itemListView.Location.Y - 24);
            itemSearchTextBox.Parent = this;

            Panel itemDescriptionView = new Panel();
            itemDescriptionView.Name = "itemDescriptionView";
            itemDescriptionView.Size = new Size(353, this.Height - 60);
            itemDescriptionView.Visible = true;
            itemDescriptionView.BorderStyle = BorderStyle.FixedSingle;
            itemDescriptionView.BackColor = Color.Transparent;
            itemDescriptionView.Parent = this;
            itemDescriptionView.Location = new Point(185, 20);
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemDescriptionView, new object[] { true });

            LoadItemList(itemListView);
        }


        //"Search items..."
        private void ItemSearchTextBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace((sender as TextBox).Text))
            {
                (sender as TextBox).ForeColor = Color.Gray;
                (sender as TextBox).Text = "Search items...";
                ItemSearchTextBox_TextChanged(sender, e);
            }
        }

        private void ItemSearchTextBox_GotFocus(object sender, EventArgs e)
        {
            (sender as TextBox).ForeColor = Color.Black;
            if ((sender as TextBox).Text == "Search items...")
            {
                (sender as TextBox).Text = "";
            }
        }

        private void ItemSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            var t = accessableForm.Controls["itemListView"] as TreeView;
            if (t.Nodes.Count <= 0) return;
            t.Nodes.Clear();
            LoadItemList(t);
        }

        private void ItemListView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            posY = 0;
            if ((sender as TreeView).SelectedNode.Nodes.Count > 0) return;
            TreeNode selectedNode = (sender as TreeView).SelectedNode;
            PictureBox itemView = ((sender as TreeView).Parent.Controls["displayItemView"] as PictureBox);
            Panel descView = ((sender as TreeView).Parent.Controls["itemDescriptionView"] as Panel);
            descView.Controls.Clear();
            string itemLocation = null;
            WolcenStaticData.ItemLocations.TryGetValue(selectedNode.Name, out itemLocation);
            string itemType = ItemDataDisplay.ParseItemNameForType(selectedNode.Name);

            if (itemLocation == null)
            {
                string[] enneractData;
                WolcenStaticData.ItemEnneract.TryGetValue(selectedNode.Name, out enneractData);
                if (enneractData != null)
                {
                    itemLocation = enneractData[1];
                }
            }

            descView.Controls.Add(createLabel(selectedNode.Name, WolcenStaticData.ItemLocalizedNames[selectedNode.Name], descView, 13, Color.White));
            itemView.Image = InventoryManager.getImageFromPath(Directory.GetCurrentDirectory() + itemLocation, itemView.Size);
            posY = 0;
        }

        public Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
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

        private void Button_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = ((sender as Button).Parent.Controls["itemListView"] as TreeView).SelectedNode;
            if (selectedNode.ImageIndex == 0) return;
            InventoryGrid newGridItem = new InventoryGrid();
            int x = Convert.ToInt32(accessablePictureBox.Name.Split('|')[0]);
            int y = Convert.ToInt32(accessablePictureBox.Name.Split('|')[1]);

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

            if (selectedNode.Name.ToLower().Contains("reagent"))
            {
                if (selectedNode.Name.ToLower().Contains("legendary"))
                {
                    newGridItem.Rarity = 4;
                }
                newGridItem.Reagent = new Reagent();
                newGridItem.Reagent.Name = selectedNode.Name;
            }
            else
            {
                newGridItem.ItemType = ItemDataDisplay.ParseItemNameForType(selectedNode.Name);
                switch (selectedNode.ImageIndex)
                {
                    case (int)InventoryManager.typeMap.Armor:        // Also accessory
                        newGridItem.Armor = new ItemArmor();
                        newGridItem.Armor.Name = selectedNode.Name;
                        break;
                    case (int)InventoryManager.typeMap.Weapon:       // Also offhand
                        newGridItem.Weapon = new ItemWeapon();
                        newGridItem.Weapon.Name = selectedNode.Name;
                        break;
                    case (int)InventoryManager.typeMap.Potion:
                        newGridItem.Potion = new Potion();
                        newGridItem.Potion.Name = selectedNode.Name;
                        break;
                    case (int)InventoryManager.typeMap.Gem:
                        newGridItem.Gem = new Gem();
                        newGridItem.Gem.Name = selectedNode.Name;
                        break;
                    case (int)InventoryManager.typeMap.Enneract:
                        newGridItem.Enneract = new Enneract();
                        string[] enneractData;
                        WolcenStaticData.ItemEnneract.TryGetValue(selectedNode.Name, out enneractData);
                        newGridItem.Enneract.Name = selectedNode.Name;
                        newGridItem.Enneract.Stats_SkillLevel = 1;
                        newGridItem.Enneract.Stats_SkillUID = enneractData[0];
                        break;
                    case (int)InventoryManager.typeMap.NPC2Consumable:
                        newGridItem.NPC2Consumable = new NPC2Consumable();
                        newGridItem.NPC2Consumable.Name = selectedNode.Name;
                        break;
                }
            }

            if (panelID == -1)
            {
                cData.Character.InventoryGrid.Add(newGridItem);
                InventoryManager.ReloadInventoryBitmap((accessablePictureBox.Parent as Panel), accessablePictureBox);
            }
            else
            {
                cData.PlayerChest.Panels[panelID].InventoryGrid.Add(newGridItem);
                StashManager.ReloadGridBitmap((accessablePictureBox.Parent as Panel), x, y, panelID);
            }
            this.Dispose();
        }

        private void LoadItemList(TreeView itemListView)
        {
            // Main Categories
            TreeNode Weapons = new TreeNode() { Name = "Weapons", Text = "Weapons", Tag = "Weapons" };
            TreeNode Armor = new TreeNode() { Name = "Armor", Text = "Armor", Tag = "Armor" };
            TreeNode Accessories = new TreeNode() { Name = "Accessories", Text = "Accessories", Tag = "Accessories" };
            TreeNode Potions = new TreeNode() { Name = "Potions", Text = "Potions", Tag = "Potions" };
            TreeNode Gems = new TreeNode() { Name = "Gems", Text = "Gems", Tag = "Gems" };
            TreeNode Reagents = new TreeNode() { Name = "Reagents", Text = "Reagents", Tag = "Reagents" };
            TreeNode Enneracts = new TreeNode() { Name = "Enneracts", Text = "Enneracts", Tag = "Enneracts" };
            TreeNode Consumables = new TreeNode() { Name = "Consumables", Text = "Consumables", Tag = "Consumables" };

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
            itemListView.Nodes.Add(Reagents);
            itemListView.Nodes.Add(Enneracts);
            itemListView.Nodes.Add(Consumables);
            
            Dictionary<string, string> dict = WolcenStaticData.ItemLocalizedNames;
            string txtboxText = accessableForm.Controls["searchItem"].Text;
            if (txtboxText != "Search items...") dict = WolcenStaticData.ItemLocalizedNames.Where(x => x.Value.ToLower().Contains(txtboxText.ToLower())).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in dict)
            {
                string tester = ItemDataDisplay.ParseItemNameForType(item.Key);
                if (WolcenStaticData.ItemLocations.ContainsKey(item.Key) && tester != null && tester != "Potions" && tester != "Gem")
                {
                    foreach (var d in InventoryManager.equipMap)
                    {
                        if (ItemDataDisplay.ParseItemTypeForBasicType(tester) == "Weapon" && tester != "Shield")
                        {
                            itemListView.Nodes["Weapons"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Weapon);
                            break;
                        }
                        if (ItemDataDisplay.ParseItemTypeForBasicType(tester) == "Armor" || tester == "Shield")
                        {
                            itemListView.Nodes["Armor"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (tester == "Shield" ? (int)InventoryManager.typeMap.Weapon : (int)InventoryManager.typeMap.Armor));
                            break;
                        }
                        if (ItemDataDisplay.ParseItemTypeForBasicType(tester) == "Accessory")
                        {
                            itemListView.Nodes["Accessories"].Nodes[tester.Trim(' ')].Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Accessory);
                            break;
                        }
                    }
                }
                else if (item.Key.ToLower().Contains("potion"))
                {
                    Potions.Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Potion);
                }
                else if (item.Key.ToLower().Contains("gem"))
                {
                    Gems.Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Gem);
                }
                else if (item.Key.ToLower().Contains("reagent"))
                {
                    Reagents.Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Reagent);
                }
                else if (item.Key.ToLower().Contains("enneract"))
                {
                    Enneracts.Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.Enneract);
                }
                else if (item.Key.ToLower().Contains("npc2_consumable"))
                {
                    Consumables.Nodes.Add(item.Key, item.Value, (int)InventoryManager.typeMap.NPC2Consumable);
                }
            }
        }
    }

    public class EditItem : Form
    {
        public Form accessableForm;
        public int panelID = -1;
        public InventoryGrid selectedGridItem;
        public TreeView magicNodes;
        private int ItemQuality = 1;
        private PictureBox accessablePictureBox;

        public EditItem(int panelid, InventoryGrid itemSelected, PictureBox accPB)
        {
            accessablePictureBox = accPB;
            selectedGridItem = itemSelected;
            panelID = panelid;
            accessableForm = this;
            Width = 700;
            Height = 400;
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = false;
            Text = "Edit Items";
            BackgroundImage = WolcenEditor.Properties.Resources.bg;
            BackgroundImageLayout = ImageLayout.Stretch;

            InitFormControls();
            this.ShowDialog();
        }

        private void InitFormControls()
        {
            ListView itemsInGrid = new ListView()
            {
                Name = "itemsGrid",
                Size = new Size(200, 190),
                MinimumSize = new Size(200, 190),
                Location = new Point(10, 10),
                Visible = true,
                BorderStyle = BorderStyle.FixedSingle,
                Parent = this,
                BackColor = ColorTranslator.FromHtml("#1d1d1d"),
                ForeColor = Color.White,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Sorting = SortOrder.Ascending,
                HideSelection = false
            };
            itemsInGrid.ItemSelectionChanged += ItemsInGrid_ItemSelectionChanged;
            itemsInGrid.Columns.Add("X", 20, HorizontalAlignment.Left);
            itemsInGrid.Columns.Add("Y", 20, HorizontalAlignment.Left);
            if(panelID != -1) itemsInGrid.Columns.Add("ID", 20, HorizontalAlignment.Left);
            itemsInGrid.Columns.Add("Name", 205, HorizontalAlignment.Left);

            PictureBox displayItemView = new PictureBox()
            {
                Name = "displayItemView",
                Size = new Size(125, 150),
                MaximumSize = new Size(125, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(10, itemsInGrid.Height + 15),
                Visible = true,
                Parent = this,
                BackColor = Color.Transparent
            };
            typeof(PictureBox).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, displayItemView, new object[] { true });

            TreeView statEditView = new TreeView()
            {
                Name = "statEditView",
                Size = new Size(250, 325),
                MaximumSize = new Size(250, 325),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(itemsInGrid.Width + 20, 35),
                Visible = true,
                Parent = this,
                BackColor = ColorTranslator.FromHtml("#1d1d1d"),
                ForeColor = Color.White,
                HideSelection = false
            };

            statEditView.AfterSelect += StatEditView_AfterSelect;
            magicNodes = statEditView;

            Button addSelectedStat = new Button()
            {
                Name = "addSelectedStat",
                Text = "Add Selected Affix",
                Size = new Size(100, 25),
                Location = new Point(475, 330),
                Visible = true,
                FlatStyle = FlatStyle.Standard,
                Enabled = true,
                Parent = this
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
                Parent = this
            };
            deleteAffix.Click += DeleteAffix_Click;

            TextBox SearchTextAffix = new TextBox()
            {
                Name = "searchAffixTextBox",
                Size = new Size(statEditView.Width, 10),
                Location = new Point(statEditView.Location.X, statEditView.Location.Y - 24),
                Visible = true,
                Enabled = true,
                Parent = this,
                Text = "Search affixes...",
                ForeColor = Color.Gray
            };
            SearchTextAffix.TextChanged += SearchTextAffix_TextChanged;
            SearchTextAffix.GotFocus += SearchTextAffix_GotFocus;
            SearchTextAffix.LostFocus += SearchTextAffix_LostFocus;

            LoadItemsFromGrid(itemsInGrid);
            LoadTreeNodes();
            LoadCurrentAffixes(panelID);
        }

        private void SearchTextAffix_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace((sender as TextBox).Text))
            {
                (sender as TextBox).ForeColor = Color.Gray;
                (sender as TextBox).Text = "Search affixes...";
                LoadCurrentAffixes(panelID);
            }
        }

        private void SearchTextAffix_GotFocus(object sender, EventArgs e)
        {
            (sender as TextBox).ForeColor = Color.Black;
            if ((sender as TextBox).Text == "Search affixes...")
            {
                (sender as TextBox).Text = "";
            }
        }

        private void SearchTextAffix_TextChanged(object sender, EventArgs e)
        {
            LoadTreeNodes();
        }

        private void LoadItemsFromGrid(ListView itemsInGrid)
        {
            if (panelID == -1)
            {
                foreach (var iGrid in cData.Character.InventoryGrid)
                {
                    ListViewItem i = null;
                    switch (iGrid.Type)
                    {
                        case (int)InventoryManager.typeMap.Weapon: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.Weapon.Name); break;
                        case (int)InventoryManager.typeMap.Armor: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.Armor.Name); break;
                        case (int)InventoryManager.typeMap.Gem: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.Gem.Name); break;
                        case (int)InventoryManager.typeMap.Potion: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.Potion.Name);  break;
                        case (int)InventoryManager.typeMap.Reagent: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.Reagent.Name); break;
                        case (int)InventoryManager.typeMap.NPC2Consumable: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), iGrid.NPC2Consumable.Name); break;
                    }
                    if (i != null)
                    {
                        if (iGrid.InventoryX == selectedGridItem.InventoryX && iGrid.InventoryY == selectedGridItem.InventoryY) i.Selected = true;
                        itemsInGrid.Items.Add(i);
                    }
                }
            }
            else
            {
                foreach (var p in cData.PlayerChest.Panels)
                {
                    if (p.InventoryGrid == null) p.InventoryGrid = new List<InventoryGrid>();
                    foreach (var iGrid in p.InventoryGrid)
                    {
                        ListViewItem i = null;
                        switch (iGrid.Type)
                        {
                            case (int)InventoryManager.typeMap.Weapon: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.Weapon.Name); break;
                            case (int)InventoryManager.typeMap.Armor: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.Armor.Name); break;
                            case (int)InventoryManager.typeMap.Gem: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.Gem.Name); break;
                            case (int)InventoryManager.typeMap.Potion: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.Potion.Name); break;
                            case (int)InventoryManager.typeMap.Reagent: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.Reagent.Name); break;
                            case (int)InventoryManager.typeMap.NPC2Consumable: i = createLVItem(iGrid.InventoryX.ToString(), iGrid.InventoryY.ToString(), (p.ID + 1).ToString(), iGrid.NPC2Consumable.Name); break;
                        }
                        if (i != null)
                        {
                            if (iGrid.InventoryX == selectedGridItem.InventoryX && iGrid.InventoryY == selectedGridItem.InventoryY && p.ID == panelID) i.Selected = true;
                            itemsInGrid.Items.Add(i);
                        }
                    }
                }
            }
        }

        private ListViewItem createLVItem(string x, string y, string panelid, string name)
        {
            ListViewItem i = new ListViewItem()
            {
                Text = x
            };
            i.SubItems.Add(y);
            i.SubItems.Add(panelid);
            i.SubItems.Add(new ListViewItem.ListViewSubItem()
            {
                Name = name,
                Text = WolcenStaticData.ItemLocalizedNames[name]
            });
            return i;
        }

        private void LoadCurrentAffixes(int panelID)
        {
            if (magicNodes.Nodes["CurrentAffixes"] != null) magicNodes.Nodes["CurrentAffixes"].Nodes.Clear();
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
            TreeNode ShieldResistance = null;
            TreeNode ShieldBlockChance = null;
            TreeNode ShieldBlockEfficiency = null;
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
            TreeNode StackSize = null;

            IList<InventoryGrid> invGrid = null;
            if (panelID == -1) invGrid = cData.Character.InventoryGrid;
            else invGrid = cData.PlayerChest.Panels[panelID].InventoryGrid;

            if (invGrid == null) return;

            foreach (var iGrid in invGrid)
            {
                if (iGrid.InventoryX == selectedGridItem.InventoryX && iGrid.InventoryY == selectedGridItem.InventoryY)
                {
                    if (iGrid.Gem == null && iGrid.Reagent == null)
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

                        Level = new TreeNode()
                        {
                            Name = "Level",
                            Text = "Item Level",
                            ImageKey = "default",
                            SelectedImageKey = iGrid.Level.ToString()
                        };

                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Rarity);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Quality);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Sockets);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Level);
                    }

                    Value = new TreeNode()
                    {
                        Name = "Value",
                        Text = "Value",
                        ImageKey = "default"
                    };
                    if (iGrid.Value == null) Value.SelectedImageKey = "0";
                    else Value.SelectedImageKey = iGrid.Value.ToString();
                    magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Value);

                    if (iGrid.Gem != null || iGrid.Reagent != null)
                    {
                        StackSize = new TreeNode()
                        {
                            Name = "StackSize",
                            Text = "Stack Size",
                            ImageKey = "default"
                        };

                        if (iGrid.Gem != null) StackSize.SelectedImageKey = iGrid.Gem.StackSize.ToString();
                        if (iGrid.Reagent != null) StackSize.SelectedImageKey = iGrid.Reagent.StackSize.ToString();

                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(StackSize);
                    }
                    if (iGrid.Type == (int)InventoryManager.typeMap.Weapon)    // Weapons & offhands
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
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(damageMin);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(damageMax);
                        if (iGrid.ItemType == "Shield")
                        {
                            ShieldResistance = new TreeNode()
                            {
                                Name = "ShieldResistance",
                                Text = "Shield Resistance",
                                ImageKey = "default",
                                SelectedImageKey = iGrid.Weapon.ShieldResistance.ToString()
                            };
                            ShieldBlockChance = new TreeNode()
                            {
                                Name = "ShieldBlockChance",
                                Text = "Shield Block Chance",
                                ImageKey = "default",
                                SelectedImageKey = iGrid.Weapon.ShieldBlockChance.ToString()
                            };
                            ShieldBlockEfficiency = new TreeNode()
                            {
                                Name = "ShieldBlockEfficiency",
                                Text = "Shield Block Efficiency",
                                ImageKey = "default",
                                SelectedImageKey = iGrid.Weapon.ShieldBlockEfficiency.ToString()
                            };
                            magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ShieldResistance);
                            magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ShieldBlockChance);
                            magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ShieldBlockEfficiency);
                        }
                        else
                        {
                            ResourceGeneration = new TreeNode()
                            {
                                Name = "ResourceGeneration",
                                Text = "Resource Generation",
                                ImageKey = "default",
                                SelectedImageKey = iGrid.Weapon.ResourceGeneration.ToString()
                            };
                            magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ResourceGeneration);
                        }
                    }

                    if (iGrid.Type == (int)InventoryManager.typeMap.Armor)
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
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Armor);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Health);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Resistance);
                    }

                    if (iGrid.Type == (int)InventoryManager.typeMap.Potion)
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
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(Charge);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ImmediateHP);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ImmediateMana);
                        magicNodes.Nodes["CurrentAffixes"].Nodes.Add(ImmediateStamina);
                    }

                    if (iGrid.Gem == null && iGrid.Reagent == null)
                    {
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

                                if (iGrid.MagicEffects.RolledAffixes != null)
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
            }
            if (defaultNode.Nodes.Count != 0) magicNodes.Nodes["CurrentAffixes"].Nodes.Add(defaultNode);
            if (affixNode.Nodes.Count != 0) magicNodes.Nodes["CurrentAffixes"].Nodes.Add(affixNode);
        }

        private void LoadTreeNodes()
        {
            magicNodes.Nodes.Clear();

            TreeNode Implicit = createNode("Implicit", "Implicit Affixes");
            TreeNode MasterSlavesMagicWeapons = createNode("AffixesMastersSlavesMagicWeapons", "Master Slave Magic Weapon Affixes");
            TreeNode MasterSlavesPhysicalWeapons = createNode("AffixesMastersSlavesPhysicalWeapons", "Master Slave Physical Weapon Affixes");
            TreeNode Accessories = createNode("AffixesAccessories", "Accessory Affixes");
            TreeNode Armors = createNode("AffixesArmors", "Armor Affixes");
            TreeNode Uniques = createNode("AffixesUniques", "Unique Affixes");
            TreeNode UniquesMax = createNode("AffixesUniquesMax", "Unique Max Affixes");
            TreeNode UniquesMaxMax = createNode("AffixesUniqueMaxMax", "Unique Max Max Affixes");
            TreeNode Weapons = createNode("AffixesWeapons", "Weapon Affixes");

            TreeNode currentAffixes = createNode("CurrentAffixes", "Current Affixes");

            magicNodes.Nodes.Add(currentAffixes);
            magicNodes.Nodes.Add(Implicit);
            magicNodes.Nodes.Add(MasterSlavesMagicWeapons);
            magicNodes.Nodes.Add(MasterSlavesPhysicalWeapons);
            magicNodes.Nodes.Add(Accessories);
            magicNodes.Nodes.Add(Armors);
            magicNodes.Nodes.Add(Uniques);
            magicNodes.Nodes.Add(UniquesMax);
            magicNodes.Nodes.Add(UniquesMaxMax);
            magicNodes.Nodes.Add(Weapons);

            AddNodes(Implicit, WolcenStaticData.AffixesImplicit);
            AddNodes(MasterSlavesMagicWeapons, WolcenStaticData.AffixesMastersSlavesMagicWeapons);
            AddNodes(MasterSlavesPhysicalWeapons, WolcenStaticData.AffixesMastersSlavesPhysicalWeapons);
            AddNodes(Accessories, WolcenStaticData.AffixesAccessories);
            AddNodes(Armors, WolcenStaticData.AffixesArmors);
            AddNodes(Uniques, WolcenStaticData.AffixesUniques);
            AddNodes(UniquesMax, WolcenStaticData.AffixesUniquesMax);
            AddNodes(UniquesMaxMax, WolcenStaticData.AffixesUniquesMaxMax);
            AddNodes(Weapons, WolcenStaticData.AffixesWeapons);
        }

        private void AddNodes(TreeNode treeNode, Dictionary<string, string> dict)
        {
            string txtboxText = accessableForm.Controls["searchAffixTextBox"].Text;
            if (txtboxText != "Search affixes...") dict = dict.Where(x => WolcenStaticData.MagicLocalized[x.Value].ToLower().Contains(txtboxText.ToLower())).ToDictionary(x => x.Key, x => x.Value);

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
                    if (semantics.Count() >= 1)
                    {
                        if (semantics[0].ToLower().Contains("percent") || semantics[0].Contains("ChanceFlatFloat"))
                        {
                            lValue = lValue.Replace("%1", "(%)");
                        }
                        else
                        {
                            lValue = lValue.Replace("%1", "(X)");
                        }

                        if (semantics.Count() >= 2)
                        {
                            if (semantics[1].ToLower().Contains("percent") || semantics[1].Contains("ChanceFlatFloat"))
                            {
                                lValue = lValue.Replace("%2", "(%)");
                            }
                            else
                            {
                                lValue = lValue.Replace("%2", "(X)");
                            }
                        }
                    }

                    node = new TreeNode();
                    node.Name = d.Key;
                    node.ImageKey = value;
                    node.Text = lValue;
                    if (checkExistingNodes(treeNode, lValue))
                    {
                        if (semantics != null && semantics.Count() > 0)
                        {
                            for (int i = 0; i < semantics.Count(); i++)
                            {
                                node.StateImageKey += semantics[i] + "|";
                            }
                        }
                        else
                        {
                            node.Text = "[*]" + node.Text;
                            node.StateImageKey = "CountInt";
                        }
                        treeNode.Nodes.Add(node);
                    }
                }
            }
        }

        private bool checkExistingNodes(TreeNode treeNode, string lValue)
        {
            foreach (TreeNode n in treeNode.Nodes)
            {
                if (n.Text == lValue)
                {
                    return false;
                }
            }
            return true;
        }

        private TreeNode createNode(string name, string text)
        {
            return new TreeNode() { Name = name, Text = text };
        }

        private ListViewItem createLVItem(string x, string y, string name)
        {
            ListViewItem i = new ListViewItem()
            {
                Text = x
            };
            i.SubItems.Add(y);
            i.SubItems.Add(new ListViewItem.ListViewSubItem()
            {
                Name = name,
                Text = WolcenStaticData.ItemLocalizedNames[name]
            });
            return i;
        }

        private void DeleteAffix_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = (this.Controls["statEditView"] as TreeView).SelectedNode;
            string effectName = selectedNode.Name;
            string effectId = selectedNode.ImageKey;

            IList<InventoryGrid> gridList = null;
            int _panelID = -1;

            ListViewItem selectedItem = (this.Controls["itemsGrid"] as ListView).SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            if (panelID != -1) _panelID = Convert.ToInt32(selectedItem.SubItems[2].Text) - 1;

            if (_panelID == -1) gridList = cData.Character.InventoryGrid;
            else gridList = cData.PlayerChest.Panels[_panelID].InventoryGrid;


            for (int i = 0; i < gridList.Count; i++)
            {
                if (gridList[i].InventoryX == x && gridList[i].InventoryY == y)
                {
                    if (selectedNode.FullPath.Contains("Default Affixes"))
                    {
                        for (int s = 0; s < gridList[i].MagicEffects.Default.Count; s++)
                        {
                            if (gridList[i].MagicEffects.Default[s].EffectId == effectId &&
                                gridList[i].MagicEffects.Default[s].EffectName == effectName)
                            {
                                if (_panelID == -1)
                                {
                                    cData.Character.InventoryGrid[i].MagicEffects.Default.RemoveAt(s);
                                }
                                else
                                {
                                    cData.PlayerChest.Panels[_panelID].InventoryGrid[i].MagicEffects.Default.RemoveAt(s);
                                }
                            }
                        }
                    } else {
                        for (int s = 0; s < gridList[i].MagicEffects.RolledAffixes.Count; s++)
                        {
                            if (gridList[i].MagicEffects.RolledAffixes[s].EffectId == effectId &&
                                gridList[i].MagicEffects.RolledAffixes[s].EffectName == effectName)
                            {
                                if (_panelID == -1)
                                {
                                    cData.Character.InventoryGrid[i].MagicEffects.RolledAffixes.RemoveAt(s);
                                }
                                else
                                {
                                    cData.PlayerChest.Panels[_panelID].InventoryGrid[i].MagicEffects.RolledAffixes.RemoveAt(s);
                                }
                            }
                        }
                    }
                }
            }

            RemoveItemEditControls();

            (this.Controls["deleteAffix"] as Button).Enabled = false;
            LoadCurrentAffixes(_panelID);
            if (_panelID == -1)
            {
                InventoryManager.ReloadInventoryBitmap((accessablePictureBox.Parent as Panel), accessablePictureBox);
                ItemDataDisplay.LoadItemData(sender, ((accessablePictureBox.Parent as Panel).Parent as TabPage).Controls["itemStatDisplay"] as Panel, cData.Character, "InventoryGrid");
            }
            else
            {
                StashManager.ReloadGridBitmap((accessablePictureBox.Parent as Panel), x, y, _panelID);
                ItemDataDisplay.LoadItemData(sender, ((accessablePictureBox.Parent as Panel).Parent as TabPage).Controls["itemStashStatDisplay"] as Panel, cData.PlayerChest.Panels, "InventoryGrid");
            }
        }

        private void AddSelectedStat_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = (this.Controls["statEditView"] as TreeView).SelectedNode;
            if (selectedNode == null) return;
            string effectName = selectedNode.Name;
            string effectId = selectedNode.ImageKey;
            string[] semantics = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] semanticValues = selectedNode.SelectedImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            
            ListViewItem selectedItem = (this.Controls["itemsGrid"] as ListView).SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            string itemName = null;
            int _panelid = -1;

            IList<InventoryGrid> inventoryGrid = null;

            if (panelID != -1) _panelid = Convert.ToInt32(selectedItem.SubItems[2].Text) - 1;

            if (panelID == -1)
            {
                itemName = selectedItem.SubItems[2].Text;
                inventoryGrid = cData.Character.InventoryGrid;
            }
            else
            {
                itemName = selectedItem.SubItems[3].Text;
                inventoryGrid = cData.PlayerChest.Panels[_panelid].InventoryGrid;
            }

            string mode = (sender as Button).Text.Contains("Update") ? "update" : "add";

            if (selectedNode.ImageKey != "default")
            {
                switch (mode)
                {
                    case "update":
                        for (int i = 0; i < inventoryGrid.Count; i++)
                        {
                            if (inventoryGrid[i].InventoryX == x && inventoryGrid[i].InventoryY == y)
                            {
                                if (selectedNode.FullPath.Contains("Default Affixes"))
                                {
                                    for (int s = 0; s < inventoryGrid[i].MagicEffects.Default.Count; s++)
                                    {
                                        if (inventoryGrid[i].MagicEffects.Default[s].EffectId == effectId
                                            && inventoryGrid[i].MagicEffects.Default[s].EffectName == effectName)
                                        {
                                            for (int d = 0; d < inventoryGrid[i].MagicEffects.Default[s].Parameters.Count; d++)
                                            {
                                                if (inventoryGrid[i].MagicEffects.Default[s].Parameters[d].semantic == semantics[d])
                                                {
                                                    inventoryGrid[i].MagicEffects.Default[s].Parameters[d].value = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat" + d.ToString()] as TextBox).Text);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int s = 0; s < inventoryGrid[i].MagicEffects.RolledAffixes.Count; s++)
                                    {
                                        if (inventoryGrid[i].MagicEffects.RolledAffixes[s].EffectId == effectId
                                            && inventoryGrid[i].MagicEffects.RolledAffixes[s].EffectName == effectName)
                                        {
                                            for (int d = 0; d < inventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters.Count; d++)
                                            {
                                                if (inventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters[d].semantic == semantics[d])
                                                {
                                                    inventoryGrid[i].MagicEffects.RolledAffixes[s].Parameters[d].value = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat" + d.ToString()] as TextBox).Text);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        RemoveItemEditControls();
                        break;
                    case "add":
                        if (selectedGridItem.Reagent != null || selectedGridItem.Gem != null)
                        {
                            MessageBox.Show("Reagent's or Gems cannot possess magic affixes", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(effectId)) return;
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

                        if (_panelid == -1)
                        {
                            foreach (var iGrid in cData.Character.InventoryGrid)
                            {
                                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                                {
                                    oldItem = iGrid;
                                    itemEditing = ObjectExtensions.Copy(iGrid);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var iGrid in cData.PlayerChest.Panels[_panelid].InventoryGrid)
                            {
                                if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                                {
                                    oldItem = iGrid;
                                    itemEditing = ObjectExtensions.Copy(iGrid);
                                    break;
                                }
                            }
                        }
                        if (itemEditing == null) return;

                        ItemMagicEffects magicEffects = itemEditing.MagicEffects;
                        Effect effect = new Effect() { EffectName = effectName, EffectId = effectId };

                        if (!selectedNode.FullPath.Contains("Current Affixes"))
                        {
                            if (magicEffects == null) magicEffects = new ItemMagicEffects();

                            if ((this.Controls["defaultAffix"] as CheckBox).Checked)
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
                            if (ep.semantic == "TriggeredSkillSelector") ep.value = 0;
                            else ep.value = Convert.ToDouble(semanticValues[i]);
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
                            if ((this.Controls["defaultAffix"] as CheckBox).Checked)
                            {
                                magicEffects.Default.Add(effect);
                            }
                            else
                            {
                                magicEffects.RolledAffixes.Add(effect);
                            }
                        }
                        itemEditing.MagicEffects = magicEffects;

                        if (_panelid == -1)
                        {
                            cData.Character.InventoryGrid.Remove(oldItem);
                            cData.Character.InventoryGrid.Add(itemEditing);
                        }
                        else
                        {
                            cData.PlayerChest.Panels[_panelid].InventoryGrid.Remove(oldItem);
                            cData.PlayerChest.Panels[_panelid].InventoryGrid.Add(itemEditing);
                        }
                        break;
                }
            }
            else
            {
                for (int i = 0; i < inventoryGrid.Count; i++)
                {
                    if (inventoryGrid[i].InventoryX == x && inventoryGrid[i].InventoryY == y)
                    {
                        if (selectedNode.Name == "StackSize" && inventoryGrid[i].Gem != null) inventoryGrid[i].Gem.StackSize = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                        if (selectedNode.Name == "StackSize" && inventoryGrid[i].Reagent != null) inventoryGrid[i].Reagent.StackSize = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                        if (selectedNode.Name == "Rarity") inventoryGrid[i].Rarity = ((sender as Button).Parent.Controls["cboRarity"] as ComboBox).SelectedIndex;
                        if (selectedNode.Name == "Quality") inventoryGrid[i].Quality = ItemQuality;
                        if (selectedNode.Name == "Sockets")
                        {
                            List<Socket> currentSockets = new List<Socket>();
                            int setNumSockets = (this.Controls["socketAmount"] as TrackBar).Value;
                            for (int s = 0; s < setNumSockets; s++)
                            {
                                Socket sock = new Socket();
                                sock.Effect = ((KeyValuePair<string, int>)((this.Controls["socketGroup" + s] as GroupBox).Controls["socketType"] as ComboBox).SelectedItem).Value;
                                if (((KeyValuePair<string, string>)((this.Controls["socketGroup" + s] as GroupBox).Controls["socketedGem"] as ComboBox).SelectedItem).Key != "[EMPTY]")
                                {
                                    sock.Gem = new Gem()
                                    {
                                        Name = ((KeyValuePair<string, string>)((this.Controls["socketGroup" + s] as GroupBox).Controls["socketedGem"] as ComboBox).SelectedItem).Value
                                    };
                                }
                                currentSockets.Add(sock);
                            }
                            inventoryGrid[i].Sockets = currentSockets;
                        }
                        if (selectedNode.Name == "Value") inventoryGrid[i].Value = ((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text;
                        if (selectedNode.Name == "Level") inventoryGrid[i].Level = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                        switch (inventoryGrid[i].Type)
                        {
                            case (int)InventoryManager.typeMap.Weapon:
                                if (selectedNode.Name == "DamageMin") inventoryGrid[i].Weapon.DamageMin = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "DamageMax") inventoryGrid[i].Weapon.DamageMax = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (inventoryGrid[i].ItemType == "Shield")
                                {
                                    if (selectedNode.Name == "ShieldResistance") inventoryGrid[i].Weapon.ShieldResistance = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                    if (selectedNode.Name == "ShieldBlockChance") inventoryGrid[i].Weapon.ShieldBlockChance = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                    if (selectedNode.Name == "ShieldBlockEfficiency") inventoryGrid[i].Weapon.ShieldBlockEfficiency = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                } else {
                                    if (selectedNode.Name == "ResourceGeneration") inventoryGrid[i].Weapon.ResourceGeneration = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                }
                                break;
                            case (int)InventoryManager.typeMap.Armor:
                                if (selectedNode.Name == "Armor") inventoryGrid[i].Armor.Armor = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "Health") inventoryGrid[i].Armor.Health = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "Resistance") inventoryGrid[i].Armor.Resistance = Convert.ToDouble(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                break;
                            case (int)InventoryManager.typeMap.Potion:
                                if (selectedNode.Name == "Charge") inventoryGrid[i].Potion.Charge = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateHP") inventoryGrid[i].Potion.ImmediateHP = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateMana") inventoryGrid[i].Potion.ImmediateMana = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                if (selectedNode.Name == "ImmediateStamina") inventoryGrid[i].Potion.ImmediateStamina = Convert.ToInt32(((sender as Button).Parent.Controls["txtStat0"] as TextBox).Text);
                                break;
                        }
                    }
                }
            }
            
            if ((this.Controls["deleteAffix"] as Button) != null) (this.Controls["deleteAffix"] as Button).Enabled = false;
            if ((this.Controls["defaultAffix"] as CheckBox) != null) (this.Controls["defaultAffix"] as CheckBox).Checked = false;
            LoadCurrentAffixes(_panelid);
            if (panelID == -1)
            {
                InventoryManager.ReloadInventoryBitmap(accessablePictureBox.Parent as Panel, x, y);
                ItemDataDisplay.LoadItemData(accessablePictureBox, ((accessablePictureBox.Parent as Panel).Parent as TabPage).Controls["itemStatDisplay"] as Panel, cData.Character, "InventoryGrid");
            }
            else
            {
                StashManager.ReloadGridBitmap(accessablePictureBox.Parent as Panel, x, y, _panelid);
                ItemDataDisplay.LoadItemData(accessablePictureBox, ((accessablePictureBox.Parent as Panel).Parent as TabPage).Controls["itemStashStatDisplay"] as Panel, cData.PlayerChest.Panels, "InventoryGrid");
            }
            RemoveItemEditControls();
        }

        private void StatEditView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = (sender as TreeView).SelectedNode;
            if (selectedNode.Nodes.Count > 0) return;
            string[] parameters = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] parameterValues = selectedNode.SelectedImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (!selectedNode.FullPath.Contains("Current Affixes"))
            {
                (this.Controls["addSelectedStat"] as Button).Text = "Add Selected Affix";
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
                (this.Controls["deleteAffix"] as Button).Enabled = (selectedNode.FullPath.Contains("Default Affixes") || selectedNode.FullPath.Contains("Rolled Affixes") ? true : false);
                (this.Controls["addSelectedStat"] as Button).Text = "Update Selected Affix";
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
                Parent = this,
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
                    Parent = this,
                    Visible = true
                };
                ComboBox rarityBox = new ComboBox()
                {
                    Name = "cboRarity",
                    Location = new Point(480, 115),
                    Size = new Size(150, 20),
                    Parent = this,
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
                    Parent = this,
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
                    Parent = this,
                    Visible = true,
                    Minimum = 0,
                };
                int maxSockets = 0;
                string itemType = getItemTypeFromGrid(x, y);
                if(itemType != null) WolcenStaticData.MaxSocketsByType.TryGetValue(itemType, out maxSockets);
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
                    Parent = this,
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
                    Parent = this,
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
                        Parent = this,
                        Location = new Point(430 + (i * 42), 115)
                    };
                    stars.FlatAppearance.BorderSize = 0;
                    stars.FlatAppearance.CheckedBackColor = Color.Transparent;
                    stars.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    stars.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    stars.Click += Stars_Click;
                }
                if (parameterValues[0] != "0")
                {
                    (this.Controls["star" + parameterValues[0]] as CheckBox).Checked = true;
                    Stars_Click(this.Controls["star" + parameterValues[0]] as CheckBox, null);
                }
            }
            else
            {
                for (int i = 0; i < parameterValues.Count(); i++)
                {
                    Label title = new Label()
                    {
                        Name = "lblStat" + i,
                        Text = parameters.Count() <= 1 ? selectedNode.Text + ":" : parameters[i] + ":",
                        Location = new Point(480, 100 + (50 * i)),
                        AutoSize = true,
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Parent = this,
                        Visible = true
                    };
                    TextBox valueBox = new TextBox();
                    valueBox.Name = "txtStat" + i.ToString();
                    valueBox.Parent = this;
                    valueBox.Location = new Point(480, 115 + (50 * i));
                    valueBox.Size = new Size(150, 20);
                    valueBox.Visible = true;
                    if(parameters.Count() >= 1) valueBox.Enabled = parameters[i] == "TriggeredSkillSelector" ? false : true;
                    if (selectedNode.ImageKey != "default") valueBox.Text = parameterValues[i];
                    else valueBox.Text = parameterValues[0];
                    valueBox.KeyPress += numberOnly_KeyPress;
                }
            }

            if (selectedNode.FullPath.Contains("Current Affixes")) (this.Controls["defaultAffix"] as CheckBox).Visible = false;
            else (this.Controls["defaultAffix"] as CheckBox).Visible = true;
        }

        private string getItemTypeFromGrid(int x, int y)
        {
            return selectedGridItem.ItemType;
        }

        private void numberOnly_KeyPress(object sender, KeyPressEventArgs e)
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

        private void Stars_Click(object sender, EventArgs e)
        {
            int checkAmount = Convert.ToInt32((sender as CheckBox).Name.Substring(4, 1));
            if ((sender as CheckBox).Checked == false)
            {
                for (int i = 5; i > checkAmount; i--)
                {
                    (this.Controls["star" + i.ToString()] as CheckBox).Checked = false;
                    starCheckChanged((this.Controls["star" + i.ToString()] as CheckBox), e);
                }
            }
            else
            {
                for (int i = 1; i <= checkAmount; i++)
                {
                    (this.Controls["star" + i.ToString()] as CheckBox).Checked = true;
                    starCheckChanged((this.Controls["star" + i.ToString()] as CheckBox), e);
                }
            }
            ItemQuality = checkAmount;
        }

        private void starCheckChanged(object sender, EventArgs e)
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

        private void NumberOfSockets_ValueChanged(object sender, EventArgs e)
        {
            TreeNode selectedNode = (this.Controls["statEditView"] as TreeView).SelectedNode;
            string[] socketEffect = selectedNode.StateImageKey.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] GemName = { "" };
            if (selectedNode.Tag != null) GemName = (selectedNode.Tag as string).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            (this.Controls["tickAmount"] as Label).Text = (this.Controls["socketAmount"] as TrackBar).Value.ToString();
            int socketsAvailable = (this.Controls["socketAmount"] as TrackBar).Value;

            for (int i = 0; i < 4; i++)
            {
                this.Controls.RemoveByKey("socketGroup" + i.ToString());
            }

            for (int i = 0; i < socketsAvailable; i++)
            {
                GroupBox socketGroup = new GroupBox()
                {
                    Name = "socketGroup" + i.ToString(),
                    Text = "Socket " + (i + 1).ToString(),
                    Size = new Size(200, 75),
                    Parent = this,
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
                    Parent = this.Controls["socketGroup" + i.ToString()],
                    BackgroundImageLayout = ImageLayout.Center
                };

                ComboBox cboSocket = new ComboBox()
                {
                    Name = "socketType",
                    Size = new Size(130, 15),
                    Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Regular),
                    Parent = this.Controls["socketGroup" + i.ToString()],
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
                if (socketEffect.Count() < socketsAvailable)
                {
                    if (i <= socketEffect.Count())
                    {
                        cboSocket.SelectedIndex = 0;
                        socketEffect = new string[i + 1];
                        for (int c = 0; c < socketEffect.Count(); c++)
                        {
                            socketEffect[c] = "0";
                        }
                    }
                }
                else { cboSocket.SelectedIndex = WolcenStaticData.SocketType.ElementAt(Convert.ToInt32(socketEffect[i])).Key; }

                ComboBox cboSocketed = new ComboBox()
                {
                    Name = "socketedGem",
                    Size = new Size(130, 15),
                    Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Regular),
                    Parent = this.Controls["socketGroup" + i.ToString()],
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

                gemDisplay.BackgroundImage = new Bitmap(Image.FromFile(Directory.GetCurrentDirectory() + WolcenStaticData.SocketImageLocation[Convert.ToInt32(socketEffect[i])]), 35, 35);
                if (i < GemName.Count())
                {
                    if (GemName[i] != "")
                    {
                        gemDisplay.Image = new Bitmap(Image.FromFile(Directory.GetCurrentDirectory() + WolcenStaticData.ItemLocations[GemName[i]]), 50, 50);
                    }
                }
            }
        }

        private static void CboSocket_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            (comboBox.Parent.Controls["gemDisplay"] as PictureBox).BackgroundImage = new Bitmap(Image.FromFile(Directory.GetCurrentDirectory() + WolcenStaticData.SocketImageLocation[((KeyValuePair<string, int>)comboBox.SelectedItem).Value]), 35, 35);
        }

        private static void CboSocketed_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            if (((KeyValuePair<string, string>)comboBox.SelectedItem).Value == "NULL") return;
            (comboBox.Parent.Controls["gemDisplay"] as PictureBox).Image = new Bitmap(Image.FromFile(Directory.GetCurrentDirectory() + WolcenStaticData.ItemLocations[((KeyValuePair<string, string>)comboBox.SelectedItem).Value]), 50, 50);
        }

        private void RemoveItemEditControls()
        {
            for (int i = 0; i < 6; i++)
            {
                this.Controls.RemoveByKey("star" + i.ToString());
                this.Controls.RemoveByKey("socketGroup" + i.ToString());
                this.Controls.RemoveByKey("tickAmount");
                this.Controls.RemoveByKey("socketAmount");
                this.Controls.RemoveByKey("cboRarity");
                this.Controls.RemoveByKey("defaultAffix");
                this.Controls.RemoveByKey("lblStat" + i.ToString());
                this.Controls.RemoveByKey("txtStat" + i.ToString());
            }
        }

        private void ItemsInGrid_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            RemoveItemEditControls();
            ListView listView = (sender as ListView);
            if (listView.SelectedItems.Count <= 0) return;
            PictureBox displayBox = ((sender as ListView).Parent as Form).Controls["displayItemView"] as PictureBox;
            ListViewItem selectedItem = listView.SelectedItems[0];
            int x = Convert.ToInt32(selectedItem.SubItems[0].Text);
            int y = Convert.ToInt32(selectedItem.SubItems[1].Text);
            string l_itemName = null;
            int _panelid = -1;

            if (panelID != -1) _panelid = Convert.ToInt32(selectedItem.SubItems[2].Text) - 1;

            if (_panelid == -1)
            {
                l_itemName = selectedItem.SubItems[2].Name;
                foreach (var iGrid in cData.Character.InventoryGrid)
                {
                    if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                    {
                        selectedGridItem = iGrid;
                        break;
                    }
                }
            }
            else
            {
                l_itemName = selectedItem.SubItems[3].Name;
                foreach (var iGrid in cData.PlayerChest.Panels[_panelid].InventoryGrid)
                {
                    if (iGrid.InventoryX == x && iGrid.InventoryY == y)
                    {
                        selectedGridItem = iGrid;
                        break;
                    }
                }
            }

            string itemLocation = null;

            WolcenStaticData.ItemLocations.TryGetValue(l_itemName, out itemLocation);
            if (itemLocation == null)
            {
                string[] enneractData;
                WolcenStaticData.ItemEnneract.TryGetValue(l_itemName, out enneractData);
                if (enneractData != null)
                {
                    itemLocation = enneractData[1];
                }
            }

            //LoadTreeNodes();
            LoadCurrentAffixes(_panelid);

            displayBox.Image = InventoryManager.getImageFromPath(Directory.GetCurrentDirectory() + itemLocation, displayBox.Size);
        }
    }
}
