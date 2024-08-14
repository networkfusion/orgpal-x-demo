
using System.Device.I2c;
using System;

namespace PalX.Drivers
{
    /// <summary>
    /// RPR_0521RS Optical Sensor
    /// </summary>
    /// <remarks>
    /// manual: https://fscdn.rohm.com/en/products/databook/datasheet/opto/optical_sensor/opto_module/rpr-0521rs-e.pdf
    /// </remarks>
    public class RPR_0521RS : IDisposable
    {

        public I2cDevice i2cDevice;


        // Commands
        const byte SYS_CTRL = 0x40;
        const byte MODE_CTRL = 0x41;
        const byte ALS_PS_CTRL = 0x42;
        const byte PS_CTRL = 0x43;
        const byte PS_DATA_LSB = 0x44;
        const byte PS_DATA_MSB = 0x45;
        const byte ALS_DATA0_LSB = 0x46;
        const byte ALS_DATA0_MSB = 0x47;
        const byte ALS_DATA1_LSB = 0x48;
        const byte ALS_DATA1_MSB = 0x49;
        const byte INTERRUPT = 0x4A;
        const byte PS_TH_LSB = 0x4B;
        const byte PS_TH_MSB = 0x4C;
        const byte PS_TL_LSB = 0x4D;
        const byte PS_TL_MSB = 0x4E;
        const byte ALS_DATA0_TH_LSB = 0x4F;
        const byte ALS_DATA0_TH_MSB = 0x50;
        const byte ALS_DATA0_TL_LSB = 0x51;
        const byte ALS_DATA0_TL_MSB = 0x52;
        const byte PS_OFFSET_LSB = 0x53;
        const byte PS_OFFSET_MSB = 0x54;
        const byte REG_MANUFACT_ID = 0x92;


        //RPR_0521RS settings

        //RPR_0521RS_REG_MODE_CONTROL                                         MSB   LSB   DESCRIPTION
        const byte RPR_0521RS_ALS_OFF = 0b00000000;                   //  7     7     ambient light sensor (ALS) on standby
        const byte RPR_0521RS_ALS_ON = 0b10000000;                    //  7     7     ambient light sensor (ALS) enabled
        const byte RPR_0521RS_PS_OFF = 0b00000000;                    //  6     6     proximity sensor (PS) on standby
        const byte RPR_0521RS_PS_ON = 0b01000000;                     //  6     6     proximity sensor (PS) enabled
        const byte RPR_0521RS_PS_PULSE_200_US = 0b00000000;           //  5     5     PS pulse width: 200 us (typical)
        const byte RPR_0521RS_PS_PULSE_330_US = 0b00100000;           //  5     5                     330 us (typical)
        const byte RPR_0521RS_PS_NORMAL = 0b00000000;                 //  4     4     normal PS operating mode
        const byte RPR_0521RS_PS_DOUBLE = 0b00010000;                 //  4     4     double measurement PS operating mode
        const byte RPR_0521RS_MEAS_TIME_STBY_STBY = 0b00000000;       //  3     0     measurement time: ALS standby,  PS standby
        const byte RPR_0521RS_MEAS_TIME_STBY_10_MS = 0b00000001;      //  3     0                       ALS standby,  PS 10 ms
        const byte RPR_0521RS_MEAS_TIME_STBY_40_MS = 0b00000010;      //  3     0                       ALS standby,  PS 40 ms
        const byte RPR_0521RS_MEAS_TIME_STBY_100_MS = 0b00000011;     //  3     0                       ALS standby,  PS 100 ms
        const byte RPR_0521RS_MEAS_TIME_STBY_400_MS = 0b00000100;     //  3     0                       ALS standby,  PS 400 ms
        const byte RPR_0521RS_MEAS_TIME_100_MS_50_MS = 0b00000101;    //  3     0                       ALS 100 ms,   PS 50 ms
        const byte RPR_0521RS_MEAS_TIME_100_MS_100_MS = 0b00000110;   //  3     0                       ALS 100 ms,   PS 100 ms
        const byte RPR_0521RS_MEAS_TIME_100_MS_400_MS = 0b00000111;   //  3     0                       ALS 100 ms,   PS 400 ms
        const byte RPR_0521RS_MEAS_TIME_400_MS_50_MS = 0b00001000;    //  3     0                       ALS 400 ms,   PS 50 ms
        const byte RPR_0521RS_MEAS_TIME_400_MS_100_MS = 0b00001001;   //  3     0                       ALS 400 ms,   PS 100 ms
        const byte RPR_0521RS_MEAS_TIME_400_MS_STBY = 0b00001010;     //  3     0                       ALS 400 ms,   PS standby
        const byte RPR_0521RS_MEAS_TIME_400_MS_400_MS = 0b00001011;   //  3     0                       ALS 400 ms,   PS 400 ms
        const byte RPR_0521RS_MEAS_TIME_50_MS_50_MS = 0b00001100;     //  3     0                       ALS 50 ms,    PS 50 ms

