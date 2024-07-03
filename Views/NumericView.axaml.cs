using System;
using System.Globalization;
using System.Threading;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RateController.Enums;

namespace RateController.Views
{
    public partial class NumericView : Window
    {
        private readonly double max;
        private readonly double min;
        private bool isFirstKey;

        public double ReturnValue { get; set; }
        
        public NumericView()
        {
           InitializeComponent();
           Loaded += FormNumeric_Load;
           this.KeyDown += RegisterKeypad1_ButtonPressed;
        }   

        public NumericView(double _min, double _max, double currentValue):this()
        {
            max = _max;
            min = _min;
            

            //this.Title = gStr.gsEnteraValue; hennie
            //fill in the display
            tboxNumber.Text = currentValue.ToString();

            isFirstKey = true;
        }

        private void FormNumeric_Load(object sender, EventArgs e)
        {
            lblMax.Text = max.ToString();
            lblMin.Text = min.ToString();
            tboxNumber.SelectionStart = tboxNumber.Text.Length;
            //tboxNumber.SelectionLength = 0; hennie
            //keypad1.Focus(); hennie
        }
        
         private char KeyToChar(Key key) 
         {
              switch(key) {
			    case Key.D0:              return '0';
		        case Key.D1:              return '1';
		        case Key.D2:              return '2';
		        case Key.D3:              return '3';
		        case Key.D4:              return '4';
		        case Key.D5:              return '5';
		        case Key.D6:              return '6';
		        case Key.D7:              return '7';
		        case Key.D8:              return '8';
		        case Key.D9:              return '9';
			    case Key.NumPad0:         return '0';
		        case Key.NumPad1:         return '1';
		        case Key.NumPad2:         return '2';
		        case Key.NumPad3:         return '3';
		        case Key.NumPad4:         return '4';
		        case Key.NumPad5:         return '5';
		        case Key.NumPad6:         return '6';
		        case Key.NumPad7:         return '7';
		        case Key.NumPad8:         return '8';
		        case Key.NumPad9:         return '9';
		     }  
		     return '0'; 
		}     

        private void RegisterKeypad1_ButtonPressed(object sender, KeyEventArgs e)
        {

            if (isFirstKey)
            {
                tboxNumber.Text = "";
                isFirstKey = false;
            }

            //clear the error as user entered new values
            if(tboxNumber.Text == "Number required")
            {
                tboxNumber.Text = "";
                lblMin.Foreground = Brushes.Red;//SystemColors.ControlText;
                lblMax.Foreground = Brushes.Yellow;//SystemColors.ControlText;
            }
			
			 
            //if its a number just add it
            var isNumber = (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            //if (Char.IsNumber(e.Key))
            if (isNumber)
            {
                tboxNumber.Text += KeyToChar(e.Key);
            }

            //Backspace key, remove 1 char
            else if (e.Key == Key.B)
            {
                if (tboxNumber.Text.Length > 0)
                {
                    tboxNumber.Text = tboxNumber.Text.Remove(tboxNumber.Text.Length - 1);
                }
            }

            //decimal point
            else if (e.Key == Key.Decimal)
            {
                //does it already have a decimal?
                if (!tboxNumber.Text.Contains(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    tboxNumber.Text += Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                    //if decimal is first char, prefix with a zero
                    if (tboxNumber.Text.IndexOf(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator) == 0)
                    {
                        tboxNumber.Text = "0" + tboxNumber.Text;
                    }

                    //neg sign then added a decimal, insert a 0 
                    if (tboxNumber.Text.IndexOf("-") == 0 && tboxNumber.Text.IndexOf(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator) == 1)
                    {
                        tboxNumber.Text = "-0" + Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    }
                }
            }

            //negative sign
            else if (e.Key == Key.Subtract)
            {
                //If already has a negative don't add again
                if (!tboxNumber.Text.Contains("-"))
                {
                    //prefix the negative sign
                    tboxNumber.Text = "-" + tboxNumber.Text;
                }
                else
                {
                    //if already has one, take it away = +/- does that
                    if (tboxNumber.Text.StartsWith("-"))
                    {
                        tboxNumber.Text = tboxNumber.Text.Substring(1);
                    }
                }
            }

            //Exit or cancel
            else if (e.Key == Key.X)
            {
                var dialogResult = DialogResult.Cancel;
                Close(dialogResult);
            }

            //clear whole display
            else if (e.Key == Key.C)
            {
                tboxNumber.Text = "";
            }

            //ok button
            else if (e.Key == Key.K)
            {
                //not ok if empty - just return
                if (tboxNumber.Text == "") return;

                //culture invariant parse to double
                double tryNumber = double.Parse(tboxNumber.Text, CultureInfo.CurrentCulture);

                //test if above or below min/max
                if (tryNumber < min)
                {
                    tboxNumber.Text = "Error";
                    lblMin.Foreground = Brushes.Red;//System.Drawing.Color.Red;
                }
                else if (tryNumber > max)
                {
                    tboxNumber.Text = "Error";
                    lblMax.Foreground = Brushes.Red;//System.Drawing.Color.Red;
                }
                else
                {
                    //all good, return the value
                    this.ReturnValue = tryNumber;
                    var dialogResult = DialogResult.OK;
                    this.Close(dialogResult);
                }
            }
        }

        private void BtnDistanceUp_MouseDown(object sender, RoutedEventArgs e)
        {
            if (tboxNumber.Text == "" || tboxNumber.Text == "-" || tboxNumber.Text == "Error") tboxNumber.Text = "0";
            double tryNumber = double.Parse(tboxNumber.Text, CultureInfo.CurrentCulture);


            tryNumber++;

            if (tryNumber > max) tryNumber = max;

            tboxNumber.Text = tryNumber.ToString();

            isFirstKey = false;
        }

        private void BtnDistanceDn_MouseDown(object sender, RoutedEventArgs e)
        {
            if (tboxNumber.Text == "" || tboxNumber.Text == "-" || tboxNumber.Text == "Error") tboxNumber.Text = "0";
            double tryNumber = double.Parse(tboxNumber.Text, CultureInfo.CurrentCulture);

            tryNumber--;
            if (tryNumber < min) tryNumber = min;

            tboxNumber.Text = tryNumber.ToString();

            isFirstKey = false;
        }
        
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
        	 if (tboxNumber.Text == "") return;

            //culture invariant parse to double
            double tryNumber = double.Parse(tboxNumber.Text, CultureInfo.CurrentCulture);

            //test if above or below min/max
            if (tryNumber < min)
            {
                tboxNumber.Text = "Error";
                lblMin.Foreground = Brushes.Red;//System.Drawing.Color.Red;
            }
            else if (tryNumber > max)
            {
                tboxNumber.Text = "Error";
                lblMax.Foreground = Brushes.Red;//System.Drawing.Color.Red;
            }
            else
            {
                //all good, return the value
                this.ReturnValue = tryNumber;
                var dialogResult = DialogResult.OK;
                this.Close(dialogResult);
            }
       }     
    }
}
