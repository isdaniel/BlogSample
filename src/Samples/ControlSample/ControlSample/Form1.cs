using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;

namespace ControlSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Data Source=127.0.0.1,1433;Database=MYDB;;User Id=sa;Password=test.123"))
            {
                conn.Open();
                var list = conn.Query<ChatSettingModel>("SELECT * FROM dbo.movie");
                dataGridView1.DataSource = list;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["chb1"].Value != null && 
                    (bool)row.Cells["chb1"].Value == true)
                {
                    MessageBox.Show(row.Cells["movieID"].Value.ToString());
                }
            }
            //dataGridView1.Rows
        }
    }
    public class ChatSettingModel
    {
        public int? movieID { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; }
        public string Director { get; set; }
    }
}
