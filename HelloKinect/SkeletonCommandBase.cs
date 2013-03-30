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
    }
}