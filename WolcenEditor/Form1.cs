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
        public static string playerChestSavePath;
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
            tabPage.Enabled = true;
            
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

            tabPage.SelectedIndexChanged += Panel1_SelectedIndexChanged;
            this.KeyDown += Panel1_KeyDown;
            this.KeyUp += Panel1_KeyUp;

            LoadComboBoxes();
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemStatDisplay, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemStashStatDisplay, new object[] { true });
            LogMe.InitLog();
        }

        private void UnloadRandomInventory()
        {
            cData.Character = null;
            cData.PlayerData = null;
            cData.PlayerChest = null;
            this.Controls.Clear();
            InitializeComponent();
            InitForm();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tabPage.Size = new Size(tabPage.Width, this.Height - 62);
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
                if (charGold.Text == "0")
                    c.PerLevel = "0";
                else
                {
                    var buffer = new byte[sizeof(UInt64)];
                    new Random().NextBytes(buffer);
                    UInt64 rnd = BitConverter.ToUInt64(buffer, 0);
                    c.PerLevel = (rnd % (Convert.ToUInt64(charGold.Text) - 0) + 0).ToString();
                }
                cData.Character.Telemetry.GoldDropped = c;
                cData.Character.Telemetry.GoldGainedQuests = c;
                cData.Character.Telemetry.GoldGainedMerchant = c;
                cData.Character.Telemetry.GoldPicked = c;
                CharacterIO.WriteCharacter(characterSavePath, cData.Character);
                PlayerChestIO.WritePlayerChest(playerChestSavePath, cData.PlayerChest);
                PlayerDataIO.WritePlayerData(playerDataSavePath, cData.PlayerData);
                hasSaved = true;
            }
        }

        private void LoadCharacterData()
        {
            LoadPlayerData();
            LoadPlayerStashData();
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

            BindToComboBox(questBox, WolcenStaticData.QuestLocalizedNames, cData.Character.Progression.LastPlayed, "QuestId");
            BindToComboBox(stepIdBox, WolcenStaticData.QuestIdLocailzation[cData.Character.Progression.LastPlayed.QuestId], cData.Character.Progression.LastPlayed, "StepId");

            LoadTelemetry();

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

            if (cData.Character.ApocalypticData.UnlockedTypes.Count == 4)
                apocUnlockCheckBox.Checked = true;

            InventoryManager.LoadCharacterInventory(charInv);
            StashManager.LoadPlayerStash(charStash);

            SkillTree.LoadSkillInformation(ref tabPage);
        }

        private void LoadPlayerStashData()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string WolcenPlayerChest = userFolder + "\\Saved Games\\wolcen\\savegames\\playerchest.json";
            if (File.Exists(WolcenPlayerChest))
            {
                if (cData.PlayerChest != null) cData.PlayerChest = null;
                cData.PlayerChest = PlayerChestIO.ReadPlayerStash(WolcenPlayerChest);
                playerChestSavePath = WolcenPlayerChest;
            }
        }

        private void LoadTelemetry()
        {
            var savedExpansionState = GetExpansionState(treeViewTelemetry.Nodes);
            treeViewTelemetry.BeginUpdate();
            treeViewTelemetry.Nodes.Clear();
            telemetryTextBox.Enabled = true;
            telemetryTextBox.Visible = true;

            var telemetry = cData.Character.Telemetry.GetType().GetProperties();
            foreach (var teleProps in telemetry)
            {
                var node = new TreeNode(teleProps.Name);
                node.Name = teleProps.Name;
                treeViewTelemetry.Nodes.Add(node);
                var xt = cData.Character.Telemetry;

                if (teleProps.PropertyType == typeof(Count))
                {
                    foreach (var countValues in teleProps.PropertyType.GetProperties())
                    {
                        var t = teleProps.GetValue(xt);
                        treeViewTelemetry.Nodes[node.Index].Nodes.Add(countValues.Name, $"{countValues.Name} : {countValues.GetValue(t, null)}");
                    }
                }
                if (teleProps.PropertyType == typeof(List<TypeCount>))
                {
                    List<TypeCount> typeCountList = (List<TypeCount>)teleProps.GetValue(cData.Character.Telemetry, null);
                    var index = 0;
                    foreach (var typeCountValues in typeCountList)
                    {
                        var innerNode = new TreeNode(typeCountValues.Type);
                        //innerNode.Name = typeCountValues.Type;
                        innerNode.Tag = index;
                        treeViewTelemetry.Nodes[node.Index].Nodes.Add(innerNode);
                        foreach (var value in typeCountValues.GetType().GetProperties())
                        {
                            treeViewTelemetry.Nodes[node.Index].Nodes[innerNode.Index].Nodes.Add(value.Name, $"{value.Name} : {value.GetValue(typeCountValues, null)} ");
                        }
                        index++;
                    }
                }
            }
            SetExpansionState(treeViewTelemetry.Nodes, savedExpansionState);
            treeViewTelemetry.EndUpdate();
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

        private void BindToComboBox<T, DictKey, DictValue>(T comboBox, Dictionary<DictKey, DictValue> mapping, object dataSource, string dataMemeber) where T : ComboBox
        {
            comboBox.ResetBindings();
            comboBox.DataSource = new BindingSource(mapping, null);
            comboBox.DisplayMember = "Value";
            comboBox.ValueMember = "Key";
            comboBox.DataBindings.Add("SelectedValue", dataSource, dataMemeber, true, DataSourceUpdateMode.OnPropertyChanged);
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

            tabPage.Enabled = true;

            SkillTree.LoadTree(ref tabPage);
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
                this.Width = 851;
            }
            else if ((sender as TabControl).SelectedTab.Text == "Stash")
            {
                this.Height = 595 + 60;
                //this.Width = 851 - 292;
            }
            else
            {
                this.Height = 595;
                this.Width = 851;
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

        private void questBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            var id = (KeyValuePair<string, string>)box.SelectedItem;
            cData.Character.Progression.LastPlayed.StepId = 1;
            BindToComboBox(stepIdBox, WolcenStaticData.QuestIdLocailzation[id.Key], cData.Character.Progression.LastPlayed, "StepId");
        }

        private void treeViewTelemetry_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewTelemetry.SelectedNode.Parent != null && treeViewTelemetry.SelectedNode.Nodes.Count < 1)
            {
                var path = GetKeyPath(treeViewTelemetry.SelectedNode).Split('.');
                //MessageBox.Show(cData.Character.Telemetry + path);
                var pinfo = "cData.Character.Telemetry" + "." + path[0];
                object value = typeof(CharacterData).GetProperty("Telemetry").GetValue(cData.Character);
                var test = value.GetType().GetProperty(path[0]).GetValue(value, null);
                var fffffff = test.GetType();
                if (test.GetType() == typeof(List<TypeCount>))
                {
                    var ar = (List<TypeCount>)test;
                    telemetryTextBox.DataBindings.Clear();
                    telemetryTextBox.DataBindings.Add("Text", ar[(int)treeViewTelemetry.SelectedNode.Parent.Tag], path[1], true, DataSourceUpdateMode.OnPropertyChanged);
                }
                else
                {
                    telemetryTextBox.DataBindings.Clear();
                    telemetryTextBox.DataBindings.Add("Text", test, path[1], true, DataSourceUpdateMode.OnPropertyChanged);
                }


            }

        }
        private void telemetryTextBox_Leave(object sender, EventArgs e)
        {
            telemetryTextBox.DataBindings.Clear();
            telemetryTextBox.Clear();
            LoadTelemetry();
        }


        public static List<string> GetExpansionState(TreeNodeCollection nodes)
        {
            return Descendants(nodes)
                        .Where(n => n.IsExpanded)
                        .Select(n => n.FullPath)
                        .ToList();
        }

        public static void SetExpansionState(TreeNodeCollection nodes, List<string> savedExpansionState)
        {
            foreach (var node in Descendants(nodes)
                                      .Where(n => savedExpansionState.Contains(n.FullPath)))
            {
                node.Expand();
            }
        }

        public static IEnumerable<TreeNode> Descendants(TreeNodeCollection c)
        {
            foreach (var node in c.OfType<TreeNode>())
            {
                yield return node;

                foreach (var child in Descendants(node.Nodes))
                {
                    yield return child;
                }
            }
        }

        public string GetKeyPath(TreeNode node)
        {
            if (node.Parent == null)
            {
                return node.Name;
            }
            if (string.IsNullOrEmpty(node.Name))
            {
                return GetKeyPath(node.Parent) + node.Name;

            }
            else
            {
                return GetKeyPath(node.Parent) + "." + node.Name;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form prompt = new Form()
            {
                Width = 265,
                Height = 125,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Enter your character name.",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 25, Top = 5, Text = "Name:" };
            TextBox textBox = new TextBox() { Left = 25, Top = 25, Width = 200 };
            Button confirmation = new Button() { Text = "Ok", Left = 75, Width = 100, Top = 50, DialogResult = DialogResult.OK };
            confirmation.Click += (sender2, e2) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            var name = prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            if(prompt.DialogResult == DialogResult.OK && !String.IsNullOrWhiteSpace(name))
            {
                name = name.Replace(" ", "");
                CreateNewCharacter(name);
            }
        }

        private void CreateNewCharacter(string name)
        {
            if (cData.Character != null)
                cData.Character = null;
            var newCharacter = new CharacterData()
            {
                Name = name,
                PlayerId = "offlineplayer",
                CharacterId = name,
                DifficultyMode = 1,
                League = 1,
                UpdatedAt = "20-01-01T00:00:00Z",
                CharacterCustomization = new CharacterCustomization()
                {
                    Sex = 0,
                    Face = 1,
                    SkinColor = 100,
                    Haircut = 1,
                    HairColor = 5,
                    Beard = 1,
                    BeardColor = 5,
                    LeftEye = 1,
                    RightEye = 1,
                    Archetype = 1
                },
                Stats = new Stats()
                {
                    Strength = 1,
                    Agility = 1,
                    Constitution = 1,
                    Power = 1,
                    Level = 1,
                    PassiveSkillPoints = 0,
                    CurrentXP = "0",
                    RemainingStatsPoints = 0,
                    Gold = "0",
                    PrimordialAffinity = "0",
                    IsAutoDashAvailable = 1,
                    DashStatusActivation = 1
                },
                UnlockedSkills = new List<UnlockedSkill>{ },
                SkillBar = new List<SkillBar>{ },
                PassiveSkills = new List<string> { },
                BeltConfig = new List<BeltConfig>{ },
                Progression = new Progression { LastPlayed = new LastPlayed { QuestId = "INTRO_Quest1", StepId = 1} },
                Telemetry = new Telemetry 
                {
                    PlayTime  = new Count { Total = "0", PerLevel = "0" },
                    PlayTimeOutTown  = new Count { Total = "0", PerLevel = "0" },
                    KillCountPerBossrank  = new List<TypeCount> {},
                    KillCountPerMobRankType  = new List<TypeCount> { },
                    MinLevelKilled  = new Count { Total = "0", PerLevel = "0" },
                    MaxLevelKilled  = new Count { Total = "0", PerLevel = "0" },
                    DeathCount  = new Count { Total = "0", PerLevel = "0" },
                    DeathCountPerBossrank  = new List<TypeCount> { },
                    XpFromQuest  = new Count { Total = "0", PerLevel = "0" },
                    XpFromKill  = new Count { Total = "0", PerLevel = "0" },
                    GoldDropped  = new Count { Total = "0", PerLevel = "0" },
                    GoldGainedQuests  = new Count { Total = "0", PerLevel = "0" },
                    GoldGainedMerchant  = new Count { Total = "0", PerLevel = "0" },
                    GoldPicked  = new Count { Total = "0", PerLevel = "0" },
                    GoldSpent  = new Count { Total = "0", PerLevel = "0" },
                    GoldSpentMerchant  = new Count { Total = "0", PerLevel = "0" },
                    GoldSpentJewelerUnsocketItem  = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinitySpent  = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinitySpentSkillLevelUp  = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinityGained  = new Count { Total = "0", PerLevel = "0" },
                    ItemsDropped  = new List<TypeCount> { },
                    ItemsPicked  = new List<TypeCount> { },
                    ItemsBought  = new List<TypeCount> { },
                    ItemsSold  = new List<TypeCount> { },
                    TimeSpentPerZone  = new List<TypeCount> { },
                    SoloReviveTokenUsedPerZone  = new List<TypeCount> { },
                    SoloDeathPerZone  = new List<TypeCount> { },
                    MultiRevivePerZone  = new List<TypeCount> { },
                    SkillUsage  = new List<TypeCount> { },
                    QuestAttempt_NPC1  = new Count { Total = "0", PerLevel = "0" },
                    QuestAttempt_NPC2  = new Count { Total = "0", PerLevel = "0" },
                    QuestSuccess_NPC1  = new Count { Total = "0", PerLevel = "0" },
                    QuestSuccess_NPC2  = new Count { Total = "0", PerLevel = "0" },
                    QuestFailed_NPC1  = new Count { Total = "0", PerLevel = "0" },
                    QuestFailed_NPC2  = new Count { Total = "0", PerLevel = "0" },
                    QuestMaxFloorReached_NPC2  = new Count { Total = "0", PerLevel = "0" },
                    UnlockChestCount  = new Count { Total = "0", PerLevel = "0" },
                    ResetPSTCount  = new Count { Total = "0", PerLevel = "0" },
                    ResetCharacterAttributesCount  = new Count { Total = "0", PerLevel = "0" },
                },
                Versions = new Versions
                {
                    SaveVersion = "1.0.0.0",
                    StatsVersion = "1.0.0.0",
                    ItemsVersion = "1.0.0.0",
                    InventoryVersion = "1.0.0.0",
                    ASTVersion = "1.0.0.0",
                    ASTVariantsVersion = "1.0.0.0",
                    PSTVersion = "1.0.0.0",
                    StorylineVersion = 	"1.0.0.0",
                    SaveAlterationsVersion = 1
                },
                CharacterCosmeticInventory = new CharacterCosmeticInventory { },
                InventoryEquipped = new List<InventoryEquipped>{ },
                InventoryGrid = new List<InventoryGrid>{ },
                InventoryBelt = new List<InventoryBelt> { },
                PSTConfig = new List<PSTConfig>{ },
                ApocalypticData = new ApocalypticData{ ChosenType = "", UnlockedTypes = new List<UnlockedTypes> { } },
                Tutorials = new List<Tutorials> { },
                Sequences = new List<Sequences>{ },
                LastGameParameters = new LastGameParameters{GameMode = 1, DifficultyMode = 1, Difficulty = 2, League = 1 , QuestId = "INTRO_Quest1", StepId = 1, Privacy = 2, Level = 3 }
            };

            cData.Character = newCharacter;
            tabPage.Enabled = true;
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string WolcenSavePath = "\\Saved Games\\wolcen\\savegames\\characters\\";
            string newPath = userFolder + WolcenSavePath;
            characterSavePath = newPath + cData.Character.Name + ".json";
            SkillTree.LoadTree(ref tabPage);
            LoadCharacterData();
        }
    }

    public static class cData
    {
        private static PlayerData playerData;
        private static CharacterData character;
        private static PlayerChest playerChest;

        public static PlayerChest PlayerChest
        {
            get { return playerChest; }
            set { playerChest = value; }
        }

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
