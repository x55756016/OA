using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AttendaceAccount;

namespace SmtPortalSetUp
{
    public partial class Form_First : Form
    {
        public Form_First()
        {
            InitializeComponent();
            GlobalParameters.fromFisrt = this;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            Form form = null;
            switch (comboBoxType.SelectedItem.ToString())
            {
                case "个人考勤":
                    form = GlobalParameters.fromSecond;
                    if (form == null) form = new Form_Second();
                    break;
                case "公司考勤":
                    form = GlobalParameters.fromCompany;
                    if (form == null) form = new AttendCompany();
                    break;
            }
            form.Show();
            this.Hide();
        }

        private void Form_First_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
