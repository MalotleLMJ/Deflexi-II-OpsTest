using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deflexi_II_OpsTest
{
    public partial class Serialnum : Form
    {
        public string sernum;
        public bool sernumState = false;
        public Serialnum()
        {
            InitializeComponent();
            textBox1.Clear();
        }

        public void GetSeNum()
        {
            sernum = textBox1.Text;
        }

        private void Serialnum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sernumState = true;
                GetSeNum();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
          
            }
        }
    }
}
