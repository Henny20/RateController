using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using RateController.Classes;
using RateController.PGNs;

namespace RateController.Views;

public partial class LargeScreenView : Window
{
    public clsAlarm RCalarm;
    private bool automode = true;
    private int Fan1RateType = 0;
    private int Fan2RateType = 0;
    private bool IsTransparent = false;
    private bool masterOn;
    private bool MasterPressed;
    private MainWindow mf;
    private double mouseX = 0;
    private double mouseY = 0;
    private clsProduct Prd;
   // private Brushes RateColour = Brushes.GreenYellow;
    private int RateType = 0;   // 0 current rate, 1 instantaneous rate, 2 overall rate
    private bool SwitchingScreens = false;
    private bool[] SwON = new bool[9];
    private int TransLeftOffset = 6;
    private int TransTopOffset = 30;
    private int windowLeft = 0;
    private int windowTop = 0;
    
    private DispatcherTimer timerMain = new();
    private DispatcherTimer tmrBorder = new();
    private DispatcherTimer tmrRelease = new();
    
    public LargeScreenView()
    {
        InitializeComponent();
        Loaded +=  frmLargeScreen_Load;
        Closing += frmLargeScreen_FormClosing;
        Closed += frmLargeScreen_FormClosed;
        Activated +=  frmLargeScreen_Activated;
        
        tmrBorder.Interval = TimeSpan.FromMilliseconds(500);
        tmrBorder.Tick += tmrBorder_tick;
        tmrRelease.Interval = TimeSpan.FromMilliseconds(500);
        tmrRelease.Tick += tmrRelease_Tick;
    }
    
     public LargeScreenView(MainWindow CallingForm):this()
    {
        #region // language

        lbRate.Text = Lang.lgCurrentRate;
        lbTarget.Text = Lang.lgTargetRate;
        lbCoverage.Text = Lang.lgCoverage;
        lbQuantity.Text = Lang.lgTank_Remaining + " ...";
/****TODO
        mnuSettings.Items["MnuProducts"].Content = Lang.lgProducts;
        mnuSettings.Items["MnuSections"].Content = Lang.lgSections;
        mnuSettings.Items["MnuRelays"].Content = Lang.lgRelays;
        mnuSettings.Items["MnuComm"].Content = Lang.lgComm;
        mnuSettings.Items["calibrateToolStripMenuItem1"].Content = Lang.lgCalibrate;
        mnuSettings.Items["networkToolStripMenuItem"].Content = Lang.lgModules;
        mnuSettings.Items["MnuOptions"].Content = Lang.lgOptions;
        mnuSettings.Items["exitToolStripMenuItem"].Content = Lang.lgExit;

        mnuSettings.Items["pressuresToolStripMenuItem1"].Content = Lang.lgPressure;
        mnuSettings.Items["commDiagnosticsToolStripMenuItem1"].Content = Lang.lgCommDiagnostics;
        mnuSettings.Items["newToolStripMenuItem"].Content = Lang.lgNew;
        mnuSettings.Items["openToolStripMenuItem"].Content = Lang.lgOpen;
        mnuSettings.Items["saveAsToolStripMenuItem"].Content = Lang.lgSaveAs;
**********/
        #endregion // language

        mf = CallingForm;
        Prd = mf.Products.Item(0);
        RCalarm = new clsAlarm(mf, btAlarm);

        mf.SwitchBox.SwitchPGNreceived += SwitchBox_SwitchPGNreceived;

        this.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlRate0.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlRate1.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlRate1.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlRate1.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlQuantity0.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlQuantity1.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlQuantity2.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        pnlQuantity3.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        btnUp.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
        btnDown.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
   /***
        foreach (Control Ctrl in Controls)
        {
            if (Ctrl.Name != "btnSettings" && Ctrl.Name != "btAuto")
            {
                Ctrl.MouseDown += mouseMove_MouseDown;
                Ctrl.MouseMove += mouseMove_MouseMove;
            }
            else if (Ctrl.Name == "btAuto")
            {
                Ctrl.MouseDown += btAuto_MouseDown;
            }
        }
        *******/
    }
    
    
     public int CurrentProduct()
        {
            return Prd.ID;
        }

