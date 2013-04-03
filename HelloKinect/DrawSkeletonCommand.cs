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
        const int DrawWidth = 640;
        const int DrawHeight = 480;
        private const DepthImageFormat DepthImageFormat = Microsoft.Kinect.DepthImageFormat.Resolution640x480Fps30;
        private const ColorImageFormat ColorImageFormat = Microsoft.Kinect.ColorImageFormat.RgbResolution640x480Fps30;
        public DrawMessage nextMessage;

        Dictionary<FrameEdges, Tuple<Point, Point>> EdgeHazardLights = new Dictionary<FrameEdges, Tuple<Point, Point>>()
        {
            {FrameEdges.Top, new Tuple<Point, Point>(new Point(0,0), new Point(DrawWidth-1,0))},
            {FrameEdges.Bottom, new Tuple<Point, Point>(new Point(0,DrawHeight -1), new Point(DrawWidth-1,DrawHeight-1))},
            {FrameEdges.Left, new Tuple<Point, Point>(new Point(0, DrawHeight-1), new Point(0,0))},
            {FrameEdges.Right, new Tuple<Point, Point>(new Point(DrawWidth-1,0),new Point(DrawWidth-1,DrawHeight-1))}
        };

        public DrawSkeletonCommand()
        {
            this.IsCommand("draw-skeleton");
        }

        protected override void AdditionalSensorSetup()
        {
            _sensor.ColorStream.Enable(ColorImageFormat);
        }

        public override int Run(string[] remainingArguments)
        {
            _displayForm = new DoubleBufferedForm();
            var t = new Timer();
            t.Interval = 200;
            t.Tick += RedrawFrame;
            t.Enabled = true;

            var delta = _displayForm.Size - _displayForm.ClientSize;
            
            _displayForm.Size = new Size(DrawWidth + delta.Width, DrawHeight + delta.Height);

            _displayForm.Show();

            return base.Run(remainingArguments);
        }

        private void RedrawFrame(object sender, EventArgs e)
        {
            var message = nextMessage;

            if (message == null)
                return;

            using (var graphics = _displayForm.CreateGraphics())
            {
                using (var cameraFrame = _sensor.ColorStream.OpenNextFrame(10))
                {
                    if (cameraFrame != null)
                    {
                        graphics.DrawImage(ShowCameraCommand.ImageToBitmap(cameraFrame), 0, 0, _displayForm.ClientSize.Width, _displayForm.ClientSize.Height);
                    }
                }

                if (message.skele == null || message.skele.TrackingState == SkeletonTrackingState.NotTracked)
                {
                    graphics.DrawString("untracked", new Font(FontFamily.GenericSerif, 40), new SolidBrush(Color.Coral), DrawWidth / 4, DrawHeight / 4);
                    return;
                }

                DrawClippedEdges(message.skele, graphics);

                DrawJoints(message.points, graphics);

                DrawJointConnections(message.jointTypes, message.points, graphics);
            }
        }

        public class DrawMessage
        {
            public Skeleton skele;
            public IEnumerable<JointType> jointTypes;
            public Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points;
        }

        protected override void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                Skeleton[] data;

                using (var frame = e.OpenSkeletonFrame())
                {
                    if (frame == null)
                        return;

                    data = GetSkeletons(frame);
                }

                var message = new DrawMessage();

                message.skele = data.FirstOrDefault();

                if (message.skele != null && message.skele.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    message.jointTypes = Enum.GetValues(typeof(JointType)).Cast<JointType>();

                    message.points = ExtractScreenPoints(message.jointTypes, message.skele);                    
                }

                nextMessage = message;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void DrawClippedEdges(Skeleton skele, Graphics graphics)
        {
            foreach (var edge in GetClippedEdges(skele))
            {
                if (EdgeHazardLights.ContainsKey(edge))
                {
                    var coordinates = EdgeHazardLights[edge];
                    graphics.DrawLine(new Pen(Color.Yellow, 40), coordinates.Item1, coordinates.Item2);
                }
            }
        }

        private static void DrawJoints(Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points, Graphics graphics)
        {
            foreach (var point in points)
            {
                if (point.Value.Item1.HasValue)
                {
                    var radius = 15;
                    graphics.FillEllipse(new SolidBrush(point.Value.Item1.Value), point.Value.Item2.X - radius, point.Value.Item2.Y - radius, radius * 2,radius * 2);
                }
            }
        }

        private static void DrawJointConnections(IEnumerable<JointType> jointTypes, Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points, Graphics graphics)
        {
            var widePen = new Pen(Color.White, 10);
            var narrowPen = new Pen(Color.Black, 5);

            foreach (var startJoint in jointTypes)
            {
                if (!JointStructure.Connections.ContainsKey(startJoint))
                    continue;

                DepthImagePoint p = points[startJoint].Item2;

                if (!points[startJoint].Item1.HasValue)
                    continue;

                var startPoint = points[startJoint].Item2;

                foreach (var endJoint in JointStructure.Connections[startJoint])
                {
                    if (!points[endJoint].Item1.HasValue)
                        continue;

                    var endPoint = points[endJoint].Item2;

                    graphics.DrawLine(widePen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                    graphics.DrawLine(narrowPen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                }
            }
        }

        private Dictionary<JointType, Tuple<Color?, DepthImagePoint>> ExtractScreenPoints(IEnumerable<JointType> jointTypes, Skeleton skele)
        {
            Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points =
                new Dictionary<JointType, Tuple<Color?, DepthImagePoint>>();

            foreach (var jointType in jointTypes)
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

                DepthImagePoint position = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                    joint.Position, DepthImageFormat);
                points.Add(jointType, new Tuple<Color?, DepthImagePoint>(color, position));
            }
            return points;
        }
    }
}
