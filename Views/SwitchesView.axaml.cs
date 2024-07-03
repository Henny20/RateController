using System;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RateController.PGNs;

namespace RateController.Views
{
    public partial class SwitchesView : Window
    {
        private bool DownPressed;
        private bool IsTransparent;
        private bool MasterPressed;
        private MainWindow mf;
        private int mouseX = 0;
        private int mouseY = 0;
        private bool[] SwON = new bool[23];
        private int TransLeftOffset = 6;
        private int TransTopOffset = 30;
        private bool UpPressed;
        private int windowLeft = 0;
        private int windowTop = 0;
        
        private DispatcherTimer tmrRelease = new();
        private DispatcherTimer timer1 = new();
        
         public SwitchesView()
        {
            InitializeComponent();
        }
        
        public SwitchesView(MainWindow CallingForm):this()
        {
            mf = CallingForm;
           
            mf.SwitchBox.SwitchPGNreceived += SwitchBox_SwitchPGNreceived;
            this.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230));//Properties.Settings.Default.DayColour;
        }
        
         public void SetDescriptions()
        {
            mf.SwitchObjects.CheckDescriptions();
            /***TODO
            btn1.Text = mf.SwitchObjects.Item(0).Description;
            btn2.Text = mf.SwitchObjects.Item(1).Description;
            btn3.Text = mf.SwitchObjects.Item(2).Description;
            btn4.Text = mf.SwitchObjects.Item(3).Description;
            btn5.Text = mf.SwitchObjects.Item(4).Description;
            btn6.Text = mf.SwitchObjects.Item(5).Description;
            btn7.Text = mf.SwitchObjects.Item(6).Description;
            btn8.Text = mf.SwitchObjects.Item(7).Description;
            ************/
        }

        private void btAuto_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.Auto);
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.sw0);
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.sw1);
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.sw2);
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.sw3);
        }

        private void btnAutoRate_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.AutoRate);
        }

        private void btnAutoSection_Click(object sender, RoutedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.AutoSection);
        }

        private void btnDown_MouseDown(object sender, PointerPressedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.RateDown);
            DownPressed = true;
            tmrRelease.IsEnabled = true;
        }

        private void btnDown_MouseUp(object sender, PointerPressedEventArgs e)
        {
            DownPressed = false;
        }

        private void btnMaster_MouseDown(object sender, PointerPressedEventArgs e)
        {
            if (mf.SwitchBox.MasterOn)
            {
                mf.vSwitchBox.PressSwitch(SwIDs.MasterOff);
                MasterPressed = true;
                tmrRelease.IsEnabled = true;
            }
            else
            {
                mf.vSwitchBox.PressSwitch(SwIDs.MasterOn);
                MasterPressed = true;
                tmrRelease.IsEnabled = true;
            }
        }

        private void btnMaster_MouseUp(object sender, PointerPressedEventArgs e)
        {
            MasterPressed = false;
        }

        private void btnUp_MouseDown(object sender, PointerPressedEventArgs e)
        {
            mf.vSwitchBox.PressSwitch(SwIDs.RateUp);
            UpPressed = true;
            tmrRelease.IsEnabled = true;
        }

        private void btnUp_MouseUp(object sender, PointerPressedEventArgs e)
        {
            UpPressed = false;
        }

        private void frmSimulation_FormClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mf.UseTransparent)
            {
                // move the window back to the default location
               // this.Top += -TransTopOffset;
              //  this.Left += -TransLeftOffset;
             // this.Position = (Position.X - TransTopOffset, Position.Y - TransLeftOffset); TODO
            }
            mf.Tls.SaveFormData(this);
            timer1.IsEnabled = false;
            mf.vSwitchBox.SwitchScreenOn = false;
        }

        private void frmSimulation_Load(object sender, RoutedEventArgs e)
        {
            mf.Tls.LoadFormData(this);
            SwON = mf.SwitchBox.Switches;
            UpdateForm();
            timer1.IsEnabled = true;
            mf.vSwitchBox.SwitchScreenOn = true;
            mf.vSwitchBox.PressSwitch(SwIDs.MasterOff);
            tmrRelease.Interval = TimeSpan.FromMilliseconds(500);
            tmrRelease.Tick += tmrRelease_Tick;
            tmrRelease.IsEnabled = true;
            UpdateForm();
        }

        private void mouseMove_MouseDown(object sender, PointerPressedEventArgs e)
        {
            // Log the current window location and the mouse location.
          //  if (e.Button == MouseButtons.Right)
           var point = e.GetCurrentPoint(sender as Control);
           var x = point.Position.X;
           var y = point.Position.Y;
           if (point.Properties.IsRightButtonPressed)
            {
                windowTop = (int)y; //this.Top;
                windowLeft = (int)x; // this.Left;
                mouseX = (int)x; //TODO
                mouseY = (int)y;
            }
        }

        private void mouseMove_MouseMove(object sender, PointerPressedEventArgs e)
        {
          //  if (e.Button == MouseButtons.Right)
           var point = e.GetCurrentPoint(sender as Control);
           var x = point.Position.X;
           var y = point.Position.Y;
           if (point.Properties.IsRightButtonPressed)
           {
                windowTop = (int)y;//this.Top;
                windowLeft = (int)x; //this.Left;

                this.Position = new PixelPoint(Position.X + (int)x - mouseX, Position.Y + (int)y - mouseY);
            }
        }

        private void SetTransparent()
        {
        /****TODO
            if (mf.UseTransparent)
            {
                this.TransparencyKey = (Properties.Settings.Default.IsDay) ? Properties.Settings.Default.DayColour : Properties.Settings.Default.NightColour;
                this.ControlBox = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Top += TransTopOffset;
                this.Left += TransLeftOffset;
                IsTransparent = true;
            }
            else
            {
                this.Text = "Switches";
                this.TransparencyKey = Color.Empty;
                this.ControlBox = true;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.Top += -TransTopOffset;
                this.Left += -TransLeftOffset;
                IsTransparent = false;
            }
            ***********/
        }

        private void SwitchBox_SwitchPGNreceived(object sender, PGN32618.SwitchPGNargs e)
        {
            SwON = e.Switches;
            UpdateForm();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void tmrRelease_Tick(object sender, EventArgs e)
        {
            if (!UpPressed && !DownPressed && !MasterPressed)
            {
                mf.vSwitchBox.ReleaseSwitch();
                tmrRelease.IsEnabled = false;
            }
        }

        private void UpdateForm()
        {
            if (mf.UseTransparent != IsTransparent) SetTransparent();

            if (SwON[0])
            {
                btAuto.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btAuto.Background = new SolidColorBrush(Colors.Red);
            }

            if (mf.SwitchBox.MasterOn)
            {
                btnMaster.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btnMaster.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[3])
            {
                btnUp.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btnUp.Background =  new SolidColorBrush(Colors.Transparent);  // this.TransparencyKey; TODO
            }

            if (SwON[4])
            {
                btnDown.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btnDown.Background =  new SolidColorBrush(Colors.Transparent);  //this.TransparencyKey; TODO
            }

            if (SwON[5])
            {
                btn1.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btn1.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[6])
            {
                btn2.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btn2.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[7])
            {
                btn3.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btn3.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[8])
            {
                btn4.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btn4.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[21])
            {
                btnAutoSection.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btnAutoSection.Background = new SolidColorBrush(Colors.Red);
            }

            if (SwON[22])
            {
                btnAutoRate.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                btnAutoRate.Background = new SolidColorBrush(Colors.Red);
            }

            if (mf.UseDualAuto)
            {
                btAuto.IsVisible = false;
                btnMaster.Width = 142;
                btnAutoRate.IsVisible = true;
                btnAutoSection.IsVisible = true;
                this.Height = 212;
                // turn off auto button
                if (SwON[0]) mf.vSwitchBox.PressSwitch(SwIDs.Auto);
            }
            else
            {
                btAuto.IsVisible = true;
                btnMaster.Width = 64;
                btnAutoRate.IsVisible = false;
                btnAutoSection.IsVisible = false;
                this.Height = 161;
                // turn off auto rate, auto section
                if (SwON[21]) mf.vSwitchBox.PressSwitch(SwIDs.AutoSection);
                if (SwON[22]) mf.vSwitchBox.PressSwitch(SwIDs.AutoRate);
            }
        }
    }
}
