using System;
using System.Windows.Forms;
using xqLib;

namespace xqTool
{
    public partial class Editor : Form
    {
        public Editor()
        {
            InitializeComponent();
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Activate();

                var xq = new Xq(openFileDialog1.OpenFile());

                foreach (var cmd in xq.debug)
                {
                    textBox1.AppendText(cmd);
                    textBox1.AppendText(Environment.NewLine);
                }
            }
        }
    }
}