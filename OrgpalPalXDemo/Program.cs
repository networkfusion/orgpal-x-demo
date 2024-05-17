using System;
using System.Diagnostics;
using System.Threading;
using PalX.Drivers;

namespace OrgpalPalXDemoApp
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Sounds.PlayDefaultSound();

            // FIXME: connect to wifi.
            // FIXME: get geolocation.

            using var display = new SSD1x06();
            using var internalAdc = new OnboardAdcDevice();

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

            for (int i = 0; i < 10; i++)
            {
                
                var mcuTemp = internalAdc.GetMcuTemperature();
                var sysTemp = internalAdc.GetPcbTemperature();
                var thermistorTemp = double.NaN; //internalAdc.GetTemperatureFromThermistorNTC10K();
                display.ClearScreen();
                display.DrawString(2, 2, $"TEMPERATURES", 1, false);
                display.DrawString(2, 14, $"mcu: {mcuTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 24, $"pcb: {sysTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 34, $"themistor: {thermistorTemp.ToString("F2")}'C", 1, false);
                display.DrawString(2, 54, $"Time: {DateTime.UtcNow.ToString("o")}", 1, false);
                display.Display();
                Thread.Sleep(10_000);
            }

            // only debugging so for the moment turn the backlight off, so we dont burn it out.
            display.ClearScreen();
            display.BacklightOn = false; // FIXME: I dont think this actually does anything.
            GoToSleep();
            Thread.Sleep(Timeout.Infinite);

        }

        static void GoToSleep()
        {
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
