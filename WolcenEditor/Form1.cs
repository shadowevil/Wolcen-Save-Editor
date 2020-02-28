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
            foreach (var d in WolcenStaticData.HairColorBank)
            {
                DropDownItem i = new DropDownItem();
                Bitmap bmp = new Bitmap(50, 50);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (SolidBrush b = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(d.Value)))
                    {
                        g.FillRectangle(b, 0, 0, 50, 50);
                    }
                }
                i.Image = bmp;
                i.Value = String.Empty;
                cboHairColor.Items.Add(i);
                cboBeardColor.Items.Add(i);
            }

            for (int i = 1; i <= Directory.GetFiles(@".\UIResources\Character\Eyes\").Length; i++)
            {
                string dirPath = @".\UIResources\Character\Eyes\";
                Bitmap bmp = new Bitmap(Image.FromFile(dirPath + Convert.ToString(i) + ".png"), 50, 50);
                DropDownItem it = new DropDownItem();
                it.Image = bmp;
                it.Value = String.Empty;
                cboLEye.Items.Add(it);
                cboREye.Items.Add(it);
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
            charName.Text = cData.Character.Name;
            charLevel.Text = Convert.ToString(cData.Character.Stats.Level);
            charExp.Text = Convert.ToString(cData.Character.Stats.CurrentXP);
            charFerocity.Text = Convert.ToString(cData.Character.Stats.Strength);
            charToughness.Text = Convert.ToString(cData.Character.Stats.Constitution);
            charAgility.Text = Convert.ToString(cData.Character.Stats.Agility);
            charWillpower.Text = Convert.ToString(cData.Character.Stats.Power);

            charGender.SelectedIndex = cData.Character.CharacterCustomization.Sex;
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
