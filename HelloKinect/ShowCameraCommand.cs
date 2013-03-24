using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ManyConsole;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class ShowCameraCommand : ConsoleCommand
    {
        static private Form EchoForm;

        public ShowCameraCommand()
        {
            this.IsCommand("show-camera");
        }

        public override int Run(string[] remainingArguments)
        {
            if (KinectSensor.KinectSensors.Count() == 0)
            {
                Console.WriteLine("Kinect was not detected");
                Console.WriteLine();
                return -1;
            }

            EchoForm = new Form();

            EchoForm.Width = 640;
            EchoForm.Height = 480;

            EchoForm.Show();

            var sensor = KinectSensor.KinectSensors[0];

            sensor.ColorStream.Enable(ColorImageFormat.RawYuvResolution640x480Fps15);

            sensor.ColorFrameReady += sensor_ColorFrameReady;

            Console.WriteLine("Hit return to exit.");
            Console.ReadLine();

            return 0;
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame frame = e.OpenColorImageFrame();
            var bitmap = ImageToBitmap(frame);

            using (var g = EchoForm.CreateGraphics())
            {
                g.DrawImage(bitmap, 0, 0);
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
