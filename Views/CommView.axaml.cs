using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RateController.Classes;

namespace RateController.Views;

public partial class CommView : Window
{
    private MainWindow mf;
    private DispatcherTimer timer1 = new();
    public CommView()
    {
        InitializeComponent();
        Closed += frmComm_FormClosed;
        Loaded += frmComm_Load;
   
    }
    
    public CommView(MainWindow CallingFrom):this()
    {
        mf = CallingFrom;
        
		#region // language

		btnConnect1.Content = Lang.lgConnect; 
		btnConnect2.Content = Lang.lgConnect;
		btnConnect3.Content = Lang.lgConnect;

		#endregion // language
    }
    
    private void bntOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConnect1_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect1.Content == Lang.lgConnect)
            {
                mf.SER[0].OpenRCport();
            }
            else
            {
                mf.SER[0].CloseRCport();
            }
            SetPortButtons1();
        }

        private void btnConnect2_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect2.Content == Lang.lgConnect)
            {
                mf.SER[1].OpenRCport();
            }
            else
            {
                mf.SER[1].CloseRCport();
            }
            SetPortButtons2();
        }

        private void btnConnect3_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect3.Content == Lang.lgConnect)
            {
                mf.SER[2].OpenRCport();
            }
            else
            {
                mf.SER[2].CloseRCport();
            }
            SetPortButtons3();
        }

        private void btnRescan_Click(object sender, RoutedEventArgs e)
        {
            LoadRCbox();
        }

        private void cboBaud1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[0].ArduinoPort.BaudRate = System.Convert.ToInt32(cboBaud1.SelectedItem);
            mf.SER[0].RCportBaud = System.Convert.ToInt32(cboBaud1.SelectedItem);
        }

        private void cboBaud2_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[1].ArduinoPort.BaudRate = System.Convert.ToInt32(cboBaud2.SelectedItem);
            mf.SER[1].RCportBaud = System.Convert.ToInt32(cboBaud2.SelectedItem);
        }

        private void cboBaud3_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[2].ArduinoPort.BaudRate = System.Convert.ToInt32(cboBaud3.SelectedItem);
            mf.SER[2].RCportBaud = System.Convert.ToInt32(cboBaud3.SelectedItem);
        }

        private void cboPort1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[0].RCportName = cboPort1.SelectedItem.ToString();
        }

        private void cboPort2_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[1].RCportName = cboPort2.SelectedItem.ToString();
        }

        private void cboPort3_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            mf.SER[2].RCportName = cboPort3.SelectedItem.ToString();
        }

        private void frmComm_FormClosed(object sender, System.EventArgs e)
        {
            mf.Tls.SaveFormData(this);
            timer1.IsEnabled = false;
        }

        private void frmComm_Load(object sender, System.EventArgs e)
        {
            timer1.IsEnabled = true;
                timer1.Tick += timer1_Tick;
                timer1.Start();
            mf.Tls.LoadFormData(this);
            UpdateForm();
            LoadRCbox();
        }

        private void groupBox3_Paint(object sender, RoutedEventArgs e)
        {
           // GroupBox box = sender as GroupBox; TODO
           // mf.Tls.DrawGroupBox(box, e.Graphics, this.Background, Color.Black, Color.Blue); TODO
        }

        private void LoadRCbox()
        {
            cboPort1.Items.Clear();
            foreach (System.String s in System.IO.Ports.SerialPort.GetPortNames())
            {
                cboPort1.Items.Add(s);
                cboPort2.Items.Add(s);
                cboPort3.Items.Add(s);
            }
            SetPortButtons1();
            SetPortButtons2();
            SetPortButtons3();
        }

        private void SetPortButtons1()
        {
           // cboPort1.SelectedIndex = cboPort1.Items.Where(item => item.ToString() = mf.SER[0].RCportName); TODO
          //  cboBaud1.SelectedIndex = cboBaud1.FindStringExact(mf.SER[0].RCportBaud.ToString()); TODO

            if (mf.SER[0].ArduinoPort.IsOpen)
            {
                cboBaud1.IsEnabled = false;
                cboPort1.IsEnabled = false;
                btnConnect1.Content = Lang.lgDisconnect;
                PortIndicator1.Text = "On"; //new Image {Source = new Bitmap("Resources/On.png")}; //Properties.Resources.On;
            }
            else
            {
                cboBaud1.IsEnabled = true;
                cboPort1.IsEnabled = true;
                btnConnect1.Content = Lang.lgConnect;
                PortIndicator1.Text = "Off"; // new Image {Source = new Bitmap("Resources/Off.png")}; //Properties.Resources.Off;
            }
        }

        private void SetPortButtons2()
        {
           // cboPort2.SelectedIndex = cboPort2.SelectedItem(mf.SER[1].RCportName); TODO
            //cboBaud2.SelectedIndex = cboBaud2.selectedItem(mf.SER[1].RCportBaud.ToString()); TODO

            if (mf.SER[1].ArduinoPort.IsOpen)
            {
                cboBaud2.IsEnabled = false;
                cboPort2.IsEnabled = false;
                btnConnect2.Content = Lang.lgDisconnect;
                PortIndicator2.Text = "On"; // new Image {Source = new Bitmap("Resources/On.png")}; // Properties.Resources.On;
            }
            else
            {
                cboBaud2.IsEnabled = true;
                cboPort2.IsEnabled = true;
                btnConnect2.Content = Lang.lgConnect;
                PortIndicator2.Text = "Off"; //Properties.Resources.Off;
            }
        }

        private void SetPortButtons3()
        {
            //cboPort3.SelectedIndex = cboPort3.FindStringExact(mf.SER[2].RCportName); TODO
            //cboBaud3.SelectedIndex = cboBaud3.FindStringExact(mf.SER[2].RCportBaud.ToString()); TODO

            if (mf.SER[2].ArduinoPort.IsOpen)
            {
                cboBaud3.IsEnabled = false;
                cboPort3.IsEnabled = false;
                btnConnect3.Content = Lang.lgDisconnect;
                PortIndicator3.Text = "On"; //Properties.Resources.On;
            }
            else
            {
                cboBaud3.IsEnabled = true;
                cboPort3.IsEnabled = true;
                btnConnect3.Content = Lang.lgConnect;
                PortIndicator3.Text = "Off"; //Properties.Resources.Off;
            }
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            SetPortButtons1();
            SetPortButtons2();
            SetPortButtons3();
        }

        private void UpdateForm()
        {
            PortIndicator1.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
            PortIndicator2.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
            PortIndicator3.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); // Properties.Settings.Default.DayColour;

            this.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;

           // foreach (Control c in this.Controls) TODO or not
           // {
           //     c.ForeColor = Color.Black;
           // }
        }
}
