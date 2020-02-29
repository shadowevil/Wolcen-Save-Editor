using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Helpers;
using System.Windows.Forms;

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

            LoadComboBoxes();
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
            LoadCombo(WolcenStaticData.HeadStyle, @".\UIResources\Character\Head\", ref cboFace);
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
                DropDownItem i = new DropDownItem("", ColorTranslator.FromHtml(d.Value), width, height);
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
                DropDownItem i = new DropDownItem("", bmp);
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
            //text field bindings
            charName.DataBindings.Add("Text", cData.Character, "Name", false, DataSourceUpdateMode.OnPropertyChanged);
            charLevel.DataBindings.Add("Text", cData.Character.Stats, "Level", false, DataSourceUpdateMode.OnPropertyChanged);
            charExp.DataBindings.Add("Text", cData.Character.Stats, "CurrentXP", false, DataSourceUpdateMode.OnPropertyChanged);
            charFerocity.DataBindings.Add("Text", cData.Character.Stats, "Strength", false, DataSourceUpdateMode.OnPropertyChanged);
            charToughness.DataBindings.Add("Text", cData.Character.Stats, "Constitution", false, DataSourceUpdateMode.OnPropertyChanged);
            charAgility.DataBindings.Add("Text", cData.Character.Stats, "Agility", false, DataSourceUpdateMode.OnPropertyChanged);
            charWillpower.DataBindings.Add("Text", cData.Character.Stats, "Power", false, DataSourceUpdateMode.OnPropertyChanged);

            //normal combobox bindings
            BindToComboBox(charGender, WolcenStaticData.Sexes, cData.Character.CharacterCustomization, "Sex");
            //BindToComboBox(cboFace, WolcenStaticData.Face, cData.Character.CharacterCustomization, "Face");

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
