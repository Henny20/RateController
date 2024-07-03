using RateController.Properties;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.IO;
//using System.Runtime.Remoting.Messaging; TODO
using System.Security.Policy;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RateController.Classes;
using RateController.PGNs;
using RateController.Enums;
using RateController.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using static RateController.PGNs.PGN32618;

namespace RateController;

 public enum SimType
    {
        None,
        VirtualNano,
        Speed
    }

public partial class MainWindow : Window
{
      
    public readonly int MaxModules = 8;
    public readonly int MaxProducts = 6;// last two are fans
    public readonly int MaxRelays = 16;
    public readonly int MaxSections = 128;
    public readonly int MaxSensors = 8;
    public readonly int MaxSwitches = 16;
    public PGN32401 AnalogData;
    public PGN254 AutoSteerPGN;
    public string[] CoverageAbbr = new string[] { "Ac", "Ha", "Min", "Hr" };
    public string[] CoverageDescriptions = new string[] { Lang.lgAcres, Lang.lgHectares, Lang.lgMinutes, Lang.lgHours };
    public bool cUseInches;
    public bool LargeScreenExit = false;
    public LargeScreenView Lscrn; 
    public PGN238 MachineConfig;
    public PGN239 MachineData;
    public PGN32700 ModuleConfig;
    public PGN32702 NetworkConfig;
    public clsPressures PressureObjects;
    public clsProducts Products;
    public clsAlarm RCalarm;
    public clsRelays RelayObjects;
    public bool Restart = false;
    public clsSectionControl SectionControl;
    public clsSections Sections;
    public PGN235 SectionsPGN;
    public SerialComm[] SER = new SerialComm[3];
    public bool ShowCoverageRemaining;
    public bool ShowQuantityRemaining;
    public Color SimColor = Color.FromRgb(255, 182, 0);
    public PGN32618 SwitchBox;
    public clsTools Tls;

    public string[] TypeDescriptions = new string[] { Lang.lgSection, Lang.lgSlave, Lang.lgMaster, Lang.lgPower,
        Lang.lgInvertSection,Lang.lgHydUp,Lang.lgHydDown,Lang.lgTramRight,
        Lang.lgTramLeft,Lang.lgGeoStop,Lang.lgNone};

    public UDPComm UDPaog;
    public UDPComm UDPmodules;
    public PGN228 VRdata;
    public clsVirtualSwitchBox vSwitchBox;
    public string WiFiIP;
    public clsZones Zones;
    private int cDefaultProduct = 0;
    private byte cPressureToShowID;
    private int cPrimeDelay = 0;
    private double cPrimeTime = 0;
    private bool cShowPressure;
    private bool cShowSwitches = false;
    private SimType cSimMode = SimType.None;
    private double cSimSpeed = 0;
    private int CurrentPage;
    private int CurrentPageLast;
    private bool cUseLargeScreen = false;
    private bool cUseTransparent = false;
    private TextBlock[] Indicators;
    private bool LoadError = false;
    private DateTime[] ModuleTime;
    private TextBlock[] ProdName;
    private TextBlock[] Rates;
    private int[] RateType = new int[6];
    private PGN32501[] RelaySettings;
    private DateTime StartTime;
    private TextBlock[] Targets;
    public PGN229 AOGsections;
    private bool cUseDualAuto;
    private bool cResumeAfterPrime; 
    private bool cMasterOverride;
    private DispatcherTimer timerMain = new();
    private DispatcherTimer timerPIDs = new();
    public clsSwitches SwitchObjects;
    public SwitchesView SwitchesForm;
       
    public static MainWindow? Instance { get; private set; }
    
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
        Loaded += FormStart_Load;
        Closing += FormStart_FormClosing;
        Closed += WindowRateControl_WindowClosed;
        Activated += WindowStart_Activated;
        
        #region // language
        
        lbRate.Text = Lang.lgCurrentRate;
        lbTarget.Text = Lang.lgTargetRate;
        lbCoverage.Text = Lang.lgCoverage;
        lbRemaining.Text = Lang.lgTank_Remaining + " ...";
        /********** Use ViewModel instead
        btnSettings.Click += (s, a) =>
        {
            MenuFlyout menuFlyout = new MenuFlyout() { Placement = PlacementMode.Bottom };
            List<MenuItem> items = new List<MenuItem>();
            items.Add(new MenuItem { Name = "MnuProducts", Header = Lang.lgProducts });
            items.Add(new MenuItem { Header =  Lang.lgSections });
            items.Add(new MenuItem { Header = Lang.lgOptions });
            items.Add(new MenuItem { Header = Lang.lgComm });
            items.Add(new MenuItem { Header =  Lang.lgRelays });
            items.Add(new MenuItem { Header = Lang.lgCalibrate });
            items.Add(new MenuItem { Header = Lang.lgModules });
            menuFlyout.ItemsSource = items;
            menuFlyout.ShowAt(btnSettings);
        };
        ************/
        MnuProducts.Header = Lang.lgProducts;
        MnuSections.Header = Lang.lgSections;
        MnuOptions.Header = Lang.lgOptions;
        MnuComm.Header = Lang.lgComm;
        MnuRelays.Header = Lang.lgRelays;
        calibrateToolStripMenuItem1.Header = Lang.lgCalibrate;
        networkToolStripMenuItem.Header = Lang.lgModules;
        //exitToolStripMenuItem.Text = Lang.lgExit;

