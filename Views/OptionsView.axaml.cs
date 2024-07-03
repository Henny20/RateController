using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RateController.Enums;
using RateController.Classes;
using RateController.Properties;

namespace RateController.Views;

public partial class OptionsView : Window
{
	private MainWindow mf;
	private bool FormEdited;
	private bool Initializing = true;
	private string[] LanguageIDs;
	private RadioButton[] LanguageRBs;
	private TabItem[] Tabs;
	private bool tbSimSpeedChanged = false;
	private bool tbSpeedChanged = false;

    public OptionsView()
    {
        InitializeComponent();
        Closed += frmOptions_FormClosed;
        Loaded += frmOptions_Load;
    }
    
    public OptionsView(MainWindow CallingFrom):this()
    {
        mf = CallingFrom;
         
		#region // language
/**TODO
		tcOptions.TabPages[0].Text = Lang.lgDisplay;
		tcOptions.TabPages[1].Text = Lang.lgPrimedStart;
		tcOptions.TabPages[2].Text = Lang.lgSwitches;
		tcOptions.TabPages[3].Text = Lang.lgConfig;
		tcOptions.TabPages[4].Text = Lang.lgLanguage;
***/
		ckMetric.Content = Lang.lgMetric;
		ckScreenSwitches.Content = Lang.lgSwitches;
		ckWorkSwitch.Content = Lang.lgWorkSwitch;
		ckLargeScreen.Content = Lang.lgLargeScreen;
		ckTransparent.Content = Lang.lgTransparent;

		lbOnTime.Text = Lang.lgOnTime;
		lbPrimedSpeed.Text = Lang.lgSpeed;
		lbDelay.Text = Lang.lgSwitchDelay;
		lbOnSeconds.Text = Lang.lgSeconds;
		lbDelaySeconds.Text = Lang.lgSeconds;

		#endregion // language

		LanguageRBs = new RadioButton[] { rbEnglish, rbDeustch, rbHungarian, rbNederlands, rbPolish, rbRussian, rbFrench };
		LanguageIDs = new string[] { "en", "de", "hu", "nl", "pl", "ru", "fr" };
		for (int i = 0; i < LanguageRBs.Length; i++)
		{
			LanguageRBs[i].IsCheckedChanged += Language_CheckedChanged;
		}

		Tabs = new TabItem[] { tabPage1, tabPage2, tabPage3, tabPage4, tabPage5 };
    }
    
