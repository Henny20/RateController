using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Media;
using RateController.Classes;
using RateController.Enums;

namespace RateController.Views;

public partial class PressureView : Window
{
	private bool FormEdited = false;
	private bool Initializing;
	private int ShowID = 1;
	private MainWindow mf;
	private DispatcherTimer timer1 = new();
    private DataGridCollectionView dataSet1;
    
     public PressureView()
     { 
        InitializeComponent();
         Loaded += FormPressure_Load;   
       // Closed += frmRelays_FormClosed;
     }   
     
    public PressureView(MainWindow CalledFrom):this()
    {
		Initializing = true;

		#region // language

		ckShowPressure.Content = Lang.lgShowPressure;
  /***TODO
		DGV.Columns[0].HeaderText = Lang.lgID;
		DGV.Columns[1].HeaderText = Lang.lgDescription;
		DGV.Columns[2].HeaderText = Lang.lgModule;
		DGV.Columns[3].HeaderText = Lang.lgSensor;
		DGV.Columns[4].HeaderText = Lang.lgUnitsVolt;
		DGV.Columns[5].HeaderText = Lang.lgOffset;
		DGV.Columns[6].HeaderText = Lang.lgPressure;
*******/
		#endregion // language

		mf = CalledFrom;
		SetDayMode();
    }
    
      private void bntOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FormEdited)
                {
                    this.Close();
                }
                else
                {
                    if (mf.Tls.ReadOnly)
                    {
                        //mf.Tls.ShowHelp("File is read only.", "Help", 5000, false, false, true);n TODO
                        Console.WriteLine("File is read only.");
                    }
                    else
                    {
                        // save changes
                        SaveGrid();
                        if (byte.TryParse(tbPressureID.Text, out byte ID))
                        {
                            mf.PressureToShow = ID;
                            ShowID = ID;
                            mf.Tls.SaveProperty("PressureID", ID.ToString());
                        }
                        mf.ShowPressure = ckShowPressure.IsChecked ?? false;

                        UpdateForm();
                        SetButtons(false);
                        timer1.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                mf.Tls.ShowHelp(ex.Message, "Pressure", 3000, true);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateForm();
            SetButtons(false);
        }

        private void ckShowPressure_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SetButtons(true);
        }
