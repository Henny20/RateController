using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RateController.Views;

public partial class PressureDisplayView : Window
{
    private MainWindow mf;
    
     public PressureDisplayView()
     {
         InitializeComponent();
     }
    
    public PressureDisplayView(MainWindow CallingFrom):this()
    {
        mf = CallingFrom;
    }
}