        //RPR_0521RS_REG_ALS_PS_CONTROL
        const byte RPR_0521RS_ALS_DATA0_GAIN_1 = 0b00000000;          //  5     4     ALS DATA0 gain: x1
        const byte RPR_0521RS_ALS_DATA0_GAIN_2 = 0b00010000;          //  5     4                     x2
        const byte RPR_0521RS_ALS_DATA0_GAIN_64 = 0b00100000;         //  5     4                     x64
        const byte RPR_0521RS_ALS_DATA0_GAIN_128 = 0b00110000;        //  5     4                     x128
        const byte RPR_0521RS_ALS_DATA1_GAIN_1 = 0b00000000;          //  3     2     ALS DATA1 gain: x1
        const byte RPR_0521RS_ALS_DATA1_GAIN_2 = 0b00000100;          //  3     2                     x2
        const byte RPR_0521RS_ALS_DATA1_GAIN_64 = 0b00001000;         //  3     2                     x64
        const byte RPR_0521RS_ALS_DATA1_GAIN_128 = 0b00001100;        //  3     2                     x128
        const byte RPR_0521RS_LED_CURRENT_25_MA = 0b00000000;         //  1     0     LED current:  25 mA
        const byte RPR_0521RS_LED_CURRENT_50_MA = 0b00000001;         //  1     0                   50 mA
        const byte RPR_0521RS_LED_CURRENT_100_MA = 0b00000010;        //  1     0                   100 mA
        const byte RPR_0521RS_LED_CURRENT_200_MA = 0b00000011;        //  1     0                   200 mA

        //RPR_0521RS_REG_PS_CONTROL
        const byte RPR_0521RS_PS_GAIN_1 = 0b00000000;                 //  5     4     PS gain:  x1
        const byte RPR_0521RS_PS_GAIN_2 = 0b00010000;                 //  5     4               x2
        const byte RPR_0521RS_PS_GAIN_4 = 0b00100000;                 //  5     4               x4


        const byte RPR_0521RS_PART_ID = 0x0A;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const byte DefaultI2cAddress = 0x38;


        /// <summary>
        /// Device ID
        /// </summary>
        public byte DeviceManufactId { get { return 0xE0; } }

        /// <summary>
        /// The proximity value read from the sensor.
        /// </summary>
        public short ProximityValue { get; set; }

        /// <summary>
        /// Ambient value in Lx
        /// </summary>
        public float AmbientLightValue { get; set; }


        public RPR_0521RS(int I2CId = Pinout.I2CBus.I2C3, int address = DefaultI2cAddress)
        {
            i2cDevice = I2cDevice.Create(new I2cConnectionSettings(I2CId, address, I2cBusSpeed.StandardMode));
        }


        public byte GetId()
        {
            //see if device is present if we can read Id of device, 0x26
            return ReadRegister(REG_MANUFACT_ID);
        }

