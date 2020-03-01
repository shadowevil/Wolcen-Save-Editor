using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Helpers;
using System.Windows.Forms;
using System.Linq;

namespace WolcenEditor
{
    public partial class Form1 : Form
    {
        public static string saveFilePath;

        public Form1()
        {
            InitializeComponent();

            panel1.Enabled = true;

            this.Resize += Form1_Resize;
            charGold.KeyPress += numberOnly_KeyPress;
            charPrimordial.KeyPress += numberOnly_KeyPress;
            charFerocity.KeyPress += numberOnly_KeyPress;
            charAgility.KeyPress += numberOnly_KeyPress;
            charToughness.KeyPress += numberOnly_KeyPress;
            charWillpower.KeyPress += numberOnly_KeyPress;
            charExp.KeyPress += numberOnly_KeyPress;
            charLevel.KeyPress += numberOnly_KeyPress;

            cboFace.SelectedIndexChanged += _SelectedIndexChanged;
            cboHaircut.SelectedIndexChanged += _SelectedIndexChanged;
            cboHairColor.SelectedIndexChanged += _SelectedIndexChanged;
            cboBeard.SelectedIndexChanged += _SelectedIndexChanged;
            cboBeardColor.SelectedIndexChanged += _SelectedIndexChanged;
            cboLEye.SelectedIndexChanged += _SelectedIndexChanged;
            cboREye.SelectedIndexChanged += _SelectedIndexChanged;
            cboSkinColor.SelectedIndexChanged += _SelectedIndexChanged;

            panel1.SelectedIndexChanged += Panel1_SelectedIndexChanged;

            LoadComboBoxes();
        }



