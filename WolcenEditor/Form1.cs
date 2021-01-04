﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Security.Cryptography;

namespace WolcenEditor
{
    public partial class Form1 : Form
    {
        public static string characterSavePath;
        public static string playerDataSavePath;
        public static string playerChestSavePath;
        public string WindowName = "Wolcen Save Editor";
        public string Version = Application.ProductVersion;
        public bool hasSaved = false;
        public bool CHAR_LOADED = false;

        public static int Scaling = 0;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        public enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        protected override void WndProc(ref Message m)
        {
            ShowScrollBar(itemStatDisplay.Handle, (int)ScrollBarDirection.SB_HORZ, false);
            ShowScrollBar(itemStashStatDisplay.Handle, (int)ScrollBarDirection.SB_HORZ, false);
            base.WndProc(ref m);
        }

        public Form1()
        {
            InitializeComponent();
            InitForm();
        }

        public void InitForm()
        {
            this.Text = WindowName + " - " + Version;
            tabPage.Enabled = false;

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

            WolcenStaticData.InitData();

            LoadComboBoxes();
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemStatDisplay, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, itemStashStatDisplay, new object[] { true });
            LogMe.InitLog();
            CityManager.initCity(charCity);
        }

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static bool DestroyIcon(IntPtr handle);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static Cursor myCursor = null;
        public static IntPtr iconPtr = new IntPtr();
        public static IconInfo icoInfo = new IconInfo();

        /// <summary>
        /// Create a cursor from a bitmap without resizing and with the specified
        /// hot spot
        /// </summary>
        public static void SetCursor(Image bitmap, int xHotSpot, int yHotSpot)
        {
            if (myCursor != null) DestroyIcon(myCursor.Handle);
            if (iconPtr != null) DestroyIcon(iconPtr);
            if (icoInfo.hbmColor != null) DeleteObject(icoInfo.hbmColor);
            if (icoInfo.hbmMask != null) DeleteObject(icoInfo.hbmMask);

            Bitmap bmp = SetImageOpacity(bitmap, 0.85f);
            Icon bmpIcon = ConvertoToIcon(bmp);
            icoInfo = new IconInfo();
            GetIconInfo(bmpIcon.Handle, ref icoInfo);
            icoInfo.xHotspot = xHotSpot;
            icoInfo.yHotspot = yHotSpot;
            icoInfo.fIcon = false;
            iconPtr = CreateIconIndirect(ref icoInfo);
            myCursor = new Cursor(iconPtr);
            bmpIcon.Dispose();
            bmp.Dispose();
            Cursor.Current = myCursor;
        }

        public static Icon ConvertoToIcon(Bitmap bmp)
        {
            IntPtr icH = bmp.GetHicon();
            var toReturn = (Icon)Icon.FromHandle(icH).Clone();
            DestroyIcon(icH);
            return toReturn;
        }

        public static Bitmap SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void UnloadRandomInventory()
        {
            CHAR_LOADED = false;
            cData.Character = null;
            cData.PlayerData = null;
            cData.PlayerChest = null;
            this.Controls.Clear();
            InitializeComponent();
            InitForm();
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
            if ((sender as ToolStripMenuItem).Text != "Open" && (sender as ToolStripMenuItem).Text != "New")
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
            if (cData.Character != null && cData.PlayerData != null && cData.PlayerChest != null)
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
            this.Text = WindowName + " - " + cData.Character.Name + " - " + Version;

            SetIndexToValueOf(ref cboFace, cData.Character.CharacterCustomization.Face);
            SetIndexToValueOf(ref cboHaircut, cData.Character.CharacterCustomization.Haircut);
            SetIndexToValueOf(ref cboHairColor, cData.Character.CharacterCustomization.HairColor);
            SetIndexToValueOf(ref cboBeard, cData.Character.CharacterCustomization.Beard);
            SetIndexToValueOf(ref cboBeardColor, cData.Character.CharacterCustomization.BeardColor);
            SetIndexToValueOf(ref cboLEye, cData.Character.CharacterCustomization.LeftEye);
            SetIndexToValueOf(ref cboREye, cData.Character.CharacterCustomization.RightEye);
            SetIndexToValueOf(ref cboSkinColor, cData.Character.CharacterCustomization.SkinColor);


            BindToComboBox(cboGender, WolcenStaticData.Gender, cData.Character.CharacterCustomization, "Sex");


            //Create the dict here rather than it being a static dict in WolcenStaticData.cs
            var expeditionLevels = new Dictionary<int, string>();
            foreach (var i in Enumerable.Range(1, 50))
                expeditionLevels.Add(i, (37 + (3 * i)).ToString());
            BindToComboBox(cboExpedition, expeditionLevels, cData.PlayerData.SoftcoreSeason, "ExpeditionsMaxLevelReached");


            if (cData.Character.Progression.LastPlayed == null)
            {
                cData.Character.Progression.LastPlayed = new LastPlayed();
                cData.Character.Progression.LastPlayed.QuestId = "ACT1_Quest1";
            }
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

            SkillManager.LoadSkillInformation(ref tabPage);
            CHAR_LOADED = true;

            closeStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
        }

