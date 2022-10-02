using Dapper;
using SaveFileToDb.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using static DevExpress.XtraEditors.Mask.MaskSettings;

namespace SaveFileToDb
{
    public partial class Form1 : Form
    {
        private string conStr = "Server=DESKTOP-R7VHHAU;Database=FileDB;User Id=sa;Password=Sql2019;Trusted_Connection=True";
        public Form1()
        {
            InitializeComponent();
            FillGrid();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                if(ofd.FileName == "") return;

                txtFileName.Text = ofd.FileName;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtFileName.Text)) return;

            using(Stream stream = File.OpenRead(txtFileName.Text))
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                string ext = new FileInfo(txtFileName.Text).Extension;
                string FileName = new FileInfo(txtFileName.Text).Name;

                var saveData = new FileModel
                {
                    FileContent = data,
                    FileExt = ext,
                    FileName = FileName
                };

                string query = "INSERT INTO Files (FileName, FileExt, FileContent) VALUES (@FileName, @FileExt, @FileContent)";

                using(SqlConnection con = new SqlConnection(conStr))
                {
                    var result = con.Query(query, saveData);

                    FillGrid();
                }

            }
        }

        private void FillGrid()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                var list = con.Query<FileModel>("SELECT * FROM Files");

                gridControl1.DataSource = list;
            }
                
        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            if(gridView1.FocusedRowHandle < 0) return;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                var data = con.Query<FileModel>("SELECT * FROM Files WHERE Id = @id", new { id = gridView1.GetFocusedRowCellValue(colId) }).SingleOrDefault();

                if(data == null) return;

                var fileData = (byte[])data.FileContent;

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = data.FileName;
                sfd.DefaultExt = data.FileExt;
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    string newFileName = sfd.FileName;
                    File.WriteAllBytes(newFileName, fileData);
                    System.Diagnostics.Process.Start(newFileName);
                }

                
            }
        }
    }
}
