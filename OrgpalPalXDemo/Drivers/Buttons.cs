using System;
using Iot.Device.Button;
using System.Diagnostics;

namespace PalX.Drivers
{
    public class Buttons : IDisposable
    {
        private bool _disposed;

        public GpioButton BootOneButton;
        public GpioButton DiagnosticButton;
        //private readonly GpioButton _resetButton;
        public GpioButton BootZeroButton;
        public GpioButton WakeButton;


        public Buttons()
        {

            BootOneButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_BOOT1);
            BootOneButton.Press += BootOneButton_Press;

            DiagnosticButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_DIAGNOSTIC);
            DiagnosticButton.Press += DiagnosticButton_Press;

            BootZeroButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_BOOT0);
            BootZeroButton.Press += BootZeroButton_Press;

            WakeButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_WAKE);
            WakeButton.Press += WakeButton_Press;
        }


        private static void BootOneButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Boot1 Button pressed...!");
        }


        private static void DiagnosticButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Diagnostic Button pressed...!");
        }


        private static void BootZeroButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Boot0 Button pressed...!");
        }


        private static void WakeButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Wake Button pressed...!");
        }



        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) { return; };
            BootOneButton.Press -= BootOneButton_Press;
            BootOneButton.Dispose();
            WakeButton.Press -= WakeButton_Press;
            WakeButton.Dispose();
            DiagnosticButton.Press -= DiagnosticButton_Press;
            DiagnosticButton.Dispose();
            BootZeroButton.Press -= BootZeroButton_Press;
            BootZeroButton.Dispose();
        }

        /// <inheritdoc/>
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
