//﻿using AgOpenGPS;
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RateController.Enums;
using RateController.Views;
using LibVLCSharp.Shared;

namespace RateController.Classes
{
    public class clsCalibrate
    {
        private int CalPWM;
        private double cCalFactor;
        private TextBox cCalFactorBox;
        private TextBlock cDescriptionTextBlock;
        private bool cEdited;
        private bool cIsEnabled;
        private TextBlock cExpected;
        private int cID = 0;
        private bool cIsLocked;
        private Button cLocked;
        private TextBox cMeasured;
        private bool ConstantUPMstart;
        private Button cPower;
        private clsProduct cProduct;
        private ProgressBar cProgress;
        private TextBlock cPulses;
        private TextBox cRateBox;
        private bool cRunning;
        //private Timer cTimer = new Timer();
        private DispatcherTimer cTimer = new DispatcherTimer();
        private bool Initializing;
        private double MeasuredAmount;
        private MainWindow mf;
        private double PulseCountStart;
        private double PulseCountTotal;
        private int SetCount;

        public clsCalibrate(MainWindow CallingFrom, int ID)
        {
            mf = CallingFrom;
            cID = ID;
            cProduct = mf.Products.Item(cID);
            cCalFactor = cProduct.MeterCal;
            PulseCountStart = cProduct.Pulses();
            cProduct.Enabled = false;
            ConstantUPMstart = cProduct.ConstantUPM;
            cProduct.ConstantUPM = true;

            cTimer.Interval = TimeSpan.FromMilliseconds(1000);
            cTimer.IsEnabled = false;
            cTimer.Tick += CTimer_Tick;
        }

        public event EventHandler<EventArgs> Edited;

        public TextBox CalFactorBox
        {
            get { return cCalFactorBox; }
            set
            {
                cCalFactorBox = value;

                //cCalFactorBox.Enter += CCalFactor_Enter; TODO
                cCalFactorBox.TextChanged += CCalFactor_TextChanged;
               // cCalFactorBox.Validating += CCalFactor_Validating;

                Initializing = true;
                cCalFactorBox.Text = cProduct.MeterCal.ToString("N1");
                Initializing = false;
            }
        }

        public TextBlock Description
        { get { return cDescriptionTextBlock; } set { cDescriptionTextBlock = value; } }

        public bool IsEnabled
        { get { return cIsEnabled; } }

        public TextBlock Expected
        { get { return cExpected; } set { cExpected = value; } }

        public int ID
        { get { return cID; } }

        public bool IsLocked
        { get { return cIsLocked; } }

        public Button Locked
        {
            get { return cLocked; }
            set
            {
                cLocked = value;
                cLocked.Click += CLocked_Click;
            }
        }

        public TextBox Measured
        {
            get { return cMeasured; }
            set
            {
                cMeasured = value;

               // cMeasured.Enter += CMeasured_Enter; TODO
                cMeasured.TextChanged += CMeasured_TextChanged;
                //cMeasured.Validating += CMeasured_Validating; TODO
            }
        }

        public Button Power
        {
            get { return cPower; }
            set
            {
                cPower = value;
                cPower.Click += CPower_Click;
            }
        }

        public ProgressBar Progress
        {
            get { return cProgress; }
            set
            {
                cProgress = value;
                cProgress.Minimum = 0;
                cProgress.Maximum = 100;
                //cProgress.Style = ProgressBarStyle.Marquee; 
                //cProgress.MarqueeAnimationSpeed = 50;
            }
        }

        public TextBlock Pulses
        { get { return cPulses; } set { cPulses = value; } }

        public TextBox RateBox
        {
            get { return cRateBox; }
            set
            {
                cRateBox = value;

               // cRateBox.Enter += CRateBox_Enter; TODO
                cRateBox.TextChanged += CRateBox_TextChanged;
                //cRateBox.Validating += CRateBox_Validating; TODO
            }
        }

        public bool Running
        {
            get { return cRunning; }
            set
            {
                if (value)
                {
                    if (cIsEnabled && (cRunning != value))
                    {
                        // calibration started
                        cRunning = true;
                        cProgress.Value = 0;
                        PulseCountStart = cProduct.Pulses();
                        MeasuredAmount = 0;
                        cProduct.Enabled = true;
                    }
                }
                else
                {
                    // calibration stopped
                    cRunning = false;
                    cProduct.Enabled = false;
                }
                cPower.IsEnabled = !value;
                Update();
            }
        }

