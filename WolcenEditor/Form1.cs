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

namespace WolcenEditor
{
    public partial class Form1 : Form
    {
        public static string characterSavePath;
        public static string playerDataSavePath;
        public string WindowName = "Wolcen Save Editor";
        public bool hasSaved = false;

        public Form1()
        {
            InitializeComponent();
            InitForm();
        }

        public void InitForm()
        {
            this.Resize += Form1_Resize;
            panel1.Enabled = true;
            
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
            this.KeyDown += Panel1_KeyDown;
            this.KeyUp += Panel1_KeyUp;

            LoadComboBoxes();
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemStatDisplay, new object[] { true });
        }

        private void UnloadRandomInventory()
        {
            cData.Character = null;
            cData.PlayerData = null;
            this.Controls.Clear();
            InitializeComponent();
            InitForm();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Size = new Size(panel1.Width, this.Height - 62);
        }

        private void _SelectedIndexChanged(object sender, EventArgs e)
        {
            int value = Convert.ToInt32((sender as ImageComboBox).SelectedItem.ToString());

            switch ((sender as ImageComboBox).Name)
            {
                case "cboSkinColor": cData.Character.CharacterCustomization.SkinColor = value; break;
                case "cboBeard": cData.Character.CharacterCustomization.Beard = value; break;
                case "cboBeardColor": cData.Character.CharacterCustomization.BeardColor = value; break;
                case "cboREye": cData.Character.CharacterCustomization.RightEye = value; break;
                case "cboLEye": cData.Character.CharacterCustomization.LeftEye = value; break;
                case "cboHairColor": cData.Character.CharacterCustomization.HairColor = value; break;
                case "cboHaircut": cData.Character.CharacterCustomization.Haircut = value; break;
                case "cboFace": cData.Character.CharacterCustomization.Face = value; break;
            }
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

        private bool onCloseCheck(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Text != "Open")
            {
                if ((sender as ToolStripMenuItem).Text != "Exit" && cData.Character == null && cData.PlayerData == null)
                {
                    return false;
                }
            }
            if (!hasSaved && cData.Character != null && cData.PlayerData != null)
            {
                DialogResult dr = MessageBox.Show("You have not yet saved your changes, would you like to save your character?",
                    "Warning!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    return true;
                }
                else if (dr == DialogResult.No)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void SaveProfile()
        {
            if (cData.Character != null && cData.PlayerData != null)
            {
                Count c = new Count();
                c.Total = charGold.Text;
                var buffer = new byte[sizeof(UInt64)];
                new Random().NextBytes(buffer);
                UInt64 rnd = BitConverter.ToUInt64(buffer, 0);
                c.PerLevel = (rnd % (Convert.ToUInt64(charGold.Text) - 0) + 0).ToString();
                cData.Character.Telemetry.GoldDropped = c;
                cData.Character.Telemetry.GoldGainedQuests = c;
                cData.Character.Telemetry.GoldGainedMerchant = c;
                cData.Character.Telemetry.GoldPicked = c;
                CharacterIO.WriteCharacter(characterSavePath, cData.Character);
                PlayerDataIO.WritePlayerData(playerDataSavePath, cData.PlayerData);
                hasSaved = true;
            }
        }

        private void LoadCharacterData()
        {
            LoadPlayerData();
            this.Text = WindowName + " - " + cData.Character.Name;

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
            charGold.Text = cData.Character.Stats.Gold.ToString();
            SetBinding(ref charGold, cData.Character.Stats, "Gold");
            charPrimordial.Text = cData.Character.Stats.PrimordialAffinity;
            SetBinding(ref charPrimordial, cData.Character.Stats, "PrimordialAffinity");

            if(cData.Character.ApocalypticData.UnlockedTypes.Count == 4)
                apocUnlockCheckBox.Checked = true;
            
            InventoryManager.LoadCharacterInventory(panel1.Controls["charInv"]);

            SkillTree.LoadSkillInformation(ref panel1);
        }

        private void LoadPlayerData()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string WolcenPlayerData = userFolder + "\\Saved Games\\wolcen\\savegames\\playerdata.json";
            if (File.Exists(WolcenPlayerData))
            {
                if (cData.PlayerData != null) cData.PlayerData = null;
                cData.PlayerData = PlayerDataIO.ReadPlayerData(WolcenPlayerData);
                playerDataSavePath = WolcenPlayerData;

                CheckPlayerData();
            }
        }

        private void CheckPlayerData()
        {
            if (cData.PlayerData.AccountCosmeticInventory.CosmeticColorsUnlocked.bitmask == PlayerDataIO.ColorsUnlockedBitmask)
            {
                if (cData.PlayerData.AccountCosmeticInventory.CosmeticWeaponsUnlocked.bitmask == PlayerDataIO.WeaponsUnlockedBitmask)
                {
                    if (cData.PlayerData.AccountCosmeticInventory.CosmeticArmorsUnlocked.bitmask == PlayerDataIO.ArmorsUnlockedBitmask)
                    {
                        chkAllCosmetics.Checked = true;
                    } else chkAllCosmetics.Checked = false;
                } else chkAllCosmetics.Checked = false;
            } else chkAllCosmetics.Checked = false;

            if (cData.PlayerData.SoftcoreNormal.CompletedStory)
            {
                chkChampion.Checked = true;
            } else chkChampion.Checked = false;
        }

        private void SetBinding(ref TextBox obj, object dataSource, string dataMember)
        {
            obj.ResetBindings();
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
            comboBox.ResetBindings();
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (cData.PlayerData == null) return;
            if (chkAllCosmetics.Checked == false)
            {
                cData.PlayerData.AccountCosmeticInventory.CosmeticArmorsUnlocked.bitmask = PlayerDataIO.ArmorsUnlockedBitmask.Replace("f", "0");
                cData.PlayerData.AccountCosmeticInventory.CosmeticColorsUnlocked.bitmask = PlayerDataIO.ColorsUnlockedBitmask.Replace("f", "0");
                cData.PlayerData.AccountCosmeticInventory.CosmeticWeaponsUnlocked.bitmask = PlayerDataIO.WeaponsUnlockedBitmask.Replace("f", "0");
            }
            else if (chkAllCosmetics.Checked == true)
            {
                cData.PlayerData.AccountCosmeticInventory.CosmeticArmorsUnlocked.bitmask = PlayerDataIO.ArmorsUnlockedBitmask;
                cData.PlayerData.AccountCosmeticInventory.CosmeticColorsUnlocked.bitmask = PlayerDataIO.ColorsUnlockedBitmask;
                cData.PlayerData.AccountCosmeticInventory.CosmeticWeaponsUnlocked.bitmask = PlayerDataIO.WeaponsUnlockedBitmask;
            }
        }

        private void chkChampion_CheckedChanged(object sender, EventArgs e)
        {
            if (cData.PlayerData == null) return;
            if (chkChampion.Checked)
            {
                cData.PlayerData.SoftcoreNormal.CompletedStory = true;
            }
            else
            {
                cData.PlayerData.SoftcoreNormal.CompletedStory = false;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProfile();
        }

        private void closeStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onCloseCheck(sender, e) == false) return;
            cData.Character = null;
            cData.PlayerData = null;
            this.Controls.Clear();
            InitializeComponent();
            InitForm();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (onCloseCheck(sender, e) == false) return;
            Environment.Exit(0);
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
            if (onCloseCheck(sender, e) == false) return;
            UnloadRandomInventory();
            string js = string.Empty;
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Title = "Open Wolcen Save Game File";
                d.Filter = "JSON files|*.json";
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string WolcenSavePath = "\\Saved Games\\wolcen\\savegames\\characters\\";
                d.InitialDirectory = userFolder + WolcenSavePath;
                if (d.ShowDialog() == DialogResult.OK)
                {
                    if (cData.Character != null) cData.Character = null;
                    cData.Character = CharacterIO.ReadCharacter(d.FileName);
                    characterSavePath = d.FileName;
                }
                else { return; }
            }

