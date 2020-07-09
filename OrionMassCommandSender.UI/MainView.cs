using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OrionMassCommandSender.UI
{
    public partial class MainView : Form
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void readCommandFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog opf = new OpenFileDialog())
            {
                if (opf.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(opf.OpenFile(), Encoding.Default))
                    {
                        while (!sr.EndOfStream)
                        {
                            listBox1.Items.Add(sr.ReadLine());
                        }
                    }
                }
            }
        }

        private void sendCommandButton_Click(object sender, EventArgs e)
        {
            string currentCommand = listBox1.SelectedItem as string;
            if (currentCommand != null)
            {
                MessageBox.Show(currentCommand);
            }
        }
    }
}
