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
    public class SkillTree
    {
        private static Image HoverImage = (Image)new Bitmap(Image.FromFile(@".\UIResources\Skills\Select.png"), 50, 50);

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
            { "player_hook", new SkillImageBox("Tracker's Reach") },
            { "player_ironguard", new SkillImageBox("Juggernaut") },
            { "player_laceration", new SkillImageBox("Bleeding Edge") },
            { "player_laser", new SkillImageBox("Annihilation") },
            { "player_leap", new SkillImageBox("Wings of Ishmir") },
            { "player_sacredground", new SkillImageBox("Bulwark of Dawn") },
            { "player_smokebomb", new SkillImageBox("Duskshroud") },
            { "player_sniper", new SkillImageBox("\"Deathgazer\" Railgun") },
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

            int x = 10;
            int y = 50;
            int lineCount = 1;
            foreach (var skill in SkillTreeDict)
            {
                skill.Value.sImage = new Bitmap(Image.FromFile(@".\UIResources\Skills\" + skill.Key + ".png"), 50, 50);

                PictureBox pb = new PictureBox();
                pb.Size = skill.Value.sImage.Size;
                pb.MaximumSize = skill.Value.sImage.Size;
                pb.BackgroundImage = skill.Value.sImage;
                pb.BackgroundImageLayout = ImageLayout.Stretch;
                pb.MouseMove += Pb_MouseMove;
                pb.MouseLeave += Pb_MouseLeave;
                pb.Visible = true;
                pb.Location = new Point(x, y);
                tabControl.TabPages["charSkills"].Controls.Add(pb);
                x += skill.Value.sImage.Width + 24;
                lineCount++;
                if (lineCount >= 12)
                {
                    x = 15;
                    lineCount = 1;
                    y += skill.Value.sImage.Height + 50;
                }
            }
        }

        private static void Pb_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.Image = null;
        }

        private static void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.Image = HoverImage;
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
