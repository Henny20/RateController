using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using RateController.Views;
using RateController.PGNs;
using RateController.Enums;

namespace RateController.Views;

public partial class ModuleConfigView : Window
{
    private MainWindow mf;
    private int BoardType = 0;  // 0 nano, 1 Teensy, 2 ESP32
    private bool FormEdited;
    private bool Initializing;

    TextBox[] RelayTB;
    
    
     public ModuleConfigView()
     { 
        InitializeComponent();
        Closed += frmModuleConfig_WindowClosed;
        Loaded += frmModuleConfig_Load;
        cbEthernet.SelectionChanged += cbEthernet_SelectedIndexChanged;
        //cbRelayControl.SelectionChanged += textbox_TextChanged; TODO not possible
     }   
     
    public ModuleConfigView(MainWindow Main):this()
    {
        #region // language  
        lbSubnet.Text = Lang.lgSelectedSubnet;
        lbIP.Text = Lang.lgConfigIP;
        lbModuleID.Text = Lang.lgModuleID;
        lbSensorCount.Text = Lang.lgSensorCount;
        lbWifiPort.Text = Lang.lgWifiPort;
        lbRelay.Text = Lang.lgRelayControl;
        ckRelayOn.Content = Lang.lgRelayOnHigh;
        ckFlowOn.Content = Lang.lgFlowOnHigh;

       //tabControl1.Items[0].Text = Lang.lgBoards;
        tabPage6.Header = Lang.lgBoards;
        //tabControl1.Items[1].Text = Lang.lgNetwork;
        tabPage3.Header = Lang.lgNetwork;
        tabPage1.Header = Lang.lgConfig;
    	tabPage2.Header = Lang.lgPins;
	    tabPage5.Header = Lang.lgRelays;
	    tabPage4.Header = Lang.lgWifiClient;
       // tabControl1.Items[2].Text = Lang.lgConfig;
       // tabControl1.Items[3].Text = Lang.lgPins;
       // tabControl1.Items[4].Text = Lang.lgRelays;
       // tabControl1.Items[5].Text = Lang.lgWifiClient;

        lbWorkPin.Text = Lang.lgWorkPin;

        #endregion // language

        mf = Main;
        for (int i = 0; i < mf.MaxProducts; i++)
        {
            mf.Products.Item(i).ArduinoModule.PinStatusChanged += ArduinoModule_PinStatusChanged;
        }

        RelayTB = new TextBox[] { tbRelay1,tbRelay2,tbRelay3,tbRelay4,tbRelay5,tbRelay6,tbRelay7,tbRelay8,
        tbRelay9,tbRelay10,tbRelay11,tbRelay12,tbRelay13,tbRelay14,tbRelay15,tbRelay16};

        for (int i = 0; i < 16; i++)
        {
            RelayTB[i].PointerEntered += tbRelays_enter;
            RelayTB[i].TextChanged += tb_TextChanged;
        }
    }
    
