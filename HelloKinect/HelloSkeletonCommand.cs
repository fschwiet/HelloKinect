using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Newtonsoft.Json;

namespace HelloKinect
{
    public class HelloSkeletonCommand : SkeletonCommandBase
    {
        public HelloSkeletonCommand()
        {
            this.IsCommand("hello-skeleton");
        }

        protected override void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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
