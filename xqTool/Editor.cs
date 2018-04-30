using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using xqLib;

namespace xqTool
{
    public partial class Editor : Form
    {
        private XqManager _xq;

        public Editor()
        {
            InitializeComponent();

            saveFileDialog1.CreatePrompt = false;
            saveFileDialog1.OverwritePrompt = true;
        }

        private void loadFile()
        {
            textBox1.Clear();

            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var data = new List<string>();

                foreach (var filename in openFileDialog1.FileNames)
                {
                    var stream = File.OpenRead(filename);

                    var xq = XqManager.FromStream(stream);
                    var cmds = xq.GetDebugData();

                    data.AddRange(cmds);
                }

                foreach (var item in data)
                {
                    textBox1.AppendText(item);
                    textBox1.AppendText(Environment.NewLine);
                }

                Activate();
            }

            /*

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _xq = XqManager.FromStream(openFileDialog1.OpenFile());

                foreach (var cmd in _xq.GetDebugData())
                {
                    textBox1.AppendText(cmd);
                    textBox1.AppendText(Environment.NewLine);
                }

                Activate();
            }
            */
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            loadFile();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var stream = saveFileDialog1.OpenFile();
                _xq.Save(stream);
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            loadFile();
        }
    }
}