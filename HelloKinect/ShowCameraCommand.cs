using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ManyConsole;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class ShowCameraCommand : ConsoleCommand
    {
        static private Form EchoForm;
        private bool UsePolling;

        public ShowCameraCommand()
        {
            this.IsCommand("show-camera");
            this.HasOption("p", "Use polling to check frame data", v => UsePolling = true);
        }

        public override int Run(string[] remainingArguments)
        {
            var sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (sensor == null)
            {
                Console.WriteLine("Kinect was not detected");
                Console.WriteLine();
                return -1;
            }

            EchoForm = new Form();

            EchoForm.Width = 640;
            EchoForm.Height = 480;

            EchoForm.Show();

            sensor.ColorStream.Enable(ColorImageFormat.RawYuvResolution640x480Fps15);

            if (!UsePolling)
            {
                sensor.ColorFrameReady += sensor_ColorFrameReady;
            }

            sensor.Start();

            if (UsePolling)
            {
                Console.WriteLine("Use any key to exit.");
                while (!Console.KeyAvailable)
                {
                    using (var frame = sensor.ColorStream.OpenNextFrame(10 * 1000))
                    {
                        HandleFrame(frame);
                    }

                    Thread.Sleep(50);
                }
            }
            else
            {
                Console.WriteLine("Hit return to exit.");
                Console.ReadLine();
            }

            return 0;
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            Console.WriteLine("Frame received");
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                HandleFrame(frame);
            }
        }

        private void HandleFrame(ColorImageFrame frame)
        {
            var bitmap = ImageToBitmap(frame);

            using (var g = EchoForm.CreateGraphics())
            {
                g.DrawImage(bitmap, 0, 0);
                Console.WriteLine("Frame drawn");
            }
        }

        // http://stackoverflow.com/questions/10848190/convert-kinect-colorframe-to-bitmap
        Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            byte[] pixeldata = new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixeldata);
            Bitmap bmap = new Bitmap(Image.Width, Image.Height, PixelFormat.Format32bppRgb);
            BitmapData bmapdata = bmap.LockBits(
                new Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.WriteOnly,
                bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }
    }
}
