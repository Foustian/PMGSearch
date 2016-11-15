using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace PMGSearchTestHarness
{
    public partial class ExcelToCSV : Form
    {
        DataTable dtData;
        public string GUIDList { get; set; }
        
        

        public ExcelToCSV()
        {
            InitializeComponent();
        }

        private void btnGenerateCSV_Click(object sender, EventArgs e)
        {
            ReadExcelFile();
            StringBuilder sb = new StringBuilder();
            if (dtData != null)
            {
                foreach (DataRow dr in dtData.Rows)
                {
                    if (!dr["GUID"].Equals(DBNull.Value))
                    {
                        sb.Append(dr["GUID"].ToString() + ",");
                    }
                }
                txtCSV.Text = sb.ToString(0, sb.ToString().Length - 1);
            }
        }

        private void ReadExcelFile()
        {
            if (txtPath.Text.Trim() != string.Empty)
            {
                string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + txtPath.Text + ";Extended Properties=\"Excel 12.0;HDR=YES;\"";

                // if you don't want to show the header row (first row)
                // use 'HDR=NO' in the string

                string strSQL = "SELECT * FROM [ExcelToCSV$]";

                OleDbConnection excelConnection = new OleDbConnection(connectionString);
                excelConnection.Open(); // This code will open excel file.

                OleDbCommand dbCommand = new OleDbCommand(strSQL, excelConnection);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(dbCommand);

                // create data table

                dtData = new DataTable();

                dataAdapter.Fill(dtData);

                // dispose used objects
                dataAdapter.Dispose();
                dbCommand.Dispose();

                excelConnection.Close();
                excelConnection.Dispose();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtCSV.Text.Trim() != string.Empty)
            {
                GUIDList = txtCSV.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

    }
}
