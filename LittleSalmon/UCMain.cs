﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pet;

namespace LittleSalmon
{


    public partial class UCMain : UserControl
    {
        private int _iconWidth { get; } = 256;
        private Bitmap[] fishPictures; // icons for the fish
        private DateTime fallenAsleep; // when the Pet fell asleep, used for sleeping time calculation
        private MeterDisplayer[] meterDisplayers;
        

        public UCMain()
        {
            InitializeComponent();
            UsefulForDesign.CenterControlHorizontally(Label_Name);
            myButton_StartOver.Visible = false;

            MainForm.PetLifeTick += (s, e) =>
            {
                UpdatePetStatusDisplay();
                UpdateStats();
            };
            MainForm.PetDead += (s, e) => PetDead();

            fishPictures = FileHelper.SplitIcons(new Bitmap(FileHelper.GetPathToResources("basicFishIcons.png")), _iconWidth);

            pictureBox_Dirty.Image = new Bitmap(FileHelper.GetPathToResources("dirtyIconTransparent.png"));
            pictureBox_Dirty.Cursor = Cursors.Hand; // make the dirty icon fancy clickable

            meterDisplayers = new MeterDisplayer[]
            {
                new MeterDisplayer(Panel_Energy, new Panel(), new Mirror(() => MainForm.thePet.EnergyMeter)),
                new MeterDisplayer(Panel_Hunger, new Panel(), new Mirror(() => MainForm.thePet.HungerMeter)),
                new MeterDisplayer(Panel_Mood, new Panel(), new Mirror(() => MainForm.thePet.MoodMeter))
            };

            Label_Name.Cursor = Cursors.Hand; // make the name label fancy clickable
            TextBox_NewNameBox.Visible = false;
           
            Button_ChangeNameSubmit.Visible = false;
            TextBox_Stats.Visible = false; // hide the Stats text box initially

            this.BackColor = MainForm.MyDefaultBackColor;

            UpdatePetStatusDisplay();
            UpdateStats();

        }


        #region visual settings

