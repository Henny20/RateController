using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace RateController.Views;

public partial class PinsView : Window
{
    private MainWindow mf;
    
     public PinsView()
     {
         InitializeComponent();
         Loaded += frmPins_Load;
         Closed += frmPins_FormClosed;
     }
    
    public PinsView(MainWindow CallingFrom):this()
    {
        mf = CallingFrom;
    }
    
    private void  frmPins_FormClosed(object sender, System.EventArgs e)
        {
                mf.Tls.SaveFormData(this);
        }

        private void frmPins_Load(object sender, System.EventArgs e)
        {
            mf.Tls.LoadFormData(this);
            this.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
        }
}