        pressuresToolStripMenuItem1.Header = Lang.lgPressure;
        commDiagnosticsToolStripMenuItem.Header = Lang.lgCommDiagnostics;
      //  newToolStripMenuItem.Text = Lang.lgNew; 
       // openToolStripMenuItem.Text = Lang.lgOpen;
      //  saveAsToolStripMenuItem.Text = Lang.lgSaveAs;
           
        /**** WinForms menu
        mnuSettings.Items["MnuProducts"].Text = Lang.lgProducts;
        mnuSettings.Items["MnuSections"].Text = Lang.lgSections;
        mnuSettings.Items["MnuOptions"].Text = Lang.lgOptions;
        mnuSettings.Items["MnuComm"].Text = Lang.lgComm;
        mnuSettings.Items["MnuRelays"].Text = Lang.lgRelays;
        mnuSettings.Items["calibrateToolStripMenuItem1"].Text = Lang.lgCalibrate;
        mnuSettings.Items["networkToolStripMenuItem"].Text = Lang.lgModules;
        mnuSettings.Items["exitToolStripMenuItem"].Text = Lang.lgExit;

        mnuSettings.Items["pressuresToolStripMenuItem1"].Text = Lang.lgPressure;
        mnuSettings.Items["commDiagnosticsToolStripMenuItem"].Text = Lang.lgCommDiagnostics;
        mnuSettings.Items["newToolStripMenuItem"].Text = Lang.lgNew; ;
        mnuSettings.Items["openToolStripMenuItem"].Text = Lang.lgOpen;
        mnuSettings.Items["saveAsToolStripMenuItem"].Text = Lang.lgSaveAs;
        **********/
        #endregion // language

        Tls = new clsTools(this);

        //UDPaog = new UDPComm(this, 16666, 17777, 16660, "127.0.0.255");       // AGIO

        UDPaog = new UDPComm(this, 17777, 15555, 1460, "UDPaog", "127.255.255.255");        // AOG
        UDPmodules = new UDPComm(this, 29999, 28888, 1480, "UDPmodules");                   // arduino

        AutoSteerPGN = new PGN254(this);
        SectionsPGN = new PGN235(this);
        MachineConfig = new PGN238(this);
        MachineData = new PGN239(this);
        VRdata = new PGN228(this);

        SwitchBox = new PGN32618(this);
        AnalogData = new PGN32401(this);

        Sections = new clsSections(this);
        Products = new clsProducts(this);
        RCalarm = new clsAlarm(this, btAlarm);

        for (int i = 0; i < 3; i++)
        {
            SER[i] = new SerialComm(this, i);
        }

        ProdName = new TextBlock[] { prd0, prd1, prd2, prd3, prd4, prd5 };
        Rates = new TextBlock[] { rt0, rt1, rt2, rt3, rt4, rt5 };
        Indicators = new TextBlock[] { idc0, idc1, idc2, idc3, idc4, idc5 };
        Targets = new TextBlock[] { tg0, tg1, tg2, tg3 };

        cUseInches = true;

        PressureObjects = new clsPressures(this);
        RelayObjects = new clsRelays(this);

        timerMain.Interval = TimeSpan.FromMilliseconds(1000);
               
        RelaySettings = new PGN32501[MaxModules];
        for (int i = 0; i < MaxModules; i++)
        {
            RelaySettings[i] = new PGN32501(this, i);
        }

