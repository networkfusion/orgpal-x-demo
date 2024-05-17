﻿
using System;

namespace PalX.Drivers
{
    public static class Pinout
    {
        public static class GpioPin
        {

            // !--- CRITICAL PINS. IF NOT SET PROPERLY SYSTEM WILL SELF POWER OFF ---!

            public static readonly int MAIN_POWER_HOLD_PH2 = PortPin('H', 2);
            public static readonly int MAIN_POWER_OFF_PJ15 = PortPin('J', 15);


            public static readonly int GPIO_NONE = -1;

            /// <summary>
            /// Debug LED definition
            /// </summary>
            public static readonly int LED_1 = PortPin('G', 6); // located on the right of the RJ45 port
            public static readonly int LED_2 = PortPin('G', 7); // located bottom middle

            // Buttons located on bottom of board (apart from diagnostics which is on the right middle)
            public static readonly int BUTTON_USER_BOOT1_PK7 = PortPin('K', 7);
            public static readonly int BUTTON_DIAGNOSTIC_PB7 = PortPin('B', 7); // Hidden by expansion board on IO_PORT 0!
            public static readonly int BUTTON_WAKE_PA0 = PortPin('A', 0);
            public static readonly int BUTTON_USER_WAKE_PE6 = PortPin('E', 6);
            public static readonly int MUX_EXT_BUTTON_WAKE_PE4 = PortPin('E', 4);

            /// <summary>
            /// PWM Speaker PINS definition
            /// </summary>
            public static readonly int PWM_SPEAKER_PH12 = PortPin('H', 12);

            /// <summary>
            /// Relay On/Off PINS definition
            /// </summary>
            public static readonly int RELAY_ON_OFF_PJ5 = PortPin('J', 5);

            /// <summary>
            /// Pulse Counter PINS definition
            /// </summary>
            public static readonly int PULSE_COUNTER_PJ12 = PortPin('J', 12);

            /// <summary>
            /// RJ45 PINS definition
            /// </summary>
            public static readonly int RJ45_POE_PIN1_CTS = PortPin('D', 3);
            public static readonly int RJ45_POE_PIN2_RTS = PortPin('D', 4);

            /// <summary>
            /// KEPAD PINS definition
            /// </summary>
            public static class KEYPAD_PORT
            {
                public static readonly int KEY_PIN1 = PortPin('I', 5);
                public static readonly int KEY_PIN2 = PortPin('I', 4);
                public static readonly int KEY_PIN3 = PortPin('I', 7);
                public static readonly int KEY_PIN4 = PortPin('I', 6);
            }

            /// <summary>
            /// PORT_0 is the righthand connector
            /// </summary>
            public static class IO_PORT0
            {
                public static readonly int IO_PORT0_PIN_6_PA4 = PortPin('A', 4);
                public static readonly int IO_PORT0_PIN_7_PH13 = PortPin('H', 13);
                public static readonly int IO_PORT0_PIN_8_PH14 = PortPin('H', 14);
                public static readonly int IO_PORT0_PIN_12_PH15 = PortPin('H', 15);
                public static readonly int IO_PORT0_PIN_13_PG9 = PortPin('G', 9);
                public static readonly int IO_PORT0_PIN_19_PG10 = PortPin('G', 10);
                public static readonly int IO_PORT0_PIN_23_INT_PI3 = PortPin('I', 3);
            }

            /// <summary>
            /// PORT_1 is the middle bottom connector
            /// </summary>
            public static class IO_PORT1
            {
                // PORT_1 is the middle bottom connector
                public static readonly int IO_PORT1_PIN_6_PK4 = PortPin('K', 4);
                public static readonly int IO_PORT1_PIN_7_PB8 = PortPin('B', 8);
                public static readonly int IO_PORT1_PIN_8_PB9 = PortPin('B', 9);
                public static readonly int IO_PORT1_PIN_12_PH9 = PortPin('H', 9);
                public static readonly int IO_PORT1_PIN_13_PH10 = PortPin('H', 10);
                public static readonly int IO_PORT1_PIN_19_PH11 = PortPin('H', 11);
                public static readonly int IO_PORT1_PIN_23_INT_PG2 = PortPin('G', 2);
            }

            /// <summary>
            /// RS485 PINS definition
            /// </summary>
            public static readonly int RS485_RECEIVERENABLE_PI_12 = PortPin('I', 12);
            public static readonly int RS485_SHUTDOWN_PI_13 = PortPin('I', 13);
            public static readonly int RS485_RESISTORONOFF_PI_14 = PortPin('I', 14);


