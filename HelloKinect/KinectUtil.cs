using System.Linq;
using ManyConsole;
using Microsoft.Kinect;


namespace HelloKinect
{
public class KinectUtil
    {
        public static KinectSensor GetKinectSensor()
        {
            var sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (sensor == null)
            {
                throw new ConsoleHelpAsException("Kinect was not detected");
            }
            return sensor;
        }
    }
}