        public void InitializeDefaultConfig()
        {

            byte aux_reg_val = ReadRegister(SYS_CTRL);

            //check valid part #
            if ((aux_reg_val & 0x3F) != RPR_0521RS_PART_ID)
                return;

            //initialize device per datasheet
            WriteRegister(ALS_PS_CTRL, RPR_0521RS_ALS_DATA0_GAIN_2 | RPR_0521RS_ALS_DATA1_GAIN_2 | RPR_0521RS_LED_CURRENT_100_MA);
            SetRegisterBits(PS_CTRL, RPR_0521RS_PS_GAIN_1, 5, 4);
            WriteRegister(MODE_CTRL, RPR_0521RS_ALS_OFF | RPR_0521RS_PS_ON | RPR_0521RS_MEAS_TIME_50_MS_50_MS);

            /*
            //TODO: implement gain and measurement time selection
            //gain and measurement time arrays
            byte alsGainTable[4] = { 1, 2, 64, 128 };
            byte psGainTable[3] = { 1, 2, 4 };
            uint16_t alsMeasureTimeTable[13] = { 0, 0, 0, 0, 0, 100, 100, 100, 400, 400, 400, 400, 50 };
            uint16_t psMeasureTimeTable[13] = { 0, 10, 40, 100, 400, 50, 100, 400, 50, 100, 0, 400, 50 };

            //set gain and measurement time
            _alsData0Gain = alsGainTable[0];
            _alsData1Gain = alsGainTable[0];
            _alsMeasurementTime = alsMeasureTimeTable[measurementTime];

            */


        }


        public short GetProximity()
        {
            short prox = (short)((ReadRegister(PS_DATA_MSB) << 8) | ReadRegister(PS_DATA_LSB));

            return prox;
        }


        public byte[] Get_PS_ALS_Balues()
        {
            //ushort ps_value, float als_value

            //short[] raw_als = new short[2];

            //proximity measurement
            //read the proximity value (does not have a real unit, this will only tell you whether an object is closer than e.g. a few centimiters)

            byte[] ps_als_values = ReadRegister(PS_DATA_LSB, 6);

            short raw_ps = BitConverter.ToInt16(ps_als_values, 0);

            //raw_ps = (short)(raw_ps | ps_als_values[0]);

            //this is float value
            //raw_als[0] = ((short)ps_als_values[3] << 8) | ps_als_values[2];
            //raw_als[1] = ((short)ps_als_values[5] << 8) | ps_als_values[4];

            //*ps_value = raw_ps;
            //*als_value = proximity11_convert_lx(ctx, raw_als);

            return ps_als_values;
        }

        private void SetRegisterBit(byte reg, byte bitNum)
        {
            byte ss = ReadRegister(reg);
            //ss.SetBit(bitNum);
            ss |= bitNum; // FIXME: Ensure correct.
            WriteRegister(reg, ss);
        }

        /// <summary>
        /// Function for setting specific bits in register; will only change bit range from msb to lsb
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="value"></param>
        /// <param name="msb"></param>
        /// <param name="lsb"></param>
        private void SetRegisterBits(byte reg, byte value, byte msb = 7, byte lsb = 0)
        {
            if ((msb > 7) || (lsb > 7) || (lsb > msb))
                return;// 0xFF;

            byte currentValue = ReadRegister(reg);
            byte newValue = (byte)(currentValue & ((0b11111111 << (msb + 1)) | (0b11111111 >> (8 - lsb))));

            WriteRegister(reg, (byte)(newValue | value));
        }


        private void ClearRegisterBit(byte reg, byte bitNum)
        {
            byte ss = ReadRegister(reg);
            //ss.ClearBit(bitNum);
            ss &= bitNum; // FIXME: Ensure correct.
            WriteRegister(reg, ss);
        }

        private byte ReadRegister(byte addrByte)
        {
            i2cDevice.WriteByte(addrByte);
            return i2cDevice.ReadByte();
        }

        private byte[] ReadRegister(byte addrByte, byte bitesToRead)
        {
            i2cDevice.WriteByte(addrByte);
            byte[] buf = new byte[bitesToRead];
            i2cDevice.Read(buf);
            return buf;
        }

        private void WriteRegister(byte reg, byte data)
        {
            i2cDevice.Write(new byte[] { reg, data });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            i2cDevice?.Dispose();
            i2cDevice = null;
        }
    }
    
}
