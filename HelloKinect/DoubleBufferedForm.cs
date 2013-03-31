using System.Windows.Forms;

namespace HelloKinect
{
    public class DoubleBufferedForm : Form
    {
        public DoubleBufferedForm()
        {
            DoubleBuffered = true;
        }
    }
}