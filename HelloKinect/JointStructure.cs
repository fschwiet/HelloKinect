using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class JointStructure
    {
        public static Dictionary<JointType, IEnumerable<JointType>> Connections = new Dictionary<JointType, IEnumerable<JointType>>()
        {
            {JointType.HipCenter, new [] { JointType.Spine, JointType.HipLeft, JointType.HipRight}},
                {JointType.Spine, new [] { JointType.ShoulderCenter}},
                    {JointType.ShoulderCenter, new [] { JointType.ShoulderLeft, JointType.Head,JointType.ShoulderRight, }},
                        {JointType.ShoulderLeft, new [] { JointType.ElbowLeft, }},
                        {JointType.ShoulderRight, new [] { JointType.ElbowRight, }},
                            {JointType.ElbowLeft, new [] { JointType.WristLeft, }},
                            {JointType.ElbowRight, new [] { JointType.WristRight, }},
                                {JointType.WristLeft, new [] { JointType.HandLeft, }},
                                {JointType.WristRight, new [] { JointType.HandRight, }},
                {JointType.HipLeft, new [] {JointType.KneeLeft, }},
                {JointType.HipRight, new [] {JointType.KneeRight, }},
                    {JointType.KneeLeft, new [] {JointType.AnkleLeft, }},
                    {JointType.KneeRight, new [] {JointType.AnkleRight, }},
                        {JointType.AnkleLeft, new [] {JointType.FootLeft, }},
                        {JointType.AnkleRight, new [] {JointType.FootRight, }},

        };
    }
}
