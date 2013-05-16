using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HFS
{
    public partial class SettingsWindow : Form
    {
        public UInt16 Port { get; set; }

        public SettingsWindow(Int32 port)
        {
            InitializeComponent();

            textBox1.Text = port.ToString();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            UInt16 port;

            if (UInt16.TryParse(textBox1.Text, out port))
            {
                Port = port;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            else
                MessageBox.Show("Hibás portszám!");
        }
    }
}