        ModuleTime = new DateTime[MaxModules];
        Zones = new clsZones(this);
        vSwitchBox = new clsVirtualSwitchBox(this);
        ModuleConfig = new PGN32700(this);
        NetworkConfig = new PGN32702(this);
        AOGsections = new PGN229(this);
        SectionControl = new clsSectionControl(this);
    }
    
     public event EventHandler ProductChanged;

        public int DefaultProduct
        {
            get { return cDefaultProduct; }
            set
            {
                if (value >= 0 && value < MaxProducts - 2)
                {
                    cDefaultProduct = value;
                    Tls.SaveProperty("DefaultProduct", cDefaultProduct.ToString());
                }
            }
        }
        public bool ResumeAfterPrime
        {
            get { return cResumeAfterPrime; }
            set
            {
                cResumeAfterPrime=value;
                Tls.SaveProperty("ResumeAfterPrime", cResumeAfterPrime.ToString());
            }
        }
        public bool UseDualAuto { get { return cUseDualAuto; } set { cUseDualAuto = value; } }
        public byte PressureToShow
        {
            get { return cPressureToShowID; }
            set
            {
                if (value >= 0 && value < 17)
                {
                    cPressureToShowID = value;
                }
            }
        }

        public int PrimeDelay
        {
            get { return cPrimeDelay; }
            set
            {
                if (value >= 0 && value < 9) { cPrimeDelay = value; }
            }
        }

        public double PrimeTime
        {
            get { return cPrimeTime; }
            set
            {
                if (value >= 0 && value < 30) { cPrimeTime = value; }
            }
        }

        public bool ShowPressure
        {
            get { return cShowPressure; }
            set
            {
                cShowPressure = value;
                Tls.SaveProperty("ShowPressure", value.ToString());
                DisplayPressure();
            }
        }

        public bool ShowSwitches
        {
            get { return cShowSwitches; }
            set
            {
                cShowSwitches = value;
                Tls.SaveProperty("ShowSwitches", cShowSwitches.ToString());
                DisplaySwitches();
            }
        }

        public SimType SimMode
        {
            get { return cSimMode; }
            set
            {
                cSimMode = value;
            }
        }

        public double SimSpeed
        {
            get { return cSimSpeed; }
            set
            {
                if (value >= 0 && value < 40) { cSimSpeed = value; }
            }
        }

        public bool UseInches
        {
            get { return cUseInches; }
            set 
            {
                cUseInches = value;
                Tls.SaveProperty("UseInches", cUseInches.ToString());
            }
        }

        public bool UseLargeScreen
        {
            get { return cUseLargeScreen; }
            set
            {
                cUseLargeScreen = value;
                Tls.SaveProperty("UseLargeScreen", cUseLargeScreen.ToString());
            }
        }
        
       public void SwitchScreens(bool SingleProduct = false)
        {
        
        /***************TODO
            try
            {
                Form fs = Tls.IsFormOpen("frmLargeScreen");
                if (cUseLargeScreen)
                {
                    if (SingleProduct)
                    {
                        // hide unused items, set product 4 as default, set product 4 id to 0
                        foreach (clsProduct Prd in Products.Items)
                        {
                            Prd.OnScreen = false;
                        }
                        clsProduct P0 = Products.Items[0];
                        clsProduct P3 = Products.Items[3];

                        P3.ProductName = P0.ProductName;
                        P3.ControlType = P0.ControlType;
                        P3.QuantityDescription = P0.QuantityDescription;
                        P3.CoverageUnits = P0.CoverageUnits;
                        P3.MeterCal = P0.MeterCal;
                        P3.ProdDensity = P0.ProdDensity;
                        P3.EnableProdDensity = P0.EnableProdDensity;
                        P3.RateSet = P0.RateSet;
                        P3.RateAlt = P0.RateAlt;
                        P3.TankSize = P0.TankSize;
                        P3.TankStart = P0.TankStart;

                        P3.UseVR = P0.UseVR;
                        P3.VRID = P0.VRID;
                        P3.VRmax = P0.VRmax;
                        P3.VRmin = P0.VRmin;

                        P3.PIDkp = P0.PIDkp;
                        P3.PIDki = P0.PIDki;
                        P3.PIDkd = P0.PIDkd;
                        P3.PIDmax = P0.PIDmax;
                        P3.PIDmin = P0.PIDmin;
                        P3.PIDscale = P0.PIDscale;

                        Products.Item(2).BumpButtons = true;
                        P0.ModuleID = 6;
                        P3.ChangeID(0, 0);
                        P3.OnScreen = true;
                        P3.ConstantUPM = P0.ConstantUPM;
                        P3.UseOffRateAlarm = P0.UseOffRateAlarm;
                        P3.OffRateSetting = P0.OffRateSetting;
                        P3.MinUPM = P0.MinUPM;
                        P3.BumpButtons = false;
                        P3.UseMultiPulse = P0.UseMultiPulse;

                        P3.CountsRev = P0.CountsRev;
                        DefaultProduct = 3;
                        UseTransparent = true;
                    }

                    if (fs == null)
                    {
                        LargeScreenExit = false;
                        Restart = false;
                        this.WindowState = WindowState.Minimized;
                        this.ShowInTaskbar = false;
                        Lscrn = new frmLargeScreen(this);
                        Lscrn.ShowInTaskbar = true;
                        Lscrn.SetTransparent();
                        Lscrn.Show();
                    }
                }
                else
                {
                    // use standard screen
                    if (fs != null) Lscrn.SwitchToStandard();
                }
            }
            catch (Exception ex)
            {
                Tls.WriteErrorLog("SwitchScreens: " + ex.Message);
            }
            *************/
        }
        
        public bool MasterOverride
        {
            get { return cMasterOverride; }
            set
            {
                cMasterOverride = value;
                Tls.SaveProperty("MasterOverride",cMasterOverride.ToString());  
            }
        }

        public bool UseTransparent
        {
            get { return cUseTransparent; }
            set
            {
                cUseTransparent = value;
                Tls.SaveProperty("UseTransparent", cUseTransparent.ToString());
            }
        }

        public bool UseZones
        {
            get
            {
                bool tmp = false;
                if (bool.TryParse(Tls.LoadProperty("UseZones"), out bool tmp2)) tmp = tmp2;
                return tmp;
            }
            set { Tls.SaveProperty("UseZones", value.ToString()); }
        }

        public void ChangeLanguage()
        {
            Restart = true;
           // Application.Restart(); TODO
        }

        public int CurrentProduct()
        {
            int Result = 0;
            if (cUseLargeScreen)
            {
                Result = Lscrn.CurrentProduct();
            }
            else
            {
                if (CurrentPage > 1) Result = CurrentPage - 1;
            }
            return Result;
        }

        public void DisplayPressure()
        {
            Window fs = Tls.IsViewOpen("PressureDisplayWindow");
           
            if (cShowPressure)
            {
                if (fs == null)
                {
                    Window frm = new PressureDisplayView(this);
                    frm.ShowDialog(this);
                }
                else
                {
                    fs.Focus();
                }
            }
            else
            {
                if (fs != null) fs.Close();
            }
        }

        public void DisplaySwitches()
        {
            Window fs = Tls.IsViewOpen("RateController.Views.SwitchesView");

            if (cShowSwitches)
            {
                if (fs == null)
                {
                    Window frm = new SwitchesView(this);
                    frm.ShowDialog(this);
                }
                else
                {
                    fs.Focus();
                }
            }
            else
            {
                if (fs != null) fs.Close();
            }
        }

        public void LoadSettings()
        {
            StartSerial();
            SetDayMode();

            if (bool.TryParse(Tls.LoadProperty("UseInches"), out bool tmp)) cUseInches = tmp;
            if (bool.TryParse(Tls.LoadProperty("UseLargeScreen"), out bool LS)) cUseLargeScreen = LS;
            if (bool.TryParse(Tls.LoadProperty("UseTransparent"), out bool Ut)) cUseTransparent = Ut;
            if (bool.TryParse(Tls.LoadProperty("ShowSwitches"), out bool SS)) cShowSwitches = SS;
            if (bool.TryParse(Tls.LoadProperty("ShowPressure"), out bool SP)) cShowPressure = SP;
            if (byte.TryParse(Tls.LoadProperty("PressureID"), out byte ID)) cPressureToShowID = ID;
            if (bool.TryParse(Tls.LoadProperty("ShowQuantityRemaining"), out bool QR)) ShowQuantityRemaining = QR;
            if (bool.TryParse(Tls.LoadProperty("ShowCoverageRemaining"), out bool CR)) ShowCoverageRemaining = CR;
            if (bool.TryParse(Tls.LoadProperty("UseDualAuto"), out bool ud)) cUseDualAuto = ud;
            if (bool.TryParse(Tls.LoadProperty("ResumeAfterPrime"), out bool re)) cResumeAfterPrime = re;

            if (int.TryParse(Tls.LoadProperty("PrimeDelay"), out int PD))
            {
                cPrimeDelay = PD;
            }
            else
            {
                cPrimeDelay = 3;
            }

            if (double.TryParse(Tls.LoadProperty("SimSpeed"), out double Spd))
            {
                cSimSpeed = Spd;
            }
            else
            {
                cSimSpeed = 5;
            }

            if (Enum.TryParse(Tls.LoadProperty("SimMode"), out SimType SM))
            {
                cSimMode = SM;
            }
            else
            {
                cSimMode = SimType.None;
            }

            if (double.TryParse(Tls.LoadProperty("PrimeTime"), out double ptime))
            {
                cPrimeTime = ptime;
            }
            else
            {
                cPrimeTime = 5;
            }

            Sections.Load();
            Sections.CheckSwitchDefinitions();

            Products.Load();
            PressureObjects.Load();
            RelayObjects.Load();

            LoadDefaultProduct();
            Zones.Load();
        }

        public bool ModuleConnected(int ModuleID)
        {
            bool Result = false;
            if (ModuleID > -1 && ModuleID < MaxModules)
            {
                Result = (DateTime.Now - ModuleTime[ModuleID]).TotalSeconds < 5;
            }
            return Result;
        }

        public void SendSerial(byte[] Data)
        {
            for (int i = 0; i < 3; i++)
            {
                SER[i].SendData(Data);
            }
        }

        public void StartLargeScreen()
        {
        /*****TODO
            UseLargeScreen = true;
            LargeScreenExit = false;
            Restart = false;
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            Lscrn = new LargeScreenView(this);
            Lscrn.ShowInTaskbar = true;
            Lscrn.SetTransparent();
            Lscrn.ShowDialog(this);
            *************/
        }

        public void StartSerial()
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    String ID = "_" + i.ToString() + "_";
                    SER[i].RCportName = Tls.LoadProperty("RCportName" + ID + i.ToString());

                    int tmp;
                    if (int.TryParse(Tls.LoadProperty("RCportBaud" + ID + i.ToString()), out tmp))
                    {
                        SER[i].RCportBaud = tmp;
                    }
                    else
                    {
                        SER[i].RCportBaud = 38400;
                    }

                    bool tmp2;
                    bool.TryParse(Tls.LoadProperty("RCportSuccessful" + ID + i.ToString()), out tmp2);
                    if (tmp2) SER[i].OpenRCport();
                }
            }
            catch (Exception ex)
            {
                Tls.WriteErrorLog("WindowRateControl/StartSerial: " + ex.Message);
                Tls.ShowHelp(ex.Message, this.Title, 3000, true);
            }
        }

        public void UpdateModuleConnected(int ModuleID)
        {
            if (ModuleID > -1 && ModuleID < MaxModules) ModuleTime[ModuleID] = DateTime.Now;
        }

        public void UpdateStatus()
        {
            try
            {
                this.Title = "RC [" + Path.GetFileNameWithoutExtension(Properties.Settings.Default.FileName) + "]";

                if (cSimMode == SimType.Speed|| SectionControl.PrimeOn)
                {
                    btnSettings.Content = Properties.Resources.SimGear;
                }
                else
                {
                    if (AutoSteerPGN.Connected())
                    {
                        btnSettings.Content = Properties.Resources.GreenGear;
                    }
                    else
                    {
                        btnSettings.Content = Properties.Resources.RedGear;
                    }
                }

                WindowatDisplay();

                if (CurrentPage == 0)
                {
                    // summary
                    for (int i = 0; i < MaxProducts; i++)
                    {
                        ProdName[i].Text = Products.Item(i).ProductName;

                            ProdName[i].Background = new SolidColorBrush(SimColor);
                           // ProdName[i].BorderStyle = BorderStyle.FixedSingle; TODO

                        Rates[i].Text = Products.Item(i).SmoothRate().ToString("N1");
                        if (i < 4)
                        {
                            Targets[i].Text = Products.Item(i).TargetRate().ToString("N1");
                        }

                        if (Products.Item(i).ArduinoModule.Connected())
                        {
                            Indicators[i].Text = "On";//Properties.Resources.OnSmall;TODO
                        }
                        else
                        {
                            Indicators[i].Text = "Off"; //Properties.Resources.OffSmall; TODO
                        }
                    }
                    lbArduinoConnected.IsVisible = false;
                }
                else
                {
                    // product pages
                    clsProduct Prd = Products.Item(CurrentPage - 1);

                    if (Prd.UseVR)
                    {
                        lbTarget.Text = "VR Target";
                    }
                    else if (Prd.UseAltRate)
                    {
                        lbTarget.Text = Lang.lgTargetRateAlt;
                    }
                    else
                    {
                        lbTarget.Text = Lang.lgTargetRate;
                    }

                    lbFan.Text = CurrentPage.ToString() + ". " + Prd.ProductName;
                    lbTargetRPM.Text = Prd.TargetRate().ToString("N0");
                    lbCurrentRPM.Text = Prd.SmoothRate().ToString("N0");
                    lbOn.IsVisible = Prd.FanOn;
                    lbOff.IsVisible = !Prd.FanOn;

                    lbProduct.Text = CurrentPage.ToString() + ". " + Prd.ProductName;
                    SetRate.Text = Prd.TargetRate().ToString("N1");
                    lblUnits.Text = Prd.Units();

                    if (ShowCoverageRemaining)
                    {
                        lbCoverage.Text = CoverageDescriptions[Prd.CoverageUnits] + " Left ...";
                        double RT = Prd.SmoothRate();
                        if (RT == 0) RT = Prd.TargetRate();

                        if ((RT > 0) & (Prd.TankStart > 0))
                        {
                            AreaDone.Text = ((Prd.TankStart - Prd.UnitsApplied()) / RT).ToString("N1");
                        }
                        else
                        {
                            AreaDone.Text = "0.0";
                        }
                    }
                    else
                    {
                        // show amount done
                        AreaDone.Text = Prd.CurrentCoverage().ToString("N1");
                        lbCoverage.Text = Prd.CoverageDescription() + " ...";
                    }

                    if (ShowQuantityRemaining)
                    {
                        lbRemaining.Text = Lang.lgTank_Remaining + " ...";
                        // calculate remaining
                        TankRemain.Text = (Prd.TankStart - Prd.UnitsApplied()).ToString("N1");
                    }
                    else
                    {
                        // show amount done
                        lbRemaining.Text = Lang.lgQuantityApplied + " ...";
                        TankRemain.Text = Prd.UnitsApplied().ToString("N1");
                    }

                    switch (RateType[CurrentPage - 1])
                    {
                        case 1:
                            lbRate.Text = Lang.lgInstantRate;
                            lbRateAmount.Text = Prd.CurrentRate().ToString("N1");
                            break;

                        case 2:
                            lbRate.Text = Lang.lgOverallRate;
                            lbRateAmount.Text = Prd.AverageRate().ToString("N1");
                            break;

                        default:
                            lbRate.Text = Lang.lgCurrentRate;
                            lbRateAmount.Text = Prd.SmoothRate().ToString("N1");
                            break;
                    }

                    if (Prd.ArduinoModule.ModuleSending())
                    {
                        if (Prd.ArduinoModule.ModuleReceiving())
                        {
                            lbArduinoConnected.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            lbArduinoConnected.Background = Brushes.LightBlue;
                        }
                    }
                    else
                    {
                        lbArduinoConnected.Background = Brushes.Red;
                    }

                    lbArduinoConnected.IsVisible = true;
                }

                if (AutoSteerPGN.Connected())
                {
                    lbAogConnected.Background = Brushes.LightGreen;
                }
                else
                {
                    lbAogConnected.Background = Brushes.Red;
                }

                // alarm
                if (!cUseLargeScreen) RCalarm.CheckAlarms();

                if (CurrentPage != CurrentPageLast)
                {
                    CurrentPageLast = CurrentPage;
                   // ProductChanged?.Invoke(this, EventArgs.Empty); TODO
                }

                // fan button
                if (CurrentPage > 0 && Products.Item(CurrentPage - 1).FanOn)
                {
                    btnFan.Content = new Image {Source= new Bitmap("Resources/FanOn.png")}; //Properties.Resources.FanOn;
                }
                else
                {
                    btnFan.Content = new Image {Source= new Bitmap("Resources/FanOff.png")}; //Properties.Resources.FanOff;
                }
            }
            catch (Exception ex)
            {
                Tls.WriteErrorLog("WindowStart/UpdateStatus: " + ex.Message);
            }
        }

        private void btAlarm_Click(object sender, RoutedEventArgs e)
        {
            RCalarm.Silence();
        }

        private void btnFan_Click(object sender, RoutedEventArgs e)
        {
            Products.Item(CurrentPage - 1).FanOn = !Products.Item(CurrentPage - 1).FanOn;
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                UpdateStatus();
            }
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < MaxProducts)
            {
                CurrentPage++;
                UpdateStatus();
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = (Button)sender;
          //  Point ptLowerLeft = new Point(0, btnSender.Height); TODO
         //   ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
         //   mnuSettings.Show(ptLowerLeft);
            UpdateStatus();
            SetDayMode();
        }

        private void calibrateToolStripMenuItem1_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            //check if window already exists
            Window fs = Tls.IsViewOpen("RateController.Views.CalibrateView");

            if (fs == null)
            {
                Window frm = new CalibrateView(this);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private void commDiagnosticsToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("RateController.Views.ModuleView");

            if (fs == null)
            {
                Window frm = new ModuleView(this);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.Close();
        }

        private void WindowatDisplay()
        {
            try
            {
                int ID = CurrentPage - 1;
                if (ID < 0) ID = 0;
                clsProduct Prd = Products.Item(ID);

                this.Width = 290;

               // btAlarm.Top = 21; TODO
              //  btAlarm.Left = 33;
                btAlarm.Margin = new Thickness(33, 21, 0, 0);
                btAlarm.IsVisible = false;

                if (CurrentPage == 0)
                {
                    // summary panel
                    panSummary.IsVisible = true;
                    panFan.IsVisible = false;
                    panProducts.IsVisible = false;
                   // panSummary.Top = 0; 
                  //  panSummary.Left = 0;
                    panSummary.Margin = new Thickness(0, 0);

                    this.Height = 283;
                   //  btnSettings.Top = 180; TODO
                    btnSettings.Margin = new Thickness(0, 180);
                    btnLeft.Margin = new Thickness(0, 180);
                    btnRight.Margin = new Thickness(0, 180);
                   // btnLeft.Top = 180;
                 //   btnRight.Top = 180;
                   // lbArduinoConnected.Top = 180;
                   lbArduinoConnected.Margin = new Thickness(0, 180);
                   lbAogConnected.Margin = new Thickness(0, 214);
                  //  lbAogConnected.Top = 214;
                }
                else
                {
                    panSummary.IsVisible = false;
                    if (Prd.ControlType == ControlTypeEnum.Fan)
                    {
                        // fan panel
                       panProducts.IsVisible = false;
                       panFan.IsVisible = true; 
                       //  panFan.Top = 0;  
                       //  panFan.Left = 0;
                       panFan.Margin = new Thickness(0, 0);

                       this.Height = 257;
                      //  btnSettings.Top = 154;
                       // btnLeft.Top = 154;
                      //  btnRight.Top = 154;
                       btnSettings.Margin = new Thickness(0, 154);
                       btnLeft.Margin = new Thickness(0, 154);
                       btnRight.Margin = new Thickness(0, 154);
                      //  lbArduinoConnected.Top = 154;
                      //  lbAogConnected.Top = 188;
                       lbArduinoConnected.Margin = new Thickness(0, 154);
                       lbAogConnected.Margin = new Thickness(0, 188);
                    }
                    else
                    {
                        panProducts.IsVisible = true;
                        panFan.IsVisible = false;
                        //panProducts.Top = 0; TODO
                        //panProducts.Left = 0;
                        panProducts.Margin = new Thickness(0, 0);

                        // product panel
                        this.Height = 257;
                       // btnSettings.Top = 154; TODO
                       // btnLeft.Top = 154;
                       // btnRight.Top = 154;
                        btnSettings.Margin = new Thickness(0, 154);
                        btnLeft.Margin = new Thickness(0, 154);
                        btnRight.Margin = new Thickness(0, 154);
                      //  lbArduinoConnected.Top = 154;
                       // lbAogConnected.Top = 188;
                       lbArduinoConnected.Margin = new Thickness(0, 154);
                       lbAogConnected.Margin = new Thickness(0, 188);
                    }
                }
            }
            catch (Exception ex)
            {
                Tls.WriteErrorLog("WindowStart/WindowatDisplay: " + ex.Message);
            }
        }

        private void WindowRateControl_WindowClosed(object sender, EventArgs e)
        {
            try
            {
                Tls.SaveFormData(this);
                if (this.WindowState == WindowState.Normal)
                {
                    Tls.SaveProperty("CurrentPage", CurrentPage.ToString());
                }

                Sections.Save();
                Products.Save();
                Tls.SaveProperty("ShowQuantityRemaining", ShowQuantityRemaining.ToString());
                Tls.SaveProperty("ShowCoverageRemaining", ShowCoverageRemaining.ToString());

                Tls.SaveProperty("PrimeTime", cPrimeTime.ToString());
                Tls.SaveProperty("PrimeDelay", cPrimeDelay.ToString());
                Tls.SaveProperty("SimSpeed", cSimSpeed.ToString());
                Tls.SaveProperty("SimMode",cSimMode.ToString());
                Tls.SaveProperty("UseDualAuto", cUseDualAuto.ToString());

                UDPaog.Close();
                UDPmodules.Close();

                timerMain.IsEnabled = false;
                timerPIDs.IsEnabled = false;
                Tls.WriteActivityLog("Stopped");
                string mes = "Run time (hours): " + ((DateTime.Now - StartTime).TotalSeconds / 3600.0).ToString("N1");
                Tls.WriteActivityLog(mes);
            }
            catch (Exception)
            {
            }
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
           // Application.Exit();
                desktop.Shutdown();
        }

        private void WindowStart_Activated(object sender, EventArgs e)
        {
            if (Restart)
            {
                ChangeLanguage();
            }
            else if (LargeScreenExit)
            {
                this.Close();
            }
        }

        private async void FormStart_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!LargeScreenExit && !Restart && !LoadError && Products.Connected())
            {
               // var Hlp = new MsgBoxView(this, "Confirm Exit?", "Exit", true);
              //  Hlp.TopMost = true;

              //  Hlp.ShowDialog();
               // bool Result = Hlp.Result;
               // Hlp.Close();
                 var messageBoxStandardWindow =
                MessageBoxManager.GetMessageBoxStandard("Confirm Exit?", "Exit");
                var result =  await messageBoxStandardWindow.ShowAsync();
                if (result == ButtonResult.Yes) e.Cancel = true;
            }
        }

        private void FormStart_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                Tls.LoadFormData(this);

                CurrentPage = 5;
                int.TryParse(Tls.LoadProperty("CurrentPage"), out CurrentPage);

                if (Tls.PrevInstance())
                {
                    Tls.ShowHelp(Lang.lgAlreadyRunning, "Help", 3000);
                    this.Close();
                }

                // UDP
                UDPmodules.StartUDPServer();
                if (!UDPmodules.IsUDPSendConnected)
                {
                    Tls.ShowHelp("UDPnetwork failed to start.", "", 3000, true, true);
                }

                UDPaog.StartUDPServer();
                if (!UDPaog.IsUDPSendConnected)
                {
                    Tls.ShowHelp("UDPagio failed to start.", "", 3000, true, true);
                }

                LoadSettings();
                Products.UpdatePID();
                UpdateStatus();

                if (cUseLargeScreen) StartLargeScreen();
                DisplaySwitches();
                DisplayPressure();

                timerMain.IsEnabled = true;
                timerMain.Tick += timerMain_Tick;
                timerMain.Start();
            }
            catch (Exception ex)
            {
                Tls.ShowHelp("Failed to load properly: " + ex.Message, "Help", 30000, true);
                LoadError = true;
                Close();
            }
            SetLanguage();
            Tls.WriteActivityLog("Started", true);
            StartTime = DateTime.Now;
        }

        private void groupBox3_Paint(object sender, RoutedEventArgs e)
        {
            //GroupBox box = sender as GroupBox;
           // Tls.DrawGroupBox(box, e.Graphics, this.Background, Brushes.Black, Brushes.Black);TODO
        }

        private void TextBlock34_Click(object sender, RoutedEventArgs e)
        {
            ShowQuantityRemaining = !ShowQuantityRemaining;
            UpdateStatus();
        }

        private void lbAogConnected_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void lbAogConnected_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Indicates if AgOpenGPS is connected. Green is connected, " +
                "red is not connected. Press to minimize window.";

            this.Tls.ShowHelp(Message, "AOG");
            hlpevent.Handled = true;
        }

        private void lbArduinoConnected_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void lbArduinoConnected_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Green indicates module is sending and receiving data, blue indicates module is sending but " +
                "not receiving (AOG needs to be connected for some Coverage Types), " +
                " red indicates module is not sending or receiving, yellow is simulation mode. Press to minimize window.";

            this.Tls.ShowHelp(Message, "MOD");
            hlpevent.Handled = true;
        }

        private void lbCoverage_Click(object sender, RoutedEventArgs e)
        {
            ShowCoverageRemaining = !ShowCoverageRemaining;
            UpdateStatus();
        }

        private void lbCoverage_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Shows either coverage done or area that can be done with the remaining quantity." +
                "\n Press to change.";

            Tls.ShowHelp(Message, "Coverage");
            hlpevent.Handled = true;
        }

        private void lbRate_Click(object sender, RoutedEventArgs e)
        {
            RateType[CurrentPage - 1]++;
            if (RateType[CurrentPage - 1] > 2) RateType[CurrentPage - 1] = 0;
            UpdateStatus();
        }

        private void lbRate_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "1 - Current Rate, shows" +
                " the target rate when it is within 10% of target. Outside this range it" +
                " shows the exact rate being applied. \n 2 - Instant Rate, shows the exact rate." +
                "\n 3 - Overall, averages total quantity applied over area done." +
                "\n Press to change.";

            Tls.ShowHelp(Message, "Rate");
            hlpevent.Handled = true;
        }

        private void lbRemaining_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Shows either quantity applied or quantity remaining." +
                "\n Press to change.";

            Tls.ShowHelp(Message, "Remaining");
            hlpevent.Handled = true;
        }

        private void lbTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!Products.Item(CurrentPage - 1).UseVR)
            {
                if (Products.Item(CurrentPage - 1).UseAltRate)
                {
                    lbTarget.Text = Lang.lgTargetRate;
                    Products.Item(CurrentPage - 1).UseAltRate = false;
                }
                else
                {
                    lbTarget.Text = Lang.lgTargetRateAlt;
                    Products.Item(CurrentPage - 1).UseAltRate = true;
                }
            }
        }

        private void lbTarget_HelpRequested(object sender, RoutedEventArgs hlpevent)
        {
            string Message = "Press to switch between base rate and alternate rate.";

            Tls.ShowHelp(Message, "Target Rate");
            hlpevent.Handled = true;
        }

        private void LoadDefaultProduct()
        {
            if (int.TryParse(Tls.LoadProperty("DefaultProduct"), out int DP)) cDefaultProduct = DP;
            int count = 0;
            int tmp = 0;
            foreach (clsProduct Prd in Products.Items)
            {
                if (Prd.OnScreen && Prd.ID < MaxProducts - 2)
                {
                    count++;
                    tmp = Prd.ID;
                }
            }
            if (count == 1) DefaultProduct = tmp;

            CurrentPage = cDefaultProduct + 1;
        }

        private async void MnuComm_Click(object sender, RoutedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("RateController.Views.CommView");

            if (fs == null)
            {
                Window frm = new CommView(this);
                await frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private async void MnuOptions_Click(object sender, RoutedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("RateController.Views.OptionsView");

            if (fs == null)
            {
                 var form = new OptionsView(this);
                await form.ShowDialog(this);
                //Form frm = new frmOptions(this);
               // frm.Show();
            }
            else
            {
                fs.Focus();
            }
        }

        private void MnuRelays_Click_1(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("RelaysView");

            if (fs == null)
            {
                Window frm = new RelaysView(this);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private void mnuSettings_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void networkToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("ModuleConfigView");

            if (fs == null)
            {
                Window frm = new ModuleConfigView(this);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private async void newToolStripMenuItem_Click_1(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
             SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Directory = Tls.FilesDir();
            saveFileDialog1.Title = "New File";
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            var FileName = await saveFileDialog1.ShowAsync(this);
            if (FileName != null)
            {
                 if (FileName != "")
                {
                    Tls.OpenFile(FileName);
                    LoadSettings();
                }
            }
        }

        private async void openToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
           // openFileDialog1.InitialDirectory = Tls.FilesDir();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Directory = Tls.FilesDir();
           // if (openFileDialog1.ShowDialog() == DialogResult.OK)
            string[] result = await openFileDialog1.ShowAsync(this);

            if (result != null)
            {
                Tls.PropertiesFile = result[0];
                Products.Load();
                LoadSettings();
            }
        }

        private void pressuresToolStripMenuItem1_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Window fs = Tls.IsViewOpen("RateController.Views.PressureView");

            if (fs == null)
            {
                Window frm = new PressureView(this);
                frm.ShowDialog(this);
            }
            else
            {
                fs.Focus();
            }
        }

        private void productsToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            //check if window already exists
            Window fs = Tls.IsViewOpen("RateController.Views.SettingsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SettingsView(this, CurrentPage);
            frm.Show();
        }

        private async void saveAsToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
           // saveFileDialog1.InitialDirectory = Tls.FilesDir();
            saveFileDialog1.Directory = Tls.FilesDir();
            saveFileDialog1.Title = "Save As";
            var FileName = await saveFileDialog1.ShowAsync(this);
            if (FileName != null)
           {
                if (FileName != "")
                {
                    Tls.SaveFile(FileName);
                    LoadSettings();
                }
            }
        }

        private void sectionsToolStripMenuItem_Click(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
       
            Window fs = Tls.IsViewOpen("SectionsView");

            if (fs != null)
            {
                fs.Focus();
                return;
            }

            Window frm = new SectionsView(this);
            frm.ShowDialog(this);
      
        }

        private void SetDayMode()
        {
        
            
            if (Properties.Settings.Default.IsDay)
            {
                this.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
               // foreach (Control c in this.Controls)TODO
              //  {
               //     c.Foreground = Brushes.Black;
               // }

                for (int i = 0; i < 5; i++)
                {
                    Indicators[i].Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
                }

                lbOn.Background =  new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;//210, 220, 230
                lbOff.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;
            }
            else
            {
                this.Background =  new SolidColorBrush(Color.FromRgb(60, 60, 60));  //Properties.Settings.Default.NightColour;
               // foreach (Control c in this.Controls) TODO
               // {
               //     c.Foreground = Brushes.White;
              //  }

                for (int i = 0; i < 5; i++)
                {
                    Indicators[i].Background = Brushes.Green; // Properties.Settings.Default.NightColour;
                }

                lbOn.Background =  new SolidColorBrush(Color.FromRgb(60, 60, 60)); //Properties.Settings.Default.NightColour
                lbOff.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)); //Properties.Settings.Default.NightColour;
            }
           
        }

        private void SetLanguage()
        {
            try
            {
                if (Settings.Default.AOG_language == Settings.Default.setF_culture)
                {
                    Settings.Default.UserLanguageChange = false;
                    Settings.Default.Save();
                }
                else
                {
                    if (!Settings.Default.UserLanguageChange)
                    {
                        Settings.Default.setF_culture = Settings.Default.AOG_language;
                        Settings.Default.Save();
                        ChangeLanguage();
                    }
                }
            }
            catch (Exception ex)
            {
                Tls.WriteErrorLog("WindowStart/SetLanguage: " + ex.Message);
            }
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            UpdateStatus();

            for (int i = 0; i < MaxModules; i++)
            {
                if (ModuleConnected(i)) RelaySettings[i].Send();
            }

            Products.Update();
            SectionControl.ReadRateSwitches();
        }

        private void timerPIDs_Tick(object sender, EventArgs e)
        {
            Products.UpdatePID();
        }
}
