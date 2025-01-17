﻿using System;
using System.Collections.Generic;

namespace RateController.Classes
{
    public class clsProducts
    {
        public IList<clsProduct> Items; // access records by index
        private List<clsProduct> cProducts = new List<clsProduct>();
        private DateTime LastSave;
        private MainWindow mf;

        public clsProducts(MainWindow CallingForm)
        {
            mf = CallingForm;
            Items = cProducts.AsReadOnly();
        }

        public bool AlarmOn()
        {
            double AlarmSetPoint;
            bool cAlarmOn = false;

            for (int i = 0; i < mf.MaxProducts; i++)
            {
                if ((cProducts[i].WorkRate() > 0) && (cProducts[i].UseOffRateAlarm))
                {
                    // too low?
                    AlarmSetPoint = (100 - cProducts[i].OffRateSetting) / 100.0;
                    if (cProducts[i].SmoothRate() < (cProducts[i].TargetRate() * AlarmSetPoint))
                    {
                        cAlarmOn = true;
                        break;
                    }

                    // too high?
                    AlarmSetPoint = (100 + cProducts[i].OffRateSetting) / 100.0;
                    if (cProducts[i].SmoothRate() > (cProducts[i].TargetRate() * AlarmSetPoint))
                    {
                        cAlarmOn = true;
                        break;
                    }
                }
            }
            return cAlarmOn;
        }

        public bool Connected()
        {
            bool Result = false;
            try
            {
                if (cProducts.Count > 0)
                {
                    // returns true if at least one module is connected
                    for (int i = 0; i < mf.MaxProducts; i++)
                    {
                        if (cProducts[i].ArduinoModule.Connected())
                        {
                            Result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mf.Tls.WriteErrorLog("clsProducts/Connected: " + ex.Message);
            }
            return Result;
        }

        public int Count()
        {
            return cProducts.Count;
        }

        public clsProduct Item(int ProdID)  // access records by Product ID
        {
            int IDX = ListID(ProdID);
            if (IDX == -1) throw new ArgumentOutOfRangeException();
            return cProducts[IDX];
        }

       public void Load(bool Reset=false)
        {
            cProducts.Clear();
            for (int i = 0; i < mf.MaxProducts; i++)
            {
                clsProduct Prod = new clsProduct(mf, i);
                cProducts.Add(Prod);
                Prod.Load();
            }

            for (int i = 0; i < mf.MaxProducts; i++)
            {
                clsProduct Prd = cProducts[i];
                if (Prd.IsNew()||Reset)
                {
                    Prd.ProductName = "P" + (i + 1).ToString();
                    Prd.ControlType = ControlTypeEnum.Valve;
                    Prd.QuantityDescription = "Gallons";
                    Prd.CoverageUnits = 0;
                    Prd.MeterCal = 1;
                    Prd.EnableProdDensity = false;
                    Prd.ProdDensity = 0;
                    Prd.RateSet = 1;
                    Prd.RateAlt = 100;
                    Prd.TankSize = 1000;
                    Prd.TankStart = 1000;
                    Prd.UseVR = false;
                    Prd.VRID = 0;
                    Prd.VRmax = 100;
                    Prd.VRmin = 0;
                    Prd.PIDkp = 1;
                    Prd.PIDki = 0;
                    Prd.PIDkd = 0;
                    Prd.PIDmax = 100;
                    Prd.PIDmin = 0;
                    Prd.PIDscale = 0;
                    Prd.ChangeID(i / 2, (byte)(i % 2), true);
                    Prd.UseMultiPulse = true;
                    Prd.OnScreen = true;
                    Prd.ConstantUPM = false;
                    Prd.OffRateSetting = 0;
                    Prd.MinUPM = 0;
                    Prd.BumpButtons = false;
                    Prd.CountsRev = 1;
                    Prd.Save();

                    mf.DefaultProduct = 0;
                }
            }
        }


        public void Save(int ProdID = 0)
        {
            if (ProdID == 0)
            {
                // save all
                for (int i = 0; i < cProducts.Count; i++)
                {
                    cProducts[i].Save();
                }
            }
            else
            {
                // save selected
                cProducts[ListID(ProdID)].Save();
            }
        }

        public bool UniqueModSen(int ModID, int SenID, int ProdID)
        {
            // checks if product module ID/sensor ID pair are unique
            bool Result = true;
            for (int i = 0; i < Count(); i++)
            {
                if ((cProducts[i].ID != ProdID) && (cProducts[i].ModuleID == ModID && cProducts[i].SensorID == SenID))
                {
                    Result = false;
                    break;
                }
            }
            return Result;
        }

        public void Update()
        {
            for (int i = 0; i < mf.MaxProducts; i++)
            {
                cProducts[i].Update();
            }

            if ((DateTime.Now - LastSave).TotalSeconds > 60)
            {
                for (int i = 0; i < mf.MaxProducts; i++)
                {
                    cProducts[i].Save();
                }
                LastSave = DateTime.Now;
            }
        }

        public void UpdatePID()
        {
            for (int i = 0; i < cProducts.Count; i++)
            {
                if (cProducts[i].ArduinoModule.Connected()) cProducts[i].SendPID();
            }
        }

        private int ListID(int ProdID)
        {
            int Result = -1;
            for (int i = 0; i < cProducts.Count; i++)
            {
                if (cProducts[i].ID == ProdID)
                {
                    Result = i;
                    break;
                }
            }
            return Result;
        }
    }
}
