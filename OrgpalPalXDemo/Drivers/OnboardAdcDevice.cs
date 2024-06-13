using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Adc;

namespace PalX.Drivers
{
    public class OnboardAdcDevice : IDisposable
    {
        private bool _disposed;
        private AdcChannel adcVBatteryChannel;
        private AdcChannel adcMcuTempChannel;
        private AdcChannel adcPcbTempChannel;
        private AdcChannel thermistorChannel;
        private AdcController adcController = new();
        //private AdcChannel adc420mA;

        // ADC constants
        private const int ANALOG_REF_VALUE = 3300;
        private const int MAX_ADC_VALUE = 4095;

        /// <summary>
        /// Read the power supply input voltage.
        /// </summary>
        /// <remarks>
        /// This should be 9-24VDC
        /// </remarks>
        /// <param name="samplesToTake">Number of samples to read for an average</param>
        /// <returns>The voltage as VDC.</returns>
        public double GetUnregulatedInputVoltage(byte samplesToTake = 5)
        {
            var voltage = 0f;

            adcController ??= new AdcController();
            adcVBatteryChannel ??= adcController.OpenChannel(Pinout.AdcChannel.Channel6_BatteryVoltage);

            var average = 0;
            for (byte i = 0; i < samplesToTake; i++)
            {
                average += adcVBatteryChannel.ReadValue();

                Thread.Sleep(50); // pause to stabilize
            }

            try
            {
                average /= samplesToTake;

                //VBat = 0.25 x VIN adc count
                //float voltage = ((ANALOG_REF_VALUE * average) / MAX_ADC_VALUE)* 4;

                voltage = ((ANALOG_REF_VALUE * average) / MAX_ADC_VALUE) * 0.004f;

                voltage += 0.25f; // small offset calibration factor for board to even drop on measure
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetUnregulatedInputVoltage failed!");
            }

            return voltage;
        }

        ///// <summary>
        ///// Reads the board PCB temperature sensor value.
        ///// </summary>
        ///// <param name="celsius">Return celsius by default, otherwise f</param>
        ///// <param name="samplesToTake">Number of samples to read for average</param>
        ///// <returns>Temperature value.</returns>
        //public double GetPcbTemperature(bool celsius = true)
        //{
        //    adcController ??= new AdcController();
        //    adcPcbTempChannel ??= adcController.OpenChannel(Pinout.AdcChannel.ADC1_IN13_TEMP);

        //    var tempInCent = 0.0d;

        //    try
        //    {
        //        var adcValue = 0;
        //        for (byte i = 0;i<10;i++)
        //        {
        //            adcValue = adcPcbTempChannel.ReadValue();
        //            Thread.Sleep(5);//pause to stabilize
        //        }
        //        double adcTempCalcValue = (ANALOG_REF_VALUE * adcValue) / MAX_ADC_VALUE;
        //        tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30f;
        //    }
        //    catch
        //    {
        //        Debug.WriteLine("OnboardAdcDevice: GetPcbTemperature failed!");
        //    }

        //    return tempInCent;


        //}

        /// <summary>
        /// Reads the board PCB temperature sensor value.
        /// </summary>
        /// <param name="celsius">Return celsius by default, otherwise f</param>
        /// <param name="samplesToTake">Number of samples to read for average</param>
        /// <returns>Temperature value.</returns>
        public double GetPcbTemperature(bool celsius = true)
        {
            adcController ??= new AdcController();
            adcPcbTempChannel ??= adcController.OpenChannel(Pinout.AdcChannel.Channel3_PcbTemperatureSensor);
            //var tempInCent = 0.0;
            return adcPcbTempChannel.ReadValue() / 100.00;

            //try
            //{
            //    double adcTempCalcValue = ANALOG_REF_VALUE * adcPcbTempChannel.ReadValue() / MAX_ADC_VALUE;
            //    tempInCent = ((13.582 - Math.Sqrt(184.470724 + (0.01732 * (2230.8 - adcTempCalcValue)))) / (-0.00866)) + 30;
            //}
            //catch
            //{
            //    Debug.WriteLine("OnboardAdcDevice: GetPcbTemperature failed!");
            //}
            //if (celsius)
            //{
            //    return tempInCent;
            //}
            //else
            //{
            //    return ((9 / 5) * tempInCent) + 32;
            //}

        }

