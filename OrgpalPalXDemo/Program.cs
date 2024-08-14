using System;
using System.Diagnostics;
using System.Threading;
using PalX.Drivers;

namespace OrgpalPalXDemoApp
{
    public class Program
    {
        private static readonly SSD1x06 display = new();
        private static readonly OnboardAdcDevice internalAdc = new();
        private static BatteryManagementSystem bms = new();

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Sounds.PlayDefaultSound();
            ShowSplashScreen();
            

            // FIXME: connect to wifi.
            // FIXME: get geolocation.

            //using var meshlink = new EasylinkMeshNode();



            for (int i = 0; i < 100; i++)
            {
                // Cycle displays
                ShowTemperatureScreen();
                Thread.Sleep(10_000);

                ShowVoltageScreen();
                Thread.Sleep(10_000);
            }


            GoToSleep();
            Thread.Sleep(Timeout.Infinite);

        }

        private static void ShowTemperatureScreen()
        {
            try
            {
                var mcuTemp = internalAdc.GetMcuTemperature();
                var sysTemp = internalAdc.GetPcbTemperature();
                var thermistorTemp = internalAdc.GetTemperatureFromThermistorNTC10K();
                display.ClearScreen();
                display.DrawString(2, 2, $"TEMPERATURES", 1, false);
                display.DrawString(2, 14, $"mcu: {mcuTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 24, $"pcb: {sysTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 34, $"themistor: {thermistorTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 54, $"Time: {DateTime.UtcNow.ToString("o")}", 1, false);
                display.Display();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to display temperatures! {ex}");
            }


        }


        private static void ShowVoltageScreen()
        {
            try
            {
                var unregVolts = internalAdc.GetUnregulatedInputVoltage();
                var sysInVolts = bms.charger.Vsys; //internalAdc.GetBatteryInputVoltage();
                var batteryVolts = bms.charger.Vbat; //internalAdc.GetBatteryVoltage();
                display.ClearScreen();
                display.DrawString(2, 2, $"VOLTAGES", 1, false);
                display.DrawString(2, 14, $"unreg: {unregVolts.ToString("F2")}VDC", 1, false);
                display.DrawString(2, 24, $"sys: {sysInVolts.VoltsDc}V", 1, false);
                display.DrawString(2, 34, $"batt: {batteryVolts.VoltsDc}V", 1, false);
                display.DrawString(2, 54, $"Time: {DateTime.UtcNow.ToString("o")}", 1, false);
                display.Display();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to display voltages! {ex}");
            }

        }


        static void ShowSplashScreen()
        {
            display.ClearScreen();
            //device.Font = new BasicFont();
            display.DrawString(2, 2, "IoT Demo", 2, true);//large size 2 font
            display.DrawString(2, 32, "nanoFramework", 1, true);//centered text
            display.Display();

            Thread.Sleep(1000);
            display.ClearScreen();
            display.DrawString(2, 2, "PalX", 2, true);
            display.DrawString(2, 34, "OrgPal.com", 1, true);
            display.Display();
            Thread.Sleep(1000);
        }

        static void GoToSleep()
        {
            // only debugging so for the moment turn the backlight off, so we dont burn it out.
            display.ClearScreen();
            display.BacklightOn = false; // FIXME: I dont think this actually does anything.

            //Trace($"Full operation took: {DateTime.UtcNow - allupOperation}");
            //Trace($"Set wakeup by timer for {minutesToGoToSleep} minutes to retry.");
            //Sleep.EnableWakeupByTimer(new TimeSpan(0, 0, minutesToGoToSleep, 0));
            Trace("FIXME: Deep sleep now with interupt on button");
            //Sleep.StartDeepSleep();
        }

        static void Trace(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