     private async void tbRelays_enter(object sender, PointerEventArgs e)
        {
            TextBox bx = (TextBox)sender;
            double temp = 0;
            if (double.TryParse(bx.Text.Trim(), out double vl)) temp = vl;
            var form = new NumericView(0, 50, temp);
        
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                bx.Text = form.ReturnValue.ToString("N0");
            }
            
        }

        private void ArduinoModule_PinStatusChanged(object sender, PGN32400.PinStatusArgs e)
        {
            if (!e.GoodPins) mf.Tls.ShowHelp("Pin configuration not correct. Using default values.");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(mf.Tls.LoadProperty("BoardType"), out int bt)) BoardType = bt;
            UpdateForm();
            SetButtons(false);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (!FormEdited)
            {
                // exit
                this.Close();
            }
            else
            {
                if (mf.Tls.ReadOnly)
                {
                   //mf.Tls.ShowHelp("File is read only.", "Help", 5000, false, false, true); TODO
                   Console.WriteLine("File is read only.");
                }
                else
                {
                    // save
                    // IP
                    mf.UDPmodules.NetworkEP = cbEthernet.SelectedItem.ToString(); //?????

                    Save();
                    SetButtons(false);
                    UpdateForm();

                    btnSendToModule.Content = "C";
                   // btnSendToModule.FlatAppearance.BorderSize = 3; TODO
                   // btnSendToModule.FlatAppearance.BorderColor = Color.DarkBlue; TODO
                }
            }
        }

        private void btnPCB_Click(object sender, RoutedEventArgs e)
        {
            Window fs = mf.Tls.IsViewOpen("RateController.Views.PinsView");

            if (fs == null)
            {
                Window frm = new PinsView(mf);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private void btnRescan_Click(object sender, RoutedEventArgs e)
        {
            UpdateForm();
        }

        private void btnRescan_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Rescan";

            mf.Tls.ShowHelp(Message, "Rescan");
            hlpevent.Handled = true;
        }

        private void btnSendSubnet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PGN32503 SetSubnet = new PGN32503(mf);
                if (SetSubnet.Send(mf.UDPmodules.NetworkEP))
                {
                    mf.Tls.ShowHelp("New Subnet address sent.", "Subnet", 10000);

                    // set app subnet
                    mf.UDPmodules.NetworkEP = (string)cbEthernet.SelectedItem;
                }
                else
                {
                    mf.Tls.ShowHelp("New Subnet address not sent.", "Subnet", 10000);
                }
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp(ex.Message, "frmModuleConfig/btnSendSubnet", 15000, true);
            }
        }

        private void btnSendSubnet_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Send subnet address to modules.";

            mf.Tls.ShowHelp(Message, "Subnet");
            hlpevent.Handled = true;
        }

        private void btnSendToModule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mf.ModuleConfig.Send();
                mf.NetworkConfig.Send();
                mf.Tls.ShowHelp("Settings sent to module", "Config", 10000);

                btnSendToModule.Content = "";
               // btnSendToModule.FlatAppearance.BorderSize = 0; TODO
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp("frmModuleConfig/btnSendToModule  " + ex.Message, "Help", 10000, true, true);
            }
        }

        private void btnSendToModule_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Send settings to module. Only have 1 module connected when sending. The button is highlighted when there are changes to be sent.";
            mf.Tls.ShowHelp(Message, "Send Config");
            hlpevent.Handled = true;
        }

        private void cbEthernet_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void cbRelayControl_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "1 - GPIOs, use the micro-controller pins.\n" +
                            "2 - PCA9555 8 relays, use 8 relay module.\n" +
                            "3 - PCA9555 16 relays, use 16 relay module.\n" +
                            "4 - MCP23017, use a MCP23017 IO expander.\n" +
                            "5 - PCA9685 single, use each PCA9685 pin to control a single relay.\n" +
                            "6 - PCA9685 paired, use consecutive pins to control relays in a complementary mode. One is on and the other off.\n" +
                            "7 - PCF8574";

            mf.Tls.ShowHelp(Message, "Relay Control");
            hlpevent.Handled = true;
        }

        private void ckClient_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void ckClient_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Module connects as a client of an external wifi network instead of connecting over its own access point network.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void SwitchBoards()
        {
            switch (BoardType)
            {
                case 1:
                    // RC11, Teensy
                    tbModuleID.Text = "0";
                    tbSensorCount.Text = "2";
                    tbWifiPort.Text = "1";
                    cbRelayControl.SelectedIndex = 1;

                    ckRelayOn.IsChecked = true;
                    ckFlowOn.IsChecked = true;
                    ckMomentary.IsChecked = false;

                    tbFlow1.Text = "28";
                    tbFlow2.Text = "29";
                    tbDir1.Text = "37";
                    tbDir2.Text = "14";
                    tbPWM1.Text = "36";
                    tbPWM2.Text = "15";
                    tbWrk.Text = "-";

                    tbRelay1.Text = "8";
                    tbRelay2.Text = "9";
                    tbRelay3.Text = "10";
                    tbRelay4.Text = "11";
                    tbRelay5.Text = "12";
                    tbRelay6.Text = "25";
                    tbRelay7.Text = "26";
                    tbRelay8.Text = "27";

                    tbRelay9.Text = "-";
                    tbRelay10.Text = "-";
                    tbRelay11.Text = "-";
                    tbRelay12.Text = "-";
                    tbRelay13.Text = "-";
                    tbRelay14.Text = "-";
                    tbRelay15.Text = "-";
                    tbRelay16.Text = "-";

                    tbSSID.Text = "Tractor";
                    tbPassword.Text = "111222333";
                    ckClient.IsChecked = false;
                    break;

                case 2:
                    // RC15, ESP32
                    tbModuleID.Text = "0";
                    tbSensorCount.Text = "2";
                    tbWifiPort.Text = "0";
                    cbRelayControl.SelectedIndex = 6;

                    ckRelayOn.IsChecked = true;
                    ckFlowOn.IsChecked = true;
                    ckMomentary.IsChecked = false;

                    tbFlow1.Text = "17";
                    tbFlow2.Text = "16";
                    tbDir1.Text = "32";
                    tbDir2.Text = "25";
                    tbPWM1.Text = "33";
                    tbPWM2.Text = "26";
                    tbWrk.Text = "-";

                    tbRelay1.Text = "-";
                    tbRelay2.Text = "-";
                    tbRelay3.Text = "-";
                    tbRelay4.Text = "-";
                    tbRelay5.Text = "-";
                    tbRelay6.Text = "-";
                    tbRelay7.Text = "-";
                    tbRelay8.Text = "-";

                    tbRelay9.Text = "-";
                    tbRelay10.Text = "-";
                    tbRelay11.Text = "-";
                    tbRelay12.Text = "-";
                    tbRelay13.Text = "-";
                    tbRelay14.Text = "-";
                    tbRelay15.Text = "-";
                    tbRelay16.Text = "-";

                    tbSSID.Text = "Tractor";
                    tbPassword.Text = "111222333";
                    ckClient.IsChecked = false;
                    break;

                default:
                    // RC12, Nano
                    tbModuleID.Text = "0";
                    tbSensorCount.Text = "1";
                    tbWifiPort.Text = "0";
                    cbRelayControl.SelectedIndex = 2;

                    ckRelayOn.IsChecked = true;
                    ckFlowOn.IsChecked = true;
                    ckMomentary.IsChecked = false;

                    tbFlow1.Text = "3";
                    tbFlow2.Text = "-";
                    tbDir1.Text = "6";
                    tbDir2.Text = "-";
                    tbPWM1.Text = "9";
                    tbPWM2.Text = "-";
                    tbWrk.Text = "-";

                    tbRelay1.Text = "-";
                    tbRelay2.Text = "-";
                    tbRelay3.Text = "-";
                    tbRelay4.Text = "-";
                    tbRelay5.Text = "-";
                    tbRelay6.Text = "-";
                    tbRelay7.Text = "-";
                    tbRelay8.Text = "-";

                    tbRelay9.Text = "-";
                    tbRelay10.Text = "-";
                    tbRelay11.Text = "-";
                    tbRelay12.Text = "-";
                    tbRelay13.Text = "-";
                    tbRelay14.Text = "-";
                    tbRelay15.Text = "-";
                    tbRelay16.Text = "-";

                    tbSSID.Text = "Tractor";
                    tbPassword.Text = "111222333";
                    ckClient.IsChecked = false;
                    break;
            }
        }
        private void ckDefaultModule_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ckDefaultModule.IsChecked ?? false)
            {
                SwitchBoards();
                SetButtons(true);
            }
        }

        private void frmModuleConfig_WindowClosed(object sender, EventArgs e)
        {
            mf.Tls.SaveFormData(this);
        }

        private void frmModuleConfig_Load(object sender, EventArgs e)
        {
            mf.Tls.LoadFormData(this);
            SetDayMode();
            if (int.TryParse(mf.Tls.LoadProperty("BoardType"), out int bt)) BoardType = bt;
            UpdateForm();

            // check for no settings
            if ((tbFlow1.Text == "0") && (tbFlow2.Text == "0") && (tbDir1.Text == "0")
                && (tbDir2.Text == "0") && (tbPWM1.Text == "0") && (tbPWM2.Text == "0"))
            {
                ckDefaultModule.IsChecked = true;
            }
        }

        private void groupBox1_Paint_1(object sender, RoutedEventArgs e)
        {
            //GroupBox box = sender as GroupBox; TODO
          //  mf.Tls.DrawGroupBox(box, e.Graphics, this.Background, Color.Black, Color.Blue);
        }

        private void LoadCombo()
        {
            // https://stackoverflow.com/questions/6803073/get-local-ip-address
            try
            {
                cbEthernet.Items.Clear();
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if ((item.NetworkInterfaceType == NetworkInterfaceType.Ethernet || item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && item.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                cbEthernet.Items.Add(ip.Address.ToString());
                            }
                        }
                    }
                }
                //cbEthernet.SelectedIndex 
                cbEthernet.SelectedItem = SubAddress(mf.UDPmodules.NetworkEP);
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("frmModuleConfig/LoadCombo " + ex.Message);
            }
        }

        private async void ModuleID_Enter(object sender, RoutedEventArgs e)
        {
            double temp;
            double.TryParse(tbModuleID.Text, out temp);
            var form = new NumericView(0, 8, temp);
           
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbModuleID.Text = form.ReturnValue.ToString("N0");
            }
        }

        private void rbESP32_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!Initializing)
            {
                BoardType = 2;
                SwitchBoards();
                ckDefaultModule.IsChecked = true;
                SetButtons(true);
            }
        }

        private void rbNano_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!Initializing)
            {
                BoardType = 0;
                SwitchBoards();
                ckDefaultModule.IsChecked = true;
                SetButtons(true);
            }
        }

        private void rbTeensy_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!Initializing)
            {
                BoardType = 1;
                SwitchBoards();
                ckDefaultModule.IsChecked = true;
                SetButtons(true);
            }
        }

        private void Save()
        {
            byte val;
            byte[] Pins = new byte[16];

            if (byte.TryParse(tbModuleID.Text, out val))
            {
                mf.ModuleConfig.ModuleID = val;
            }
            if (byte.TryParse(tbSensorCount.Text, out val))
            {
                mf.ModuleConfig.SensorCount = val;
            }
            if (byte.TryParse(tbWifiPort.Text, out val))
            {
                mf.ModuleConfig.WifiPort = val;
            }
            mf.ModuleConfig.RelayType = (byte)cbRelayControl.SelectedIndex;
            mf.ModuleConfig.RelayOnHigh = ckRelayOn.IsChecked ?? false;
            mf.ModuleConfig.FlowOnHigh = ckFlowOn.IsChecked ?? false;
            mf.ModuleConfig.Momentary = ckMomentary.IsChecked ?? false;

            // flow
            if (byte.TryParse(tbFlow1.Text, out val))
            {
                mf.ModuleConfig.Sensor0Flow = val;
            }
            else
            {
                mf.ModuleConfig.Sensor0Flow = 255;
            }
            if (byte.TryParse(tbFlow2.Text, out val))
            {
                mf.ModuleConfig.Sensor1Flow = val;
            }
            else
            {
                mf.ModuleConfig.Sensor1Flow = 255;
            }

            // motor
            if (byte.TryParse(tbDir1.Text, out val))
            {
                mf.ModuleConfig.Sensor0Dir = val;
            }
            else
            {
                mf.ModuleConfig.Sensor0Dir = 255;
            }
            if (byte.TryParse(tbDir2.Text, out val))
            {
                mf.ModuleConfig.Sensor1Dir = val;
            }
            else
            {
                mf.ModuleConfig.Sensor1Dir = 255;
            }
            if (byte.TryParse(tbPWM1.Text, out val))
            {
                mf.ModuleConfig.Sensor0PWM = val;
            }
            else
            {
                mf.ModuleConfig.Sensor0PWM = 255;
            }
            if (byte.TryParse(tbPWM2.Text, out val))
            {
                mf.ModuleConfig.Sensor1PWM = val;
            }
            else
            {
                mf.ModuleConfig.Sensor1PWM = 255;
            }
            if (byte.TryParse(tbWrk.Text, out val))
            {
                mf.ModuleConfig.WorkPin = val;
            }
            else
            {
                mf.ModuleConfig.WorkPin = 255;
            }

            // Pins
            for (int i = 0; i < 16; i++)
            {
                Pins[i] = 255;
            }

            if (byte.TryParse(tbRelay1.Text, out val))
            {
                Pins[0] = val;
            }
            if (byte.TryParse(tbRelay2.Text, out val))
            {
                Pins[1] = val;
            }
            if (byte.TryParse(tbRelay3.Text, out val))
            {
                Pins[2] = val;
            }
            if (byte.TryParse(tbRelay4.Text, out val))
            {
                Pins[3] = val;
            }

            if (byte.TryParse(tbRelay5.Text, out val))
            {
                Pins[4] = val;
            }
            if (byte.TryParse(tbRelay6.Text, out val))
            {
                Pins[5] = val;
            }
            if (byte.TryParse(tbRelay7.Text, out val))
            {
                Pins[6] = val;
            }
            if (byte.TryParse(tbRelay8.Text, out val))
            {
                Pins[7] = val;
            }

            if (byte.TryParse(tbRelay9.Text, out val))
            {
                Pins[8] = val;
            }
            if (byte.TryParse(tbRelay10.Text, out val))
            {
                Pins[9] = val;
            }
            if (byte.TryParse(tbRelay11.Text, out val))
            {
                Pins[10] = val;
            }
            if (byte.TryParse(tbRelay12.Text, out val))
            {
                Pins[11] = val;
            }

            if (byte.TryParse(tbRelay13.Text, out val))
            {
                Pins[12] = val;
            }
            if (byte.TryParse(tbRelay14.Text, out val))
            {
                Pins[13] = val;
            }
            if (byte.TryParse(tbRelay15.Text, out val))
            {
                Pins[14] = val;
            }
            if (byte.TryParse(tbRelay16.Text, out val))
            {
                Pins[15] = val;
            }

            mf.ModuleConfig.ClientMode = ckClient.IsChecked ?? false;
            mf.ModuleConfig.RelayPins(Pins);
            mf.ModuleConfig.Save();

            mf.NetworkConfig.NetworkName = tbSSID.Text;
            mf.NetworkConfig.NetworkPassword = tbPassword.Text;
            mf.NetworkConfig.Save();

            // board type
            mf.Tls.SaveProperty("BoardType", BoardType.ToString());
        }

        private async void SensorCount_Enter(object sender, RoutedEventArgs e)
        {
            double temp;
            double.TryParse(tbSensorCount.Text, out temp);
            var form = new NumericView(0, 2, temp);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbSensorCount.Text = form.ReturnValue.ToString("N0");
            }
            
        }

        private void SetButtons(bool Edited)
        {
            if (!Initializing)
            {
                if (Edited)
                {
                    btnCancel.IsEnabled = true;
                    btnClose.Content = "Save"; //Properties.Resources.Save;
                    btnRescan.IsEnabled = false;
                    btnSendSubnet.IsEnabled = false;
                    btnSendToModule.IsEnabled = false;
                }
                else
                {
                    btnCancel.IsEnabled = false;
                    btnClose.Content = "OK"; //Properties.Resources.OK;
                    btnRescan.IsEnabled = true;
                    btnSendSubnet.IsEnabled = true;
                    btnSendToModule.IsEnabled = true;
                }

                FormEdited = Edited;
            }
        }

        private void SetDayMode()
        {
            try
            {
                this.Background =  new SolidColorBrush(Color.FromRgb(243, 243, 243));  //Properties.Settings.Default.DayColour;
                    tabPage6.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
					tabPage3.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
					tabPage1.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
					tabPage2.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
					tabPage5.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
					tabPage4.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); 
               
                
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp(ex.Message, this.Title, 3000, true);
            }
        }

        private string SubAddress(string Address)
        {
            IPAddress IP;
            string[] data;
            string Result = "";

            if (IPAddress.TryParse(Address, out IP))
            {
                data = Address.Split('.');
                Result = data[0] + "." + data[1] + "." + data[2] + ".";
            }
            return Result;
        }

        private void tbPassword_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Password for the network the module connects to in Client Mode.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void tbPassword_TextChanged(object sender, TextInputEventArgs e)
        {
            SetButtons(true);
        }

        private void tbSSID_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Name of the network the module connects to in Client Mode.";

            mf.Tls.ShowHelp(Message);
            hlpevent.Handled = true;
        }

        private void tbSSID_TextChanged(object sender, TextInputEventArgs e)
        {
            SetButtons(true);
        }

        private async void textBox_Enter(object sender, RoutedEventArgs e)
        {
        /****TODO
            double temp;
            bool Found = false;
            for (int j = 0; j < tabControl1.ItemCount; j++)
            {
                for (int i = 0; i < tabControl1.Items[j].Controls.Count; i++)
                {
                    if (sender.Equals(tabControl1.Items[j].Controls[i]))
                    {
                        double.TryParse(tabControl1.Items[j].Controls[i].Text, out temp);
                        var form = new NumericView(0, 255, temp);
                        
                        var result = await form.ShowDialog<DialogResult>(this);
                        if (result == DialogResult.OK)
                        {
                            tabControl1.Items[j].Controls[i].Text = form.ReturnValue.ToString("N0");
                        }
                        
                        Found = true;
                        break;
                    }
                }
                if (Found) break;
            }
            ****************/
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            SetButtons(true);
        }
        // causing problems in Avalonia
        private void textbox_TextChanged(object sender, TextInputEventArgs e)
        {
            SetButtons(true);
        }
        
        private void textbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }

        private void UpdateForm()
        {
            Initializing = true;

            byte[] data = mf.ModuleConfig.GetData();
            string[] display = new string[data.Length];

            tbModuleID.Text = data[2].ToString();
            tbSensorCount.Text = data[3].ToString();
            ckRelayOn.IsChecked = ((data[4] & 1) == 1);
            ckFlowOn.IsChecked = ((data[4] & 2) == 2);
            cbRelayControl.SelectedIndex = data[5];
            tbWifiPort.Text = data[6].ToString();
            ckMomentary.IsChecked = ((data[4] & 8) == 8);

            // flow, motor
            for (int i = 7; i < 13; i++)
            {
                if (data[i] > 60)
                {
                    display[i] = "-";
                }
                else
                {
                    display[i] = data[i].ToString();
                }
            }
            tbFlow1.Text = display[7].ToString();
            tbDir1.Text = display[8].ToString();
            tbPWM1.Text = display[9].ToString();
            tbFlow2.Text = display[10].ToString();
            tbDir2.Text = display[11].ToString();
            tbPWM2.Text = display[12].ToString();

            // work pin
            if (data[29] > 60)
            {
                tbWrk.Text = "-";
            }
            else
            {
                tbWrk.Text = data[29].ToString();
            }

            // relays
            for (int i = 13; i < 29; i++)
            {
                if (data[i] > 60)
                {
                    display[i] = "-";
                }
                else
                {
                    display[i] = data[i].ToString();
                }
            }
            tbRelay1.Text = display[13].ToString();
            tbRelay2.Text = display[14].ToString();
            tbRelay3.Text = display[15].ToString();
            tbRelay4.Text = display[16].ToString();

            tbRelay5.Text = display[17].ToString();
            tbRelay6.Text = display[18].ToString();
            tbRelay7.Text = display[19].ToString();
            tbRelay8.Text = display[20].ToString();

            tbRelay9.Text = display[21].ToString();
            tbRelay10.Text = display[22].ToString();
            tbRelay11.Text = display[23].ToString();
            tbRelay12.Text = display[24].ToString();

            tbRelay13.Text = display[25].ToString();
            tbRelay14.Text = display[26].ToString();
            tbRelay15.Text = display[27].ToString();
            tbRelay16.Text = display[28].ToString();

            LoadCombo();
            lbModuleIP.Text = mf.UDPmodules.SubNet;

            tbSSID.Text = mf.NetworkConfig.NetworkName;
            tbPassword.Text = mf.NetworkConfig.NetworkPassword;
            ckClient.IsChecked = ((data[4] & 4) == 4);

            switch (BoardType)
            {
                case 1:
                    rbTeensy.IsChecked = true; 
                    break;

                case 2:
                    rbESP32.IsChecked = true;
                    break;

                default:
                    rbNano.IsChecked = true;
                    break;
            }

            ckDefaultModule.IsChecked = false;

            Initializing = false;
        }

        private async void WifiPort_Enter(object sender, RoutedEventArgs e)
        {
            double temp;
            double.TryParse(tbWifiPort.Text, out temp);
            var form = new NumericView(0, 8, temp);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbWifiPort.Text = form.ReturnValue.ToString("N0");
            }
        }

        private void tbSensorCount_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "The number of sensors connected.";

            mf.Tls.ShowHelp(Message, "Sensors");
            hlpevent.Handled = true;

        }
}
