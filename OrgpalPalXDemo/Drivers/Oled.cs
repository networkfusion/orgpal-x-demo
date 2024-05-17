using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace PalX.Drivers
{
    /// <summary>
    /// OLED Driver class for SSH1106 and SSD1306 controlled screens.
    /// </summary>
    public class SSD1x06 : IDisposable
    {
        private byte ScreenWidthPx = 132;
        private byte ScreenHeightPx = 64;
        private byte ScreenMemoryPages;
        private I2cDevice i2c = null;
        private GpioPin lcdPowerOnOff;
        private byte[] buffer;
        private byte[] pageData;
        //private DisplayType _displayType;
        private IFont CurrentFont = null;

        /// <summary>
        /// Sequence of bytes that should be sent to a 128x64 OLED display to setup the device.
        /// First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] oled128x64Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5,0x80,  // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x3f, // set multiplex ratio 0x00-0x3f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x12, // set COM pins HW configuration
            0x81, 0xcf, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
        /// Sequence of bytes that should be sent to a 128x32 OLED display to setup the device.
        /// First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] oled128x32Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5,0x80,  // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x1f, // set multiplex ratio 0x00-0x1f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x02, // set COM pins HW configuration
            0x81, 0x8f, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /*
		

				public void Rotate(bool rotate = true)
		{
			i2c.Write(new byte[]
			{
				0x00, // is command              
                (byte)(rotate?0xA1:0xA0),   // SH1106 [06] set segment re-map 0xA0/0xA1 normal/reverse                
                (byte)(rotate?0xC8:0xC0),   // SH1106 [13] set output scan direction 0xC0/0xC8               
            });
		}

		public void InvertDisplay(bool invert = true)
		{
			i2c.Write(new byte[]
			{
				0x00, // is command       
				(byte)(invert ? 0xA7 : 0xA6) //INVERTDISPLAY = 0xA7, NORMALDISPLAY = 0xA6,
			});
		}

		public void Power(bool on)
		{
			i2c.Write(new byte[]
			{
				0x00, // is command               
                (byte)(on?0xAF:0xAE)       // SH1106 [11] display off/on 0xAE/0xAF 
            });
		}

				public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool colored = true)
		{
			DrawLine(x0, y0, x1, y1, colored);
			DrawLine(x1, y1, x2, y2, colored);
			DrawLine(x2, y2, x0, y0, colored);
		}

		public void DrawCircle(int x0, int y0, int r, bool useColor = true)
		{
			r--;
			int d = (5 - r * 4) / 4;
			int x = 0;
			int y = r;

			do
			{
				DrawPixel(x0 + x, y0 + y, useColor);
				DrawPixel(x0 + x, y0 - y, useColor);
				DrawPixel(x0 - x, y0 + y, useColor);
				DrawPixel(x0 - x, y0 - y, useColor);
				DrawPixel(x0 + y, y0 + x, useColor);
				DrawPixel(x0 + y, y0 - x, useColor);
				DrawPixel(x0 - y, y0 + x, useColor);
				DrawPixel(x0 - y, y0 - x, useColor);
				if (d < 0)
				{
					d += 2 * x + 1;
				}
				else
				{
					d += 2 * (x - y) + 1;
					y--;
				}
				x++;

			} while (x <= y);
		}



		*/

        private byte[] pageCmd = new byte[]
        {
            0x00, // is command
            0xB0, // page address (B0-B7)
            0x00, // lower columns address =0
            0x10, // upper columns address =0
        };

        /// <summary>
        /// Constructor setting up defaults.
        /// </summary>
        /// <param name="fontSize">Font size to use, available 4 (4x6), 6 (6x8) and 8 (8x8 - default).</param>
        /// <param name="I2CBus">I2C bus id/number.</param>
        /// <param name="address">Address to use for I2C device on bus.</param>
        /// <param name="displayType">Type of OLED based on resolution.</param>
        public SSD1x06(byte fontSize = 8, int I2CBus = 3, int address = 0x3C, DisplayType displayType = DisplayType.OLED128x64)
        {
            //seems not used/needed on the OLED as is not turning on/off but only light level
            //lcdPowerOnOff = PalHelper.GpioPort(PalThreePins.GpioPin.IO_PORT0_PIN_20_PI0, PinMode.Output, PinValue.High);

            i2c = I2cDevice.Create(new I2cConnectionSettings(I2CBus, address));

            switch (displayType)
            {
                case DisplayType.OLED128x64:
                    ScreenWidthPx = 132;//SH1106 buffer is 132
                    ScreenHeightPx = 64;
                    i2c.Write(oled128x64Init);
                    break;
                case DisplayType.OLED128x32:
                    ScreenWidthPx = 132;//SH1106 buffer is 132
                    ScreenHeightPx = 32;
                    i2c.Write(oled128x32Init);
                    break;
                    //case DisplayType.OLED96x16:
                    //    ScreenWidthPx = 100;//SH1106 buffer is 100
                    //    ScreenHeightPx = 16;
                    //    i2c.Write(oled96x16Init);
                    //    break;
            }

            ScreenMemoryPages = (byte)(ScreenHeightPx / 8);
            buffer = new byte[ScreenMemoryPages * ScreenWidthPx];
            pageData = new byte[ScreenWidthPx + 1];


            //if (fontSize == 4)
            //	CurrentFont = new Font4x6();
            //else if (fontSize == 6)
            //	CurrentFont = new Font6x8();
            //else //if 8 or invalid font size default to 8
            CurrentFont = new Font8x8();

            ClearScreen();
        }

        /// <summary>
        /// Sets a new font size for current driver.
        /// </summary>
        /// <param name="fontSize">Valid font sizes, 4, 6, or 8</param>
        //public void ChangeFontSize(byte fontSize)
        //{
        //	//if (fontSize == 4)
        //	//	CurrentFont = new Font4x6();
        //	//else if (fontSize == 6)
        //	//	CurrentFont = new Font6x8();
        //	//else //if 8 or invalid font size default to 8
        //		CurrentFont = new Font8x8();
        //}


        public bool BacklightOn
        {
            get; set;

            //get { return lcdPowerOnOff.Read() == PinValue.High ? true : false; }

            //set
            //{
            //	if (value)//turn it on
            //		lcdPowerOnOff.Write(PinValue.High);
            //	else //turn it off
            //		lcdPowerOnOff.Write(PinValue.Low);
            //}
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public void ClearScreen()
        {
            Array.Clear(buffer, 0, buffer.Length);
            Display();
        }

        /// <summary>
        /// Shows the pending drawings to the screen.
        /// </summary>
        public void Display()
        {
            for (byte i = 0; i < ScreenMemoryPages; i++)
            {
                pageCmd[1] = (byte)(0xB0 + i); // page number
                i2c.Write(pageCmd);

                pageData[0] = 0x40; // is data
                Array.Copy(buffer, i * ScreenWidthPx, pageData, 1, ScreenWidthPx);
                i2c.Write(pageData);
            }
        }

        /// <summary>
        /// Displays text without clearing it.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void DisplayText(string txt = "", byte x = 2, byte y = 2, byte size = 1)
        {
            //attempt to auto fit long text on the screen
            //screen size is 128x64,  font usually is 8x8  for size 1,  so try to auto fit

            if (size == 1 && txt.Length > 16)//try to auto fit
            {
                byte txtIdx = 0;
                DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 1
                txtIdx = 16;
                y += 10;//next line 8 + 2 space
                if (txt.Length > 16)
                    DrawString(x, y, txt.Substring(txtIdx), size); //line 2

                if (txt.Length > 32)
                {
                    txtIdx = 32;
                    y += 10;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 3
                }

                if (txt.Length > 48)
                {
                    txtIdx = 48;
                    y += 10;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 4
                }

                if (txt.Length > 64)
                {
                    txtIdx = 64;
                    y += 10;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 5
                }

                if (txt.Length > 80)
                {
                    txtIdx = 80;
                    y += 10;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 6
                }

                if (txt.Length > 96)
                {
                    txtIdx = 96;
                    y += 10;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 16), size); //line 6
                }

            }
            else if (size == 2 && txt.Length > 8)//try to auto fit
            {
                byte txtIdx = 0;
                DrawString(x, y, txt.Substring(txtIdx, 8), size); //line 1
                txtIdx = 8;
                y += 16;//next line 14 + 2 space
                DrawString(x, y, txt.Substring(txtIdx, 8), size); //line 2
                if (txt.Length > 16)
                {
                    txtIdx = 16;
                    y += 16;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 8), size); //line 3
                }
                if (txt.Length > 24)
                {
                    txtIdx = 24;
                    y += 16;//next line 8 + 2 space
                    DrawString(x, y, txt.Substring(txtIdx, 8), size); //line 3
                }
            }
            else
            {
                DrawString(x, y, txt, size);
            }

            Display();
        }

        /// <summary>
        /// Clears the screen then shows the text.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="line"></param>
        public void ShowText(string txt = "", byte x = 2, byte y = 2, byte size = 1)
        {
            this.ClearScreen();

            DrawString(x, y, txt, size);
            Display();
        }

        /// <summary>
        /// Draws a pixel on the screen.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="inverted">Indicates if color to be used turn the pixel on, or leave off.</param>
        public void DrawPixel(int x, int y, bool inverted = true)
        {
            if ((x >= ScreenWidthPx) || (y >= ScreenHeightPx))
                return;

            // x is which column
            int idx = x + (y / 8) * ScreenWidthPx;

            if (inverted)
                buffer[idx] |= (byte)(1 << (y & 7));
            else
                buffer[idx] &= (byte)~(1 << (y & 7));

            //INVERSE COLOR: buffer[idx] ^= (byte)(1 << (y & 7));
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="x0">x coordinate starting of the line.</param>
        /// <param name="y0">y coordinate starting of line.</param>
        /// <param name="x1">x coordinate ending of the line.</param>
        /// <param name="y1">y coordinate ending of the line.</param>
        public void DrawLine(int x0, int y0, int x1, int y1, bool inverted = true)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            var dx = x1 - x0;
            var dy = Math.Abs(y1 - y0);
            var error = dx / 2;
            var ystep = y0 < y1 ? 1 : -1;
            var y = y0;
            for (var x = x0; x <= x1; x++)
            {
                DrawPixel(steep ? y : x, steep ? x : y, inverted);
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x0">x coordinate starting of the top left.</param>
        /// <param name="y0">y coordinate starting of the top left.</param>
        /// <param name="width">Width of rectabgle in pixels.</param>
        /// <param name="height">Height of rectangle in pixels</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawRectangle(int x0, int y0, int width, int height, bool inverted = true)
        {
            width--;
            height--;
            if (width < 0 || height < 0)
                return;

            DrawLine(x0, y0, x0 + width, y0, inverted);
            DrawLine(x0 + width, y0, x0 + width, y0 + height, inverted);
            DrawLine(x0 + width, y0 + height, x0, y0 + height, inverted);
            DrawLine(x0, y0, x0, y0 + height, inverted);
        }

        /// <summary>
        /// Draws a rectangle that is solid/filled.
        /// </summary>
        /// <param name="x0">x coordinate starting of the top left.</param>
        /// <param name="y0">y coordinate starting of the top left.</param>
        /// <param name="width">Width of rectabgle in pixels.</param>
        /// <param name="height">Height of rectangle in pixels</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawFilledRectangle(int xLeft, int yTop, int width, int height, bool colored = true)
        {
            width--;
            height--;

            if (width < 0 || height < 0)
                return;

            for (int i = 0; i <= height; i++)
            {
                DrawLine(xLeft, yTop + i, xLeft + width, yTop + i, colored);
            }
        }

        /// <summary>
        /// Simple method to write OrgPal Telemetry in small 4x4 font on bottom of screen.
        /// </summary>
        /// <param name="x">The x pixel-coordinate on the screen.</param>
        /// <param name="y">The y pixel-coordinate on the screen.</param>
        public void DrawOrgpalTelemetry(int x, int y)
        {
            //bytes to draw   ORGPAL TELEMETRY in 4x4 size font on the screen.
            byte[] bitMap = { 0x22, 0x36, 0x12, 0x70, 0x17, 0x57, 0x77, 0x52, 0x55, 0x51, 0x15, 0x20, 0x11, 0x71, 0x21, 0x55, 0x55, 0x51, 0x15, 0x20, 0x13, 0x73, 0x23, 0x75, 0x35, 0x35, 0x17, 0x20, 0x11, 0x51, 0x21, 0x23, 0x55, 0x15, 0x15, 0x20, 0x11, 0x51, 0x21, 0x25, 0x52, 0x13, 0x75, 0x20, 0x77, 0x57, 0x27, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height, CurrentFont.Height, bitMap);
        }

        /*
		public void DrawCharacter(int x, int y, Char c, byte size = 1, bool inverted = true)
		{
			for (int i = 0; i < 5; i++)
			{
				buffer[x + (y * 128)] = Font5x7[(c * 5) + i];
				x++;
			}
		}

		public void DrawString(int x, int y, string str, byte size = 1)
		{
			foreach (Char c in str)
			{
				DrawCharacter(x, y, c, size);
				x += 6; // 6 pixels wide, font is 5 plus 1 px space
				if (x + 6 >= Width)
				{
					x = 0;    // no more line
					y++;
				}

				if (y >= Height / 8)
				{
					return;        // no  more space
				}
			}

		}
		*/


        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x pixel-coordinate on the screen.</param>
        /// <param name="y">The y pixel-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
        /// <param name="center">Indicates if text should be centered if possible.</param>
        /// <seealso cref="Write"/>
        public void DrawString(int x, int y, string str, byte size = 1, bool center = false)
        {
            if (center && str != null)
            {
                int padSize = (ScreenWidthPx / size / CurrentFont.Width - str.Length) / 2;
                if (padSize > 0)
                    str = str.PadLeft(str.Length + padSize);
            }

            byte[] bitMap = GetBytesForTextBitmap(str);

            //Debug.WriteLine(BitConverter.ToString(bitMap));//if needed to use for byte array generation/store

            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height, CurrentFont.Height, bitMap, size);
        }


        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x text-coordinate on the screen.</param>
        /// <param name="y">The y text-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
        /// <seealso cref="DrawString"/>
        public void Write(int x, int y, string str, byte size = 1, bool center = false)
        {
            DrawString(x * CurrentFont.Width, y * CurrentFont.Height, str, size, center);
        }


        /// <summary>
        /// Displays the  1 bit bit map.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="width">Width in bytes.</param>
        /// <param name="height">Height in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="size">Drawing size, normal = 1, larger use 2,3 etc.</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, byte size = 1)
        {
            if ((width * height) != bitmap.Length)
                throw new ArgumentException("Width and height do not match the bitmap size.");

            for (var yO = 0; yO < height; yO++)
            {
                byte mask = 0x01;

                for (var xA = 0; xA < width; xA++)
                {
                    var b = bitmap[(yO * width) + xA];

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if (size == 1)
                        {
                            DrawPixel(x + (8 * xA) + pixel, y + yO, (b & mask) > 0);
                        }
                        else
                        {
                            DrawFilledRectangle((x + (8 * xA) + pixel) * size, (y / size + yO) * size, size, size, (b & mask) > 0);
                        }
                        mask <<= 1;
                    }
                    mask = 0x01;//reset needed for SSH1106  support
                }
            }
        }

        private byte[] GetBytesForTextBitmap(string text)
        {
            byte[] bitMap = null;

            if (text == null)
                return bitMap;

            if (CurrentFont.Width == 8) //just copy bytes
            {
                //bitMap = new byte[text.Length * CurrentFont.Height * CurrentFont.Width / 8];
                bitMap = new byte[text.Length * CurrentFont.Height];

                for (int i = 0; i < text.Length; i++)
                {
                    var characterMap = CurrentFont[text[i]];

                    for (int segment = 0; segment < CurrentFont.Height; segment++)
                    {
                        bitMap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else if (CurrentFont.Width == 4)
            {
                var len = (text.Length + text.Length % 2) / 2;
                bitMap = new byte[len * CurrentFont.Height];
                byte[] characterMap1, characterMap2;

                for (int i = 0; i < len; i++)
                {
                    characterMap1 = CurrentFont[text[2 * i]];
                    characterMap2 = (i * 2 + 1 < text.Length) ? CurrentFont[text[2 * i + 1]] : CurrentFont[' '];

                    for (int j = 0; j < characterMap1.Length; j++)
                    {
                        bitMap[i + (j * 2 + 0) * len] = (byte)((characterMap1[j] & 0x0F) | (characterMap2[j] << 4));
                        bitMap[i + (j * 2 + 1) * len] = (byte)((characterMap1[j] >> 4) | (characterMap2[j] & 0xF0));
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 4, or 8");
            }
            return bitMap;
        }

        /// <summary>
        /// Swaps two int values.
        /// </summary>
        /// <param name="a">Value one.</param>
        /// <param name="b">Value two.</param>
        void Swap(ref int a, ref int b)
        {
            var t = a;
            a = b;
            b = t;
        }


        public void DisposeI2C()
        {
            i2c?.Dispose();
            i2c = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            i2c?.Dispose();
            i2c = null;

            lcdPowerOnOff?.Dispose();
            lcdPowerOnOff = null;
        }
    }

    /// <summary>
    /// Display type options.
    /// </summary>
    public enum DisplayType
    {
        OLED128x64,
        OLED128x32,
        OLED96x16,
    }

    public abstract class IFont
    {
        /// <summary>
        /// Width of font character 
        /// </summary>
        public virtual int Width { get; }

        /// <summary>
        /// Height of font character.
        /// </summary> 
        public virtual int Height { get; }

        /// <summary>
        /// Get the binary representation of the ASCII character from the font table
        /// </summary>
        /// <param name="character">Character to look up</param>
        /// <returns>Array of bytes representing the binary bit pattern of the character</returns>
        public abstract byte[] this[char character] { get; }
    }

    /*
        public class Font4x6 : IFont
        {
            /// <summary>
            /// Width of font character 
            /// </summary>
            public override int Width { get { return 4; } }

            /// <summary>
            /// Height of font character.
            /// </summary> 
            public override int Height { get { return 6; } }

            /// <summary>
            /// Font table containing the binary representation of ASCII characters.
            /// </summary>
            private static readonly byte[][] _fontTable =
            {
                new byte[]{0x00, 0x00, 0x00}, //0020( )
                new byte[]{0x22, 0x02, 0x02}, //0021(!)
                new byte[]{0x55, 0x00, 0x00}, //0022(")
                new byte[]{0x75, 0x75, 0x05}, //0023(#)
                new byte[]{0x62, 0x61, 0x24}, //0024($)
                new byte[]{0x45, 0x12, 0x05}, //0025(%)
                new byte[]{0x17, 0x13, 0x07}, //0026(&)
                new byte[]{0x22, 0x00, 0x00}, //0027(')
                new byte[]{0x24, 0x22, 0x04}, //0028(()
                new byte[]{0x21, 0x22, 0x01}, //0029())
                new byte[]{0x25, 0x05, 0x00}, //002A(*)
                new byte[]{0x20, 0x27, 0x00}, //002B(+)
                new byte[]{0x00, 0x00, 0x11}, //002C(,)
                new byte[]{0x00, 0x07, 0x00}, //002D(-)
                new byte[]{0x00, 0x00, 0x01}, //002E(.)
                new byte[]{0x44, 0x12, 0x01}, //002F(/)
                new byte[]{0x57, 0x55, 0x07}, //0030(0)
                new byte[]{0x22, 0x22, 0x02}, //0031(1)
                new byte[]{0x47, 0x17, 0x07}, //0032(2)
                new byte[]{0x47, 0x47, 0x07}, //0033(3)
                new byte[]{0x55, 0x47, 0x04}, //0034(4)
                new byte[]{0x17, 0x47, 0x07}, //0035(5)
                new byte[]{0x17, 0x57, 0x07}, //0036(6)
                new byte[]{0x47, 0x44, 0x04}, //0037(7)
                new byte[]{0x57, 0x57, 0x07}, //0038(8)
                new byte[]{0x57, 0x47, 0x07}, //0039(9)
                new byte[]{0x00, 0x02, 0x02}, //003A(:)
                new byte[]{0x00, 0x02, 0x22}, //003B(;)
                new byte[]{0x24, 0x21, 0x04}, //003C(<)
                new byte[]{0x70, 0x70, 0x00}, //003D(=)
                new byte[]{0x21, 0x24, 0x01}, //003E(>)
                new byte[]{0x47, 0x06, 0x02}, //003F(?)
                new byte[]{0x72, 0x35, 0x07}, //0040(@)
                new byte[]{0x57, 0x57, 0x05}, //0041(A)
                new byte[]{0x53, 0x57, 0x07}, //0042(B)
                new byte[]{0x17, 0x11, 0x07}, //0043(C)
                new byte[]{0x53, 0x55, 0x07}, //0044(D)
                new byte[]{0x17, 0x13, 0x07}, //0045(E)
                new byte[]{0x17, 0x13, 0x01}, //0046(F)
                new byte[]{0x17, 0x55, 0x07}, //0047(G)
                new byte[]{0x55, 0x57, 0x05}, //0048(H)
                new byte[]{0x27, 0x22, 0x07}, //0049(I)
                new byte[]{0x44, 0x54, 0x07}, //004A(J)
                new byte[]{0x55, 0x53, 0x05}, //004B(K)
                new byte[]{0x11, 0x11, 0x07}, //004C(L)
                new byte[]{0x75, 0x55, 0x05}, //004D(M)
                new byte[]{0x54, 0x57, 0x01}, //004E(N)
                new byte[]{0x57, 0x55, 0x07}, //004F(O)
                new byte[]{0x57, 0x17, 0x01}, //0050(P)
                new byte[]{0x57, 0x55, 0x47}, //0051(Q)
                new byte[]{0x53, 0x53, 0x05}, //0052(R)
                new byte[]{0x16, 0x47, 0x07}, //0053(S)
                new byte[]{0x27, 0x22, 0x02}, //0054(T)
                new byte[]{0x55, 0x55, 0x07}, //0055(U)
                new byte[]{0x55, 0x55, 0x02}, //0056(V)
                new byte[]{0x55, 0x75, 0x05}, //0057(W)
                new byte[]{0x55, 0x52, 0x05}, //0058(X)
                new byte[]{0x55, 0x22, 0x02}, //0059(Y)
                new byte[]{0x47, 0x12, 0x07}, //005A(Z)
                new byte[]{0x26, 0x22, 0x06}, //005B([)
                new byte[]{0x11, 0x42, 0x04}, //005C(\)
                new byte[]{0x23, 0x22, 0x03}, //005D(])
                new byte[]{0x52, 0x00, 0x00}, //005E(^)
                new byte[]{0x00, 0x00, 0x70}, //005F(_)
                new byte[]{0x21, 0x00, 0x00}, //0060(`)
                new byte[]{0x60, 0x55, 0x06}, //0061(a)
                new byte[]{0x31, 0x55, 0x03}, //0062(b)
                new byte[]{0x60, 0x11, 0x06}, //0063(c)
                new byte[]{0x64, 0x55, 0x06}, //0064(d)
                new byte[]{0x20, 0x35, 0x06}, //0065(e)
                new byte[]{0x16, 0x13, 0x01}, //0066(f)
                new byte[]{0x60, 0x65, 0x24}, //0067(g)
                new byte[]{0x11, 0x53, 0x05}, //0068(h)
                new byte[]{0x02, 0x22, 0x02}, //0069(i)
                new byte[]{0x02, 0x22, 0x12}, //006A(j)
                new byte[]{0x51, 0x53, 0x05}, //006B(k)
                new byte[]{0x22, 0x22, 0x02}, //006C(l)
                new byte[]{0x50, 0x57, 0x05}, //006D(m)
                new byte[]{0x30, 0x55, 0x05}, //006E(n)
                new byte[]{0x20, 0x55, 0x02}, //006F(o)
                new byte[]{0x30, 0x55, 0x13}, //0070(p)
                new byte[]{0x60, 0x55, 0x46}, //0071(q)
                new byte[]{0x50, 0x13, 0x01}, //0072(r)
                new byte[]{0x60, 0x43, 0x03}, //0073(s)
                new byte[]{0x72, 0x22, 0x02}, //0074(t)
                new byte[]{0x50, 0x55, 0x06}, //0075(u)
                new byte[]{0x50, 0x55, 0x02}, //0076(v)
                new byte[]{0x50, 0x75, 0x05}, //0077(w)
                new byte[]{0x50, 0x52, 0x05}, //0078(x)
                new byte[]{0x50, 0x25, 0x02}, //0079(y)
                new byte[]{0x70, 0x24, 0x07}, //007A(z)
                new byte[]{0x26, 0x23, 0x06}, //007B({)
                new byte[]{0x22, 0x22, 0x22}, //007C(|)
                new byte[]{0x23, 0x26, 0x03}, //007D(})
                new byte[]{0x5A, 0x00, 0x00}, //007E(~)
                new byte[]{0x00, 0x00, 0x00}, //00A0( )
            };


            /// <summary>
            /// Get the binary representation of an ASCII character from the
            /// font table.
            /// </summary>
            /// <param name="character">Character to look up.</param>
            /// <returns>
            /// Byte array containing the rows of pixels in the character.  Unknown byte codes will result in a space being
            /// returned.
            /// </returns>
            public override byte[] this[char character]
            {
                get
                {
                    var index = (byte)character;
                    if ((index < 32) || (index > 127))
                    {
                        return _fontTable[0x20];
                    }
                    return _fontTable[(byte)character - 0x20];
                }
            }
        }


        public class Font6x8 : IFont
        {
            /// <summary>
            /// Width of font character 
            /// </summary>
            public override int Width { get { return 6; } }

            /// <summary>
            /// Height of font character.
            /// </summary> 
            public override int Height { get { return 8; } }

            /// <summary>
            /// Font table containing the binary representation of ASCII characters.
            /// </summary>
            private static readonly byte[][] _fontTable =
            {
                new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, //0020( )
                new byte[] {0x04, 0x41, 0x10, 0x04, 0x40, 0x00}, //0021(!)
                new byte[] {0x8A, 0xA2, 0x00, 0x00, 0x00, 0x00}, //0022(")
                new byte[] {0x80, 0xF2, 0x29, 0x9F, 0x02, 0x00}, //0023(#)
                new byte[] {0x84, 0x57, 0x38, 0xD4, 0x43, 0x00}, //0024($)
                new byte[] {0x42, 0x29, 0x21, 0x84, 0x94, 0x42}, //0025(%)
                new byte[] {0x42, 0x51, 0x08, 0x55, 0x62, 0x01}, //0026(&)
                new byte[] {0x82, 0x20, 0x00, 0x00, 0x00, 0x00}, //0027(')
                new byte[] {0x08, 0x21, 0x08, 0x02, 0x81, 0x00}, //0028(()
                new byte[] {0x02, 0x81, 0x20, 0x08, 0x21, 0x00}, //0029())
                new byte[] {0x00, 0xA0, 0x10, 0x0A, 0x00, 0x00}, //002A(*)
                new byte[] {0x00, 0x41, 0x7C, 0x04, 0x01, 0x00}, //002B(+)
                new byte[] {0x00, 0x00, 0x00, 0x80, 0x41, 0x08}, //002C(,)
                new byte[] {0x00, 0x00, 0x7C, 0x00, 0x00, 0x00}, //002D(-)
                new byte[] {0x00, 0x00, 0x00, 0x80, 0x61, 0x00}, //002E(.)
                new byte[] {0x00, 0x84, 0x10, 0x42, 0x00, 0x00}, //002F(/)
                new byte[] {0x4E, 0x94, 0x55, 0x53, 0xE4, 0x00}, //0030(0)
                new byte[] {0x84, 0x41, 0x10, 0x04, 0x41, 0x00}, //0031(1)
                new byte[] {0x4E, 0x04, 0x21, 0x84, 0xF0, 0x01}, //0032(2)
                new byte[] {0x4E, 0x04, 0x39, 0x50, 0xE4, 0x00}, //0033(3)
                new byte[] {0x48, 0x92, 0x24, 0x1F, 0x82, 0x00}, //0034(4)
                new byte[] {0x5F, 0x10, 0x38, 0x50, 0xE4, 0x00}, //0035(5)
                new byte[] {0x4E, 0x14, 0x3C, 0x51, 0xE4, 0x00}, //0036(6)
                new byte[] {0x1F, 0x04, 0x41, 0x10, 0x04, 0x01}, //0037(7)
                new byte[] {0x4E, 0x14, 0x39, 0x51, 0xE4, 0x00}, //0038(8)
                new byte[] {0x4E, 0x14, 0x79, 0x50, 0xE4, 0x00}, //0039(9)
                new byte[] {0x80, 0x61, 0x00, 0x80, 0x61, 0x00}, //003A(:)
                new byte[] {0x80, 0x61, 0x00, 0x80, 0x41, 0x08}, //003B(;)
                new byte[] {0x08, 0x21, 0x04, 0x02, 0x81, 0x00}, //003C(<)
                new byte[] {0x00, 0xF0, 0x03, 0x3F, 0x00, 0x00}, //003D(=)
                new byte[] {0x02, 0x81, 0x40, 0x08, 0x21, 0x00}, //003E(>)
                new byte[] {0x4E, 0x04, 0x31, 0x04, 0x40, 0x00}, //003F(?)
                new byte[] {0x4E, 0x14, 0x35, 0x41, 0xE4, 0x00}, //0040(@)
                new byte[] {0x4E, 0x14, 0x7D, 0x51, 0x14, 0x01}, //0041(A)
                new byte[] {0x4F, 0x14, 0x3D, 0x51, 0xF4, 0x00}, //0042(B)
                new byte[] {0x4E, 0x14, 0x04, 0x41, 0xE4, 0x00}, //0043(C)
                new byte[] {0x4F, 0x14, 0x45, 0x51, 0xF4, 0x00}, //0044(D)
                new byte[] {0x5F, 0x10, 0x1C, 0x41, 0xF0, 0x01}, //0045(E)
                new byte[] {0x5F, 0x10, 0x1C, 0x41, 0x10, 0x00}, //0046(F)
                new byte[] {0x4E, 0x14, 0x74, 0x51, 0xE4, 0x00}, //0047(G)
                new byte[] {0x51, 0x14, 0x7D, 0x51, 0x14, 0x01}, //0048(H)
                new byte[] {0x04, 0x41, 0x10, 0x04, 0x41, 0x00}, //0049(I)
                new byte[] {0x10, 0x04, 0x41, 0x50, 0xE4, 0x00}, //004A(J)
                new byte[] {0x51, 0x52, 0x0C, 0x45, 0x12, 0x01}, //004B(K)
                new byte[] {0x41, 0x10, 0x04, 0x41, 0xF0, 0x01}, //004C(L)
                new byte[] {0xD1, 0x56, 0x45, 0x51, 0x14, 0x01}, //004D(M)
                new byte[] {0x51, 0x34, 0x55, 0x59, 0x14, 0x01}, //004E(N)
                new byte[] {0x4E, 0x14, 0x45, 0x51, 0xE4, 0x00}, //004F(O)
                new byte[] {0x4F, 0x14, 0x3D, 0x41, 0x10, 0x00}, //0050(P)
                new byte[] {0x4E, 0x14, 0x45, 0x51, 0xE4, 0x40}, //0051(Q)
                new byte[] {0x4F, 0x14, 0x3D, 0x51, 0x14, 0x01}, //0052(R)
                new byte[] {0x5E, 0x10, 0x38, 0x10, 0xF4, 0x00}, //0053(S)
                new byte[] {0x1F, 0x41, 0x10, 0x04, 0x41, 0x00}, //0054(T)
                new byte[] {0x51, 0x14, 0x45, 0x51, 0xE4, 0x00}, //0055(U)
                new byte[] {0x51, 0x14, 0x45, 0x91, 0x42, 0x00}, //0056(V)
                new byte[] {0x51, 0x14, 0x55, 0x55, 0xA5, 0x00}, //0057(W)
                new byte[] {0x51, 0xA4, 0x10, 0x4A, 0x14, 0x01}, //0058(X)
                new byte[] {0x51, 0xA4, 0x10, 0x04, 0x41, 0x00}, //0059(Y)
                new byte[] {0x1F, 0x84, 0x10, 0x42, 0xF0, 0x01}, //005A(Z)
                new byte[] {0x0C, 0x41, 0x10, 0x04, 0xC1, 0x00}, //005B([)
                new byte[] {0x40, 0x20, 0x10, 0x08, 0x04, 0x00}, //005C(\)
                new byte[] {0x06, 0x41, 0x10, 0x04, 0x61, 0x00}, //005D(])
                new byte[] {0x84, 0x02, 0x00, 0x00, 0x00, 0x00}, //005E(^)
                new byte[] {0x00, 0x00, 0x00, 0x00, 0xF0, 0x01}, //005F(_)
                new byte[] {0x82, 0x40, 0x00, 0x00, 0x00, 0x00}, //0060(`)
                new byte[] {0x00, 0xE0, 0x45, 0x51, 0x66, 0x01}, //0061(a)
                new byte[] {0x41, 0xF0, 0x44, 0x51, 0xF4, 0x00}, //0062(b)
                new byte[] {0x00, 0xE0, 0x44, 0x41, 0xE4, 0x00}, //0063(c)
                new byte[] {0x10, 0xE4, 0x45, 0x51, 0xE4, 0x01}, //0064(d)
                new byte[] {0x00, 0xE0, 0x44, 0x5F, 0xE0, 0x00}, //0065(e)
                new byte[] {0x08, 0x45, 0x38, 0x04, 0x41, 0x00}, //0066(f)
                new byte[] {0x00, 0xE0, 0x44, 0x91, 0x07, 0x39}, //0067(g)
                new byte[] {0x41, 0xF0, 0x44, 0x51, 0x14, 0x01}, //0068(h)
                new byte[] {0x00, 0x01, 0x10, 0x04, 0x41, 0x00}, //0069(i)
                new byte[] {0x00, 0x02, 0x20, 0x08, 0xA2, 0x10}, //006A(j)
                new byte[] {0x41, 0x90, 0x14, 0x43, 0x91, 0x00}, //006B(k)
                new byte[] {0x04, 0x41, 0x10, 0x04, 0x41, 0x00}, //006C(l)
                new byte[] {0x00, 0xF0, 0x54, 0x55, 0x55, 0x01}, //006D(m)
                new byte[] {0x00, 0xF0, 0x44, 0x51, 0x14, 0x01}, //006E(n)
                new byte[] {0x00, 0xE0, 0x44, 0x51, 0xE4, 0x00}, //006F(o)
                new byte[] {0x00, 0xF0, 0x44, 0xD1, 0x13, 0x04}, //0070(p)
                new byte[] {0x00, 0xE0, 0x45, 0x91, 0x07, 0x41}, //0071(q)
                new byte[] {0x00, 0xD0, 0x4C, 0x41, 0x10, 0x00}, //0072(r)
                new byte[] {0x00, 0xE0, 0x05, 0x0E, 0xF4, 0x00}, //0073(s)
                new byte[] {0x04, 0xE1, 0x10, 0x04, 0x41, 0x00}, //0074(t)
                new byte[] {0x00, 0x10, 0x45, 0x51, 0xE4, 0x00}, //0075(u)
                new byte[] {0x00, 0x10, 0x45, 0x91, 0x42, 0x00}, //0076(v)
                new byte[] {0x00, 0x10, 0x45, 0x51, 0xA5, 0x00}, //0077(w)
                new byte[] {0x00, 0x10, 0x29, 0x84, 0x12, 0x01}, //0078(x)
                new byte[] {0x00, 0x10, 0x45, 0x91, 0x42, 0x10}, //0079(y)
                new byte[] {0x00, 0xF0, 0x21, 0x84, 0xF0, 0x01}, //007A(z)
                new byte[] {0x08, 0x41, 0x08, 0x04, 0x81, 0x00}, //007B({)
                new byte[] {0x04, 0x41, 0x10, 0x04, 0x41, 0x10}, //007C(|)
                new byte[] {0x02, 0x41, 0x20, 0x04, 0x21, 0x00}, //007D(})
                new byte[] {0x80, 0x52, 0x00, 0x00, 0x00, 0x00}, //007E(~)
            };

            /// <summary>
            /// Get the binary representation of an ASCII character from the
            /// font table.
            /// </summary>
            /// <param name="character">Character to look up.</param>
            /// <returns>
            /// Byte array containing the rows of pixels in the character.  Unknown byte codes will result in a space being
            /// returned.
            /// </returns>
            public override byte[] this[char character]
            {
                get
                {
                    var index = (byte)character;
                    if ((index < 32) || (index > 127))
                    {
                        return _fontTable[0x20];
                    }
                    return _fontTable[(byte)character - 0x20];
                }
            }
        }
        */

    public class Font8x8 : IFont
    {

        /// <summary>
        /// Width of font character 
        /// </summary>
        public override int Width { get { return 8; } }

        /// <summary>
        /// Height of font character.
        /// </summary> 
        public override int Height { get { return 8; } }

        /// <summary>
        ///     Font table containing the binary representation of ASCII characters.
        /// </summary>
        private static readonly byte[][] _fontTable =
            {
            new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},   // U+0020 (space)
            new byte [] { 0x18, 0x3C, 0x3C, 0x18, 0x18, 0x00, 0x18, 0x00},   // U+0021 (!)
            new byte [] { 0x36, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},   // U+0022 (")
            new byte [] { 0x36, 0x36, 0x7F, 0x36, 0x7F, 0x36, 0x36, 0x00},   // U+0023 (#)
            new byte [] { 0x0C, 0x3E, 0x03, 0x1E, 0x30, 0x1F, 0x0C, 0x00},   // U+0024 ($)
            new byte [] { 0x00, 0x63, 0x33, 0x18, 0x0C, 0x66, 0x63, 0x00},   // U+0025 (%)
            new byte [] { 0x1C, 0x36, 0x1C, 0x6E, 0x3B, 0x33, 0x6E, 0x00},   // U+0026 (&)
            new byte [] { 0x06, 0x06, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00},   // U+0027 (')
            new byte [] { 0x18, 0x0C, 0x06, 0x06, 0x06, 0x0C, 0x18, 0x00},   // U+0028 (()
            new byte [] { 0x06, 0x0C, 0x18, 0x18, 0x18, 0x0C, 0x06, 0x00},   // U+0029 ())
            new byte [] { 0x00, 0x66, 0x3C, 0xFF, 0x3C, 0x66, 0x00, 0x00},   // U+002A (*)
            new byte [] { 0x00, 0x0C, 0x0C, 0x3F, 0x0C, 0x0C, 0x00, 0x00},   // U+002B (+)
            new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x0C, 0x06},   // U+002C (,)
            new byte [] { 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x00},   // U+002D (-)
            new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x0C, 0x00},   // U+002E (.)
            new byte [] { 0x60, 0x30, 0x18, 0x0C, 0x06, 0x03, 0x01, 0x00},   // U+002F (/)
            new byte [] { 0x3E, 0x63, 0x73, 0x7B, 0x6F, 0x67, 0x3E, 0x00},   // U+0030 (0)
            new byte [] { 0x0C, 0x0E, 0x0C, 0x0C, 0x0C, 0x0C, 0x3F, 0x00},   // U+0031 (1)
            new byte [] { 0x1E, 0x33, 0x30, 0x1C, 0x06, 0x33, 0x3F, 0x00},   // U+0032 (2)
            new byte [] { 0x1E, 0x33, 0x30, 0x1C, 0x30, 0x33, 0x1E, 0x00},   // U+0033 (3)
            new byte [] { 0x38, 0x3C, 0x36, 0x33, 0x7F, 0x30, 0x78, 0x00},   // U+0034 (4)
            new byte [] { 0x3F, 0x03, 0x1F, 0x30, 0x30, 0x33, 0x1E, 0x00},   // U+0035 (5)
            new byte [] { 0x1C, 0x06, 0x03, 0x1F, 0x33, 0x33, 0x1E, 0x00},   // U+0036 (6)
            new byte [] { 0x3F, 0x33, 0x30, 0x18, 0x0C, 0x0C, 0x0C, 0x00},   // U+0037 (7)
            new byte [] { 0x1E, 0x33, 0x33, 0x1E, 0x33, 0x33, 0x1E, 0x00},   // U+0038 (8)
            new byte [] { 0x1E, 0x33, 0x33, 0x3E, 0x30, 0x18, 0x0E, 0x00},   // U+0039 (9)
            new byte [] { 0x00, 0x0C, 0x0C, 0x00, 0x00, 0x0C, 0x0C, 0x00},   // U+003A (:)
            new byte [] { 0x00, 0x0C, 0x0C, 0x00, 0x00, 0x0C, 0x0C, 0x06},   // U+003B (;)
            new byte [] { 0x18, 0x0C, 0x06, 0x03, 0x06, 0x0C, 0x18, 0x00},   // U+003C (<)
            new byte [] { 0x00, 0x00, 0x3F, 0x00, 0x00, 0x3F, 0x00, 0x00},   // U+003D (=)
            new byte [] { 0x06, 0x0C, 0x18, 0x30, 0x18, 0x0C, 0x06, 0x00},   // U+003E (>)
            new byte [] { 0x1E, 0x33, 0x30, 0x18, 0x0C, 0x00, 0x0C, 0x00},   // U+003F (?)
            new byte [] { 0x3E, 0x63, 0x7B, 0x7B, 0x7B, 0x03, 0x1E, 0x00},   // U+0040 (@)
            new byte [] { 0x0C, 0x1E, 0x33, 0x33, 0x3F, 0x33, 0x33, 0x00},   // U+0041 (A)
            new byte [] { 0x3F, 0x66, 0x66, 0x3E, 0x66, 0x66, 0x3F, 0x00},   // U+0042 (B)
            new byte [] { 0x3C, 0x66, 0x03, 0x03, 0x03, 0x66, 0x3C, 0x00},   // U+0043 (C)
            new byte [] { 0x1F, 0x36, 0x66, 0x66, 0x66, 0x36, 0x1F, 0x00},   // U+0044 (D)
            new byte [] { 0x7F, 0x46, 0x16, 0x1E, 0x16, 0x46, 0x7F, 0x00},   // U+0045 (E)
            new byte [] { 0x7F, 0x46, 0x16, 0x1E, 0x16, 0x06, 0x0F, 0x00},   // U+0046 (F)
            new byte [] { 0x3C, 0x66, 0x03, 0x03, 0x73, 0x66, 0x7C, 0x00},   // U+0047 (G)
            new byte [] { 0x33, 0x33, 0x33, 0x3F, 0x33, 0x33, 0x33, 0x00},   // U+0048 (H)
            new byte [] { 0x1E, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x1E, 0x00},   // U+0049 (I)
            new byte [] { 0x78, 0x30, 0x30, 0x30, 0x33, 0x33, 0x1E, 0x00},   // U+004A (J)
            new byte [] { 0x67, 0x66, 0x36, 0x1E, 0x36, 0x66, 0x67, 0x00},   // U+004B (K)
            new byte [] { 0x0F, 0x06, 0x06, 0x06, 0x46, 0x66, 0x7F, 0x00},   // U+004C (L)
            new byte [] { 0x63, 0x77, 0x7F, 0x7F, 0x6B, 0x63, 0x63, 0x00},   // U+004D (M)
            new byte [] { 0x63, 0x67, 0x6F, 0x7B, 0x73, 0x63, 0x63, 0x00},   // U+004E (N)
            new byte [] { 0x1C, 0x36, 0x63, 0x63, 0x63, 0x36, 0x1C, 0x00},   // U+004F (O)
            new byte [] { 0x3F, 0x66, 0x66, 0x3E, 0x06, 0x06, 0x0F, 0x00},   // U+0050 (P)
            new byte [] { 0x1E, 0x33, 0x33, 0x33, 0x3B, 0x1E, 0x38, 0x00},   // U+0051 (Q)
            new byte [] { 0x3F, 0x66, 0x66, 0x3E, 0x36, 0x66, 0x67, 0x00},   // U+0052 (R)
            new byte [] { 0x1E, 0x33, 0x07, 0x0E, 0x38, 0x33, 0x1E, 0x00},   // U+0053 (S)
            new byte [] { 0x3F, 0x2D, 0x0C, 0x0C, 0x0C, 0x0C, 0x1E, 0x00},   // U+0054 (T)
            new byte [] { 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x3F, 0x00},   // U+0055 (U)
            new byte [] { 0x33, 0x33, 0x33, 0x33, 0x33, 0x1E, 0x0C, 0x00},   // U+0056 (V)
            new byte [] { 0x63, 0x63, 0x63, 0x6B, 0x7F, 0x77, 0x63, 0x00},   // U+0057 (W)
            new byte [] { 0x63, 0x63, 0x36, 0x1C, 0x1C, 0x36, 0x63, 0x00},   // U+0058 (X)
            new byte [] { 0x33, 0x33, 0x33, 0x1E, 0x0C, 0x0C, 0x1E, 0x00},   // U+0059 (Y)
            new byte [] { 0x7F, 0x63, 0x31, 0x18, 0x4C, 0x66, 0x7F, 0x00},   // U+005A (Z)
            new byte [] { 0x1E, 0x06, 0x06, 0x06, 0x06, 0x06, 0x1E, 0x00},   // U+005B ([)
            new byte [] { 0x03, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x40, 0x00},   // U+005C (\)
            new byte [] { 0x1E, 0x18, 0x18, 0x18, 0x18, 0x18, 0x1E, 0x00},   // U+005D (])
            new byte [] { 0x08, 0x1C, 0x36, 0x63, 0x00, 0x00, 0x00, 0x00},   // U+005E (^)
            new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF},   // U+005F (_)
            new byte [] { 0x0C, 0x0C, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00},   // U+0060 (`)
            new byte [] { 0x00, 0x00, 0x1E, 0x30, 0x3E, 0x33, 0x6E, 0x00},   // U+0061 (a)
            new byte [] { 0x07, 0x06, 0x06, 0x3E, 0x66, 0x66, 0x3B, 0x00},   // U+0062 (b)
            new byte [] { 0x00, 0x00, 0x1E, 0x33, 0x03, 0x33, 0x1E, 0x00},   // U+0063 (c)
            new byte [] { 0x38, 0x30, 0x30, 0x3e, 0x33, 0x33, 0x6E, 0x00},   // U+0064 (d)
            new byte [] { 0x00, 0x00, 0x1E, 0x33, 0x3f, 0x03, 0x1E, 0x00},   // U+0065 (e)
            new byte [] { 0x1C, 0x36, 0x06, 0x0f, 0x06, 0x06, 0x0F, 0x00},   // U+0066 (f)
            new byte [] { 0x00, 0x00, 0x6E, 0x33, 0x33, 0x3E, 0x30, 0x1F},   // U+0067 (g)
            new byte [] { 0x07, 0x06, 0x36, 0x6E, 0x66, 0x66, 0x67, 0x00},   // U+0068 (h)
            new byte [] { 0x0C, 0x00, 0x0E, 0x0C, 0x0C, 0x0C, 0x1E, 0x00},   // U+0069 (i)
            new byte [] { 0x30, 0x00, 0x30, 0x30, 0x30, 0x33, 0x33, 0x1E},   // U+006A (j)
            new byte [] { 0x07, 0x06, 0x66, 0x36, 0x1E, 0x36, 0x67, 0x00},   // U+006B (k)
            new byte [] { 0x0E, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x1E, 0x00},   // U+006C (l)
            new byte [] { 0x00, 0x00, 0x33, 0x7F, 0x7F, 0x6B, 0x63, 0x00},   // U+006D (m)
            new byte [] { 0x00, 0x00, 0x1F, 0x33, 0x33, 0x33, 0x33, 0x00},   // U+006E (n)
            new byte [] { 0x00, 0x00, 0x1E, 0x33, 0x33, 0x33, 0x1E, 0x00},   // U+006F (o)
            new byte [] { 0x00, 0x00, 0x3B, 0x66, 0x66, 0x3E, 0x06, 0x0F},   // U+0070 (p)
            new byte [] { 0x00, 0x00, 0x6E, 0x33, 0x33, 0x3E, 0x30, 0x78},   // U+0071 (q)
            new byte [] { 0x00, 0x00, 0x3B, 0x6E, 0x66, 0x06, 0x0F, 0x00},   // U+0072 (r)
            new byte [] { 0x00, 0x00, 0x3E, 0x03, 0x1E, 0x30, 0x1F, 0x00},   // U+0073 (s)
            new byte [] { 0x08, 0x0C, 0x3E, 0x0C, 0x0C, 0x2C, 0x18, 0x00},   // U+0074 (t)
            new byte [] { 0x00, 0x00, 0x33, 0x33, 0x33, 0x33, 0x6E, 0x00},   // U+0075 (u)
            new byte [] { 0x00, 0x00, 0x33, 0x33, 0x33, 0x1E, 0x0C, 0x00},   // U+0076 (v)
            new byte [] { 0x00, 0x00, 0x63, 0x6B, 0x7F, 0x7F, 0x36, 0x00},   // U+0077 (w)
            new byte [] { 0x00, 0x00, 0x63, 0x36, 0x1C, 0x36, 0x63, 0x00},   // U+0078 (x)
            new byte [] { 0x00, 0x00, 0x33, 0x33, 0x33, 0x3E, 0x30, 0x1F},   // U+0079 (y)
            new byte [] { 0x00, 0x00, 0x3F, 0x19, 0x0C, 0x26, 0x3F, 0x00},   // U+007A (z)
            new byte [] { 0x38, 0x0C, 0x0C, 0x07, 0x0C, 0x0C, 0x38, 0x00},   // U+007B ({)
            new byte [] { 0x18, 0x18, 0x18, 0x00, 0x18, 0x18, 0x18, 0x00},   // U+007C (|)
            new byte [] { 0x07, 0x0C, 0x0C, 0x38, 0x0C, 0x0C, 0x07, 0x00},   // U+007D (})
            new byte [] { 0x6E, 0x3B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},   // U+007E (~)
            new byte [] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}    // U+007F
			};

        /// <summary>
        /// Get the binary representation of an ASCII character from the font table.
        /// </summary>
        /// <param name="character">Character to look up.</param>
        /// <returns>
        /// Byte array containing the rows of pixels in the character.
        /// </returns>
        public override byte[] this[char character]
        {
            get
            {
                var index = (byte)character;
                if ((index < 32) || (index > 127))
                {
                    return _fontTable[0x20];
                }
                return _fontTable[(byte)character - 0x20];
            }
        }
    }

}