        public void Close()
        {
            // restore initial settings
            cProduct.Enabled = true;
            cProduct.ConstantUPM = ConstantUPMstart;
            cProduct.Save();
            cProduct.CalUseBaseRate = false;
        }

        public void Load()
        {
            double.TryParse(mf.Tls.LoadProperty(Name() + "_Pulses"), out PulseCountTotal);
            double.TryParse(mf.Tls.LoadProperty(Name() + "_Amount"), out MeasuredAmount);
            int.TryParse(mf.Tls.LoadProperty(Name() + "_CalPWM"), out CalPWM);
        }

        public void Reset()
        {
            bool Last = Initializing;
            Initializing = true;
            cPulses.Text = PulseCountTotal.ToString("N0");
            cCalFactorBox.Text = cCalFactor.ToString("N1");
            MeasuredAmount = 0;
            cMeasured.Text = MeasuredAmount.ToString("N1");
            cRateBox.Text = cProduct.RateSet.ToString("N1");
            Initializing = Last;
        }

        public void Save()
        {
            if (cEdited && cIsEnabled)
            {
                mf.Tls.SaveProperty(Name() + "_Pulses", PulseCountTotal.ToString());
                mf.Tls.SaveProperty(Name() + "_Amount", MeasuredAmount.ToString());
                mf.Tls.SaveProperty(Name() + "_CalPWM", CalPWM.ToString());

                double.TryParse(cCalFactorBox.Text, out cCalFactor);
                cProduct.MeterCal = cCalFactor;
                double.TryParse(cRateBox.Text, out double tmp);
                cProduct.RateSet = tmp;
                cProduct.Save();

                cEdited = false;
                PulseCountStart = cProduct.Pulses();
                Update();
            }
        }

        public void Update()
        {
            Initializing = true;
            if (cIsEnabled)
            {
                cPower.Content = new Image {Source = new Bitmap("Resources/FanOn.png")}; //Properties.Resources.FanOn;
            }
            else
            {
                cPower.Content = new Image {Source = new Bitmap("Resources/FanOff.png")}; //Properties.Resources.FanOff;
                cRunning = false;
            }

            cTimer.IsEnabled = cRunning;
            cCalFactorBox.IsEnabled = !cRunning && cIsEnabled;
            cMeasured.IsEnabled = !cRunning && cIsEnabled && cIsLocked;
            cRateBox.IsEnabled = !cRunning && cIsEnabled;
            cLocked.IsEnabled = !cRunning && cIsEnabled;

            if (cRunning)
            {
                if (!cIsLocked && MeterIsSet())
                {
                    cIsLocked = true;
                    switch (cProduct.ControlType)
                    {
                        case ControlTypeEnum.Valve:
                        case ControlTypeEnum.ComboCloseTimed:
                        case ControlTypeEnum.ComboClose:
                            cProduct.ManualPWM = 0;
                            break;

                        case ControlTypeEnum.Motor:
                        case ControlTypeEnum.MotorWeights:
                        case ControlTypeEnum.Fan:
                            //cProduct.ManualPWM = (int)cProduct.PWM();
                            CalPWM = (int)cProduct.PWM();
                            break;
                    }
                }

                cProduct.CalRun = cIsLocked;
                cProduct.CalSetMeter = !cIsLocked;
                cProduct.ManualPWM = CalPWM;
            }
            else
            {
                Reset();
                cProduct.CalRun = false;
                cProduct.CalSetMeter = false;
            }

            if (cIsLocked)
            {
                cLocked.Content = new Image {Source = new Bitmap("Images/ColorLocked.png")};//Properties.Resources.ColorLocked; 
                cLocked.IsVisible = true;
                cProgress.IsVisible = false;
            }
            else
            {
                cLocked.Content = new Image {Source = new Bitmap("Images/ColorUnlocked.png")}; //Properties.Resources.ColorUnlocked;
                cLocked.IsVisible = !cRunning;
                cProgress.IsVisible = cRunning;
            }

            PulseCountTotal = cProduct.Pulses() - PulseCountStart;
            cPulses.Text = PulseCountTotal.ToString("N0");
            cRateBox.Text = cProduct.RateSet.ToString("N1");
            cMeasured.Text = MeasuredAmount.ToString("N1");
            if (cCalFactor > 0)
            {
                cExpected.Text = (PulseCountTotal / cCalFactor).ToString("N1");
            }
            else
            {
                cExpected.Text = "0.0";
            }

            cDescriptionTextBlock.Text = (cID + 1).ToString() + ". " + cProduct.ProductName
                + "  - " + mf.Tls.ControlTypeDescription(cProduct.ControlType);

            Initializing = false;
        }

