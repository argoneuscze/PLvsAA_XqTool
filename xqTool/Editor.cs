using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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

        private void LoadFile()
        {
            Hide();
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
                    //var cmds = xq.dumpStrings();
                    //var cmds = xq.dumpBuiltinFunctions("EventMapFadeRGB3", "EventMapFadeRGB4", "EventMapFadeRGB");

                    data.AddRange(cmds);

                    _xq = xq;
                }

                foreach (var item in data)
                {
                    textBox1.AppendText(item);
                    textBox1.AppendText(Environment.NewLine);
                }

                Show();
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
            LoadFile();
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
            LoadFile();
        }
    }
}