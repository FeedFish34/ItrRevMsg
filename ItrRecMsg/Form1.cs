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
        OCRSerialDevice ocr = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ocr = new OCRSerialDevice();
            ocr.StartWorking();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(ocr != null)
            {
                ocr.StartWorking();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ocr != null)
            {
                ocr.StopWork();
            }
        }
    }
}