        public float GetMcuTemperature()
        {
            adcController ??= new AdcController();
            adcMcuTempChannel ??= adcController.OpenChannel(Pinout.AdcChannel.Channel4_McuTemperatureSensor);
            return adcMcuTempChannel.ReadValue() / 100.00f;

            //https://www.st.com/resource/en/datasheet/stm32f769ni.pdf
            //https://electronics.stackexchange.com/questions/324321/reading-internal-temperature-sensor-stm32
            //const int ADC_TEMP_3V3_30C = 0x1FF0F44C //0x1FFF7A2C;
            //const int ADC_TEMP_3V3_110C = 0x1FF0 F44E //0x1FFF7A2E;
            //const float CALIBRATION_REFERENCE_VOLTAGE = 3.3F;
            //const float REFERENCE_VOLTAGE = 3.0F; // supplied with Vref+ or VDDA

            // scale constants to current reference voltage
            //float adcCalTemp30C = getRegisterValue(ADC_TEMP_3V3_30C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);
            //float adcCalTemp110C = getRegisterValue(ADC_TEMP_3V3_110C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);

            // return (adcMcuTempChannel.ReadValue() - adcCalTemp30C)/(adcCalTemp110C - adcCalTemp30C) *(110.0F - 30.0F) + 30.0F);
        }

        public double GetTemperatureFromThermistorNTC10K()
        {
            //Vishay NTCALUG01A103F specs
            //B25/85-value  3435 to 4190 K 
            const double NTC_A = 0.0011415995549162692;
            const double NTC_B = 0.000232134233116422;
            const double NTC_C = 9.520031026040015e-8;
            const double V_REF = 3.3;
            const double R_REF = 10_000;

            adcController ??= new AdcController();

            thermistorChannel ??= adcController.OpenChannel(Pinout.AdcChannel.Channel0_ThermistorInput); // FIXME: this is incorrect, likely ADC1_IN2_P2

            //calculate temperature from resistance

            var channelReadVolts = thermistorChannel.ReadValue();
            var volts = V_REF * channelReadVolts / MAX_ADC_VALUE;

            var thermistorResistance = volts * R_REF;// / (V_REF - volts); //3.3 V for thermistor, 10K resistor/thermistor
            var lnR = Math.Log(thermistorResistance);
            var Tk = 1 / (NTC_A + NTC_B * lnR + NTC_C * (lnR * lnR * lnR));
            var tempC = Tk - 273.15;

            Debug.WriteLine($"T:{channelReadVolts,0:F1}V");
            Debug.WriteLine($"T:{tempC,0:F1}C");

            return tempC;
        }


        //public float Read420mAValue()
        //{
        //    adcController ??= new AdcController();

        //    adc420mA ??= adcController.OpenChannel(Pinout.AdcChannel.ADC1_IN12_420MA);

        //    //get 4-20 output from probe
        //    var val420 = PalHelper.Get4to20MaValue(adcController, 10, adc420mA);

        //    if (val420 < 4)
        //        val420 = 4;//default to minimal value
        //    else if (val420 > 20)
        //        val420 = 20;


        //    return val420;
        //}

        //public float Get4to20MaValue(AdcController adc = null, int samplesToTake = 10, AdcChannel adc420mA = null)
        //{
        //    adcController ??= new AdcController();

        //    adc420mA ??= adcController.OpenChannel(Pinout.AdcChannel.ADC1_IN12_420MA);

        //    int average = 0;
        //    for (byte i = 0; i < samplesToTake; i++)
        //    {
        //        average += adc420mA.ReadValue();
        //        Thread.Sleep(5);//pauze to stabilize
        //    }
        //    average = average / samplesToTake;

        //    float voltFactor = 0.008056640625f;// (3.3 / 4096) * 10 for 4 - 20 showing;
        //    float miliAmpsRead = average * voltFactor;

        //    //truncate all but the 2 decimals after the point
        //    miliAmpsRead = miliAmpsRead - (miliAmpsRead % 0.01f);

        //    if (miliAmpsRead < 3.98)
        //        miliAmpsRead = 0;
        //    else if (miliAmpsRead > 20)
        //        miliAmpsRead = 20;

        //    return miliAmpsRead;
        //}


        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            adcVBatteryChannel.Dispose();
            adcPcbTempChannel.Dispose();
            adcMcuTempChannel.Dispose();
            thermistorChannel.Dispose();
            adcController = null;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
