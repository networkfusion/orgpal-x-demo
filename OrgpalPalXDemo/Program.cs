using System;
using System.Device.I2c;
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

            var device = new SSD1x06();

            device.ClearScreen();
            //device.Font = new BasicFont();
            device.DrawString(2, 2, "nF IOT!", 2, true);//large size 2 font
            device.DrawString(2, 32, "nanoFramework", 1, true);//centered text
            device.Display();

            Thread.Sleep(2000);
            device.ClearScreen();
            device.DrawString(2, 2, "Orgpal", 2, true);
            device.DrawString(2, 34, "PalX", 1, true);
            device.Display();

            for (; ; )
            {
                Thread.Sleep(10_000);
                var sysTemp = OnboardDevices.GetSysTemperature();
                device.ClearScreen();
                device.DrawString(2, 2, "Temperatures:", 1, false);
                device.DrawString(2, 24, $"system: {sysTemp}", 1, false);
                device.DrawString(2, 34, "themistor:", 1, false);
                device.Display();
            }

            //Thread.Sleep(Timeout.Infinite);

        }
    }
}
