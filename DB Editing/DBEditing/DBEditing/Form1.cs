using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DBEditing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DB_Connect con = new DB_Connect();

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "SELECT [ProductId],[ItemCode],[ItemDescription],[FamilyName],[GroupHeader],[MainGroup],[SubGroup1],[SubGroup2],[SubGroup3],[SubGroup4],[SubGroup5],[SubGroup6]";
                str += ",[SubGroup7],[SubGroup8],[ProductFamily],[ItemPurchaseType],[Price in USD - P.Y],[Price in USD - C.Y],[Price in INR - P.Y],[Price in INR - C.Y],[AnnexureId],[UnitofMeasures]";
                str += ",[EDApplicable],[VATApplicable],[PackageType],[VariantCode],[VariantDescription],[BelongsTo] FROM [ProductMasterNew]";
                DataTable dt = new DataTable();
                con.Select_DT(str, ref dt);

                StringBuilder sb = new StringBuilder();
                string[] columnNames = dt.Columns.Cast<DataColumn>().
                                      Select(column => column.ColumnName).
                                      ToArray();
                sb.AppendLine(string.Join(",", columnNames));
                foreach (DataRow row in dt.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                    ToArray();
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ProductMaster.csv"), sb.ToString());
                MessageBox.Show("ProductMaster.csv File Downloaded to MyDocuments folder");
            }
            catch (Exception ex) { MessageBox.Show("Error in download - " + ex.ToString()); }
        }

        private void btnBrowseExisting_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Browse CSV Files";
            openFileDialog1.DefaultExt = "csv";
            openFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.ShowDialog();
            txtExisting.Text = openFileDialog1.FileName;
            openFileDialog1.Multiselect = false;
        }

        private void btnBrowseNew_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Browse CSV Files";
            openFileDialog1.DefaultExt = "csv";
            openFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.ShowDialog();
            txtNew.Text = openFileDialog1.FileName;
            openFileDialog1.Multiselect = false;
        }

        private void btnUploadExisting_Click(object sender, EventArgs e)
        {
            if (txtExisting.Text.Length > 0)
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt = ReadCsvFile(txtExisting.Text);
                    string str = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        str = "UPDATE [ProductMasterNew] SET [ItemCode] = '" + dr["ItemCode"] + "',[ItemDescription] = '" + dr["ItemDescription"] + "'";
                        str += ",[FamilyName] = '" + dr["FamilyName"] + "',[GroupHeader] = '" + dr["GroupHeader"] + "',[MainGroup] = '" + dr["MainGroup"] + "',[SubGroup1] = '" + dr["SubGroup1"] + "'";
                        str += ",[SubGroup2] = '" + dr["SubGroup2"] + "',[SubGroup3] = '" + dr["SubGroup3"] + "',[SubGroup4] = '" + dr["SubGroup4"] + "',[SubGroup5] = '" + dr["SubGroup5"] + "',[SubGroup6] = '" + dr["SubGroup6"] + "'";
                        str += ",[SubGroup7] = '" + dr["SubGroup7"] + "',[SubGroup8] = '" + dr["SubGroup8"] + "',[ProductFamily] = '" + dr["ProductFamily"] + "',[ItemPurchaseType] = '" + dr["ItemPurchaseType"] + "'";
                        str += ",[Price in USD - P.Y] = '" + dr["Price in USD - P.Y"] + "',[Price in USD - C.Y] = '" + dr["Price in USD - C.Y"] + "',[Price in INR - P.Y] = '" + dr["Price in INR - P.Y"] + "'";
                        str += ",[Price in INR - C.Y] = '" + dr["Price in INR - C.Y"] + "',[AnnexureId]=" + dr["AnnexureId"] + ",[UnitofMeasures] = '" + dr["UnitofMeasures"] + "',[EDApplicable] = '" + dr["EDApplicable"] + "',[VATApplicable] = '" + dr["VATApplicable"] + "'";
                        str += ",[PackageType] = '" + dr["PackageType"] + "',[VariantCode] = '" + dr["VariantCode"] + "',[VariantDescription] = '" + dr["VariantDescription"] + "',[BelongsTo]='" + dr["BelongsTo"] + "' where [ProductId] = " + dr["ProductId"];
                        con.Non_Query(str);
                    }

                    MessageBox.Show("Update Completed");
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }

        private void btnUploadNew_Click(object sender, EventArgs e)
        {
            if (txtNew.Text.Length > 0)
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt = ReadCsvFile(txtNew.Text);
                    string str = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        str = "INSERT INTO [ProductMaster]([ItemCode],[ItemDescription],[FamilyName],[GroupHeader],[MainGroup],[SubGroup1],[SubGroup2],[SubGroup3]";
                        str += ",[SubGroup4],[SubGroup5],[SubGroup6],[SubGroup7],[SubGroup8],[ProductFamily],[ItemPurchaseType],[Price in USD - P.Y]";
                        str += ",[Price in USD - C.Y],[Price in INR - P.Y],[Price in INR - C.Y],[UnitofMeasures],[EDApplicable],[VATApplicable],[PackageType]";
                        str += ",[VariantCode],[VariantDescription]) VALUES";
                        str += "('" + dr["ItemCode"] + "'";
                        str += ",'" + dr["ItemDescription"] + "'";
                        str += ",'" + dr["FamilyName"] + "'";
                        str += ",'" + dr["GroupHeader"] + "'";
                        str += ",'" + dr["MainGroup"] + "'";
                        str += ",'" + dr["SubGroup1"] + "'";
                        str += ",'" + dr["SubGroup2"] + "'";
                        str += ",'" + dr["SubGroup3"] + "'";
                        str += ",'" + dr["SubGroup4"] + "'";
                        str += ",'" + dr["SubGroup5"] + "'";
                        str += ",'" + dr["SubGroup6"] + "'";
                        str += ",'" + dr["SubGroup7"] + "'";
                        str += ",'" + dr["SubGroup8"] + "'";
                        str += ",'" + dr["ProductFamily"] + "'";
                        str += ",'" + dr["ItemPurchaseType"] + "'";
                        str += ",'" + dr["Price in USD - P.Y"] + "'";
                        str += ",'" + dr["Price in USD - C.Y"] + "'";
                        str += ",'" + dr["Price in INR - P.Y"] + "'";
                        str += ",'" + dr["Price in INR - C.Y"] + "'";
                        str += ",'" + dr["UnitofMeasures"] + "'";
                        str += ",'" + dr["EDApplicable"] + "'";
                        str += ",'" + dr["VATApplicable"] + "'";
                        str += ",'" + dr["PackageType"] + "'";
                        str += ",'" + dr["VariantCode"] + "'";
                        str += ",'" + dr["VariantDescription"] + "')";

                        con.Non_Query(str);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }


        public DataTable ReadCsvFile(string FileSaveWithPath)
        {

            DataTable dtCsv = new DataTable();
            string Fulltext;


            using (StreamReader sr = new StreamReader(FileSaveWithPath))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                    string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                    for (int i = 0; i < rows.Count() - 1; i++)
                    {
                        string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                        {
                            if (i == 0)
                            {
                                for (int j = 0; j < rowValues.Count(); j++)
                                {
                                    dtCsv.Columns.Add(rowValues[j]); //add headers  
                                }
                            }
                            else
                            {
                                DataRow dr = dtCsv.NewRow();
                                for (int k = 0; k < rowValues.Count(); k++)
                                {
                                    dr[k] = rowValues[k].ToString();
                                }
                                dtCsv.Rows.Add(dr); //add other rows  
                            }
                        }
                    }
                }
            }

            return dtCsv;
        }
    }
}