        public void SetTransparent()
        {
            IsTransparent = mf.UseTransparent;
            if (mf.UseTransparent)
            {
                this.Content = string.Empty;
               // this.TransparencyKey = (Properties.Settings.Default.IsDay) ? Properties.Settings.Default.DayColour : Properties.Settings.Default.NightColour;
                //this.Opacity = 0;
               // this.HelpButton = false;
                //this.ControlBox = false; 
                //this.FormBorderStyle = FormBorderStyle.None; TODO
               // this.Top += TransTopOffset;
               // this.Left += TransLeftOffset;
                this.Margin = new Thickness(TransLeftOffset, TransTopOffset); //TODO
                var txtcolor = Brushes.White;//new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)); 
                lbRate.Foreground = txtcolor;
                lbTarget.Foreground = txtcolor;
                lbCoverage.Foreground = txtcolor;
                lbQuantity.Foreground = txtcolor;
                lbRateAmount.Foreground = txtcolor;
                lbTargetAmount.Foreground = txtcolor;
                lbCoverageAmount.Foreground = txtcolor;
                lbQuantityAmount.Foreground = txtcolor;
                lbRPM1.Foreground = txtcolor;
                lbRPM2.Foreground = txtcolor;
                lbUnits.Foreground = txtcolor;
                lblManAuto.Foreground = txtcolor;
            }
            else
            {
                this.Content = "RateController";
               // this.TransparencyKey = Brushes.Transparent;
                //this.Opacity = 100;
               // this.HelpButton = true;
                //this.ControlBox = true;
                //this.FormBorderStyle = FormBorderStyle.FixedDialog; TODO
               // this.Top += -TransTopOffset;
               // this.Left += -TransLeftOffset;
               this.Margin = new Thickness(-TransLeftOffset, -TransTopOffset, 0, 0);

                var txtcolor = Brushes.Blue; //TODO
                lbRate.Foreground = txtcolor;
                lbTarget.Foreground = txtcolor;
                lbCoverage.Foreground = txtcolor;
                lbQuantity.Foreground = txtcolor;
                lbRateAmount.Foreground = txtcolor;
                lbTargetAmount.Foreground = txtcolor;
                lbCoverageAmount.Foreground = txtcolor;
                lbQuantityAmount.Foreground = txtcolor;
                lbRPM1.Foreground = txtcolor;
                lbRPM2.Foreground = txtcolor;
                lbUnits.Foreground = txtcolor;
                lblManAuto.Foreground = txtcolor;
            }
            SetFont();
        }

        public void SwitchToStandard()
        {
            this.ShowInTaskbar = false;
            mf.ShowInTaskbar = true;
            mf.UseLargeScreen = false;
            SwitchingScreens = true;
            this.Close();
        }

        private void btAlarm_Click(object sender, RoutedEventArgs e)
        {
            RCalarm.Silence();
        }

        private void btAuto_Click(object sender, RoutedEventArgs e)
        {
            if (automode)
            {
                mf.vSwitchBox.PressSwitch(SwIDs.Auto, true);
            }
            else
            {
                if (masterOn)
                {
                    mf.vSwitchBox.PressSwitch(SwIDs.MasterOff, true);
                    MasterPressed = true;
                    tmrRelease.IsEnabled = true;
                }
                else
                {
                    mf.vSwitchBox.PressSwitch(SwIDs.MasterOn, true);
                    MasterPressed = true;
                    tmrRelease.IsEnabled = true;
                }
            }
        }

        private void btAuto_MouseDown(object sender, PointerPressedEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            var point = e.GetCurrentPoint(sender as Control);
            if (point.Properties.IsRightButtonPressed)
            {
                automode = !automode;
                if (automode)
                {
                    lblManAuto.Text = "AUTO";
                }
                else
                {
                    lblManAuto.Text = "MASTER";
                }

                UpdateSwitches();
            }
        }

        private void btAuto_MouseUp(object sender, PointerPressedEventArgs e)
        {
            MasterPressed = false;
        }

        private void btMinimize_Click(object sender, RoutedEventArgs e)
        {
           // Form restoreform = new RCRestore(this); TODO
          //  restoreform.Show();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (SwON[0])
            {
                Prd.RateSet = Prd.RateSet / 1.05;
            }
            else
            {
                Prd.ManualPWM -= 5;
            }
        }

        private void btnFan1_Click(object sender, RoutedEventArgs e)
        {
            clsProduct fn = mf.Products.Item(mf.MaxProducts - 2);
            fn.FanOn = !fn.FanOn;
        }

        private void btnFan2_Click(object sender, RoutedEventArgs e)
        {
            clsProduct fn = mf.Products.Item(mf.MaxProducts - 1);
            fn.FanOn = !fn.FanOn;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
          //  Button btnSender = (Button)sender;  TODO
          //  Point ptLowerLeft = new Point(0, btnSender.Height);
          //  ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
           // mnuSettings.Show(ptLowerLeft);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (SwON[0])
            {
                Prd.RateSet = Prd.RateSet * 1.05;
            }
            else
            {
                Prd.ManualPWM += 5;
            }
        }

        private void calibrateToolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            //check if window already exists
            /**************
            Form fs = mf.Tls.IsFormOpen("frmCalibrate");

            if (fs == null)
            {
                Form frm = new frmCalibrate(mf);
                frm.Show();
            }
            else
            {
                fs.Focus();
            }
            ************/
            Window fs = mf.Tls.IsViewOpen("RateController.Views.CalibrateView");

