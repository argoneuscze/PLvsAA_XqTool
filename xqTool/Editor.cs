using System;
using System.Windows.Forms;
using xqLib;

namespace xqTool
{
    public partial class Editor : Form
    {
        private Xq _xq;

        public Editor()
        {
            InitializeComponent();

            saveFileDialog1.CreatePrompt = false;
            saveFileDialog1.OverwritePrompt = true;
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _xq = new Xq(openFileDialog1.OpenFile());

                foreach (var cmd in _xq.debug)
                {
                    textBox1.AppendText(cmd);
                    textBox1.AppendText(Environment.NewLine);
                }

                Activate();
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var stream = saveFileDialog1.OpenFile();
                _xq.Save(stream);
            }
        }
    }
}