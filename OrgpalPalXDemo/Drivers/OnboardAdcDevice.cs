using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Adc;

namespace PalX.Drivers
{
    public class OnboardAdcDevice : IDisposable
    {
        private bool _disposed;
        private AdcChannel adcVBAT;
        private AdcChannel adcMcuTemp;
        private AdcChannel adcPcbTemp;
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
            adcVBAT ??= adcController.OpenChannel(Pinout.AdcChannel.Channel_Vbatt);

            var average = 0;
            for (byte i = 0; i < samplesToTake; i++)
            {
                average += adcVBAT.ReadValue();

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

        /// <summary>
        /// Reads the board PCB temperature sensor value.
        /// </summary>
        /// <param name="celsius">Return celsius by default, otherwise f</param>
        /// <param name="samplesToTake">Number of samples to read for average</param>
        /// <returns>Temperature value.</returns>
        public double GetPcbTemperature(bool celsius = true)
        {
            adcController ??= new AdcController();
            adcPcbTemp ??= adcController.OpenChannel(Pinout.AdcChannel.Channel_PcbTemperatureSensor);

            var tempInCent = 0.0d;

            try
            {
                double adcTempCalcValue = (ANALOG_REF_VALUE * adcPcbTemp.ReadValue()) / MAX_ADC_VALUE;
                tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30f;
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetPcbTemperature failed!");
            }
            if (celsius)
            {
                return tempInCent;
            }
            else
            {
                return ((9f / 5f) * tempInCent) + 32f;
            }

        }

        public float GetMcuTemperature()
        {
            adcController ??= new AdcController();
            adcMcuTemp ??= adcController.OpenChannel(Pinout.AdcChannel.Channel_McuTemeratureSensor);
            return adcMcuTemp.ReadValue() / 100.00f;

            //https://www.st.com/resource/en/datasheet/stm32f769ni.pdf
            //https://electronics.stackexchange.com/questions/324321/reading-internal-temperature-sensor-stm32
            //const int ADC_TEMP_3V3_30C = 0x1FF0F44C //0x1FFF7A2C;
            //const int ADC_TEMP_3V3_110C = 0x1FF0 F44E //0x1FFF7A2E;
            //const float CALIBRATION_REFERENCE_VOLTAGE = 3.3F;
            //const float REFERENCE_VOLTAGE = 3.0F; // supplied with Vref+ or VDDA

            // scale constants to current reference voltage
            //float adcCalTemp30C = getRegisterValue(ADC_TEMP_3V3_30C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);
            //float adcCalTemp110C = getRegisterValue(ADC_TEMP_3V3_110C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);

            // return (adcTemp.ReadValue() - adcCalTemp30C)/(adcCalTemp110C - adcCalTemp30C) *(110.0F - 30.0F) + 30.0F);
        }

        //public float GetTemperatureFromThermistorNTC10K()
        //{
        //    Temperature = -99;

        //    //Vishay NTCALUG01A103F specs
        //    //B25/85-value  3435 to 4190 K 
        //    float NTC_A = 0.0011415995549162692f;
        //    float NTC_B = 0.000232134233116422f;
        //    float NTC_C = 9.520031026040015e-8f;

        //    if (adcController == null)
        //        adcController = new AdcController();

        //    if (ch == null)
        //        ch = adc.OpenChannel(PalThreePins.AdcChannel.ADC3_IN6_PF8_IO_PIN17);

        //    //calculate temperature from resistance
        //    float volts = 3.3f * ch.ReadValue() / 4095;
        //    volts = 3.3f * ch.ReadValue() / 4095;

        //    double thermistorResistance = volts * 10000f / (3.3f - volts);//3.3 V for thermistor, 10K resistor/thermistor
        //    double lnR = Math.Log(thermistorResistance);
        //    double Tk = 1 / (NTC_A + NTC_B * lnR + NTC_C * (lnR * lnR * lnR));
        //    Temperature = (float)Tk - 273.15f;

        //    Debug.WriteLine($"T:{Temperature,0:F1}C {TemperatureF,0:F1}F");

        //    return Temperature;
        //}


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

            adcVBAT.Dispose();
            adcPcbTemp.Dispose();
            adcMcuTemp.Dispose();
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
