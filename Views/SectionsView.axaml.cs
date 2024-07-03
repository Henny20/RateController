using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RateController.Views;

public partial class SectionsView : Window
{

    private MainWindow mf;
    
    public SectionsView()
    {
         InitializeComponent();
    }     
     
    public SectionsView(MainWindow CallingFrom):this()
    {
         mf = CallingFrom;
    }
}
