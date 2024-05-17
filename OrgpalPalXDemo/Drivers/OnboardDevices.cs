using System;
using System.Device.Adc;
using System.Diagnostics;
using System.Threading;

namespace PalX.Drivers
{
    internal static class OnboardDevices
    {
        //public static float GetSysTemperature()
        //{
        //    AdcController adc1 = new AdcController();
        //    var adcTemp = adc1.OpenChannel(Pinout.AdcChannel.ADC1_IN13_TEMP);

        //    double tempInCent = 0;

        //    try
        //    {
        //        int averageADC = 0;
        //        for (byte i = 0; i < 10; i++)
        //        {
        //            averageADC += adcTemp.ReadValue();
        //            Thread.Sleep(5);//pauze to stabilize
        //        }
        //        averageADC = averageADC / 10;

        //        //LMT87LP sensor
        //        //maximumValue = 4095; analogReference = 3300;
        //        double adcTempCalcValue = (3300 * averageADC) / 4095;
        //        tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30;
        //        // float tempInF = ((9f / 5f) * tempInCent) + 32f;
        //    }
        //    catch { }

        //    return (float)tempInCent;
        //}


        //public float GetTemperatureFromThermistorNTC10K()
        //{
        //    Temperature = -99;

        //    //Vishay NTCALUG01A103F specs
        //    //B25/85-value  3435 to 4190 K 
        //    float NTC_A = 0.0011415995549162692f;
        //    float NTC_B = 0.000232134233116422f;
        //    float NTC_C = 9.520031026040015e-8f;

        //    if (adc == null)
        //        adc = new AdcController();

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
        //    if (adc == null)
        //        adc = new AdcController();

        //    if (adc420mA == null)
        //        adc420mA = adc.OpenChannel(PalThreePins.AdcChannel.ADC1_IN12_420MA);

        //    //get 4-20 output from probe
        //    var val420 = PalHelper.Get4to20MaValue(adc, 10, adc420mA);

        //    if (val420 < 4)
        //        val420 = 4;//default to minimal value
        //    else if (val420 > 20)
        //        val420 = 20;


        //    return val420;
        //}
        //public static float Get4to20MaValue(AdcController adc = null, int samplesToTake = 10, AdcChannel adc420mA = null)
        //{
        //    if (adc420mA == null && adc == null)//only create if needed
        //        adc = new AdcController();

        //    if (adc420mA == null)
        //        adc420mA = adc.OpenChannel(PalThreePins.AdcChannel.ADC1_IN12_420MA);

        //    int average = 0;
        //    for (byte i = 0; i < samplesToTake; i++)
        //    {
        //        average += adc420mA.ReadValue();
        //        Thread.Sleep(5);//pauze to stabilize
        //    }
        //    average = average / samplesToTake;

        //    float voltFactor = 0.008056640625f;// (3.3 / 4096) * 10 for 4 - 20 showing;
        //    float miliAmpsRead = average * voltFactor;

        //    //trunkate all but the 2 decimals after the point
        //    miliAmpsRead = miliAmpsRead - (miliAmpsRead % 0.01f);

        //    if (miliAmpsRead < 3.98)
        //        miliAmpsRead = 0;
        //    else if (miliAmpsRead > 20)
        //        miliAmpsRead = 20;

        //    return miliAmpsRead;
        //}
    }
}
