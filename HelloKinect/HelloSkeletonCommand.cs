using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ManyConsole;
using Microsoft.Kinect;
using Newtonsoft.Json;

namespace HelloKinect
{
    public class HelloSkeletonCommand : ConsoleCommand
    {
        public HelloSkeletonCommand()
        {
            this.IsCommand("hello-skeleton");
        }

        public override int Run(string[] remainingArguments)
        {
            var sensor = KinectUtil.GetKinectSensor();

            sensor.SkeletonFrameReady += SkeletonFrameReady;

            sensor.SkeletonStream.Enable();

            sensor.Start();

            Application.Run();

            return 0;
        }

        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeleton = e.OpenSkeletonFrame())
            {
                Skeleton[] data = new Skeleton[skeleton.SkeletonArrayLength];
                skeleton.CopySkeletonDataTo(data);

                int count = 0;
                foreach (var skele in data)
                {
                    Console.WriteLine("#{0}", ++count);
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(skele, Formatting.Indented));
                }
            }
        }
    }
}