        /// <summary>
        /// shows color dialog to choose a background color for the background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ChooseColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = this.BackColor; 
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    MainForm.MyDefaultBackColor = colorDialog.Color;
                    this.BackColor = MainForm.MyDefaultBackColor; // set the background color to the selected color
                }
            }
        }

        /// <summary>
        /// adjusts the progress bar value and color based on the given value.
        /// </summary>
        private void SetPanelProgressBarValue(MeterDisplayer m)
        {
            int barWidth = m.BackgroundPanel.Width * m.Mirror.Value / m.Max;

            m.BarPanel.Width = barWidth;
            m.BarPanel.Height = m.BackgroundPanel.Height;

            if (m.Mirror.Value < 15)
                m.BarPanel.BackColor = Color.Red;
            else if (m.Mirror.Value < 30)
                m.BarPanel.BackColor = Color.Orange;
            else if (m.Mirror.Value < 50)
                m.BarPanel.BackColor = Color.Yellow;
            else if (m.Mirror.Value < 100)
                m.BarPanel.BackColor = Color.Green;
            else
                m.BarPanel.BackColor = Color.Blue;

            m.BackgroundPanel.Controls.Clear();
            m.BackgroundPanel.Controls.Add(m.BarPanel);
        }


        /// <summary>
        /// Updates the Pet's status display with current values from the Pet object.
        /// </summary>
        private void UpdatePetStatusDisplay()
        {
            PictureBox_PetBox.Image = fishPictures[(int)MainForm.thePet.LifeState];
            Label_Name.Text = MainForm.thePet.Name;
            UsefulForDesign.CenterControlHorizontally(Label_Name);
            Label_FoodCountLabel.Text = "Food count: " + MainForm.thePet.FoodCount.ToString();

            foreach (MeterDisplayer m in meterDisplayers)
            {
                SetPanelProgressBarValue(m);
            }
            
            if (MainForm.thePet.IsDirty)
            {
                pictureBox_Dirty.Visible = true;
                pictureBox_Dirty.BringToFront();
            }
            else
            {
                pictureBox_Dirty.Visible = false;
            }
        }

        private void button_ShowStats_Click(object sender, EventArgs e)
        {
            TextBox_Stats.Visible = !TextBox_Stats.Visible;
        }
        private void UpdateStats()
        {
            StringBuilder statsDisplay = new StringBuilder();
            foreach (var stat in MainForm.theStats.StatsDict)
            {
                statsDisplay.AppendLine($"{stat.Key}: {stat.Value}");
            }
            TextBox_Stats.Text = statsDisplay.ToString();
        }

        


        private void ReportToUser(string message)
        {
            TextBox_Reporter.Text = message + Environment.NewLine + TextBox_Reporter.Text;
        }



        private void PetDead()
        {
            myButton_StartOver.Visible = true;
            myButton_Feed.Enabled = false;
            myButton_SelectGame.Enabled = false;
            myButton_Sleep.Enabled = false;
            Label_Name.Text = "Your pet " + MainForm.thePet.Name + " has died.";
            UsefulForDesign.CenterControlHorizontally(Label_Name);
            PictureBox_PetBox.Image = fishPictures[(int)Pet.Pet.LifeStates.Dead];
            this.BackColor = Color.Maroon;
            MessageBox.Show("Your pet " + MainForm.thePet.Name + " has died.", "Pet Dead", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        public event EventHandler? StartOver;
        private void button_StartOver_Click(object sender, EventArgs e)
        {
            myButton_StartOver.Visible = false;
            StartOver?.Invoke(this, EventArgs.Empty);
            
        }


        #endregion

        #region name change
        private void Label_Name_Click(object sender, EventArgs e)
        {
            TextBox_NewNameBox.Visible = true;
            TextBox_NewNameBox.Text = MainForm.thePet.Name;
            Button_ChangeNameSubmit.Visible = true;
        }

        private void Button_ChangeNameSubmit_Click(object sender, EventArgs e)
        {
            string newName = TextBox_NewNameBox.Text.Trim();
            if (newName.Length == 0)
            {
                MessageBox.Show("You must give your pet a name!");
                return;
            }
            MainForm.thePet.Name = newName;
            UpdatePetStatusDisplay();
            TextBox_NewNameBox.Visible = false;
            Button_ChangeNameSubmit.Visible = false;
        }
        private ToolTip tooltip = new ToolTip();
        private void LabelName_MouseHover(object sender, EventArgs e)
        {
            Point mousePos = Label_Name.PointToClient(Cursor.Position);
            tooltip.Show("change name", Label_Name, mousePos.X + 15, mousePos.Y + 15, 1500);
        }



        #endregion

        #region games button
        public event EventHandler? GamesButtonClicked;
        private void button_GamesButton_Click(object sender, EventArgs e)
        {
            GamesButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region feeding

        /// <summary>
        /// Calls TryFeed on Pet and shows the player the result of the feeding attempt.
        /// </summary>
        private void button_Feed_Click(object sender, EventArgs e)
        {
            Pet.Pet.FeedingResult result = MainForm.thePet.TryFeed();
            switch (result)
            {
                case Pet.Pet.FeedingResult.Successful:
                    MainForm.theStats.AdjustStat("Food units fed", 1);
                    UpdateStats();
                    ReportToUser("You fed your pet " + MainForm.thePet.Name + ".");
                    break;
                case Pet.Pet.FeedingResult.Fell:
                    MainForm.theStats.AdjustStat("Food units fell", 1);
                    UpdateStats();
                    ReportToUser("Yay, the food must have fallen somewhere...");
                    break;
                case Pet.Pet.FeedingResult.NoFood:
                    ReportToUser("You have NO food to feed your pet.");
                    break;
                case Pet.Pet.FeedingResult.TooMuch:
                    ReportToUser("You overfed your pet " + MainForm.thePet.Name + ".");
                    break;
            }
            UpdatePetStatusDisplay();
        }
        #endregion

        #region sleeping
        private void SleepUpdateComponents()
        {
            myButton_Feed.Enabled = false;
            myButton_SelectGame.Enabled = false;
            myButton_Sleep.Text = "wake up";
        }
        private void WakeUpUpdateComponents()
        {
            myButton_Feed.Enabled = true;
            myButton_SelectGame.Enabled = true;
            myButton_Sleep.Text = "sleep";
        }
        private void button_Sleep_Click(object sender, EventArgs e)
        {
            if (MainForm.thePet.LifeState == Pet.Pet.LifeStates.Awake)
            {
                MainForm.thePet.Sleep();
                fallenAsleep = DateTime.Now; // record the time when the Pet fell asleep
                ReportToUser("Your pet " + MainForm.thePet.Name + " fell asleep.");
                SleepUpdateComponents();
                UpdatePetStatusDisplay();
            }
            else
            {
                TimeSpan timeSlept = DateTime.Now - fallenAsleep; // calculate how long the Pet has been asleep
                MainForm.theStats.AdjustStat("Seconds slept", (int)timeSlept.TotalSeconds);
                MainForm.thePet.WakeUp();
                ReportToUser("Your pet " + MainForm.thePet.Name + " woke up.");
                WakeUpUpdateComponents();
                UpdatePetStatusDisplay();
            }
        }
        #endregion

        #region cleaning
        private void dirtyComponent_Click(object sender, EventArgs e)
        {
            MainForm.thePet.Clean();
            ReportToUser("You cleaned your pet " + MainForm.thePet.Name + ".");
            MainForm.theStats.AdjustStat("Excrement disposed", 1);
            UpdatePetStatusDisplay();
        }
        #endregion
    }
}