     private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateForm();
            btnOK.Focus();
            SetButtons(false);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FormEdited)
                {
                    this.Close();
                }
                else
                {
                    if (mf.Tls.ReadOnly)
                    {
                        //mf.Tls.ShowHelp("File is read only.", "Help", 5000, false, false, true);
                        System.Console.WriteLine("File is read only.");
                    }
                    else
                    {
                        Save();
                        SetButtons(false);
                        if (ckDefaultProduct.IsChecked ?? false) mf.Products.Load(true);
                        UpdateForm();
                    }
                }
            }
            catch (System.Exception ex)
            {
               // mf.Tls.ShowHelp(ex.Message, this.Title, 3000, true);
               System.Console.WriteLine(ex.Message + this.Title);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            mf.SwitchObjects.Reset();
            SetButtons(true);
            UpdateForm();
        }

        private void CheckRelayDefs(byte RelayID, byte ModuleID)
        {
            // check if relay is defined as 'Switch' type
            if (RelayID > 0)
            {
                clsRelay Rly = mf.RelayObjects.Item(RelayID - 1, ModuleID);
               // if (Rly.Type != RelayTypes.Switch)TODO wat is Switch
              //  {
                /************TODO
                    var Hlp = new MsgBoxView(mf, "Change relay type from '" + Rly.TypeDescription + "' to 'Switch'?", "Switches", true);
                    Hlp.TopMost = true;
                    Hlp.ShowDialog();
                    if (Hlp.Result)
                    {
                        // change relay type
                        Rly.Type = RelayTypes.Switch;
                        Rly.Save();
                    }
                    Hlp.Close();
                    *********/
              //  }
            }
        }

        private void ckDefaultProduct_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ckDefaultProduct.IsChecked ?? false) SetButtons(true);
        }

        private void ckDefaultProduct_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Set product values to default.";

            mf.Tls.ShowHelp(Message, "Sensors");
            hlpevent.Handled = true;
        }

        private void ckDualAuto_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void ckNoMaster_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void ckNoMaster_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Send master switch always on to modules.";

            mf.Tls.ShowHelp(Message, "Master Switch");
            hlpevent.Handled = true;
        }

        private void ckSingle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ckSingle.IsChecked ?? false) SetButtons(true);
        }

        private void ckTransparent_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void DGV_CellClick(object sender, RoutedEventArgs e)
        {
        /****TODO
            try
            {
                double Temp;
                string val = DGV.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue.ToString();
                switch (e.ColumnIndex)
                {
                    case 2:
                        // module
                        double.TryParse(val, out Temp);
                        using (var form = new FormNumeric(0, 7, Temp))
                        {
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                DGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = form.ReturnValue;
                            }
                        }
                        break;

                    case 3:
                        // relay
                        double.TryParse(val, out Temp);
                        using (var form = new FormNumeric(0, 16, Temp))
                        {
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                DGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = form.ReturnValue;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("frmOptions/CellClick " + ex.Message);
            }
            *************/
        }

        private void DGV_CellValueChanged(object sender, RoutedEventArgs e)
        {
          //  if (!Initializing) SetButtons(true);TODO
        }

        private void DGV_DataError(object sender, RoutedEventArgs e)
        {
          //  mf.Tls.WriteErrorLog("frmOptions/DGV_DataError: Row,Column: " + e.RowIndex.ToString() + ", " + e.ColumnIndex.ToString()
          //      + " Exception: " + e.Exception.ToString());TODO
        }

        private void frmOptions_FormClosed(object sender, System.EventArgs e)
        {
            mf.Tls.SaveFormData(this);
        }

        private void frmOptions_Load(object sender, System.EventArgs e)
        {
            mf.Tls.LoadFormData(this);

           // DGV.BackgroundColor = DGV.DefaultCellStyle.Background;TODO
           // DGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;TODO

            SetDayMode();
            UpdateForm();
        }

        private void Language_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void LoadData(bool UpdateObject = false)
        {
        /*************TODO
            try
            {
                if (UpdateObject) mf.SwitchObjects.Load();
                dataSet1.Clear();
                foreach (clsSwitch SW in mf.SwitchObjects.Items)
                {
                    DataRow Rw = dataSet1.Tables[0].NewRow();
                    Rw[0] = SW.ID + 1;
                    Rw[1] = SW.Description;
                    Rw[2] = SW.ModuleID;

                    if (SW.RelayID == 0)
                    {
                        Rw[3] = "";
                    }
                    else
                    {
                        Rw[3] = SW.RelayID;
                    }

                    dataSet1.Tables[0].Rows.Add(Rw);
                }
            }
            catch (System.Exception ex)
            {
                mf.Tls.WriteErrorLog("frmOptions/LoadData: " + ex.Message);
            }
            ****************/
        }

        private void rbLarge_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void Save()
        {
            try
            {
                if (tbSimSpeedChanged)
                {
                    if (double.TryParse(tbSimSpeed.Text, out double Speed)) mf.SimSpeed = Speed;
                    tbSimSpeedChanged = false;
                }
                else if (tbSpeedChanged)
                {
                    if (double.TryParse(tbSpeed.Text, out double Spd)) mf.SimSpeed = Spd;
                    tbSpeedChanged = false;
                }

                if (double.TryParse(tbTime.Text, out double Time)) mf.PrimeTime = Time;
                if (int.TryParse(tbDelay.Text, out int Delay)) mf.PrimeDelay = Delay;

                mf.MasterOverride = ckNoMaster.IsChecked ?? false;
                mf.UseTransparent = ckTransparent.IsChecked ?? false;
                mf.UseInches = !ckMetric.IsChecked ?? false;
                mf.ShowSwitches = ckScreenSwitches.IsChecked ?? false;
                mf.SwitchBox.UseWorkSwitch = ckWorkSwitch.IsChecked ?? false;
                mf.ShowPressure = ckPressure.IsChecked ?? false;

                if (ckSimSpeed.IsChecked ?? false)
                {
                    mf.SimMode = SimType.Speed;
                }
                else
                {
                    mf.SimMode = SimType.None;
                }

                mf.UseLargeScreen = ckLargeScreen.IsChecked ?? false;
                if (ckSingle.IsChecked ?? false) mf.SwitchScreens(true);

                mf.UseDualAuto = ckDualAuto.IsChecked ?? false;
                mf.ResumeAfterPrime = ckResume.IsChecked ?? false;

                SaveLanguage();
               /********** TODO
                // data grid
                for (int i = 0; i < DGV.Rows.Count; i++)
                {
                    clsSwitch SW = mf.SwitchObjects.Item(i);
                    for (int j = 1; j < 4; j++)
                    {
                        string val = DGV.Rows[i].Cells[j].EditedFormattedValue.ToString();
                        if (val == "") val = "0";
                        switch (j)
                        {
                            case 1:
                                // description
                                SW.Description = val;
                                break;

                            case 2:
                                // module
                                if (byte.TryParse(val, out byte md))
                                {
                                    SW.ModuleID = md;
                                }
                                break;

                            case 3:
                                // relay
                                if (byte.TryParse(val, out byte rly))
                                {
                                    SW.RelayID = rly;
                                }
                                break;
                        }
                    }
                    CheckRelayDefs(SW.RelayID, SW.ModuleID);
                }
                *************/
                mf.SwitchObjects.Save();
                if (mf.SwitchesForm != null) mf.SwitchesForm.SetDescriptions();
            }
            catch (System.Exception ex)
            {
                mf.Tls.WriteErrorLog("frmOptions/SaveData: " + ex.Message);
            }
        }

        private void SaveLanguage()
        {
            int Lan = 0;
            for (int i = 0; i < LanguageRBs.Length; i++)
            {
                if (LanguageRBs[i].IsChecked ?? false)
                {
                    Lan = i;
                    break;
                }
            }

            if (Properties.Settings.Default.setF_culture != LanguageIDs[Lan])
            {
                Properties.Settings.Default.setF_culture = LanguageIDs[Lan];
                Settings.Default.UserLanguageChange = true;
                Properties.Settings.Default.Save();

                Window fs = mf.Tls.IsViewOpen("RateController.LargeScreenView");
                if (fs != null)
                {
                    mf.Restart = true;
                    mf.Lscrn.Close();
                }
                else
                {
                    mf.ChangeLanguage();
                }
            }
        }

        private void SetButtons(bool Edited = false)
        {
            if (!Initializing)
            {
                if (Edited)
                {
                    btnCancel.IsEnabled = true;
                    btnOK.Content = "Save"; //new Image {Source = new Bitmap("Resources/Save.png")}; 
                }
                else
                {
                    btnCancel.IsEnabled = false;
                    btnOK.Content =  "OK"; //new Image {Source = new Bitmap("Resources/OK64.png")};
                    btnOK.IsEnabled = true;
                }
                FormEdited = Edited;
            }
        }

        private void SetDayMode()
        {
            if (Properties.Settings.Default.IsDay)
            {
                this.Background =  new SolidColorBrush(Color.FromRgb(243, 243, 243)) ; //Properties.Settings.Default.DayColour;
                //foreach (Control c in this.Controls)
              //  {TODO
               //     c.Foreground = Brushes.Black;
              //  }

                for (int i = 0; i < Tabs.Length; i++)
                {
                    Tabs[i].Background =  new SolidColorBrush(Color.FromRgb(243, 243, 243)) ; //Properties.Settings.Default.DayColour;
                }
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));  //Properties.Settings.Default.NightColour;
                //foreach (Control c in this.Controls)
               //{ TODO
                //    c.Foreground = Color.White;
             //   }

                for (int i = 0; i < Tabs.Length; i++)
                {
                    Tabs[i].Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));  //Properties.Settings.Default.NightColour;
                }
            }
        }

        private async void tbDelay_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(tbDelay.Text, out tempD);
            var form = new NumericView(0, 8, tempD);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbDelay.Text = form.ReturnValue.ToString("N0");
            }
            
        }

        private void tbDelay_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(tbDelay.Text, out tempD);
            if (tempD < 0 || tempD > 8)
            {
                //System.Media.SystemSounds.Exclamation.Play();TODO
                e.Cancel = true;
            }
        }

        private async void tbSimSpeed_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(tbSimSpeed.Text, out tempD);
            var form = new NumericView(0, 40, tempD);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbSimSpeed.Text = form.ReturnValue.ToString("N1");
            }
            
        }

        private void tbSimSpeed_TextChanged(object sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (!Initializing) tbSimSpeedChanged = true;
            SetButtons(true);
        }

        private void tbSimSpeed_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(tbSimSpeed.Text, out tempD);
            if (tempD < 0 || tempD > 40)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private async void tbSpeed_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(tbSpeed.Text, out tempD);
            var form = new NumericView(0, 40, tempD);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbSpeed.Text = form.ReturnValue.ToString("N1");
            }
            
        }

        private void tbSpeed_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(tbSpeed.Text, out tempD);
            if (tempD < 0 || tempD > 40)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private async void tbTime_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(tbTime.Text, out tempD);
            var form = new NumericView(0, 30, tempD);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbTime.Text = form.ReturnValue.ToString("N1");
            }
            
        }

        private void tbTime_TextChanged(object sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (!Initializing) tbSpeedChanged = true;
            SetButtons(true);
        }

        private void tbTime_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(tbTime.Text, out tempD);
            if (tempD < 0 || tempD > 30)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private void tcOptions_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            btnReset.IsVisible = (tcOptions.SelectedIndex == 4);
        }

        private void UpdateForm(bool UpdateObject = false)
        {
            Initializing = true;

            tbSpeed.Text = mf.SimSpeed.ToString("N1");
            tbSimSpeed.Text = mf.SimSpeed.ToString("N1");

            tbTime.Text = mf.PrimeTime.ToString("N0");
            tbDelay.Text = mf.PrimeDelay.ToString("N0");

            ckTransparent.IsChecked = mf.UseTransparent;
            ckMetric.IsChecked = !mf.UseInches;
            ckScreenSwitches.IsChecked = mf.ShowSwitches;
            ckWorkSwitch.IsChecked = mf.SwitchBox.UseWorkSwitch;
            ckPressure.IsChecked = mf.ShowPressure;
            ckSimSpeed.IsChecked = (mf.SimMode == SimType.Speed);
            ckDualAuto.IsChecked = mf.UseDualAuto;
            ckResume.IsChecked = mf.ResumeAfterPrime;
            ckNoMaster.IsChecked = mf.MasterOverride;
            ckLargeScreen.IsChecked = mf.UseLargeScreen;

            // language
            for (int i = 0; i < LanguageRBs.Length; i++)
            {
                if (LanguageIDs[i] == Properties.Settings.Default.setF_culture)
                {
                    LanguageRBs[i].IsChecked = true;
                    break;
                }
            }

            if (mf.UseInches)
            {
                lbSpeed.Text = "MPH";
                lbSimUnits.Text = "MPH";
            }
            else
            {
                lbSpeed.Text = "KMH";
                lbSimUnits.Text = "KMH";
            }
           // LoadData(UpdateObject);TODO

            ckDefaultProduct.IsChecked = false;
            ckSingle.IsChecked = false;

            Initializing = false;
        }
}    

