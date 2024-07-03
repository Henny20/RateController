using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace RateController.Views;

public partial class HelpView : Window
{
    private MainWindow mf;
    private DispatcherTimer timer1 = new();
    
     public HelpView()
     {
         InitializeComponent();
         Loaded += frmHelp_Load;
         Closed += frmHelp_FormClosed;

     }
    
    public HelpView(MainWindow CallingFrom, string Message, string Title = "Help", int timeInMsec = 30000):this()
    {
        mf = CallingFrom;
        this.Title = Title;
        label1.Text = Message;
        
		timer1.IsEnabled = true;
		timer1.Tick += timer1_Tick;
		timer1.Interval = TimeSpan.FromMilliseconds(timeInMsec);
		timer1.Start();
      
        int len = Message.Length;
        this.Width = 450;

        int ht = 20 + (len / 34) * 40;
        if (ht < 160)
        {
            ht = 160;
        }
        else if (ht > 500)
        {
            ht = 500;
        }

        this.Height = ht;

        panel1.Width = this.Width - 40;
        panel1.Height = this.Height - 40;
        label1.MaxWidth = panel1.Width - 10;
        label1.MaxHeight = 0;
    }
    
    private void timer1_Tick(object sender, EventArgs e)
    {
        timer1.IsEnabled = false;
        timer1.Stop(); //same as above
       // Dispose(); ??????????

        mf.Tls.SaveFormData(this);
        Close();
    }
    
     private void frmHelp_Load(object sender, EventArgs e)
        {
            try
            {
                mf.Tls.LoadFormData(this);
                this.Background = new SolidColorBrush(Color.FromRgb(210, 220, 230));//Properties.Settings.Default.DayColour;

            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("frmHelp/frmHelp_Load: " + ex.Message);
            }
        }

        private void frmHelp_FormClosed(object sender, EventArgs e)
        {
                mf.Tls.SaveFormData(this);
        }

        private void panel1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
}        
