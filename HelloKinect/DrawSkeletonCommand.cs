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

        public DrawSkeletonCommand()
        {
            this.IsCommand("draw-skeleton");
        }

        public override int Run(string[] remainingArguments)
        {
            _displayForm = new Form();

            _displayForm.Width = DrawWidth;
            _displayForm.Height = DrawHeight;

            _displayForm.Show();

            return base.Run(remainingArguments);
        }

        protected override void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                using (var frame = e.OpenSkeletonFrame())
                {
                    var data = GetSkeletons(frame);

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

                        var jointTypes = Enum.GetValues(typeof (JointType)).Cast<JointType>();

                        var points = ExtractScreenPoints(jointTypes, skele);

                        DrawClippedEdges(skele, graphics);

                        DrawJointConnections(jointTypes, points, graphics);

                        DrawJoints(points, graphics);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        Dictionary<FrameEdges,Tuple<Point,Point>> EdgeHazardLights = new Dictionary<FrameEdges, Tuple<Point, Point>>()
        {
            {FrameEdges.Bottom, new Tuple<Point, Point>(new Point(0,0), new Point(DrawWidth-1,0))},
            {FrameEdges.Top, new Tuple<Point, Point>(new Point(0,DrawHeight-1), new Point(DrawWidth-1,DrawHeight-1))},
            {FrameEdges.Left, new Tuple<Point, Point>(new Point(0, DrawHeight-1), new Point(0,0))},
            {FrameEdges.Right, new Tuple<Point, Point>(new Point(DrawWidth-1,0),new Point(DrawWidth-1,DrawHeight-1))}
        };

        private void DrawClippedEdges(Skeleton skele, Graphics graphics)
        {
            foreach (var edge in GetClippedEdges(skele))
            {
                if (EdgeHazardLights.ContainsKey(edge))
                {
                    var coordinates = EdgeHazardLights[edge];
                    graphics.DrawLine(new Pen(Color.Yellow, 20), coordinates.Item1, coordinates.Item2);
                }
            }
        }

        private static void DrawJoints(Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points, Graphics graphics)
        {
            foreach (var point in points)
            {
                if (point.Value.Item1.HasValue)
                {
                    graphics.FillEllipse(new SolidBrush(point.Value.Item1.Value), point.Value.Item2.X, point.Value.Item2.Y, 5,5);
                }
            }
        }

        private static void DrawJointConnections(IEnumerable<JointType> jointTypes, Dictionary<JointType, Tuple<Color?, DepthImagePoint>> points, Graphics graphics)
        {
            var grayPen = new Pen(Color.Gray);

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

                    graphics.DrawLine(grayPen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
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
                    joint.Position, DepthImageFormat.Resolution640x480Fps30);
                points.Add(jointType, new Tuple<Color?, DepthImagePoint>(color, position));
            }
            return points;
        }
    }
}
