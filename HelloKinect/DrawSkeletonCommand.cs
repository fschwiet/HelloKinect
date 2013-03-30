using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class DrawSkeletonCommand : SkeletonCommandBase
    {
        protected Form _displayForm;

        public DrawSkeletonCommand()
        {
            this.IsCommand("draw-skeleton");
        }

        public override int Run(string[] remainingArguments)
        {
            _displayForm = new Form();

            _displayForm.Width = 640;
            _displayForm.Height = 480;

            _displayForm.Show();

            return base.Run(remainingArguments);
        }

        protected override void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeleton = e.OpenSkeletonFrame())
            {
                Skeleton[] data = new Skeleton[skeleton.SkeletonArrayLength];
                skeleton.CopySkeletonDataTo(data);

                var skele = data.FirstOrDefault();

                if (skele == null)
                    return;

                if (skele.TrackingState != SkeletonTrackingState.Tracked)
                {
                    Console.Write(".");
                    return;
                }

                using (var graphics = _displayForm.CreateGraphics())
                {
                    graphics.Clear(Color.Black);

                    foreach (var jointType in Enum.GetValues(typeof(JointType)).Cast<JointType>())
                    {
                        Joint joint = skele.Joints[jointType];

                        Color? color = null;

                        switch (joint.TrackingState)
                        {
                            case JointTrackingState.Tracked:
                                color = Color.Aqua;
                                break;
                            case JointTrackingState.Inferred:
                                color = Color.Orange;
                                break;

                        }

                        var position = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, DepthImageFormat.Resolution640x480Fps30);
                        graphics.DrawString(jointType.ToString(), new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(color.Value), position.X, position.Y);
                    }
                }
            }
        }
    }
}
