using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class CityManager
    {
        private static TabPage charCityForm;
        public static void initCity(TabPage charCity)
        {
            charCityForm = charCity;
            LoadBuildingImages();
        }

        // Load a grid of images for each city building
        private static void LoadBuildingImages()
        {
            var buildings = WolcenStaticData.CityBuildings.Keys;

            var xPos = 48;
            var yPos = 20;
            foreach(var building in buildings)
            {
                var path = @"UIResources\Buildings\" + building + ".png";
                var buildingPictureBox = new PictureBox();
                var buildingImage = GetBuildingImage(path);
                buildingPictureBox.Image = buildingImage;
                buildingPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                buildingPictureBox.Location = new Point(xPos, yPos);
                buildingPictureBox.Name = building;
                buildingPictureBox.MouseHover += BuildingPictureBox_MouseHover;
                charCityForm.Controls.Add(buildingPictureBox);

                var buildingLabel = new Label();
                buildingLabel.Text = WolcenStaticData.CityBulidingsLocalization[building];
                buildingLabel.Location = new Point(xPos - 13, yPos + 110);

                buildingLabel.MinimumSize = new Size(110, 0);
                buildingLabel.MaximumSize = new Size(110, 0); 
                buildingLabel.AutoSize = true;
                buildingLabel.TextAlign = ContentAlignment.MiddleCenter;
                buildingLabel.ForeColor = Color.White;
                charCityForm.Controls.Add(buildingLabel);

                xPos += 130;
                if (xPos >= 800)
                {
                    xPos = 48;
                    yPos += 140;
                }
            }
        }

        // On MouseHover of a building image display a list of projects for the respective building.
        // and set the appropriate checkboxes if the building was already marked as finished in the save file.
        private static void BuildingPictureBox_MouseHover(object sender, EventArgs e)
        {
            if (cData.Character == null || cData.PlayerData == null)
                return;
            var clickedElement = (PictureBox)sender;
            var padding = 15;
            var projectsPanel = new Panel
            {
                Name = clickedElement.Name,
                AutoScroll = true,
                Location = new Point(clickedElement.Location.X - (padding * 3), clickedElement.Location.Y - padding),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(29, 29, 29),
                ForeColor = Color.White,
                Size = new Size(81 + (padding * 6), 106 + (padding * 6))
            };
            charCityForm.Controls.Add(projectsPanel);

            List<string> buildingProjects = WolcenStaticData.CityBuildings[clickedElement.Name];
            buildingProjects = buildingProjects.Select(x => WolcenStaticData.CityProjectLocalization[x]).ToList();
            var buildingListView = new CheckedListBox();
            buildingListView.Items.AddRange(buildingProjects.ToArray());
            buildingListView.BackColor = Color.FromArgb(29, 29, 29);
            buildingListView.ForeColor = Color.White;
            buildingListView.BorderStyle = BorderStyle.None;
            //buildingListView.Font = new Font(Form1.DefaultFont.FontFamily, 8, FontStyle.Regular);
            buildingListView.Parent = projectsPanel;
            buildingListView.Dock = DockStyle.Fill;
            buildingListView.CheckOnClick = true;

            buildingListView.MouseLeave += BuildingListView_MouseLeave;

            foreach (var project in buildingProjects)
            {
                var i = buildingListView.FindStringExact(project);
                foreach (var item in cData.PlayerData.SoftcoreNormal.CityBuilding.FinishedProjects)
                {
                    if (project == WolcenStaticData.CityProjectLocalization[item.Name])
                        buildingListView.SetItemChecked(i, true);
                }
            }

            projectsPanel.Controls.Add(buildingListView);

            charCityForm.ControlAdded += (sender2, e2) => { charCityForm.Controls.Remove(projectsPanel); };
            projectsPanel.BringToFront();

        }

        // On MouseLeave add the currently selected options to the save file and delete any unchecked options from the file.
        private static void BuildingListView_MouseLeave(object sender, EventArgs e)
        {
            var buildingListView = (CheckedListBox)sender;

            for(int i = 0; i < buildingListView.Items.Count; i++)
            {
                // Item is checked and does not already exist in our save file.
                var localizedName = WolcenStaticData.CityProjectInFile[buildingListView.Items[i].ToString()];
                if (buildingListView.GetItemChecked(i) == true && !cData.PlayerData.SoftcoreNormal.CityBuilding.FinishedProjects.Any(x => x.Name == localizedName))
                {
                    cData.PlayerData.SoftcoreNormal.CityBuilding.FinishedProjects.Add(new FinishedProjects { Name = localizedName });
                }
                else if (buildingListView.GetItemChecked(i) == false)
                {
                    var toBeRemoved = cData.PlayerData.SoftcoreNormal.CityBuilding.FinishedProjects.Where(x => x.Name == localizedName);
                    foreach(var item in toBeRemoved.ToList())
                    {
                        cData.PlayerData.SoftcoreNormal.CityBuilding.FinishedProjects.Remove(item);
                    }
                }
            }
        }

        // Gets the image of a building image and scales it to half by default.
        private static Bitmap GetBuildingImage(string filePath, double scale = 0.5)
        {
            var originalImage = new Bitmap(filePath);
            var newSize = new Size((int) (originalImage.Width * scale), (int) (originalImage.Height * scale));
            var newImage = new Bitmap(originalImage, newSize);
            return newImage;
        }
    }
}
