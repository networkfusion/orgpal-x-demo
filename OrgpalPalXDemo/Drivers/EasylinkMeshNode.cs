//using nanoFramework.TI.EasyLink;
//using System;
//using System.Diagnostics;
//using System.Threading;

//namespace PalX.Drivers
//{
//    internal class EasylinkMeshNode : IDisposable
//    {
//        private bool _disposed;
//        const byte s_concentratorAddress = 0x00; // Gateway address
//        static byte s_nodeAddress = 0x33; // This node address
//        private readonly EasyLinkController controller;

//        public EasylinkMeshNode()
//        {
//            controller = new EasyLinkController(PhyType._5kbpsSlLr);

//            // need to initialize the EasyLink layer on the target before any operation is allowed
//            var initResult = controller.Initialize();

//            if (initResult == Status.Success)
//            {
//                //controller.AddAddressToFilter(new byte[] { s_concentratorAddress });

//                // start a debug thread.
//                new Thread(() =>
//                {
//                    DebugReader();
//                }).Start();

//            }
//            else
//            {
//                Debug.WriteLine($"Failed to initialize SimpleLink. Error: {initResult}");
//            }


//        }

//        void DebugReader()
//        {
//            var destinationAddress = new byte[] { s_concentratorAddress };

//            byte counter = 0;
//            for (;;)
//            {
//                var packet = new TransmitPacket(
//                                                destinationAddress,
//                                                new byte[] { counter++ }
//                                            );

//                var txResult = controller.Transmit(packet);

//                if (txResult == Status.Success)
//                {
//                    Debug.WriteLine($"Tx packet: {packet.Payload[0]}");
//                }
//                else
//                {
//                    Debug.WriteLine($"Error when Tx'ing: {txResult}");
//                }

//                Thread.Sleep(3000);
//            }
//        }

//        /// <summary>
//        /// Releases unmanaged resources
//        /// and optionally release managed resources
//        /// </summary>
//        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (_disposed) return;
//            controller.Dispose();
//        }

//        public void Dispose()
//        {
//            if (!_disposed)
//            {
//                Dispose(true);
//                _disposed = true;
//            }

//            GC.SuppressFinalize(this);
//        }
//    }
//}
