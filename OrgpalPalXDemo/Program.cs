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

            using var display = new SSD1x06();
            using var internalAdc = new OnboardAdcDevice();

            display.ClearScreen();
            //device.Font = new BasicFont();
            display.DrawString(2, 2, "nF IOT!", 2, true);//large size 2 font
            display.DrawString(2, 32, "nanoFramework", 1, true);//centered text
            display.Display();

            Thread.Sleep(2000);
            display.ClearScreen();
            display.DrawString(2, 2, "Orgpal", 2, true);
            display.DrawString(2, 34, "PalX", 1, true);
            display.Display();

            for (; ; )
            {
                Thread.Sleep(10_000);
                var mcuTemp = internalAdc.GetMcuTemperature();
                var sysTemp = internalAdc.GetPcbTemperature();
                var thermistorTemp = double.NaN; //OnboardDevices.GetTemperatureFromThermistorNTC10K();
                display.ClearScreen();
                display.DrawString(2, 2, $"TEMPERATURES", 1, false);
                display.DrawString(2, 14, $"mcu: {mcuTemp}", 1, false);
                display.DrawString(2, 24, $"pcb: {sysTemp}", 1, false);
                display.DrawString(2, 34, $"themistor: {thermistorTemp}", 1, false);
                display.DrawString(2, 54, $"Time: {DateTime.UtcNow.ToString("o")}", 1, false);
                display.Display();
            }

            //Thread.Sleep(Timeout.Infinite);

        }
    }
}
