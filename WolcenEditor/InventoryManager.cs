using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class InventoryManager
    {
        public static Dictionary<string, int> charMap = new Dictionary<string, int>
        {
            {"charHelm", 3 },
            {"charChest" , 1},
            {"charLPad", 6 },
            {"charRPad" , 5},
            {"charLHand", 10 },
            {"charRHand" , 9},
            {"charBelt", 19 },
            {"charPants", 11 },
            {"charNeck" , 14},
            {"charBoots", 17 },
            {"charLRing" , 22},
            {"charRRing", 21 },
            {"charLWeapon" , 15},
            {"charRWeapon" , 16},
        };

        public static void LoadCharacterInventory(object sender)
        {
            TabPage tabPage = (sender as TabPage);
            bool flip = false;

            foreach (Control control in tabPage.Controls)
            {
                try
                {
                    PictureBox pictureBox = (control as PictureBox);
                    if(charMap[pictureBox.Name] >= 0)
                    {
                        if (pictureBox.Name == "charLPad" || pictureBox.Name == "charLHand") flip = true;
                        pictureBox.Image = GetInventoryEquippedBitmap(charMap[pictureBox.Name], flip);
                        pictureBox.Click += LoadItemData;
                        //pictureBox.BackgroundImage = setRarityBackground(pictureBox);
                    }
                }
                catch (Exception) { }
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
                        if (inv.Armor != null)
                        {
                            if (inv.ItemType == "Belt" || inv.ItemType == "Ring" || inv.ItemType == "Amulet") pb.Image = GetInventoryGridBitmap(inv.Armor.Name, pb);
                            else
                            {
                                pb.Size = new Size(pb.Size.Width, pb.Size.Height + 50);
                                pb.Image = GetInventoryGridBitmap(inv.Armor.Name, pb);
                            }
                            
                        }
                        if (inv.Weapon != null)
                        {
                            pb.Size = new Size(pb.Size.Width, pb.Size.Height + 50);
                            pb.Image = GetInventoryGridBitmap(inv.Weapon.Name, pb);
                        }
                        if (inv.Gem != null)
                        {
                            pb.Image = GetInventoryGridBitmap(inv.Gem.Name, pb);
                        }
                    }
                }
            }
        }

        private static void Pb_DragDrop(object sender, DragEventArgs e)
        {
            Bitmap bmp = (e.Data.GetData(DataFormats.Bitmap) as Bitmap);
            (sender as PictureBox).Image = bmp;
        }

        private static void Pb_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private static void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as PictureBox).Image == null) return;
            Bitmap bmp = new Bitmap((sender as PictureBox).Image);
            if (bmp == null) return;
            if ((sender as PictureBox).DoDragDrop(bmp, DragDropEffects.Move) == DragDropEffects.Move)
            {
                (sender as PictureBox).Image = null;
            }
        }


        private static int posY = 0;

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
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        
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

        private static Image setRarityBackground(PictureBox pictureBox)
        {
            int bodyPart = charMap[pictureBox.Name];
            Image returnImage = null;
            if (pictureBox.Name == "charLRing" || pictureBox.Name == "charRRing") returnImage = WolcenEditor.Properties.Resources.charRing;
            else if (pictureBox.Name == "charLWeapon" || pictureBox.Name == "charRWeapon") returnImage = WolcenEditor.Properties.Resources.charWeapon;
            else returnImage = (Properties.Resources.ResourceManager.GetObject(pictureBox.Name) as Image);

            string dirPath = @".\UIResources\ItemBorders\";
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return new Bitmap(dirPath + WolcenStaticData.itemBordersByRarity[equip.Rarity]);
                }
            }
            return returnImage;
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

        private static Bitmap GetInventoryGridBitmap(string itemName, PictureBox pb)
        {
            string dirPath = @".\UIResources\";
            foreach (var i in cData.Character.InventoryGrid)
            {
                string imagePathName = "";
                if (i.Armor != null)
                {
                    if (i.Armor.Name == itemName)
                    {
                        if (WolcenStaticData.ItemArmor.ContainsKey(i.Armor.Name)) imagePathName = WolcenStaticData.ItemArmor[i.Armor.Name];
                        if (WolcenStaticData.ItemAccessories.ContainsKey(i.Armor.Name)) imagePathName = WolcenStaticData.ItemAccessories[i.Armor.Name];

                        return CombineGridBitmaps(dirPath, imagePathName, i.Rarity, pb);
                    }
                }
                else if (i.Weapon != null)
                {
                    if (i.Weapon.Name == itemName)
                    {
                        if (WolcenStaticData.ItemWeapon.ContainsKey(i.Weapon.Name)) imagePathName = WolcenStaticData.ItemWeapon[i.Weapon.Name];
                        if (WolcenStaticData.ItemAccessories.ContainsKey(i.Weapon.Name)) imagePathName = WolcenStaticData.ItemAccessories[i.Weapon.Name];

                        return CombineGridBitmaps(dirPath, imagePathName, i.Rarity, pb);
                    }
                }
                else if (i.Gem != null)
                {
                    if (i.Gem.Name == itemName)
                    {
                        return CombineGridBitmaps(dirPath, i.Gem.Name + ".png", i.Rarity, pb);
                    }
                }
            }
            return null;
        }

        private static Bitmap CombineGridBitmaps(string dirPath, string itemName, int quality, PictureBox pb)
        {
            Bitmap Background = new Bitmap(Image.FromFile(dirPath + "ItemBorders\\" + quality + ".png"), pb.Width, pb.Height);
            Bitmap ItemImage = new Bitmap(Image.FromFile(dirPath + "Items\\" + itemName));
            Bitmap FinalImage = new Bitmap(pb.Width, pb.Height);

            using (Graphics g = Graphics.FromImage(FinalImage))
            {
                g.Clear(Color.Black);
                g.DrawImage(Background, 0, 0);
                g.DrawImage(ItemImage, 5, 5, pb.Width - 10, pb.Height - 10);
            }

            return FinalImage;
        }

        public static void EditItem(object sender, ToolStripItemClickedEventArgs e)
        {
            var contextMenuEditItem = (sender as ContextMenuStrip);
            var bodyPartName = contextMenuEditItem.SourceControl?.Name;
            var selectedOption = e.ClickedItem;
            if (selectedOption.Text == "Edit Sockets")
            {
                LoadSocketEditor(bodyPartName);
            }
            //PictureBox pictureBox = (sender as PictureBox);
        }

        private static void LoadSocketEditor(string bodyPartName)
        {
            var socketCount = new Dictionary<string, int>
            {
                {"charHelm", 2 },
                {"charChest" , 3},
                {"charLPad", 0 },
                {"charRPad" , 0},
                {"charLHand", 0 },
                {"charRHand" , 0},
                {"charBelt", 1 },
                {"charPants", 2 },
                {"charNeck" , 1},
                {"charBoots", 0 },
                {"charLRing" , 1},
                {"charRRing", 1 },
                {"charLWeapon" , 3},
                {"charRWeapon" , 3},
            };
            if(socketCount[bodyPartName] == 0)
                return;

            // Setup Form
            var socketForm = new Form();
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, socketForm, new object[] { true });

            var resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            socketForm.Width = 550;
            socketForm.Height = 200;
            socketForm.BackgroundImage = ((Image)(resources.GetObject("charInv.BackgroundImage")));
            socketForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            socketForm.MaximizeBox = false;
            socketForm.MinimizeBox = false;
            socketForm.StartPosition = FormStartPosition.CenterParent;

            #region Socket Controls

            var socketLabel = new Label
            {
                Width = 50,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Text = "Sockets",
                Location = new Point(40, 10)
            };
            socketForm.Controls.Add(socketLabel);


            var addSocketButton = new Button
            {
                Width = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "+",
                Location = new Point(90, 10)
            };
            socketForm.Controls.Add(addSocketButton);


            var removeSocketButton = new Button
            {
                Width = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "-",
                Location = new Point(20, 10)
            };
            socketForm.Controls.Add(removeSocketButton);

            #endregion

            #region Gem Controls
            var gemLabel = new Label
            {
                Width = 50,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Text = "Gems:",
                Location = new Point(180, 10)
            };
            socketForm.Controls.Add(gemLabel);

            var addGemButton = new Button
            {
                Width = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "+",
                Location = new Point(230, 10)
            };
            socketForm.Controls.Add(addGemButton);



            var removeGemButton = new Button
            {
                Width = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "-",
                Location = new Point(160, 10)
            };
            socketForm.Controls.Add(removeGemButton);

            #endregion

            var sockets = getSockets(charMap[bodyPartName]);

            addSocketButton.Click += AddSocketButton_Click;
            removeSocketButton.Click += RemoveSocketButton_Click;
            addGemButton.Click += AddGemButton_Click;
            removeGemButton.Click += RemoveGemButton_Click;

            void AddSocketButton_Click(object sender, EventArgs e)
            {
                if(sockets != null && sockets.Count < socketCount[bodyPartName])
                {
                    sockets.Add(new Socket { });
                    createSocketDropDowns(sockets, socketForm);

                }
            }

            void RemoveSocketButton_Click(object sender, EventArgs e)
            {
                if (sockets != null && sockets.Count >= 1)
                {
                    sockets.RemoveAt(sockets.Count - 1);
                    createSocketDropDowns(sockets, socketForm);

                }
            }

             void AddGemButton_Click(object sender, EventArgs e)
            {
                if (sockets != null && sockets.Count >= 1)
                {
                    foreach (var socket in sockets)
                    {
                        if (socket.Gem == null)
                        {
                            socket.Gem = new Gem { Name = "Fire_Gem_Tier_01" };
                            break;
                        }
                    }
                    createSocketDropDowns(sockets, socketForm);
                }
            }

            void RemoveGemButton_Click(object sender, EventArgs e)
            {
                if (sockets != null && sockets.Count >= 1)
                {
                    for(int i = (sockets.Count - 1); i >= 0; i--)
                    {
                        if(sockets[i].Gem != null)
                        {
                            sockets[i].Gem = null;
                            createSocketDropDowns(sockets, socketForm);
                            break;
                        }

                    }
                }
            }

            createSocketDropDowns(sockets, socketForm);

            socketForm.ShowDialog();
        }


        private static void createSocketDropDowns(List<Socket> sockets, Form socketForm)
        {
            socketForm.Controls.RemoveByKey("SocketCombo_1");
            socketForm.Controls.RemoveByKey("SocketCombo_2");
            socketForm.Controls.RemoveByKey("SocketCombo_3");

            try
            {
                socketForm.Controls.RemoveByKey("GemCombo_1");
            }
            catch (ArgumentNullException ex) { }
            try
            {
                socketForm.Controls.RemoveByKey("GemCombo_2");

            }
            catch (ArgumentNullException ex) { }
            try
            {
                socketForm.Controls.RemoveByKey("GemCombo_3");

            }
            catch (ArgumentNullException ex) { }

            if (sockets != null)
            {
                var y = 1;
                //create a combobox for each socket
                foreach (var socket in sockets)
                {
                    if (socket != null)
                    {

                        var socketComboBox = new ComboBox();
                        socketComboBox.Width = 100;
                        socketComboBox.Location = new Point(10, 15 + (y * 30));
                        socketComboBox.Name = "SocketCombo_" + y.ToString();
                        socketComboBox.DataSource = new BindingSource(WolcenStaticData.SocketType, null);
                        socketComboBox.DisplayMember = "Value";
                        socketComboBox.ValueMember = "Key";
                        socketComboBox.DataBindings.Add("SelectedValue", socket, "Effect", true, DataSourceUpdateMode.OnPropertyChanged);
                        socketForm.Controls.Add(socketComboBox);

                        if (socket.Gem != null)
                        {
                            var gemComboBox = new ComboBox();
                            gemComboBox.Location = new Point(150, 15 + (y * 30));
                            gemComboBox.Name = "GemCombo_" + y.ToString();
                            gemComboBox.DataSource = new BindingSource(WolcenStaticData.GemLocalization, null);
                            gemComboBox.DisplayMember = "Value";
                            gemComboBox.ValueMember = "Key";
                            gemComboBox.DataBindings.Add("SelectedValue", socket, "Gem.Name", true, DataSourceUpdateMode.OnPropertyChanged);
                            socketForm.Controls.Add(gemComboBox);
                            var GemEffectsLabel = new Label
                            {
                                Width = 500,
                                Height = 20,
                                TextAlign = ContentAlignment.MiddleLeft,
                                BackColor = Color.Orange,
                                ForeColor = Color.White,
                                Location = new Point(270, 15 + (y * 30))
                            };
                            var text = getGemStats(socket.Gem.Name, socket.Effect);

                            //GemEffectsLabel.DataBindings.Add("Text", text, null, true, DataSourceUpdateMode.OnPropertyChanged);
                            //GemEffectsLabel.DataBindings.Add("SelectedValue", socket, "Effect", true, DataSourceUpdateMode.OnPropertyChanged);
                            socketForm.Controls.Add(GemEffectsLabel);

                            socketComboBox.SelectedValueChanged += SocketComboBox_SelectedValueChanged;
                            gemComboBox.SelectedValueChanged += SocketComboBox_SelectedValueChanged;


                            void SocketComboBox_SelectedValueChanged(object sender, EventArgs e)
                            {
                                GemEffectsLabel.Text = getGemStats(socket.Gem.Name, socket.Effect);
                            }



                        }
                    }
                    y++;
                }
            }
        }



        private static Bitmap GetInventoryEquippedBitmap(int bodyPart, bool flip = false)
        {
            foreach (var i in cData.Character.InventoryEquipped)
            {
                string dirPath = @".\UIResources\Items\";
                if (i.BodyPart == bodyPart)
                {
                    string itemName = "";
                    int itemRarity = i.Rarity;
                    if (bodyPart == 16 || bodyPart == 15)
                        itemName = WolcenStaticData.ItemWeapon[i.Weapon.Name];
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemName = WolcenStaticData.ItemAccessories[i.Armor.Name];
                    else
                        itemName = WolcenStaticData.ItemArmor[i.Armor.Name];

                    if (File.Exists(dirPath + itemName))
                    {
                        if (flip == true)
                        {
                            Bitmap bmp = ResizeAndCombine(dirPath, itemName, itemRarity);
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                            return bmp;
                        }
                        else
                        {
                            return ResizeAndCombine(dirPath, itemName, itemRarity);
                        }
                    }
                    else
                    {
                        if (i.BodyPart == 15 || i.BodyPart == 16)
                        {
                            return new Bitmap(Image.FromFile(dirPath + "unknown_weapon.png"));
                        }
                        else
                        {
                            return new Bitmap(Image.FromFile(dirPath + "unknown_armor.png"));
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

            var itemImage = new Bitmap(Image.FromFile(dirPath + itemName));

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