            public static readonly int FLASH_USER_SPI1_CS = PortPin('I', 15);
            public static readonly int FLASH_USER_WRITE_PROTECT = PortPin('J', 3);
            public static readonly int FLASH_USER_HOLD_ACTIVE_LOW = PortPin('J', 4);


            public static readonly int FLASH_SYSTEM_SPI1_CS = PortPin('B', 6);
            public static readonly int FLASH_SYSTEM_WRITE_PROTECT = PortPin('E', 2);
            public static readonly int FLASH_SYSTEM_HOLD_ACTIVE_LOW = PortPin('D', 13);



            /// <summary>
            /// SD Card Detect Pin
            /// </summary>
            public static readonly int SD_CARD_DETECT = PortPin('E', 5);

            // POWER ON/OFF FOR VARIOUS ON BOARD PERIPHERALS
            public static readonly int POWER_LCD_ON_OFF = PortPin('K', 3);
            public static readonly int POWER_4V_ON_OFF_PJ13 = PortPin('J', 13);
            public static readonly int POWER_RS485_ON_OFF_PJ14 = PortPin('J', 14);
            public static readonly int POWER_ETHERNET_ON_OFF_PD7 = PortPin('D', 7);
            public static readonly int POWER_IO_0_P20_ON_OFF_PI0 = PortPin('I', 0);
            public static readonly int POWER_IO_1_P20_ON_OFF_PK6 = PortPin('K', 6);

            // MULTIPLEXING VARIOUS INTERFACES
            public static readonly int USB_HOST_SEL_SWITCH_PE3 = PortPin('E', 3);


            // Unused & low power pins to be put low.

            public static readonly int RMII_REF_CLK = PortPin('A', 1);
            public static readonly int RMII_MDIO = PortPin('A', 2);
            public static readonly int RMII_CRS_DV = PortPin('A', 7);
            public static readonly int RMII_MDC = PortPin('C', 1);
            public static readonly int RMII_RXD0 = PortPin('C', 4);
            public static readonly int RMII_RXD1 = PortPin('C', 5);
            public static readonly int RMII_TX_EN = PortPin('G', 11);
            public static readonly int RMII_RST = PortPin('C', 12);
            public static readonly int RMII_TXD0 = PortPin('C', 13);
            public static readonly int RMII_TXD1 = PortPin('C', 14);
            public static readonly int RMII_RXER = PortPin('I', 10);

            // LVDS pins (unused at moment).

        }

        /// <summary>
        /// Analog channel definition.
        /// </summary>
        public static class AdcChannel
        {
            // this channel is mapped @ position 2
            public const int ADC1_IN8_VBAT = 2;

            // this channel is mapped @ position 4
            public const int ADC1_IN12_420MA = 4;

            // this channel is mapped @ position 5
            public const int ADC1_IN13_TEMP = 5;

            // this channel is mapped @ position 8 - internal MCU temperature
            public const int ADC_CHANNEL_SENSOR = 9;
        }

        /// <summary>Uart port definition.</summary>
        public static class UartPort
        {
            /// <summary>
            /// Socket definition.
            /// </summary>
            public const string UART2_RJ45_POE = "COM2";
            /// <summary
            /// >Socket definition.
            /// </summary>
            public const string UART3_RS485 = "COM3";

            /// <summary>
            /// Socket definition.
            /// </summary>
            public const string UART6_IO_PORT0 = "COM6";
            /// <summary>
            /// Socket definition.
            /// </summary>
            public const string UART7_IO_PORT1 = "COM7";
        }

        /// <summary>
        /// SPI Bus definition.
        /// </summary>
        public static class SpiBus
        {
            /// <summary>
            /// Socket definition.
            /// </summary>
            public const string SPI1_FLASH_USER = "SPI1";
            public const string SPI2_IO_PORT0 = "SPI2";
        }

        /// <summary>
        /// I2C Bus definition.
        /// </summary>
        public static class I2CBus
        {
            /// <summary>
            /// Socket definition.
            /// </summary>
            public const int I2C2 = 2;

            /// <summary>
            /// Socket definition.
            /// </summary>
            public const int I2C3 = 3;
        }


        public static class PwmChannel
        {
            public static class Speaker
            {
                public const string Id = "TIM5"; //TIM5_CH3
            }
        }

        internal static int PortPin(char port, byte pin)
        {
            if (port < 'A' || port > 'K')
                throw new ArgumentException("Invalid Port definition");

            return ((port - 'A') * 16) + pin;

        }
    }

}
