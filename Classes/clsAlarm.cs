using Avalonia.Media;
using Avalonia.Controls;
//using System.Windows.Forms;

namespace RateController.Classes
{
    public class clsAlarm
    {
        private bool AlarmColour;
        private double AlarmDelay;
        private Button cAlarmButton;
        private bool cShowAlarm;
        private bool cSilenceAlarm;
        private MainWindow mf;
        private System.Media.SoundPlayer Sounds;

        public clsAlarm(MainWindow CallingFrom, Button AlarmButton)
        {
            mf = CallingFrom;
            cAlarmButton = AlarmButton;
            System.IO.Stream Str = Properties.Resources.Loud_Alarm_Clock_Buzzer_Muk1984_493547174;
           // Sounds = new System.Media.SoundPlayer(Str); 
        }

        public void CheckAlarms()
        {
        /*********
            bool cRateAlarm = mf.Products.AlarmOn();
            bool cPressureAlarm = mf.PressureObjects.AlarmOn();
            string cMessage;

            if (cRateAlarm || cPressureAlarm)
            {
                cMessage = "Alarm";
                if (cPressureAlarm) cMessage = "Pressure  " + cMessage;
                if (cRateAlarm) cMessage = "Rate  " + cMessage;
                cAlarmButton.Content = cMessage;

                if (cSilenceAlarm)
                {
                    Sounds.Stop();
                }
                else
                {
                    AlarmDelay++;
                    if (AlarmDelay > 5)
                    {
                        Sounds.Play();
                        cShowAlarm = true;
                    }
                }

                cAlarmButton.IsVisible = cShowAlarm;
                //cAlarmButton.BringToFront(); TODO

                AlarmColour = !AlarmColour;
                if (AlarmColour)
                {
                    cAlarmButton.Background = Brushes.Red;
                }
                else
                {
                    cAlarmButton.Background = Brushes.Yellow;
                }
            }
            else
            {
                AlarmDelay = 0;
                Sounds.Stop();
                cSilenceAlarm = false;
                cShowAlarm = false;
                cAlarmButton.IsVisible = false;
            }
            ****/
        }

        public void Silence()
        { cSilenceAlarm = true; }
    }
}