        private async void CCalFactor_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(cCalFactorBox.Text, out tempD);
            var form = new NumericView(0, 2000, tempD);
   
            var result = DialogResult.None;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
			   result = await form.ShowDialog<DialogResult>(desktop.MainWindow);
			}
            if (result == DialogResult.OK)
            {
                cCalFactorBox.Text = form.ReturnValue.ToString();
            }
            
        }

        private void CCalFactor_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!Initializing)
            {
                cEdited = true;
                Edited(this, RoutedEventArgs.Empty);
            }
        }

        private void CCalFactor_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(cCalFactorBox.Text, out tempD);
            if (tempD < 0 || tempD > 2000)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private void CLocked_Click(object sender, RoutedEventArgs e)
        {
            cIsLocked = !cIsLocked;
            if (cIsLocked) cCalFactor = cProduct.MeterCal;   // use saved cal factor
            Update();
        }

        private async void CMeasured_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(cMeasured.Text, out tempD);
            var form = new NumericView(0, 2000, tempD);
            
            var result = DialogResult.None;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
			   result = await form.ShowDialog<DialogResult>(desktop.MainWindow);
			}
            if (result == DialogResult.OK)
            {
               cMeasured.Text = form.ReturnValue.ToString();
            }
            
        }

        private void CMeasured_TextChanged(object sender, EventArgs e)
        {
            if (!Initializing)
            {
                cEdited = true;
                Edited(this, EventArgs.Empty);
                double.TryParse(cMeasured.Text, out MeasuredAmount);
                cCalFactorBox.Text = NewCalFactor().ToString("N1");
            }
        }

        private void CMeasured_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(cMeasured.Text, out tempD);
            if (tempD < 0 || tempD > 2000)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private void CPower_Click(object sender, RoutedEventArgs e)
        {
            cIsEnabled = !cIsEnabled;
            cProduct.CalUseBaseRate = cIsEnabled;
            Update();
        }

        private async void CRateBox_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(cRateBox.Text, out tempD);
            var result = DialogResult.None;
            var form = new NumericView(0, 50000, tempD);
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
			   result = await form.ShowDialog<DialogResult>(desktop.MainWindow);
			}

            if (result == DialogResult.OK)
            {
                cRateBox.Text = form.ReturnValue.ToString();
            }
            
        }

        private void CRateBox_TextChanged(object sender, EventArgs e)
        {
            if (!Initializing)
            {
                cEdited = true;
                Edited(this, EventArgs.Empty);
            }
        }

        private void CRateBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double tempD;
            double.TryParse(cRateBox.Text, out tempD);
            if (tempD < 0 || tempD > 50000)
            {
               // System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private void CTimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private bool MeterIsSet()
        {
            // true if within 10%
            bool Result = false;
            if (cProduct.RateSet > 0)
            {
                double Ratio = Math.Abs(cProduct.RateSet - cProduct.RateApplied()) / cProduct.RateSet;
                if (Ratio < 0.1)
                {
                    SetCount++;
                    if (SetCount > 3) Result = true;
                }
                else
                {
                    SetCount = 0;
                }
            }
            return Result;
        }

        private string Name()
        {
            return "Calibrate" + cID.ToString();
        }

        private double NewCalFactor()
        {
            double Result = 0;
            if (MeasuredAmount > 0)
            {
                // Meter Cal = PPM/UPM
                // = pulses/amount in same time frame
                Result = PulseCountTotal / MeasuredAmount;
            }
            return Result;
        }
    }
}