        private void LoadPlayerStashData()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string WolcenPlayerChest = userFolder + "\\Saved Games\\wolcen\\savegames\\playerchest_1_3.json";
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
                    }
                    else chkAllCosmetics.Checked = false;
                }
                else chkAllCosmetics.Checked = false;
            }
            else chkAllCosmetics.Checked = false;

            if (cData.PlayerData.SoftcoreStandard.CompletedStory || cData.PlayerData.SoftcoreSeason.CompletedStory)
                chkChampion.Checked = true;
            else
                chkChampion.Checked = false;

            if (cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects.Any(x => x.Name == "wonder_2_construct"))
                extraSkillButton.Checked = true;
            else
                extraSkillButton.Checked = false;
        }

        #region Events

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NewTextDialog importAbout = new NewTextDialog(410, 130, "About importing builds"))
            {
                importAbout.DrawLabel(5, "You can import builds from the site", 12);
                importAbout.DrawLinkLabel(18, "https://wolcen-universe.com", new Uri("https://wolcen-universe.com"), 12, true);
                importAbout.DrawLabel(40, "Things are still very much a work in progress so if something", 12);
                importAbout.DrawLabel(52, "doesn't work here let us know in our discord.", 12);
                importAbout.ShowDialog(this);

                if (importAbout.DialogResult == DialogResult.OK)
                {
                    importAbout.Dispose();
                }
            }

            //    var aboutMessage = MessageBox.Show("You can import builds from the site https://wolcen-universe.com!\n\n" +
            //        "Things are still very much a work in progress so if something doesn't work here let us know at\n" +
            //        "https://discord.gg/R8WKtQr\n\n\n" +
            //        "Would you like to be taken to https://wolcen-universe.com/ right now?"
            //        , "About Improrting Builds."
            //    , MessageBoxButtons.YesNo);

            //if (aboutMessage == DialogResult.Yes)
            //    Process.Start("https://wolcen-universe.com/");
        }

        private void importStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cData.Character == null)
            {
                MessageBox.Show("You need to open or create a new character to begin");
                return;
            }
            bool displayBackingText = true;
            Form importForm = new Form()
            {
                Width = 350,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = "Import a build",
                BackgroundImage = WolcenEditor.Properties.Resources.bg,
                BackgroundImageLayout = ImageLayout.Center,
            };
            Label BuildLabel = new Label()
            {

                Text = "Import Build",
                ForeColor = Color.White,
                Font = new Font(Form1.DefaultFont.FontFamily, 30, FontStyle.Regular),
                Visible = true,
                Location = new Point(50, 7),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            TextBox BuildUrlTextBox = new TextBox()
            {
                Text = "Enter Build URL Here...",
                ForeColor = Color.Gray,

                Width = 210,
                Location = new Point(60, 60)
            };
            Button AcceptButton = new Button
            {

                Text = "Accept",
                Location = new Point(85, 90),
                DialogResult = DialogResult.OK

            };
            Button CancelButton = new Button
            {

                Text = "Cancel",
                Location = new Point(165, 90)

            };

            BuildUrlTextBox.GotFocus += (source, e2) =>
            {
                if (displayBackingText)
                {
                    displayBackingText = false;
                    BuildUrlTextBox.Text = "";
                    BuildUrlTextBox.ForeColor = Color.Black;

                }
            };
            BuildUrlTextBox.LostFocus += (source, e2) =>
            {
                if (!displayBackingText && string.IsNullOrEmpty(BuildUrlTextBox.Text))
                {
                    displayBackingText = true;
                    BuildUrlTextBox.Text = "Enter Build URL Here...";
                    BuildUrlTextBox.ForeColor = Color.Gray;

                }
            };
            CancelButton.Click += (sender2, e2) =>
            {
                importForm.Close();
                importForm.Dispose();
            };

            importForm.Controls.Add(AcceptButton);
            importForm.Controls.Add(CancelButton);
            importForm.Controls.Add(BuildUrlTextBox);
            importForm.Controls.Add(BuildLabel);

            string url = importForm.ShowDialog() == DialogResult.OK ? BuildUrlTextBox.Text : "";

            if (importForm.DialogResult == DialogResult.OK && !String.IsNullOrWhiteSpace(url))
            {
                if (!url.Contains("wolcen-universe.com/builds/"))
                    MessageBox.Show("That doesn't seem to be a valid url.");
                else
                {
                    // splits up the URL to get at the build id we need for the api request.
                    var urlSections = url.Split('-');
                    var pathSplit = urlSections[1].Split('/');
                    var buildId = pathSplit[2];

                    // get back json data from the wolcen-universe api for the given build id
                    string jsonData = OnlineBuildRequest.RequestBuild(buildId);

                    // converts the json into a dynamic json object.
                    dynamic resultData = JObject.Parse(jsonData);

                    //sets level and main stats to the data from the jsonObject

                    var level = resultData["data"]["build"]["passiveSkillTree"]["level"];
                    if (level != null)
                    {
                        cData.Character.Stats.Level = level;
                        cData.Character.Stats.Strength = resultData["data"]["build"]["passiveSkillTree"]["strength"] + level;
                        cData.Character.Stats.Constitution = resultData["data"]["build"]["passiveSkillTree"]["constitution"] + level;
                        cData.Character.Stats.Agility = resultData["data"]["build"]["passiveSkillTree"]["agility"] + level;
                        cData.Character.Stats.Power = resultData["data"]["build"]["passiveSkillTree"]["power"] + level;
                    }

                    //sets the passive skills
                    List<String> newPassiveSkills = resultData["data"]["build"]["passiveSkillTree"]["nodes"].ToObject(typeof(List<string>));
                    newPassiveSkills.Remove("root");
                    cData.Character.PassiveSkills = newPassiveSkills;

                    //sets the rotation of the skill wheels
                    List<int> rotations = resultData["data"]["build"]["passiveSkillTree"]["rotations"].ToObject(typeof(List<int>));
                    List<PSTConfig> newPassiveConfig = new List<PSTConfig>
                    {
                        new PSTConfig { Id = 0, Mode = 3},
                        new PSTConfig { Id = 1, Mode = 6},
                        new PSTConfig { Id = 2, Mode = 12},
                    };
                    for (int i = 0; i < newPassiveConfig.Count; i++)
                    {
                        int counter = newPassiveConfig[i].Mode;
                        newPassiveConfig[i].Mode = (counter - rotations[i]) % counter;
                    }
                    cData.Character.PSTConfig = newPassiveConfig;

                    // list of skills needed by the build
                    var skills = resultData["data"]["build"]["passiveSkillTree"]["skills"];
                    if (skills != null)
                    {
                        // create a new list to store all of our skills in.
                        var newUnlockedSkillList = new List<UnlockedSkill>();

                        // if we already have some skills then set them in this new list so we don't lose them.
                        if (cData.Character.UnlockedSkills != null)
                            newUnlockedSkillList = cData.Character.UnlockedSkills.ToList();

                        //setup the skillbar with proper slots
                        var newSkillBar = new List<SkillBar>()
                        {
                            new SkillBar { Slot = 1, SkillName = "" },
                            new SkillBar { Slot = 2, SkillName = "" },
                            new SkillBar { Slot = 3, SkillName = ""  },
                            new SkillBar { Slot = 4, SkillName = ""  },
                            new SkillBar { Slot = 5, SkillName = ""  },
                            new SkillBar { Slot = 12, SkillName = ""  },
                        };
                        for (int i = 0; i < skills.Count; i++)
                        {
                            if (skills[i] == null)
                                break;

                            // this sets the names of the skills we want in our hotbar.
                            newSkillBar[i].SkillName = skills[i]["id"];

                            // converts our jsonObject to a string that represents the skill name.
                            string newSkillName = skills[i]["id"].ToObject(typeof(string));

                            // if the skill does not exist on our character we create a brand new UnlockedSkill and add it to the list of skills
                            bool alreadyExists = newUnlockedSkillList.Any(x => x.SkillName == newSkillName);
                            if (!alreadyExists)
                            {
                                string skillId = skills[i]["id"];
                                UnlockedSkill newSkill = new UnlockedSkill();
                                newSkill.SkillName = skillId;
                                newSkill.CurrentXp = "0";
                                newSkill.Level = 90;

                                string[] skillMod = skills[i]["modifiers"].ToObject(typeof(string[]));
                                newSkill.Variants = TranslateSkillModifiers(skillId, skillMod);
                                newUnlockedSkillList.Add(newSkill);
                            }
                            else //if the skill does exist we find it in our list and just change the values of it.
                            {
                                foreach (var skill in newUnlockedSkillList)
                                {
                                    if (skill.SkillName == newSkillName)
                                    {
                                        skill.Level = 90;
                                        string[] skillMod = skills[i]["modifiers"].ToObject(typeof(string[]));
                                        skill.Variants = TranslateSkillModifiers(skill.SkillName, skillMod); ;
                                        break;
                                    }
                                }
                            }
                        }
                        //sets our characters actual data to the new skillbar and unlocked skill list we just made.

                        cData.Character.SkillBar = newSkillBar;
                        cData.Character.UnlockedSkills = newUnlockedSkillList;
                    }

                    MessageBox.Show($"Successfully Imported Character From:\n{url}");
                    SkillManager.LoadTree(ref tabPage);
                    LoadCharacterData();
                    importForm.Dispose();
                }
            }
        }

        private void extraSkillButton_CheckedChanged(object sender, EventArgs e)
        {
            if (cData.PlayerData == null) return;
            if (extraSkillButton.Checked == true && !cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects.Any(x => x.Name == "wonder_2_construct"))
            {
                cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects.Add(new FinishedProjects { Name = "wonder_2_construct" });
            }
            else if (extraSkillButton.Checked == false)
            {
                if (cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects.Any(x => x.Name == "wonder_2_construct"))
                {
                    for (int i = 0; i < cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects.Count; i++)
                    {
                        if (cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects[i].Name == "wonder_2_construct")
                        {
                            cData.PlayerData.SoftcoreSeason.CityBuilding.FinishedProjects[i].Name = "";
                        }
                    }
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onCloseCheck(sender, e) == false) return;

            bool displayBackingText = true;
            Form importForm = new Form()
            {
                Width = 350,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = "Create a new character",
                BackgroundImage = WolcenEditor.Properties.Resources.bg,
                BackgroundImageLayout = ImageLayout.Center,
            };

            Label BuildLabel = new Label()
            {
                Text = "Name New Character",
                ForeColor = Color.White,
                Font = new Font(Form1.DefaultFont.FontFamily, 20, FontStyle.Regular),
                Visible = true,
                Location = new Point(33, 15),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            TextBox NameTextBox = new TextBox()
            {
                Text = "Enter Name Here...",
                ForeColor = Color.Gray,
                Width = 210,
                Location = new Point(60, 60)
            };
            Button AcceptButton = new Button
            {
                Text = "Accept",
                Location = new Point(85, 90),
                DialogResult = DialogResult.OK
            };
            Button CancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(165, 90)
            };
            NameTextBox.GotFocus += (source, e2) =>
            {
                if (displayBackingText)
                {
                    displayBackingText = false;
                    NameTextBox.Text = "";
                    NameTextBox.ForeColor = Color.Black;
                }
            };
            NameTextBox.LostFocus += (source, e2) =>
            {
                if (!displayBackingText && string.IsNullOrEmpty(NameTextBox.Text))
                {
                    displayBackingText = true;
                    NameTextBox.Text = "Enter Name Here...";
                    NameTextBox.ForeColor = Color.Gray;

                }
            };
            CancelButton.Click += (sender2, e2) => { importForm.Close(); };

            importForm.Controls.Add(AcceptButton);
            importForm.Controls.Add(CancelButton);
            importForm.Controls.Add(NameTextBox);
            importForm.Controls.Add(BuildLabel);

            var name = importForm.ShowDialog() == DialogResult.OK ? NameTextBox.Text : "";
            if (importForm.DialogResult == DialogResult.OK && !String.IsNullOrWhiteSpace(name))
            {
                UnloadRandomInventory();

                name = name.Replace(" ", "");

                if (cData.Character != null)
                    cData.Character = null;
                cData.Character = CreateNewCharacter(name);

                tabPage.Enabled = true;
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string WolcenSavePath = "\\Saved Games\\wolcen\\savegames\\characters\\";
                string newPath = userFolder + WolcenSavePath;
                characterSavePath = newPath + cData.Character.Name + ".json";
                SkillManager.LoadTree(ref tabPage);
                LoadCharacterData();
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
                cData.PlayerData.SoftcoreStandard.CompletedStory = true;
                cData.PlayerData.SoftcoreSeason.CompletedStory = true;
            }
            else
            {
                cData.PlayerData.SoftcoreStandard.CompletedStory = false;
                cData.PlayerData.SoftcoreSeason.CompletedStory = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProfile();
        }

        private void closeStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onCloseCheck(sender, e) == false) return;
            UnloadRandomInventory();
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

            SkillManager.LoadTree(ref tabPage);
            LoadCharacterData();
        }

        private void Panel1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                SkillManager.isShiftDown = false;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                SkillManager.isCtrlDown = false;
            }
        }

        private void Panel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                SkillManager.isShiftDown = true;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                SkillManager.isCtrlDown = true;
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

        private void questBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!CHAR_LOADED)
                return;
            var box = (ComboBox)sender;
            var id = (KeyValuePair<string, string>)box.SelectedItem;
            if (cData.Character.Progression.LastPlayed == null)
            {
                cData.Character.Progression.LastPlayed = new LastPlayed();
                cData.Character.Progression.LastPlayed.QuestId = "ACT1_Quest1";
                cData.Character.Progression.LastPlayed.StepId = 1;
            }
            if (!WolcenStaticData.QuestSelections[id.Key].Contains(cData.Character.Progression.LastPlayed.StepId))
            {
                cData.Character.Progression.LastPlayed.StepId = 1;
                questBox.SelectedIndex = 1;
            }
            BindToComboBox(stepIdBox, WolcenStaticData.QuestIdLocailzation[id.Key], cData.Character.Progression.LastPlayed, "StepId");
        }

        private void treeViewTelemetry_AfterSelect(object sender, TreeViewEventArgs e)
        {
            telemetryTextBox.Enabled = false;
            telemetryTextBox.Visible = false;
            // Ignore top level nodes
            if (treeViewTelemetry.SelectedNode.Parent != null && treeViewTelemetry.SelectedNode.Nodes.Count < 1)
            {
                telemetryTextBox.Enabled = true;
                telemetryTextBox.Visible = true;

                // Get the path of the selected node
                string[] path = GetKeyPath(treeViewTelemetry.SelectedNode).Split('.');

                //gets the properties inside the telemetry object.
                object value = typeof(CharacterData).GetProperty("Telemetry").GetValue(cData.Character);

                // Get the properties of the selected nodes path
                var nodeProperties = value.GetType().GetProperty(path[0]).GetValue(value, null);

                //bind the selected treeview node value to our textbox.
                if (nodeProperties.GetType() == typeof(List<TypeCount>))
                {
                    var typeCountProperties = (List<TypeCount>)nodeProperties;
                    telemetryTextBox.DataBindings.Clear();
                    telemetryTextBox.DataBindings.Add("Text", typeCountProperties[(int)treeViewTelemetry.SelectedNode.Parent.Tag], path[1], true, DataSourceUpdateMode.OnPropertyChanged);
                }
                else
                {
                    telemetryTextBox.DataBindings.Clear();
                    telemetryTextBox.DataBindings.Add("Text", nodeProperties, path[1], true, DataSourceUpdateMode.OnPropertyChanged);
                }
            }
        }

        private void telemetryTextBox_Leave(object sender, EventArgs e)
        {
            telemetryTextBox.DataBindings.Clear();
            telemetryTextBox.Clear();
            LoadTelemetry();
        }

        #endregion

        #region Helper Functions
        /// <summary>
        /// Gets the expanded nodes of a TreeView 
        /// </summary>
        /// <param name="nodes">The top level of a TreeView</param>
        /// <returns>A list of expaneded nodes.</returns>
        public static List<string> GetExpansionState(TreeNodeCollection nodes)
        {
            return Descendants(nodes)
                        .Where(n => n.IsExpanded)
                        .Select(n => n.FullPath)
                        .ToList();
        }

        /// <summary>
        /// Sets the nodes of a TreeView to a 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="savedExpansionState"></param>
        public static void SetExpansionState(TreeNodeCollection nodes, List<string> savedExpansionState)
        {
            foreach (var node in Descendants(nodes)
                                      .Where(n => savedExpansionState.Contains(n.FullPath)))
            {
                node.Expand();
            }
        }

        /// <summary>
        /// returns an Enumerable containing a list of child objects.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>IEnumerable of child objects.</returns>
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

        /// <summary>
        /// Get the path of a tree node as a string sectioned by '.'. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns>A string representing the path of your node.</returns>
        public string GetKeyPath(TreeNode node)
        {
            if (node.Parent == null)
                return node.Name;
            if (string.IsNullOrEmpty(node.Name))
                return GetKeyPath(node.Parent) + node.Name;
            else
                return GetKeyPath(node.Parent) + "." + node.Name;
        }

        private string TranslateSkillModifiers(string skillName, string[] listOfModifiers)
        {
            char[] modifier = new char[16];
            for (int i = 0; i < modifier.Length; i++)
            {
                modifier[i] = '0';
            }
            foreach (var str in listOfModifiers)
            {
                int modIndex = WolcenStaticData.SkillModifiers[skillName][str];
                modifier[modIndex] = '1';
            }
            return new string(modifier);
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
        #endregion


        private CharacterData CreateNewCharacter(string name)
        {
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
                    HuntCurrency = "0",
                    IsAutoDashAvailable = 1,
                    DashStatusActivation = 1
                },
                UnlockedSkills = new List<UnlockedSkill> { },
                SkillBar = new List<SkillBar> { },
                PassiveSkills = new List<string> { },
                BeltConfig = new List<BeltConfig> { new BeltConfig { Id = 0, Locked = 0 }, new BeltConfig { Id = 1, Locked = 0 } },
                Progression = new Progression { LastPlayed = new LastPlayed { QuestId = "INTRO_Quest1", StepId = 1 } },
                Telemetry = new Telemetry
                {
                    PlayTime = new Count { Total = "0", PerLevel = "0" },
                    PlayTimeOutTown = new Count { Total = "0", PerLevel = "0" },
                    KillCountPerBossrank = new List<TypeCount> { },
                    KillCountPerMobRankType = new List<TypeCount> { },
                    MinLevelKilled = new Count { Total = "0", PerLevel = "0" },
                    MaxLevelKilled = new Count { Total = "0", PerLevel = "0" },
                    DeathCount = new Count { Total = "0", PerLevel = "0" },
                    DeathCountPerBossrank = new List<TypeCount> { },
                    XpFromQuest = new Count { Total = "0", PerLevel = "0" },
                    XpFromKill = new Count { Total = "0", PerLevel = "0" },
                    GoldDropped = new Count { Total = "0", PerLevel = "0" },
                    GoldGainedQuests = new Count { Total = "0", PerLevel = "0" },
                    GoldGainedMerchant = new Count { Total = "0", PerLevel = "0" },
                    GoldPicked = new Count { Total = "0", PerLevel = "0" },
                    GoldSpent = new Count { Total = "0", PerLevel = "0" },
                    GoldSpentMerchant = new Count { Total = "0", PerLevel = "0" },
                    GoldSpentJewelerUnsocketItem = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinitySpent = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinitySpentSkillLevelUp = new Count { Total = "0", PerLevel = "0" },
                    PrimordialAffinityGained = new Count { Total = "0", PerLevel = "0" },
                    ItemsDropped = new List<TypeCount> { },
                    ItemsPicked = new List<TypeCount> { },
                    ItemsBought = new List<TypeCount> { },
                    ItemsSold = new List<TypeCount> { },
                    TimeSpentPerZone = new List<TypeCount> { },
                    SoloReviveTokenUsedPerZone = new List<TypeCount> { },
                    SoloDeathPerZone = new List<TypeCount> { },
                    MultiRevivePerZone = new List<TypeCount> { },
                    SkillUsage = new List<TypeCount> { },
                    QuestAttempt_NPC1 = new Count { Total = "0", PerLevel = "0" },
                    QuestAttempt_NPC2 = new Count { Total = "0", PerLevel = "0" },
                    QuestSuccess_NPC1 = new Count { Total = "0", PerLevel = "0" },
                    QuestSuccess_NPC2 = new Count { Total = "0", PerLevel = "0" },
                    QuestFailed_NPC1 = new Count { Total = "0", PerLevel = "0" },
                    QuestFailed_NPC2 = new Count { Total = "0", PerLevel = "0" },
                    QuestMaxFloorReached_NPC2 = new Count { Total = "0", PerLevel = "0" },
                    UnlockChestCount = new Count { Total = "0", PerLevel = "0" },
                    ResetPSTCount = new Count { Total = "0", PerLevel = "0" },
                    ResetCharacterAttributesCount = new Count { Total = "0", PerLevel = "0" },
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
                    StorylineVersion = "1.0.0.0",
                    SaveAlterationsVersion = 1
                },
                CharacterCosmeticInventory = new CharacterCosmeticInventory { },
                InventoryEquipped = new List<InventoryEquipped> { },
                InventoryGrid = new List<InventoryGrid> { },
                InventoryBelt = new List<InventoryBelt> { },
                PSTConfig = new List<PSTConfig> { },
                ApocalypticData = new ApocalypticData { ChosenType = "", UnlockedTypes = new List<UnlockedTypes> { } },
                Tutorials = new List<Tutorials> { },
                Sequences = new List<Sequences> { },
                LastGameParameters = new LastGameParameters { GameMode = 1, DifficultyMode = 1, Difficulty = 2, League = 1, QuestId = "INTRO_Quest1", StepId = 1, Privacy = 2, Level = 3 }
            };
            return newCharacter;

        }

        private void characterDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cData.Character != null)
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
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "JSON File|*.json";
                    sfd.InitialDirectory = playerDataSavePath;
                    sfd.FileName = cData.Character.Name.ToLower() + ".json";
                    DialogResult dr = sfd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        CharacterIO.WriteCharacter(sfd.FileName, cData.Character, false);
                    }
                }
            }
        }

        private void stashDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cData.PlayerChest != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "JSON File|*.json";
                    sfd.InitialDirectory = Directory.GetParent(playerChestSavePath).FullName;
                    sfd.FileName = "playerchest.json";
                    DialogResult dr = sfd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        PlayerChestIO.WritePlayerChest(sfd.FileName, cData.PlayerChest, false);
                    }
                }
            }
        }

        private void aboutUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var aboutUsDialog = new NewTextDialog(400, 130, "About us"))
            {
                aboutUsDialog.DrawLabel(5, "About Us", 15, true);
                aboutUsDialog.DrawLabel(20, "We are here to provide everyone with a better editing experience!", 12);
                aboutUsDialog.DrawLabel(48, "Thanks for using our program.", 12);
                aboutUsDialog.DrawLabel(60, "ShadowEvil" + ", " + "Stoned Puppy", 12, true);
                aboutUsDialog.ShowDialog(this);
            }
        }

        private void discordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var discordDialog = new NewTextDialog(400, 130, "Discord link"))
            {
                discordDialog.DrawLabel(5, "Discord Link", 15, true);
                discordDialog.DrawLinkLabel(44, "https://discord.gg/v67rjPm", new Uri("https://discord.gg/v67rjPm"), 13, true);
                if (discordDialog.ShowDialog(this) == DialogResult.OK)
                {
                    discordDialog.Close();
                }
            }
        }
    }

    public static class cData
    {
        public static PlayerChest PlayerChest { get; set; }

        public static PlayerData PlayerData { get; set; }

        public static CharacterData Character { get; set; }

    }
}