            if (fs == null)
            {
                Window frm = new CalibrateView(mf);
                frm.ShowDialog(mf);
            }
            else
            {
                fs.Focus();
            }
        }

        private void commDiagnosticsToolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
           /******
            Form fs = mf.Tls.IsFormOpen("frmModule");

            if (fs == null)
            {
                Form frm = new frmModule(mf);
                frm.Show();
            }
            else
            {
                fs.Focus();
            }
            ***********/
             Window fs = mf.Tls.IsViewOpen("RateController.Views.ModuleView");

            if (fs == null)
            {
                Window frm = new ModuleView(mf);
                frm.ShowDialog(mf);
            }
            else
            {
                fs.Focus();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void frmLargeScreen_Activated(object sender, EventArgs e)
        {
            lbName0.Focus();
        }

        private void frmLargeScreen_FormClosed(object sender, EventArgs e)
        {
            mf.Tls.SaveFormData(this);

            timerMain.IsEnabled = false;
            if (mf.UseTransparent)
            {
                // move the window back to the default location
              //  this.Top += -TransTopOffset;
              //  this.Left += -TransLeftOffset;
               var pos = new PixelPoint(-TransTopOffset, -TransLeftOffset);
               this.Position = pos;
            }

            if (mf.UseLargeScreen) mf.LargeScreenExit = true;
            mf.WindowState = WindowState.Normal;
            mf.vSwitchBox.LargeScreenOn = false;
        }

        private async void frmLargeScreen_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!SwitchingScreens && !mf.Restart && mf.Products.Connected())
            {
               // var Hlp = new frmMsgBox(mf, "Confirm Exit?", "Exit", true);
               // Hlp.TopMost = true;
              //  Hlp.ShowDialog();
              //  bool Result = Hlp.Result;
              //  Hlp.Close();
              //  if (!Result) e.Cancel = true;
               var messageBoxStandardWindow =
                MessageBoxManager.GetMessageBoxStandard("Confirm Exit?", "Exit");
                var result =  await messageBoxStandardWindow.ShowAsync();
                if (result == ButtonResult.Yes) e.Cancel = true;
            }
        }

        private void frmLargeScreen_Load(object sender, EventArgs e)
        {
            mf.Tls.LoadFormData(this);
            Prd = mf.Products.Item(mf.DefaultProduct);

            UpdateForm();
            timerMain.IsEnabled = true;
            SwitchingScreens = false;
            mf.vSwitchBox.LargeScreenOn = true;
            mf.vSwitchBox.PressSwitch(SwIDs.MasterOff);
            tmrRelease.IsEnabled = true;
            UpdateForm();
        }

        private void lbCoverage_Click(object sender, RoutedEventArgs e)
        {
            mf.ShowCoverageRemaining = !mf.ShowCoverageRemaining;
            UpdateForm();
        }

        private void lbCoverage_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Shows either coverage done or area that can be done with the remaining quantity." +
                "\n Press to change.";

            mf.Tls.ShowHelp(Message, "Coverage");
            hlpevent.Handled = true;
        }

        private void lbFan1_Click(object sender, RoutedEventArgs e)
        {
            //check if window already exists
            /****
            Form fs = mf.Tls.IsFormOpen("FormSettings");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Form frm = new FormSettings(mf, mf.MaxProducts - 1);
            frm.Show();
            ************/
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, mf.MaxProducts - 1);
            frm.Show();
        }

        private void lbFan2_Click(object sender, RoutedEventArgs e)
        {
            //check if window already exists
            /************
            Form fs = mf.Tls.IsFormOpen("FormSettings");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Form frm = new FormSettings(mf, mf.MaxProducts);
            frm.Show();
            
            ********/
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, mf.MaxProducts);
            frm.Show();
        }

        private void lbName0_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(0);
            UpdateForm();

           //check if window already exists
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, Prd.ID + 1);
            frm.Show();
        }

        private void lbName0_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Indicates if the sensor is connected. Click to access settings.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void lbName1_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(1);
            UpdateForm();

            //check if window already exists
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, Prd.ID + 1);
            frm.Show();
        }

        private void lbName2_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(2);
            UpdateForm();

            //check if window already exists
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, Prd.ID + 1);
            frm.Show();
        }

        private void lbName3_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(3);
            UpdateForm();

            //check if window already exists
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, Prd.ID + 1);
            frm.Show();
        }

        private void lbQuantity_Click(object sender, RoutedEventArgs e)
        {
            mf.ShowQuantityRemaining = !mf.ShowQuantityRemaining;
            UpdateForm();
        }

        private void lbQuantity_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Shows either quantity applied or quantity remaining." +
                "\n Press to change.";

            mf.Tls.ShowHelp(Message, "Remaining");
            hlpevent.Handled = true;
        }

        private void lbRate_Click(object sender, RoutedEventArgs e)
        {
            RateType++;
            if (RateType > 2) RateType = 0;
            UpdateForm();
        }

        private void lbRate_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "1 - Current Rate, shows" +
                " the target rate when it is within 10% of target. Outside this range it" +
                " shows the exact rate being applied. \n 2 - Instant Rate, shows the exact rate." +
                "\n 3 - Overall, averages total quantity applied over area done." +
                "\n Press to change.";

            mf.Tls.ShowHelp(Message, "Rate");
            hlpevent.Handled = true;
        }

        private void lbRPM1_Click(object sender, RoutedEventArgs e)
        {
            Fan1RateType++;
            if (Fan1RateType > 1) Fan1RateType = 0;
            UpdateForm();
        }

        private void lbRPM2_Click(object sender, RoutedEventArgs e)
        {
            Fan2RateType++;
            if (Fan2RateType > 1) Fan2RateType = 0;
            UpdateForm();
        }

        private void lbTarget_Click(object sender, RoutedEventArgs e)
        {
            if (lbTarget.Text == Lang.lgTargetRate)
            {
                lbTarget.Text = Lang.lgTargetRateAlt;
                Prd.UseAltRate = true;
            }
            else
            {
                lbTarget.Text = Lang.lgTargetRate;
                Prd.UseAltRate = false;
            }
        }

        private void lbTarget_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Press to switch between base rate and alternate rate.";

            mf.Tls.ShowHelp(Message, "Target Rate");
            hlpevent.Handled = true;
        }

        private void mainform_MouseClick(object sender, PointerPressedEventArgs e)
        {
            //if (e.Button == MouseButtons.Left && mf.UseTransparent)
           var point = e.GetCurrentPoint(sender as Control);
           if (point.Properties.IsRightButtonPressed)
           {
               // this.FormBorderStyle = FormBorderStyle.FixedSingle; TODO
                tmrBorder.Start();
            }
        }

        private void MnuComm_Click(object sender, RoutedEventArgs e)
        {
              Window fs = mf.Tls.IsViewOpen("CommView");

            if (fs == null)
            {
                Window frm = new CommView(mf);
                frm.ShowDialog(mf);
            }
            else
            {
                fs.Focus();
            }
        }

        private async void MnuOptions_Click(object sender, RoutedEventArgs e)
        {
           Window fs = mf.Tls.IsViewOpen("RateController.View.OptionsView");

            if (fs == null)
            {
                 var form = new OptionsView(mf);
                await form.ShowDialog(mf);
              
            }
            else
            {
                fs.Focus();
            }
        }

        private void MnuProducts_Click(object sender, RoutedEventArgs e)
        {
            //check if window already exists
                  
            Window fs = mf.Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(mf, Prd.ID + 1);
            frm.ShowDialog(mf);
        }

        private void MnuRelays_Click(object sender, RoutedEventArgs e)
        {
        /********
            Form fs = mf.Tls.IsFormOpen("frmRelays");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Form frm = new frmRelays(mf);
            frm.Show();
            ***********/
            Window fs = mf.Tls.IsViewOpen("RelaysView");

            if (fs != null)
            {
               fs.Focus();
               return;
               
            }
            Window frm = new RelaysView(mf);
            frm.ShowDialog(mf);
        }

        private void MnuSections_Click(object sender, RoutedEventArgs e)
        {
            Window fs = mf.Tls.IsViewOpen("SectionsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

             Window frm = new SectionsView(mf);
            frm.ShowDialog(mf);
        }

        private void mouseMove_MouseDown(object sender, PointerPressedEventArgs args)
        {
            // Log the current window location and the mouse location.
            var point = args.GetCurrentPoint(sender as Control);
            var x = point.Position.X;
            var y = point.Position.Y;
            if (point.Properties.IsRightButtonPressed)
            {
               // windowTop = this.Top;
              //  windowLeft = this.Left;
               PixelPoint windowPos = this.Position;
                mouseX = x;
                mouseY = y;
            }
        }

        private void mouseMove_MouseMove(object sender, PointerPressedEventArgs args)
        {
        
            var point = args.GetCurrentPoint(sender as Control);
            var x = point.Position.X;
            var y = point.Position.Y;
            if (point.Properties.IsRightButtonPressed)
            {
                //windowTop = this.Top;
                //windowLeft = this.Left;
                PixelPoint windowPos = this.Position;


                var posX = windowPos.X + x - mouseX;
                var posY = windowPos.Y + y - mouseY;
                this.Position = new PixelPoint((int)posX, (int)posY);
            }
        }

        private void networkToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window fs = mf.Tls.IsViewOpen("RateController.Views.ModuleConfigView");

            if (fs == null)
            {
                Window frm = new ModuleConfigView(mf);
                frm.ShowDialog(mf);
            }
            else
            {
                fs.Focus();
            }
           
        }

        private async void newToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
        /********
            saveFileDialog1.InitialDirectory = mf.Tls.FilesDir();
            saveFileDialog1.Title = "New File";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    mf.Tls.OpenFile(saveFileDialog1.FileName);
                    mf.LoadSettings();
                    UpdateForm();
                }
            }
            *************/
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Directory = mf.Tls.FilesDir();
            saveFileDialog1.Title = "New File";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            var FileName = await saveFileDialog1.ShowAsync(this);
            if (FileName != null)
            {
                 if (FileName != "")
                {
                    mf.Tls.OpenFile(FileName);
                    mf.LoadSettings();
                    UpdateForm();
                }
            }
        }

        private async void openToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            /************
            openFileDialog1.InitialDirectory = mf.Tls.FilesDir();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                mf.Tls.PropertiesFile = openFileDialog1.FileName;
                mf.Products.Load();
                mf.LoadSettings();
                UpdateForm();
            }
            ******/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Directory = mf.Tls.FilesDir();
            string[] result = await openFileDialog1.ShowAsync(this);

            if (result != null)
            {
                mf.Tls.PropertiesFile = result[0];
                mf.Products.Load();
                mf.LoadSettings();
                UpdateForm();
            }
        }

        private void pbRate0_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Indicates Current rate compared to Target rate." +
                " Target rate is the centre of the graph. " +
                "Within 10 % of target, the graph is dark green, otherwise red." +
                " Click to select product and view" +
                " the product's information.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void pressuresToolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            Window fs = mf.Tls.IsViewOpen("RateController.PressureView");

            if (fs == null)
            {
                Window frm = new PressureView(mf);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private async void saveAsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
           /********
            saveFileDialog1.InitialDirectory = mf.Tls.FilesDir();
            saveFileDialog1.Title = "Save As";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    mf.Tls.SaveFile(saveFileDialog1.FileName);
                    mf.LoadSettings();
                    UpdateForm();
                }
            }
            ************/
               SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Directory = mf.Tls.FilesDir();
            saveFileDialog1.Title = "Save As";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            var FileName = await saveFileDialog1.ShowAsync(this);
            if (FileName != null)
            {
                 if (FileName != "")
                {
                    mf.Tls.OpenFile(FileName);
                    mf.LoadSettings();
                    UpdateForm();
                }
            }
        }

        private void SetFont()
        {
          /******** TODO
            if (mf.UseTransparent)
            {
                string TransparentFont = "MS Gothic";
                //string TransparentFont = "Courier New";
                //string TransparentFont = "Candara Light";
                //string TransparentFont = "Tahoma";

                foreach (Control Ctrl in Controls)
                {
                    if (Ctrl.Name != "lbName0" && Ctrl.Name != "lbName1" && Ctrl.Name != "lbName2" && Ctrl.Name != "lbName3"
                         && Ctrl.Name != "lbFan1" && Ctrl.Name != "lbFan2" && Ctrl.Name != "btAuto" && Ctrl.Name != "lblManAuto")
                    {
                        Ctrl.Font = new Font(TransparentFont, 14, FontStyle.Bold);
                    }
                    else if (Ctrl.Name == "btAuto" || Ctrl.Name == "lblManAuto")
                    {
                        Ctrl.Font = new Font(TransparentFont, 10, FontStyle.Bold);
                    }
                    else
                    {
                        Ctrl.Font = new Font(TransparentFont, 14, FontStyle.Bold);
                    }
                }
            }
            else
            {
                foreach (Control Ctrl in Controls)
                {
                    Ctrl.Font = new Font("Tahoma", 14);
                }
            }
            ************/
        }

        private void ShowProducts()
        {
            clsProduct Prd = mf.Products.Item(0);
            lbName0.IsVisible = Prd.OnScreen;
            pnlRate0.IsVisible = Prd.OnScreen;
            pnlQuantity0.IsVisible = Prd.OnScreen;
            pnlSelect0.IsVisible = Prd.OnScreen;

            Prd = mf.Products.Item(1);
            lbName1.IsVisible = Prd.OnScreen;
            pnlRate1.IsVisible = Prd.OnScreen;
            pnlQuantity1.IsVisible = Prd.OnScreen;
            pnlSelect1.IsVisible = Prd.OnScreen;

            Prd = mf.Products.Item(2);
            lbName2.IsVisible = Prd.OnScreen;
            pnlRate2.IsVisible = Prd.OnScreen;
            pnlQuantity2.IsVisible = Prd.OnScreen;
            pnlSelect2.IsVisible = Prd.OnScreen;

            Prd = mf.Products.Item(3);
            lbName3.IsVisible = Prd.OnScreen;
            pnlRate3.IsVisible = Prd.OnScreen;
            pnlQuantity3.IsVisible = Prd.OnScreen;
            pnlSelect3.IsVisible = Prd.OnScreen;

            Prd = mf.Products.Item(4);
            lbFan1.IsVisible = Prd.OnScreen;
            lbRPM1.IsVisible = Prd.OnScreen;
            btnFan1.IsVisible = Prd.OnScreen;

            Prd = mf.Products.Item(5);
            lbFan2.IsVisible = Prd.OnScreen;
            lbRPM2.IsVisible = Prd.OnScreen;
            btnFan2.IsVisible = Prd.OnScreen;

            for (int i = 0; i < 5; i++)
            {
                Prd = mf.Products.Item(i);
                if (i == 4)
                {
                    btnDown.IsVisible = false;
                    btnDown.IsEnabled = false;
                    btnUp.IsVisible = false;
                    btnUp.IsEnabled = false;
                }
                else if (Prd.BumpButtons)
                {
                    btnUp.IsVisible = true;
                    btnDown.IsVisible = true;
                    btnUp.IsEnabled = true;
                    btnDown.IsEnabled = true;

                   // TextBlock posLbl = (TextBlock)(this.Controls.Find("lbName" + i, true)[0]); 
                    TextBlock posLbl = this.GetControl<TextBlock>("lbName" + i);
                    ProgressBar posPb = this.GetControl<ProgressBar>("pbRate" + i); //(ProgressBar)(this.Controls.Find("pbRate" + i, true)[0]);

                    double posX = posLbl.Margin.Left;
                    double posY = posLbl.Margin.Top;
                    double Width = posLbl.Width;
                    double Height = posPb.Height + posLbl.Height;

                    //btnUp.Left = posX;
                    btnUp.Margin =  new Thickness(posX, posY, 0, 0);
                   // btnDown.Left = posX;
                    btnUp.Width = Width;
                    btnDown.Width = Width;
                   /// btnUp.Top = posY;
                    btnUp.Height = (Height + 10) / 2;
                   // btnDown.Top = posY + btnUp.Height + 10;
                    btnDown.Margin = new Thickness(posX,  posY + btnUp.Height + 10, 0, 0);
                    btnDown.Height = btnUp.Height;
                    break;
                }
            }
        }

        private void SwitchBox_SwitchPGNreceived(object sender, PGN32618.SwitchPGNargs e)
        {
            SwON = e.Switches;
            UpdateSwitches();
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void tmrBorder_tick(object sender, EventArgs e)
        {
           // this.FormBorderStyle = FormBorderStyle.None; TODO
            tmrBorder.Stop();
        }

        private void tmrRelease_Tick(object sender, EventArgs e)
        {
            if (!MasterPressed)
            {
                mf.vSwitchBox.ReleaseSwitch();
                tmrRelease.IsEnabled = false;
            }
        }

        private void UpdateForm()
        {
            if (mf.UseTransparent != IsTransparent) SetTransparent();

            this.Title = "RC [" + Path.GetFileNameWithoutExtension(Properties.Settings.Default.FileName) + "]";

            if (Prd.UseVR)
            {
                lbTarget.Text = "VR Target";
            }
            else if (Prd.UseAltRate)
            {
                lbTarget.Text = Lang.lgTargetRateAlt;
            }
            else
            {
                lbTarget.Text = Lang.lgTargetRate;
            }

            // set highlight
            if (mf.SimMode == SimType.VirtualNano)
            {
                pnlSelect0.Background = new SolidColorBrush(mf.SimColor);
                pnlSelect1.Background = new SolidColorBrush(mf.SimColor);
                pnlSelect2.Background = new SolidColorBrush(mf.SimColor);
                pnlSelect3.Background = new SolidColorBrush(mf.SimColor);
            }
            else
            {
                pnlSelect0.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
                pnlSelect1.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
                pnlSelect2.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
                pnlSelect3.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
            }
            switch (Prd.ID)
            {
                case 0:
                    pnlSelect0.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)); //SystemColors.Highlight;
                    break;

                case 1:
                    pnlSelect1.Background =  new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)); //SystemColors.Highlight;
                    break;

                case 2:
                    pnlSelect2.Background =  new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)); //SystemColors.Highlight;
                    break;

                case 3:
                    pnlSelect3.Background =  new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)); //SystemColors.Highlight;
                    break;
            }

            // product info
            lbName0.Content = mf.Products.Item(0).ProductName;
            lbName1.Content = mf.Products.Item(1).ProductName;
            lbName2.Content = mf.Products.Item(2).ProductName;
            lbName3.Content = mf.Products.Item(3).ProductName;

            if (lbName0.Content == "") lbName0.Content = "1";
            if (lbName1.Content == "") lbName1.Content = "2";
            if (lbName2.Content == "") lbName2.Content = "3";
            if (lbName3.Content == "") lbName3.Content = "4";

            clsProduct Product = mf.Products.Item(0);
            if (Product.ArduinoModule.ModuleSending())
            {
                if (Product.ArduinoModule.ModuleReceiving())
                {
                    lbName0.Background = Brushes.LightGreen;
                }
                else
                {
                    lbName0.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbName0.Background = Brushes.Red;
            }

            // 1
            Product = mf.Products.Item(1);
            if (Product.ArduinoModule.ModuleSending())
            {
                if (Product.ArduinoModule.ModuleReceiving())
                {
                    lbName1.Background = Brushes.LightGreen;
                }
                else
                {
                    lbName1.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbName1.Background = Brushes.Red;
            }

            // 2
            Product = mf.Products.Item(2);
            if (Product.ArduinoModule.ModuleSending())
            {
                if (Product.ArduinoModule.ModuleReceiving())
                {
                    lbName2.Background = Brushes.LightGreen;
                }
                else
                {
                    lbName2.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbName2.Background = Brushes.Red;
            }

            // 3
            Product = mf.Products.Item(3);
            if (Product.ArduinoModule.ModuleSending())
            {
                if (Product.ArduinoModule.ModuleReceiving())
                {
                    lbName3.Background = Brushes.LightGreen;
                }
                else
                {
                    lbName3.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbName3.Background = Brushes.Red;
            }

            // rate
            switch (RateType)
            {
                case 1:
                    lbRate.Text = Lang.lgInstantRate;
                    lbRateAmount.Text = Prd.CurrentRate().ToString("N1");
                    break;

                case 2:
                    lbRate.Text = Lang.lgOverallRate;
                    lbRateAmount.Text = Prd.AverageRate().ToString("N1");
                    break;

                default:
                    lbRate.Text = Lang.lgCurrentRate;
                    lbRateAmount.Text = Prd.SmoothRate().ToString("N1");
                    break;
            }

            lbTargetAmount.Text = Prd.TargetRate().ToString("N1");
            lbUnits.Text = Prd.Units();

            // coverage
            if (mf.ShowCoverageRemaining)
            {
                lbCoverage.Text = mf.CoverageDescriptions[Prd.CoverageUnits] + " Left ...";
                double RT = Prd.SmoothRate();
                if (RT == 0) RT = Prd.TargetRate();

                if ((RT > 0) & (Prd.TankStart > 0))
                {
                    lbCoverageAmount.Text = ((Prd.TankStart - Prd.UnitsApplied()) / RT).ToString("N1");
                }
                else
                {
                    lbCoverageAmount.Text = "0.0";
                }
            }
            else
            {
                // show amount done
                lbCoverageAmount.Text = Prd.CurrentCoverage().ToString("N1");
                lbCoverage.Text = Prd.CoverageDescription() + " ...";
            }

            // quantity
            if (mf.ShowQuantityRemaining)
            {
                lbQuantity.Text = Lang.lgTank_Remaining + " ...";
                // calculate remaining
                lbQuantityAmount.Content = (Prd.TankStart - Prd.UnitsApplied()).ToString("N1");
            }
            else
            {
                // show amount done
                lbQuantity.Text = Lang.lgQuantityApplied + " ...";
                lbQuantityAmount.Content= Prd.UnitsApplied().ToString("N1");
            }

            // aog
            if (mf.SimMode == SimType.Speed)
            {
                btnMenu.Content = Properties.Resources.SimGear;
            }
            else
            {
                if (mf.AutoSteerPGN.Connected())
                {
                    btnMenu.Content = Properties.Resources.GreenGear;
                }
                else
                {
                    btnMenu.Content = Properties.Resources.RedGear;
                }
            }

            // graphs
            // product 0
            clsProduct PD = mf.Products.Item(0);
            double Size = PD.TankSize;
            double Rem = PD.TankStart - PD.UnitsApplied();
            if (Size == 0 || Size < Rem) Size = Rem * 2;
            if (Size == 0) Size = 100;
            int Level = (int)(Rem / Size * 100);
            if (Level > 100) Level = 100;
            if (Level < 0) Level = 0;
            pbQuantity0.Value = Level;

            double Rt = PD.SmoothRate();
            double Tg = PD.TargetRate();
            int RtLevel = 0;
            if (Tg > 0) RtLevel = (int)((Rt / Tg) * 50) - 30;
            if (RtLevel > 40) RtLevel = 40;
            if (RtLevel < 0) RtLevel = 0;
            if (Tg > 0 && RtLevel < 1) RtLevel = 1;
            if (RtLevel > 25 || RtLevel < 15)
            {
                pbRate0.Foreground = Brushes.Red;
            }
            else
            {
                pbRate0.Foreground = Brushes.GreenYellow;//RateColour;
            }
            pbRate0.Value = RtLevel;

            // product 1
            PD = mf.Products.Item(1);
            Size = PD.TankSize;
            Rem = PD.TankStart - PD.UnitsApplied();
            if (Size == 0 || Size < Rem) Size = Rem * 2;
            if (Size == 0) Size = 100;
            Level = (int)(Rem / Size * 100);
            if (Level > 100) Level = 100;
            if (Level < 0) Level = 0;
            pbQuantity1.Value = Level;

            Rt = PD.SmoothRate();
            Tg = PD.TargetRate();
            RtLevel = 0;
            if (Tg > 0) RtLevel = (int)((Rt / Tg) * 50) - 30;
            if (RtLevel > 40) RtLevel = 40;
            if (RtLevel < 0) RtLevel = 0;
            if (Tg > 0 && RtLevel < 1) RtLevel = 1;
            if (RtLevel > 25 || RtLevel < 15)
            {
                pbRate1.Foreground = Brushes.Red;
            }
            else
            {
                pbRate1.Foreground = Brushes.GreenYellow;//RateColour;
            }
            pbRate1.Value = RtLevel;

            // product 2
            PD = mf.Products.Item(2);
            Size = PD.TankSize;
            Rem = PD.TankStart - PD.UnitsApplied();
            if (Size == 0 || Size < Rem) Size = Rem * 2;
            if (Size == 0) Size = 100;
            Level = (int)(Rem / Size * 100);
            if (Level > 100) Level = 100;
            if (Level < 0) Level = 0;
            pbQuantity2.Value = Level;

            Rt = PD.SmoothRate();
            Tg = PD.TargetRate();
            RtLevel = 0;
            if (Tg > 0) RtLevel = (int)((Rt / Tg) * 50) - 30;
            if (RtLevel > 40) RtLevel = 40;
            if (RtLevel < 0) RtLevel = 0;
            if (Tg > 0 && RtLevel < 1) RtLevel = 1;
            if (RtLevel > 25 || RtLevel < 15)
            {
                pbRate2.Foreground = Brushes.Red;
            }
            else
            {
                pbRate2.Foreground = Brushes.GreenYellow;//RateColour;
            }
            pbRate2.Value = RtLevel;

            // product 3
            PD = mf.Products.Item(3);
            Size = PD.TankSize;
            Rem = PD.TankStart - PD.UnitsApplied();
            if (Size == 0 || Size < Rem) Size = Rem * 2;
            if (Size == 0) Size = 100;
            Level = (int)(Rem / Size * 100);
            if (Level > 100) Level = 100;
            if (Level < 0) Level = 0;
            pbQuantity3.Value = Level;

            Rt = PD.SmoothRate();
            Tg = PD.TargetRate();
            RtLevel = 0;
            if (Tg > 0) RtLevel = (int)((Rt / Tg) * 50) - 30;
            if (RtLevel > 40) RtLevel = 40;
            if (RtLevel < 0) RtLevel = 0;
            if (Tg > 0 && RtLevel < 1) RtLevel = 1;
            if (RtLevel > 25 || RtLevel < 15)
            {
                pbRate3.Foreground = Brushes.Red;
            }
            else
            {
                pbRate3.Foreground = Brushes.GreenYellow; //RateColour;
            }
            pbRate3.Value = RtLevel;

            clsProduct prd = mf.Products.Item(mf.MaxProducts - 2);

            if (Fan1RateType == 1)
            {
                lbRPM1.Content= prd.CurrentRate().ToString("N0") + " RPM-I";
            }
            else
            {
                lbRPM1.Content = prd.SmoothRate().ToString("N0") + " RPM";
            }

            if (prd.ArduinoModule.ModuleSending())
            {
                if (prd.ArduinoModule.ModuleReceiving())
                {
                    lbFan1.Background = Brushes.LightGreen;
                }
                else
                {
                    lbFan1.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbFan1.Background = Brushes.Red;
            }

            // fan 2
            prd = mf.Products.Item(mf.MaxProducts - 1);
            if (Fan2RateType == 1)
            {
                lbRPM2.Content = prd.CurrentRate().ToString("N0") + " RPM-I";
            }
            else
            {
                lbRPM2.Content = prd.SmoothRate().ToString("N0") + " RPM";
            }

            if (prd.ArduinoModule.ModuleSending())
            {
                if (prd.ArduinoModule.ModuleReceiving())
                {
                    lbFan2.Background = Brushes.LightGreen;
                }
                else
                {
                    lbFan2.Background = Brushes.LightBlue;
                }
            }
            else
            {
                lbFan2.Background = Brushes.Red;
            }

            // fan 1 button
            clsProduct fn = mf.Products.Item(mf.MaxProducts - 2);
            if (fn.FanOn)
            {
                btnFan1.Content = Properties.Resources.FanOn;
            }
            else
            {
                btnFan1.Content = Properties.Resources.FanOff;
            }

            // fan 2 button
            fn = mf.Products.Item(mf.MaxProducts - 1);
            if (fn.FanOn)
            {
                btnFan2.Content = Properties.Resources.FanOn;
            }
            else
            {
                btnFan2.Content = Properties.Resources.FanOff;
            }

            RCalarm.CheckAlarms();
            ShowProducts();
        }

        private void UpdateSwitches()
        {
            if (automode)
            {
                // show auto button
                if (SwON[0])
                {
                    btAuto.Background = Brushes.LightGreen;
                    btAuto.Content = "AUTO";
                    btAuto.Foreground = Brushes.Black;
                }
                else
                {
                    btAuto.Background = Brushes.Red;
                    btAuto.Content = "OFF";
                    btAuto.Foreground = Brushes.White;
                }
            }
            else
            {
                // show master button
                if (mf.SwitchBox.MasterOn)
                {
                    btAuto.Background = Brushes.Yellow;
                    btAuto.Content = "ON";
                    btAuto.Foreground = Brushes.Black;
                    automode = false;
                    masterOn = true;
                }
                else
                {
                    btAuto.Background = Brushes.Red;
                    btAuto.Content = "OFF";
                    btAuto.Foreground = Brushes.White;
                    masterOn = false;
                }
            }

            if (SwON[3])
            {
                btnUp.Background = Brushes.Blue;
            }
            else
            {
                btnUp.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
            }

            if (SwON[4])
            {
                btnDown.Background = Brushes.Blue;
            }
            else
            {
                btnDown.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230)); //Properties.Settings.Default.DayColour;
            }
        }

        private void verticalProgressBar0_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Indicates quantity remaining. Click to select product and view" +
                " the product's information.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void verticalProgressBar1_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(0);
            UpdateForm();
        }

        private void verticalProgressBar2_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(1);
            UpdateForm();
        }

        private void verticalProgressBar3_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(2);
            UpdateForm();
        }

        private void verticalProgressBar4_Click(object sender, RoutedEventArgs e)
        {
            Prd = mf.Products.Item(3);
            UpdateForm();
        }
    }