            panel1.Enabled = true;

            SkillTree.LoadTree(ref panel1);
            LoadCharacterData();
        }

        private void Panel1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                SkillTree.isShiftDown = false;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                SkillTree.isCtrlDown = false;
            }
        }

        private void Panel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                SkillTree.isShiftDown = true;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                SkillTree.isCtrlDown = true;
            }
        }

        private void Panel1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as TabControl).SelectedTab.Text == "Inventory" || (sender as TabControl).SelectedTab.Text == "Skills")
            {
                this.Height = 595 + 35;
            }
            else
            {
                this.Height = 595;
            }
        }

        private void apocUnlockCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (cData.PlayerData == null) return;
            if (apocUnlockCheckBox.Checked)
            {
                cData.Character.ApocalypticData.UnlockedTypes = new List<UnlockedTypes>
                {
                    new UnlockedTypes { Type = "rogue" },
                    new UnlockedTypes { Type = "mage" },
                    new UnlockedTypes { Type = "warrior" },
                    new UnlockedTypes { Type = "tank" },
                };
            }
            else
            {
                cData.Character.ApocalypticData.UnlockedTypes = new List<UnlockedTypes> { };
            }

        }

        private void unlockAllButton_Click(object sender, EventArgs e)
        {
            var skillList = new List<UnlockedSkill>();
            foreach (var skill in SkillTree.SkillTreeDict.Keys)
            {
                var skillObj = SkillTree.ActivateSkill("_" + skill);
                skillObj.Level = 90;
                skillList.Add(skillObj);
            }
            cData.Character.UnlockedSkills = skillList;
            TabControl tabControl = (SkillTree.skillPage.Parent as TabControl);
            SkillTree.LoadSkillInformation(ref tabControl);

        }

        private void lockAllButton_Click(object sender, EventArgs e)
        {
            foreach (var skill in cData.Character.UnlockedSkills.ToList())
            {
                var pic = new PictureBox();
                pic.Name = "_" + skill.SkillName;
                SkillTree.RemoveSkill(pic);
            }
        }

        private void contextMenuEditItem_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            InventoryManager.EditItem(sender, e);
        }
    }

    public static class cData
    {
        private static PlayerData playerData;
        private static CharacterData character;

        public static PlayerData PlayerData
        {
            get
            {
                return playerData;
            }
            set
            {
                playerData = value;
            }
        }

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
