using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItrRecMsg
{
    public partial class Form1 : Form
    {
        BiuTCPServerPort port = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string IP = txtIP.Text;
            int Port = Convert.ToInt32(txtPort.Text);
            port = new BiuTCPServerPort(IP, Port);
            port.SpcialItrID = "ASTM";
            port.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(port != null)
            {
                port.Open();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (port != null)
            {
                port.Close();
            }
        }
    }
}
