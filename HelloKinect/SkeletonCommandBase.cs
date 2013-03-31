using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ManyConsole;
using Microsoft.Kinect;

namespace HelloKinect
{
    public abstract class SkeletonCommandBase : ConsoleCommand
    {
        protected abstract void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e);
        protected KinectSensor _sensor;

        public override int Run(string[] remainingArguments)
        {
            _sensor = KinectUtil.GetKinectSensor();

            _sensor.SkeletonFrameReady += SkeletonFrameReady;

            _sensor.SkeletonStream.Enable();

            _sensor.Start();

            Application.Run();

            return 0;
        }

        protected static Skeleton[] GetSkeletons(SkeletonFrame skeleton)
        {
            Skeleton[] data = new Skeleton[skeleton.SkeletonArrayLength];
            skeleton.CopySkeletonDataTo(data);
            return data;
        }

        protected static List<FrameEdges> GetClippedEdges(Skeleton skeleton)
        {
            List<FrameEdges> clipped = new List<FrameEdges>();

            foreach (var edge in Enum.GetValues(typeof (FrameEdges)).Cast<FrameEdges>())
            {
                if (skeleton.ClippedEdges.HasFlag(edge))
                    clipped.Add(edge);
            }

            return clipped;
        }
    }
}