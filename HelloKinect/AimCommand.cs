using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ManyConsole;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class AimCommand : ConsoleCommand
    {
        public int Steps = 14;

        public AimCommand()
        {
            this.IsCommand("aim", "Aims the kinect sensor to vertical contain the skeleton model.");
        }

        public override int Run(string[] remainingArguments)
        {
            var sensor = KinectUtil.GetKinectSensor();

            sensor.SkeletonStream.Enable();

            sensor.Start();

            Console.WriteLine("Elevation is {0} (range: {1} - {2}). ",sensor.ElevationAngle, sensor.MinElevationAngle, sensor.MaxElevationAngle);

            var start = sensor.MinElevationAngle;
            var stop = sensor.MaxElevationAngle;
            var step = (stop - start) / (double)(Steps-1);

            Dictionary<int, IEnumerable<FrameEdges>> clippedTracking = new Dictionary<int, IEnumerable<FrameEdges>>();

            for (var i = 0; i < Steps; i++)
            {
                ChangeAngle(sensor, (int)(start + step * i));

                var clipped = GetClipped(sensor);

                if (clipped != null)
                {
                    Console.WriteLine("Angle: {0}, CrossedEdges: {1}", sensor.ElevationAngle, string.Join(",", clipped));
                    clippedTracking[sensor.ElevationAngle] = clipped.Where(c => c != FrameEdges.None).ToArray();
                }
                else
                {
                    //  Apparently we've gone past the region the user is in
                    if (clippedTracking.Any())
                    {
                        break;
                    }
                }
            }

            var chosenAngle = clippedTracking.Keys.First(key => !clippedTracking[key].Any());

            Console.WriteLine("Aiming for " + chosenAngle);

            ChangeAngle(sensor, chosenAngle);

            return 0;
        }

        private static List<FrameEdges> GetClipped(KinectSensor sensor)
        {
            var attemptsLeft = 10;

            while (true)
            {
                using (var frame = sensor.SkeletonStream.OpenNextFrame(1000))
                {
                    var skeletons = SkeletonCommandBase.GetSkeletons(frame);

                    var skeleton = skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked).FirstOrDefault();

                    if (skeleton == null)
                    {
                        Console.Write(".");
                        Thread.Sleep(1000);

                        if (attemptsLeft-- > 0)
                        {
                            continue;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return SkeletonCommandBase.GetClippedEdges(skeleton);
                }
            }

        }

        private static void ChangeAngle(KinectSensor sensor, int target)
        {
            sensor.ElevationAngle = target;

            int matchCount = 0;

            do
            {
                matchCount = 0;
                var separator = "";
                for (var wait = 0; wait < 4; wait++)
                {
                    Thread.Sleep(250);
                    var elevationAngle = sensor.ElevationAngle;
                    if (Math.Abs(elevationAngle - target) <= 1)
                        matchCount++;
                    separator = ", ";
                }
            } while (matchCount < 4);
        }
    }
}
