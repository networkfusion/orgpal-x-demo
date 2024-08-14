using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Bq2579x;

namespace PalX.Drivers
{
    public class BatteryManagementSystem
    {
        public Bq2579x charger;

        public BatteryManagementSystem()
        {

            //using var controller = new GpioController();
            // var chargMode = PalHelper.GpioPort(PalThreePins.GpioPin.PALX_CHARGER_MODE_SEL, PinMode.Output, PinValue.Low);
            //var chargeMode = controller.Read(Pinout.GpioPin.PALX_CHARGER_MODE_SEL);
            //controller.Write(Pinout.GpioPin.PALX_CHARGER_MODE_SEL, PinValue.Low);
            // var chargeEnable = PalHelper.GpioPort(Pinout.GpioPin.PALX_CHARGER_ENABLE, PinMode.Output, PinValue.Low);
            //var chargeEnable = controller.Read(Pinout.GpioPin.PALX_CHARGER_ENABLE);
            //controller.Write(Pinout.GpioPin.PALX_CHARGER_ENABLE, PinValue.Low);

            I2cConnectionSettings settings = new(Pinout.I2CBus.I2C3, Bq2579x.DefaultI2cAddress);
            charger = new(I2cDevice.Create(settings))
            {
                // disabling I2C watchog for simplicity
                WatchdogTimerSetting = WatchdogSetting.Disable,
                AdcEnable = true,

                //suspend charge at high temps - above 45 C
                ChargeVoltageHighTempRange = ChargeVoltage.Vreg400mV,
                ChargeCurrentHighTempRange = ChargeCurrent.Ichg40


                //leave low temp charge for now on - b at 0 C
                //ChargeCurrentLowTempRange = ChargeCurrent.ChargeSuspend;

                //XXX not working but see if could be used when no battery present to output 12V
                //MinimalSystemVoltage = new ElectricPotentialDc(12000, ElectricPotentialDcUnit.MillivoltDc);

            };

            //check settings to make sure are all good
            Debug.WriteLine("After change");
            Debug.WriteLine($"CV at High Temp: {charger.ChargeVoltageHighTempRange:N3}");
            Debug.WriteLine($"CC at High Temp: {charger.ChargeCurrentHighTempRange:N3}");
            Debug.WriteLine($"CC at Low Temp: {charger.ChargeCurrentLowTempRange:N3}");
            Debug.WriteLine($"Minimum System Voltage is config @ {charger.MinimalSystemVoltage.VoltsDc:N3}V");

            Debug.WriteLine($"Die Temp: {charger.DieTemperature.DegreesCelsius:N1}°C");


        }
    }
}
