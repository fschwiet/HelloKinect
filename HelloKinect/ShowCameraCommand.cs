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
        public ColorImageFormat Format = ColorImageFormat.RgbResolution640x480Fps30;
        private List<DateTime> FrameTimes;

        public ShowCameraCommand()
        {
            this.IsCommand("show-camera");
            this.HasOption<ColorImageFormat>("f=", "Format (one of " + string.Join(", ", Enum.GetValues(typeof (ColorImageFormat)).Cast<ColorImageFormat>().Select(v => v.ToString())) + ")", v => Format = v);
            
            FrameTimes = new List<DateTime>();
        }

        public override int Run(string[] remainingArguments)
        {
            var sensor = KinectUtil.GetKinectSensor();

            sensor.ColorStream.Enable(Format);

            EchoForm = new Form();

            EchoForm.Width = sensor.ColorStream.FrameWidth;
            EchoForm.Height = sensor.ColorStream.FrameHeight;

            EchoForm.Show();

            sensor.ColorFrameReady += sensor_ColorFrameReady;

            sensor.Start();

            Console.WriteLine("Running...");
            Application.Run();

            return 0;
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                    HandleFrame(frame);
            }
        }

        private void HandleFrame(ColorImageFrame frame)
        {
            var bitmap = ImageToBitmap(frame);

            using (var g = EchoForm.CreateGraphics())
            {
                g.DrawImage(bitmap, 0, 0);
            }

            FrameTimes.Add(DateTime.Now);

            if (FrameTimes.Count() > 10)
            {
                var reportedFrameTimes = FrameTimes.ToArray();
                FrameTimes = new List<DateTime>();

                TimeSpan sum = TimeSpan.FromMilliseconds(0);
                var size = 0;

                for (var i = 1; i < reportedFrameTimes.Length; i++)
                {
                    sum += reportedFrameTimes[i] - reportedFrameTimes[i - 1];
                    size++;
                }

                Console.WriteLine("Frame time: {0:00} ms.", (sum.TotalMilliseconds / size));
            }
        }

        // http://stackoverflow.com/questions/10848190/convert-kinect-colorframe-to-bitmap
        public static Bitmap ImageToBitmap(ColorImageFrame Image)
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
