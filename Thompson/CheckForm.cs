using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thompson
{
    public partial class CheckForm : Form
    {
        NFA nfa;
        public CheckForm(NFA nfa)
        {
            InitializeComponent();
            this.nfa = nfa;
        }

        private void but_check_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgv.Rows.Count - 1; ++i)
            {
                if (nfa.make(dgv.Rows[i].Cells[0].Value.ToString()))
                {
                    dgv.Rows[i].Cells[1].Value = "Верно";
                    dgv.Rows[i].Cells[1].Style.ForeColor = Color.Green;
                }
                else
                {
                    dgv.Rows[i].Cells[1].Value = "Не верно";
                    dgv.Rows[i].Cells[1].Style.ForeColor = Color.Red;
                }
            }
        }
    }
}
