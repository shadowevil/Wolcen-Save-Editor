using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WolcenEditor
{
    public class SkillManager
    {
        public static bool isShiftDown { get; set; }
        public static bool isCtrlDown { get; set; }
        public static TabPage skillPage { get; private set; }

        public static Dictionary<string, SkillImageBox> SkillTreeDict = new Dictionary<string, SkillImageBox>
        {
            { "player_aetherblade", new SkillImageBox("Infinity Blades") },
            { "player_aetherblast", new SkillImageBox("Eclipse") },
            { "player_arrowsrain", new SkillImageBox("Wailing Arrows") },
            { "player_bladeslinger", new SkillImageBox("Phantom Blades") },
            { "player_bomb", new SkillImageBox("Havoc Orb") },
            { "player_brutalstrike", new SkillImageBox("Anvil's Woe") },
            { "player_bulleye", new SkillImageBox("Gunslinger's Brand") },
            { "player_chainlightning", new SkillImageBox("Thunderstrike") },
            { "player_charge", new SkillImageBox("Warpath") },
            { "player_corpseexplosion", new SkillImageBox("Plagueburst") },
            { "player_deathmark", new SkillImageBox("Mark of Impurity") },
            { "player_dualstrike", new SkillImageBox("Slayer's Flurry") },
            { "player_fireball", new SkillImageBox("Consuming Embers") },
            { "player_frostnova", new SkillImageBox("Winter's Grasp") },
            { "player_frostcomet", new SkillImageBox("Tear of Etheliel") },
            { "player_frostlance", new SkillImageBox("Arctic Spear") },
            { "player_hammer", new SkillImageBox("Flight of Gaavanir") },
            { "player_holydive", new SkillImageBox("Light-bringer") },
            { "player_hook", new SkillImageBox("Tracker's Reach") },
            { "player_ironguard", new SkillImageBox("Juggernaut") },
            { "player_laceration", new SkillImageBox("Bleeding Edge") },
            { "player_laser", new SkillImageBox("Annihilation") },
            { "player_leap", new SkillImageBox("Wings of Ishmir") },
            { "player_possession", new SkillImageBox("Parasite") },
            { "player_sacredground", new SkillImageBox("Bulwark of Dawn") },
            { "player_smokebomb", new SkillImageBox("Duskshroud") },
            { "player_sniper", new SkillImageBox("\"Deathgazer\" Railgun") },
            { "player_spreadshot", new SkillImageBox("Stings of Krearion") },
            { "player_summon_champion", new SkillImageBox("Livor Mortis") },
            { "player_summon_melee", new SkillImageBox("Feeding Swarm") },
            { "player_summon_ranged", new SkillImageBox("Hunting Swarm") },
            { "player_teleport", new SkillImageBox("Aether Jump") },
            { "player_turret", new SkillImageBox("\"Avenger\" Autoturret") },
            { "player_vault", new SkillImageBox("Evasion") },
            { "player_vortex", new SkillImageBox("Anomaly") },
            { "player_warcry", new SkillImageBox("Sovereign Shout") },
            { "player_whirlwind", new SkillImageBox("Bladestorm") },
            { "player_ringofpain", new SkillImageBox("Blood for Blood") },
            { "player_reave", new SkillImageBox("Wrath of Baapheth") },
            { "player_solarfall", new SkillImageBox("Solarfall") }
        };

        public static void LoadTree(ref TabControl tabControl)
        {
            isShiftDown = false;
            int x = 15;
            int y = 25;
            int lineCount = 1;
            skillPage = tabControl.TabPages["charSkills"];

            foreach (var skill in SkillTreeDict)
            {
                skill.Value.sImage = new Bitmap(Image.FromFile(@".\UIResources\Skills\" + skill.Key + ".png"), 50, 50);

                string pbName = "_" + skill.Key;
                skillPage.Controls.Add(createPictureBox(pbName, skill.Value.sImage.Size, skill.Value.sImage, new Point(x, y)));
                PictureBox pb = (skillPage.Controls.Find(pbName, true)[0] as PictureBox);

                ToolTip tp = new ToolTip();
                tp.SetToolTip(pb, "Left click to activate or level up a skill.\n"
                                + "Right click to remove a level.\n"
                                + "Shift (right/left) click to (add/remove) levels.\n"
                                + "Ctrl right click to remove skill from player.");

                skillPage.Controls.Add(createLabel("_cap" + skill.Key, skill.Value.Name, new Size(74, 30),
                    ContentAlignment.TopCenter, new Point(x - 12, y + pb.Height + 2), 7));

                skillPage.Controls.Add(createLabel("_lbl" + skill.Key, "0", new Size(30, 20),
                    ContentAlignment.MiddleCenter, new Point(x + 20 - 10, y + pb.Height + 2 + 30), 11, true, true));

                x += skill.Value.sImage.Width + 24;
                lineCount++;
                if (lineCount >= 12)
                {
                    x = 15;
                    lineCount = 1;
                    y += skill.Value.sImage.Height + 75;
                }
            }

            var unlockAllButton = new Button();
            unlockAllButton.Location = new System.Drawing.Point(735, 477);
            unlockAllButton.Name = "unlockAllButton";
            unlockAllButton.Size = new System.Drawing.Size(75, 23);
            unlockAllButton.TabIndex = 0;
            unlockAllButton.Text = "Unlock All";
            unlockAllButton.UseVisualStyleBackColor = true;
            unlockAllButton.Click += unlockAllButton_Click;
            skillPage.Controls.Add(unlockAllButton);

            var lockAllButton = new Button();
            lockAllButton.Location = new System.Drawing.Point(735, 506);
            lockAllButton.Name = "lockAllButton";
            lockAllButton.Size = new System.Drawing.Size(75, 23);
            lockAllButton.TabIndex = 1;
            lockAllButton.Text = "Lock All";
            lockAllButton.UseVisualStyleBackColor = true;
            lockAllButton.Click += lockAllButton_Click;
            skillPage.Controls.Add(lockAllButton);


        }

        private static PictureBox createPictureBox(string name, Size size, Image bgImage, Point location)
        {
            PictureBox pb = new PictureBox();
            pb.Name = name;
            pb.Size = size;
            pb.MaximumSize = pb.Size;
            pb.BackgroundImage = bgImage;
            pb.BackgroundImageLayout = ImageLayout.Stretch;
            pb.Image = WolcenEditor.Properties.Resources.c_beltSlot;
            pb.MouseMove += Pb_MouseMove;
            pb.MouseLeave += Pb_MouseLeave;
            pb.MouseClick += Pb_MouseClick;
            pb.Visible = true;
            pb.Location = location;
            pb.BorderStyle = BorderStyle.Fixed3D;
            return pb;
        }

        private static Label createLabel(string name, string text, Size size, ContentAlignment align, Point location, int fontSize, bool AutoSize = true, bool border = false)
        {
            Label lb = new Label();
            lb.Name = name;
            lb.Text = text;
            lb.MaximumSize = size;
            lb.MinimumSize = lb.MaximumSize;
            lb.AutoSize = AutoSize;
            lb.TextAlign = align;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, fontSize, FontStyle.Regular);
            lb.ForeColor = Color.White;
            lb.Location = location;
            if(border) lb.BorderStyle = BorderStyle.FixedSingle;
            return lb;
        }

        public static UnlockedSkill ActivateSkill(string name)
        {
            UnlockedSkill uSkill = new UnlockedSkill();
            uSkill.SkillName = name.Substring(1, name.Length - 1);
            uSkill.Level = 0;
            uSkill.CurrentXp = "0";
            uSkill.Variants = "0000000000000000";
            return uSkill;
        }

        public static void RemoveSkill(PictureBox pb)
        {
            int i = 0;
            foreach (var uSkill in cData.Character.UnlockedSkills)
            {
                if (pb.Name.Substring(1, pb.Name.Length - 1) == uSkill.SkillName)
                {
                    cData.Character.UnlockedSkills.RemoveAt(i);
                    TabControl tabControl = (skillPage.Parent as TabControl);
                    LoadSkillInformation(ref tabControl);
                    return;
                }
                i++;
            }
        }

        private static void Pb_MouseClick(object sender, MouseEventArgs e)
        {
            if (cData.Character == null) return;
            PictureBox pb = (sender as PictureBox);

            if (e.Button == MouseButtons.Left)
            {
                if (pb.Image != null)
                {
                    cData.Character.UnlockedSkills.Add(ActivateSkill(pb.Name));
                    pb.Image = null;
                }
                else
                {
                    int i = 0;
                    foreach (UnlockedSkill skill in cData.Character.UnlockedSkills)
                    {
                        if (pb.Name == ("_" + skill.SkillName))
                        {
                            if (isShiftDown)
                            {
                                if (cData.Character.UnlockedSkills[i].Level + 10 >= 90) cData.Character.UnlockedSkills[i].Level = 90;
                                else cData.Character.UnlockedSkills[i].Level += 10;
                            }
                            else
                            {
                                if (cData.Character.UnlockedSkills[i].Level >= 90) return;
                                cData.Character.UnlockedSkills[i].Level++;
                            }
                            skillPage.Controls["_lbl" + skill.SkillName].Text = cData.Character.UnlockedSkills[i].Level.ToString();
                            return;
                        }
                        i++;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (isCtrlDown && pb.Image == null) RemoveSkill(pb);
                
                int i = 0;
                foreach (UnlockedSkill skill in cData.Character.UnlockedSkills)
                {
                    if (pb.Name == ("_" + skill.SkillName))
                    {
                        if (isShiftDown)
                        {
                            if (cData.Character.UnlockedSkills[i].Level - 10 <= 0) cData.Character.UnlockedSkills[i].Level = 0;
                            else cData.Character.UnlockedSkills[i].Level -= 10;
                        }
                        else
                        {
                            if (cData.Character.UnlockedSkills[i].Level - 1 < 0) return;
                            cData.Character.UnlockedSkills[i].Level--;
                        }
                        skillPage.Controls["_lbl" + skill.SkillName].Text = cData.Character.UnlockedSkills[i].Level.ToString();
                        return;
                    }
                    i++;
                }
            }
        }

        public static void LoadSkillInformation(ref TabControl tabControl)
        {
            foreach (var skill in SkillTreeDict)
            {
                skillPage.Controls["_lbl" + skill.Key].Text = "0";
                (skillPage.Controls["_" + skill.Key] as PictureBox).Image = WolcenEditor.Properties.Resources.c_beltSlot;
                if (cData.Character != null)
                {
                    foreach (UnlockedSkill uSkill in cData.Character.UnlockedSkills)
                    {
                        if (uSkill.SkillName == skill.Key)
                        {
                            PictureBox pb = (skillPage.Controls["_" + skill.Key] as PictureBox);
                            pb.Image = null;
                            Label lb2 = (skillPage.Controls["_lbl" + uSkill.SkillName] as Label);
                            lb2.Text = uSkill.Level.ToString();
                        }
                    }
                }
            }
        }

        private static void Pb_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.BorderStyle = BorderStyle.Fixed3D;
        }

        private static void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void unlockAllButton_Click(object sender, EventArgs e)
        {
            var skillList = new List<UnlockedSkill>();
            foreach (var skill in SkillManager.SkillTreeDict.Keys)
            {
                var skillObj = SkillManager.ActivateSkill("_" + skill);
                skillObj.Level = 90;
                skillList.Add(skillObj);
            }
            cData.Character.UnlockedSkills = skillList;
            TabControl tabControl = (SkillManager.skillPage.Parent as TabControl);
            LoadSkillInformation(ref tabControl);
        }

        private static void lockAllButton_Click(object sender, EventArgs e)
        {
            foreach (var skill in cData.Character.UnlockedSkills.ToList())
            {
                var pic = new PictureBox();
                pic.Name = "_" + skill.SkillName;
                RemoveSkill(pic);
            }
        }
    }

    public class SkillImageBox
    {
        public string Name { get; set; }
        public Image sImage { get; set; }
        public int Level { get; set; }

        public SkillImageBox(string _name)
        {
            Name = _name;
            sImage = null;
            Level = 0;
        }
    }
}
