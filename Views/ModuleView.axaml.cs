using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Media;
using RateController.Classes;


namespace RateController.Views;

public partial class ModuleView : Window
{
    private MainWindow mf;
    private int CommPort = 0;// 0-2
    private bool FreezeUpdate;
    private DispatcherTimer timer1 = new();
    
    public ModuleView()
    {
        InitializeComponent(); 
        Loaded += frmModule_Load;
        Closed += frmModule_FormClosed;
        cboPort1.SelectionChanged +=  cboPort1_SelectedIndexChanged;
            
    }
    
    public ModuleView(MainWindow CallingFrom):this()
    {
        mf = CallingFrom;
		#region // language

		label27.Text = Lang.lgSubnet;
		this.Title = Lang.lgCommDiagnostics;

		#endregion // language
    }
    
        private void bntOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(mf.Tls.FilesDir() + "\\Ethernet Log.txt", tbEthernet.Text);
                File.WriteAllText(mf.Tls.FilesDir() + "\\Serial Log.txt", tbSerial.Text);
                mf.Tls.ShowHelp("File saved.", "Save", 10000);
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("frmModules/btnSave_Click: " + ex.Message);
            }
        }

        private void btnSave_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Save information to file. In location 'Documents/RateController'";

            mf.Tls.ShowHelp(Message, "Save");
            hlpevent.Handled = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (FreezeUpdate)
            {
                FreezeUpdate = false;
                btnStart.Content = "Pause"; //Properties.Resources.Pause;
            }
            else
            {
                FreezeUpdate = true;
                btnStart.Content = "Start"; //Properties.Resources.Start;
            }
        }

        private void cboPort1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CommPort = Convert.ToByte(cboPort1.SelectedIndex);
            PortName.Text = "(" + mf.SER[CommPort].RCportName + ")";
        }

        private void frmModule_FormClosed(object sender, EventArgs e)
        {
            mf.Tls.SaveFormData(this);
            timer1.IsEnabled = false;
        }

        private void frmModule_Load(object sender, EventArgs e)
        {
            mf.Tls.LoadFormData(this);
            lbIP.Text = mf.UDPmodules.SubNet;
            lbAppVersion.Text = mf.Tls.AppVersion();
            lbDate.Text = mf.Tls.VersionDate();
     
              timer1.IsEnabled = true;
                timer1.Tick += timer1_Tick;
                timer1.Start();

            this.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
           // foreach (Control c in this.Controls)
          //  {
           //     c.Foreground = Color.Black;
          //  }

            foreach (TabItem p in tabControl1.Items)
            {
                p.Background = this.Background;
            }

            tbEthernet.Background = this.Background;
            tbSerial.Background = this.Background;
            tbActivity.Background = this.Background;
            tbErrors.Background = this.Background;

            cboPort1.SelectedIndex = 0;
            UpdateLogs();
        }

        private void linkLabel1_LinkClicked(object sender, RoutedEventArgs e)
        {
           /***********TODO
            try
            {
                VisitLink("https://github.com/SK21/AOG_RC/tree/master/Help");
                linkLabel1.LinkVisited = true;
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp(ex.Message, "Help", 15000, true);
            }
            *****************/
        }

        private void linkLabel2_LinkClicked(object sender, RoutedEventArgs e)
        {
           // try
          //  {
          //      VisitLink("https://github.com/SK21/AOG_RC/wiki");
          //      linkLabel2.LinkVisited = true;
          //  }
          //  catch (Exception ex)
          //  {
           //     mf.Tls.ShowHelp(ex.Message, "Help", 15000, true);
           // }
        }

        private void linkLabel3_LinkClicked(object sender, RoutedEventArgs e)
        {
        /************TODO 
            try
            {
                VisitLink("https://github.com/AgHardware/Rate_Control");
                linkLabel3.LinkVisited = true;
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp(ex.Message, "Help", 15000, true);
            }
            **************/
        }

        private void linkLabel4_LinkClicked(object sender, RoutedEventArgs e)
        {
          //  try
          //  {
           //     VisitLink("https://github.com/SK21/PCBsetup");
            //    linkLabel4.LinkVisited = true;
            //}
            //catch (Exception ex)
           // {
            //    mf.Tls.ShowHelp(ex.Message, "Help", 15000, true);
          //  }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLogs();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clsProduct Prod = mf.Products.Item(mf.CurrentProduct());

            if (Prod.ElapsedTime > 4000)
            {
                lbInoID.Text = "--";
                lbModID.Text = "--";
                lbTime.Text = "--";
            }
            else
            {
                lbInoID.Text = mf.AnalogData.InoID(Prod.ModuleID).ToString();
                lbModID.Text = Prod.ModuleID.ToString();
                lbTime.Text = (Prod.ElapsedTime / 1000.0).ToString("N3");
            }

            if (!FreezeUpdate)
            {
                tbSerial.Text = mf.SER[CommPort].Log();
                tbSerial.SelectAll(); //(tbSerial.Text.Length, 0);
                tbSerial.CaretIndex = tbSerial.Text.Length; //ScrollToCaret();

                tbEthernet.Text = mf.UDPmodules.Log();
                tbEthernet.SelectAll(); //Select(tbEthernet.Text.Length, 0);
                tbEthernet.CaretIndex = tbEthernet.Text.Length; //ScrollToCaret();

                UpdateLogs();
            }

            lbIP.Text = mf.UDPmodules.SubNet;
        }

        private void UpdateLogs()
        {
            tbActivity.Text = mf.Tls.ReadTextFile("Activity Log.txt");
            tbActivity.SelectAll(); //(tbActivity.Text.Length, 0);
            tbActivity.CaretIndex = tbActivity.Text.Length; //ScrollToCaret();

            tbErrors.Text = mf.Tls.ReadTextFile("Error Log.txt");
            tbErrors.SelectAll(); //(tbErrors.Text.Length, 0);
            tbErrors.CaretIndex = tbErrors.Text.Length;
        }

        private void VisitLink(string Link)
        {
            System.Diagnostics.Process.Start(Link);
        }
}