/***********TODO
        private void DGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            timer1.Enabled = false;
            switch (e.ColumnIndex)
            {
                case 2:
                case 3:
                    // module, sensor
                    using (var form = new FormNumeric(0, 15, 0))
                    {
                        var result = form.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            DGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = form.ReturnValue;
                        }
                    }
                    break;

                case 4:
                case 5:
                    // units/volt, offset
                    using (var form = new FormNumeric(0, 3000, 0))
                    {
                        var result = form.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            DGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = form.ReturnValue;
                        }
                    }
                    break;
            }
        }

        private void DGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (IsBlankRow(e.RowIndex))
            {
                if (e.ColumnIndex > 1)
                {
                    // suppress 0's
                    if (e.Value.ToString() == "0")
                    {
                        e.Value = "";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private void DGV_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!Initializing) SetButtons(true);
        }

        private void FormPressure_FormClosed(object sender, FormClosedEventArgs e)
        {
                mf.Tls.SaveFormData(this);
            timer1.Enabled = false;
        }
**************/
        private void FormPressure_Load(object sender, EventArgs e)
        {
            mf.Tls.LoadFormData(this);
            /**********TODO
            DGV.BackgroundColor = DGV.DefaultCellStyle.BackColor;
            DGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DGV.Columns[0].DefaultCellStyle.BackColor = Properties.Settings.Default.DayColour;
            DGV.Columns[6].DefaultCellStyle.BackColor = Properties.Settings.Default.DayColour;
            ***************/
            ckShowPressure.IsChecked = mf.ShowPressure;
            if (int.TryParse(mf.Tls.LoadProperty("PressureID"), out int tmp)) ShowID = tmp;

            UpdateForm();
      
			timer1.IsEnabled = true;
			timer1.Tick += timer1_Tick;
			timer1.Start();
        }

        private bool IsBlankRow(int row)
        {
           // string Des = DGV.Rows[row].Cells[1].Value.ToString();   // description
         //   string cal = DGV.Rows[row].Cells[5].Value.ToString();   // cal
         //   return (mf.Tls.StringToInt(cal) == 0 && Des == ""); TODO
            return true;
        }

        private void LoadGrid()
        {
           /****************TODO
            try
            {
                dataSet1.Clear();
                foreach (clsPressure Pres in mf.PressureObjects.Items)
                {
                    DataRow Rw = dataSet1.Tables[0].NewRow();
                    Rw["ID"] = Pres.ID + 1;
                    Rw["Description"] = Pres.Description;
                    Rw["ModuleID"] = Pres.ModuleID;
                    Rw["SensorID"] = Pres.SensorID;
                    Rw["UnitsPerVolt"] = Pres.UnitsVolts;
                    Rw["Pressure"] = Pres.Pressure();
                    Rw["Offset"] = Pres.Offset;

                    dataSet1.Tables[0].Rows.Add(Rw);
                }
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("FormPressure/LoadGrid: " + ex.Message);
            }
            **************/
        }

        private void SaveGrid()
        {
        /*****************
            try
            {
                for (int i = 0; i < DGV.Rows.Count; i++)
                {
                    for (int j = 1; j < 6; j++)
                    {
                        string val = DGV.Rows[i].Cells[j].EditedFormattedValue.ToString();
                        if (val == "") val = "0";
                        switch (j)
                        {
                            case 1:
                                // description
                                mf.PressureObjects.Item(i).Description = DGV.Rows[i].Cells[j].EditedFormattedValue.ToString();
                                break;

                            case 2:
                                // module ID
                                mf.PressureObjects.Item(i).ModuleID = mf.Tls.StringToInt(val);
                                break;

                            case 3:
                                // Sensor ID
                                mf.PressureObjects.Item(i).SensorID = mf.Tls.StringToInt(val);
                                break;

                            case 4:
                                // units/volts
                                mf.PressureObjects.Item(i).UnitsVolts = (float)Convert.ToDouble(val);
                                break;

                            case 5:
                                // offset
                                mf.PressureObjects.Item(i).Offset = (int)Convert.ToDouble(val);
                                break;
                        }
                    }
                }
                mf.PressureObjects.Save();
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("FormPressure/SaveGrid: " + ex.Message);
                mf.Tls.ShowHelp(ex.Message, "Pressure", 3000, true);
            } 
            *****************/
        }

        private void SetButtons(bool Edited)
        {
            if (!Initializing)
            {
                if (Edited)
                {
                    btnCancel.IsEnabled = true;
                    btnOK.Content = "Save"; //Properties.Resources.Save;
                }
                else
                {
                    btnCancel.IsEnabled = false;
                    btnOK.Content = "OK"; //Properties.Resources.OK;
                }

                FormEdited = Edited;
            }
        }

        private void SetDayMode()
        {
            if (Properties.Settings.Default.IsDay)
            {
                this.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243)); //Properties.Settings.Default.DayColour;

               // foreach (Control c in this.Controls)
             //   {
             //       c.ForeColor = Color.Black;
            //    }
            }
        }

        private async void tbPressureID_Enter(object sender, RoutedEventArgs e)
        {
            double tempD;
            double.TryParse(tbPressureID.Text, out tempD);
            var form = new NumericView(1, 16, tempD);
            
            var result = await form.ShowDialog<DialogResult>(this);
            if (result == DialogResult.OK)
            {
                tbPressureID.Text = form.ReturnValue.ToString();
            }
            
        }

        private void tbPressureID_TextChanged(object sender, TextInputEventArgs e)
        {
            SetButtons(true);
        }

        private void tbPressureID_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int tempInt;
            int.TryParse(tbPressureID.Text, out tempInt);
            if (tempInt < 1 || tempInt > 16)
            {
                System.Media.SystemSounds.Exclamation.Play();
                e.Cancel = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Initializing = true;

            LoadGrid();

            Initializing = false;
        }

        private void UpdateForm()
        {
            Initializing = true;

            LoadGrid();
            tbPressureID.Text = ShowID.ToString();

            Initializing = false;
        }
}