        private void Panel1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as TabControl).SelectedTab.Text == "Inventory")
            {
                this.Height += 35;
            }
            else
            {
                this.Height = 595;
            }
        }

        private void _SelectedIndexChanged(object sender, EventArgs e)
        {
            int value = Convert.ToInt32((sender as ImageComboBox).SelectedItem.ToString());

            switch ((sender as ImageComboBox).Name)
            {
                case "cboSkinColor": cData.Character.CharacterCustomization.SkinColor = value; break;
                case "cboBeard": cData.Character.CharacterCustomization.Beard = value; break;
                case "cboBeadColor": cData.Character.CharacterCustomization.BeardColor = value; break;
                case "cboREye": cData.Character.CharacterCustomization.RightEye = value; break;
                case "cboLEye": cData.Character.CharacterCustomization.LeftEye = value; break;
                case "cboHairColor": cData.Character.CharacterCustomization.HairColor = value; break;
                case "cboHaircut": cData.Character.CharacterCustomization.Haircut = value; break;
                case "cboFace": cData.Character.CharacterCustomization.Face = value; break;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Size = new Size(this.Width - 10, this.Height - 65);
        }

        private void LoadComboBoxes()
        {
            LoadComboColor(WolcenStaticData.HairColorBank, 100, 45, ref cboHairColor);
            LoadComboColor(WolcenStaticData.HairColorBank, 100, 45, ref cboBeardColor);
            LoadComboColor(WolcenStaticData.SkinColor, 100, 20, ref cboSkinColor);
            LoadCombo(WolcenStaticData.Face, @".\UIResources\Character\Head\", ref cboFace);
            LoadCombo(WolcenStaticData.HairStyle, @".\UIResources\Character\Hair\", ref cboHaircut);
            LoadCombo(WolcenStaticData.EyeColor, @".\UIResources\Character\Eyes\", ref cboLEye);
            LoadCombo(WolcenStaticData.EyeColor, @".\UIResources\Character\Eyes\", ref cboREye);
            LoadCombo(WolcenStaticData.Beard, @".\UIResources\Character\Beard\", ref cboBeard);
        }

        private void LoadComboColor(Dictionary<int, string> dictionary, int width, int height, ref ImageComboBox imgBox)
        {
            foreach (var d in dictionary)
            {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (SolidBrush b = new SolidBrush(ColorTranslator.FromHtml(d.Value)))
                    {
                        g.FillRectangle(b, 0, 0, width, height);
                    }
                }
                DropDownItem i = new DropDownItem(d.Key.ToString(), ColorTranslator.FromHtml(d.Value), width, height);
                i.Image = bmp;
                imgBox.Items.Add(i);
            }
        }

        private void LoadCombo(Dictionary<int, string> dictionary, string path, ref ImageComboBox imgBox)
        {
            foreach (var d in dictionary)
            {
                int width = 45, height = 45;
                Bitmap bmp = new Bitmap(Image.FromFile(path + d.Value), width, height);
                DropDownItem i = new DropDownItem(d.Key.ToString(), bmp);
                imgBox.Items.Add(i);
            }
        }

        private void numberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string js = string.Empty;
            var d = new OpenFileDialog();
            d.Title = "Open Wolcen Save Game File";
            d.Filter = "JSON files|*.json";
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string WolcenSavePath = "\\Saved Games\\wolcen\\savegames\\characters\\";
            d.InitialDirectory = userFolder + WolcenSavePath;
            if (d.ShowDialog() == DialogResult.OK)
            {
                cData.Character = CharacterIO.ReadCharacter(d.FileName);
                saveFilePath = d.FileName;
            }
            else { return; }

            panel1.Enabled = true;

            LoadCharacterData();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cData.Character != null)
            {
                CharacterIO.WriteCharacter(saveFilePath, cData.Character);
            }
        }

        private void LoadCharacterData()
        {
            SetIndexToValueOf(ref cboFace, cData.Character.CharacterCustomization.Face);
            SetIndexToValueOf(ref cboHaircut, cData.Character.CharacterCustomization.Haircut);
            SetIndexToValueOf(ref cboHairColor, cData.Character.CharacterCustomization.HairColor);
            SetIndexToValueOf(ref cboBeard, cData.Character.CharacterCustomization.Beard);
            SetIndexToValueOf(ref cboBeardColor, cData.Character.CharacterCustomization.BeardColor);
            SetIndexToValueOf(ref cboLEye, cData.Character.CharacterCustomization.LeftEye);
            SetIndexToValueOf(ref cboREye, cData.Character.CharacterCustomization.RightEye);
            SetIndexToValueOf(ref cboSkinColor, cData.Character.CharacterCustomization.SkinColor);

            BindToComboBox(cboGender, WolcenStaticData.Gender, cData.Character.CharacterCustomization, "Sex");
            SetBinding(ref charName, cData.Character, "Name");
            SetBinding(ref charLevel, cData.Character.Stats, "Level");
            SetBinding(ref charExp, cData.Character.Stats, "CurrentXP");
            SetBinding(ref charFerocity, cData.Character.Stats, "Strength");
            SetBinding(ref charToughness, cData.Character.Stats, "Constitution");
            SetBinding(ref charAgility, cData.Character.Stats, "Agility");
            SetBinding(ref charWillpower, cData.Character.Stats, "Power");
            SetBinding(ref charGold, cData.Character.Stats, "Gold");
            SetBinding(ref charPrimordial, cData.Character.Stats, "PrimordialAffinity");

            LoadCharacterInventory();
        }

        private void LoadCharacterInventory()
        {
            charHelm.Image = GetItemBitmap(3);
            charChest.Image = GetItemBitmap(1);
            charLPad.Image = GetItemBitmap(6, true);
            charRPad.Image = GetItemBitmap(5);
            charLHand.Image = GetItemBitmap(10, true);
            charRHand.Image = GetItemBitmap(9);
            charBelt.Image = GetItemBitmap(19);
            charPants.Image = GetItemBitmap(11);
            charNeck.Image = GetItemBitmap(14);
            charBoots.Image = GetItemBitmap(17);
            charLRing.Image = GetItemBitmap(22, true);
            charRRing.Image = GetItemBitmap(21);
            charLWeapon.Image = GetItemBitmap(15, true);
            charRWeapon.Image = GetItemBitmap(16);

            charHelm.Click += LoadItemData;
            charChest.Click += LoadItemData;

            charLPad.Click += LoadItemData;
            charRPad.Click += LoadItemData;

            charLHand.Click += LoadItemData;
            charRHand.Click += LoadItemData;

            charBelt.Click += LoadItemData;
            charPants.Click += LoadItemData;

            charNeck.Click += LoadItemData;
            charBoots.Click += LoadItemData;

            charLRing.Click += LoadItemData;
            charRRing.Click += LoadItemData;

            charLWeapon.Click += LoadItemData;
            charRWeapon.Click += LoadItemData;

            //this is for when above values use MouseEnter instead of Click
            //charInv.MouseEnter += UnLoadItemData;
        }
        private void UnLoadItemData(object sender, EventArgs e)
        {
            listBoxEquipItems.DataSource = new List<string>();
        }

        private void LoadItemData(object sender, EventArgs e)
        {
            var charMap = new Dictionary<string,int>
            {
                {"charHelm", 3 },
                {"charChest" , 1},
                {"charLPad", 5 },
                {"charRPad" , 6},
                {"charLHand", 9 },
                {"charRHand" , 10},
                {"charBelt", 19 },
                {"charPants", 11 },
                {"charNeck" , 14},
                {"charBoots", 17 },
                {"charLRing" , 22},
                {"charRRing", 21 },
                {"charLWeapon" , 15},
                {"charRWeapon" , 16},
            };
            var rarityMap = new Dictionary<string, string>
            {
                {"0", "Basic"},
                {"1", "Basic"},
                {"2", "Magic"},
                {"3", "Rare"},
                {"5", "Set"},
                {"6", "Unique"},
                {"7", "Quest"}
            };

            var picBoxName = ((PictureBox)sender).Name;
            var statList = new List<string>();

            //WIP currently supports armor and weapons. Sockets and affixes are still needed
            foreach (var item in cData.Character.InventoryEquipped)
            {
                if (item.BodyPart == charMap[picBoxName])
                {
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var statName = prop.Name;
                        var statValue = prop.GetValue(item, null);
                        if (statName == "BodyPart" || statName == "Type" || statValue == null)
                            continue;

                        if (statName == "Rarity")
                            statValue = rarityMap[statValue.ToString()];

                        statList.Add($"{statName}: {statValue}");
                        if (prop.PropertyType == typeof(ItemArmor) && item.Armor != null || prop.PropertyType == typeof(ItemWeapon) && item.Weapon != null)
                        {
                            foreach (var inner in prop.PropertyType.GetProperties())
                            {
                                    var innerName = inner.Name;
                                    var innerValue = inner.GetValue(statValue, null).ToString();
                                    if (innerValue == "0")
                                        continue;
                                    statList.Add($"{innerName}: {innerValue}");
                            }
                        }
                    }
                }
            }
            listBoxEquipItems.DataSource = new BindingSource(statList, null);
        }

        // Body Parts:
        //  - Chest:             1
        //  - Helmet:            3
        //  - (right) Shoulder:  5
        //  - (left) Shoulder:   6
        //  - (right) Glove:     9
        //  - (left) Glove:      10
        //  - Pants:             11
        //  - Necklace:          14
        //  - Weapon 1:          15
        //  - Weapon 2:          16
        //  - Feet:              17
        //  - Belt:              19
        //  - (right) Ring:      21
        //  - (left) Ring:       22

        private Bitmap GetItemBitmap(int bodyPart, bool flip = false)
        {
            foreach (var i in cData.Character.InventoryEquipped)
            {
                string dirPath = @".\UIResources\Items\";
                if (i.BodyPart == bodyPart)
                {
                    string itemName ="";
                    //string itemName = bodyPart == 16 || bodyPart == 15 ? i.Weapon.Name : i.Armor.Name;
                    if (bodyPart == 16 || bodyPart == 15)
                        itemName = i.Weapon.Name;
                    else
                    if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemName = WolcenStaticData.ItemAccessories[i.Armor.Name];
                    else
                        itemName = i.Armor.Name;

                    if (File.Exists(dirPath + itemName + ".png"))
                    {
                        if (flip == true)
                        {
                            Bitmap bmp = new Bitmap(Image.FromFile(dirPath + itemName + ".png"));
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                            return bmp;
                        }
                        else
                        {
                            return new Bitmap(Image.FromFile(dirPath + itemName + ".png"));
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

        private void SetBinding(ref TextBox obj, object dataSource, string dataMember)
        {
            obj.DataBindings.Add("Text", dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void SetIndexToValueOf(ref ImageComboBox cbo, int Value)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (cbo.Items[i].ToString() == Value.ToString())
                {
                    cbo.SelectedIndex = i;
                }
            }
        }

        private void BindToComboBox<T>(T comboBox, Dictionary<int, string> mapping, object dataSource, string dataMemeber) where T : ComboBox
        {
            comboBox.DataSource = new BindingSource(mapping, null);
            comboBox.DisplayMember = "Value";
            comboBox.ValueMember = "Key";
            comboBox.DataBindings.Add("SelectedValue", dataSource, dataMemeber, true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void charBelt1_CheckedChanged(object sender, EventArgs e)
        {
            if (charBelt1.Checked == true)
            {
                charBelt1.BackgroundImage = WolcenEditor.Properties.Resources.c_beltSlot;
            }
            else
            {
                charBelt1.BackgroundImage = WolcenEditor.Properties.Resources.e_beltSlot;
            }
        }

        private void charBelt2_CheckedChanged(object sender, EventArgs e)
        {
            if (charBelt2.Checked == true)
            {
                charBelt2.BackgroundImage = WolcenEditor.Properties.Resources.c_beltSlot;
            }
            else
            {
                charBelt2.BackgroundImage = WolcenEditor.Properties.Resources.e_beltSlot;
            }
        }
    }

    public static class cData
    {
        private static CharacterData character;

        public static CharacterData Character
        {
            get
            {
                return character;
            }
            set
            {
                character = value;
            }
        }

    }
}
