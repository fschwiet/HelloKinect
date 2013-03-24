using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace HelloKinect
{
    public class Program
    {
        static public int Main(string[] args)
        {
            var commands = ManyConsole.ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
            return ManyConsole.ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }
    }
}
