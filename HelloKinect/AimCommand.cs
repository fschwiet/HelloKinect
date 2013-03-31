using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManyConsole;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class AimCommand : SkeletonCommandBase
    {
        public AimCommand()
        {
            this.IsCommand("aim", "Aims the kinect sensor to vertical contain the skeleton model.");
        }

        protected override void SkeletonFrameReady(object sender, Microsoft.Kinect.SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                var skeleton = GetSkeletons(frame).FirstOrDefault(skele => skele.TrackingState != SkeletonTrackingState.NotTracked);

                if (skeleton == null)
                {
                    Console.Write(".");
                    return;
                }

                var clipped = GetClippedEdges(skeleton);

                foreach(var edge in clipped)
                    Console.WriteLine("clipping " + edge);
            }
        }
    }
}
