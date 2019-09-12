using iTextSharp.text;
using iTextSharp.text.pdf;
using KewauneeTaskAssigner;
using Spire.Pdf;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;
namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for SalesQuote.xaml
    /// </summary>
    public partial class SalesQuote : Window
    {
        private bool p1;
        private bool p2;
        private bool p3;

        private string _connectionString = string.Empty;
        private DataTable dtCustomer = new DataTable();
        private DataTable dtProjects = new DataTable();
        public DataTable dtProducts = new DataTable();
        private DataTable dtItems = new DataTable();
        private DataTable dtBulkInsert = new DataTable();
        private DataTable dtPartCodes = new DataTable();
        private DataTable dtVariants = new DataTable();
        private string totalText = string.Empty;
        private string mainTotal = string.Empty;
        public SalesQuote(bool p1, bool p2, bool p3)
        {
            InitializeComponent();
            // TODO: Complete member initialization
            try
            {
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                _connectionString = Properties.Settings.Default.ConnectionString;
                LoadCustomerDetails();
                LoadProjectDetails();
                GetProducts();
                PopulateData();
                GetPartCodes();
                GetVariants();
                LoadSearchComboBoxValues();
                dtBulkInsert = new DataTable();
                dtItems = TaskAssigner.GetLoadedItems(ClsProperties.commandData, dtProducts, dtPartCodes, dtVariants, ClsProperties.dictFumeHoodFamilies);
                //MessageBox.Show("2");
                if (dtItems.Rows.Count > 0)
                {
                    LoadTotalValues();
                    //MessageBox.Show("3");
                    TaxProcess();

                }
                ClsProperties.isClosed = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadTotalValues()
        {
            txtTotal.Text = dtItems.Rows.OfType<DataRow>().ToList().Sum(x => Convert.ToInt32(x["TotalUnitPrice"].ToString())).ToString();
            txtFinalTotal.Text = dtItems.Rows.OfType<DataRow>().ToList().Sum(x => Convert.ToInt32(x["TotalUnitPrice"].ToString())).ToString();
            totalText = txtTotal.Text;
            mainTotal = txtTotal.Text;
            txtSpecialDiscount.Text = "0";


            //dgBOM.Items.SortDescriptions.Clear();
            //dgBOM.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ItemCode", System.ComponentModel.ListSortDirection.Ascending));
            dtItems.DefaultView.Sort = "ItemCode";
            //int i = 1;
            //dtItems.DefaultView.ToTable().Rows.OfType<DataRow>().ToList().ForEach(x => 
            //{
            //    x["SlNo"] = i;
            //    i++;
            //});

            int i = 1;
            foreach (DataRowView dr in dtItems.DefaultView)
            {
                dr["SlNo"] = i;
                i++;
            }


            dgBOM.ItemsSource = null;
            dgBOM.ItemsSource = dtItems.DefaultView;

            dtItems = dtItems.DefaultView.ToTable();

        }

        private void GetVariants()
        {
            try
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    var query = "select * from VariantDetails";
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        dtVariants = new DataTable();
                        dtVariants.Load(scmd.ExecuteReader());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClose_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GetPartCodes()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from NewPartCodes";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtPartCodes = new DataTable();
                    dtPartCodes.Load(scmd.ExecuteReader());
                }
            }
        }

        private void LoadCustomerDetails()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Customer where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtCustomer = new DataTable();
                    dtCustomer.Load(scmd.ExecuteReader());
                }
            }
        }

        private void LoadProjectDetails()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Projects where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProjects = new DataTable();
                    dtProjects.Load(scmd.ExecuteReader());
                    foreach (DataRow dr in dtProjects.Rows)
                    {
                        var item = (from object itm in cmbProjectNo.Items
                                    where itm.Equals(dr["ProjectNo"].ToString())
                                    select itm).FirstOrDefault();
                        if (item == null)
                            cmbProjectNo.Items.Add(dr["ProjectNo"].ToString());
                    }
                }
            }
        }

        private void GetProducts()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from ProductMasterNew where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProducts = new DataTable();
                    dtProducts.Load(scmd.ExecuteReader());
                }
            }
        }

        private void PopulateData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UIInputs.ProjectNo))
                {
                    if (ClsProperties.DocumentName == null) return;
                    ClsProperties.DocumentName = ClsProperties.DocumentName.Substring(0, ClsProperties.DocumentName.LastIndexOf("_"));
                    UIInputs.ProjectNo = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].ToString().Equals(ClsProperties.DocumentName)).Select(x => x["ProjectNo"].ToString()).FirstOrDefault();
                }
                if (!string.IsNullOrWhiteSpace(UIInputs.ProjectNo))
                {
                    cmbProjectNo.SelectedIndex = cmbProjectNo.Items.IndexOf(dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(UIInputs.ProjectNo)).Select(x => x["ProjectNo"].ToString()).FirstOrDefault());
                    string customerAddress = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(cmbProjectNo.SelectedItem.ToString())).Select(x => x["CustomerNameAddress"].ToString()).FirstOrDefault();
                    string[] customerDetails = customerAddress.Split('\r', '\n');
                    txtName.Text = customerDetails[0].ToString();
                    txtCode.Text = dtCustomer.Rows.OfType<DataRow>().ToList().Where(x => x["Name"].ToString().Equals(txtName.Text)).Select(x => x["Code"].ToString()).FirstOrDefault();
                    txtCode.Text = txtCode.Text.Length == 1 ? "CUST000" + txtCode.Text : txtCode.Text.Length == 2 ? "CUST00" + txtCode.Text : txtCode.Text.Length == 3 ? "CUST0" + txtCode.Text : txtCode.Text;
                    txtAddress.Text = dtCustomer.Rows.OfType<DataRow>().ToList().Where(x => x["Name"].ToString().Equals(txtName.Text)).Select(x => x["Address"].ToString()).FirstOrDefault();
                }
                else
                {
                    MessageBox.Show("Please open valid project file to generate BOM.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    ClsProperties.isSalesClosed = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmbProjectNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProjectNo.SelectedIndex == -1)
                return;
            txtProjectName.Text = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(cmbProjectNo.SelectedItem.ToString())).Select(x => x["ProjectName"].ToString()).FirstOrDefault();
            txtQutationRevNo.Text = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(cmbProjectNo.SelectedItem.ToString())).Select(x => x["DrawingRevNo"].ToString()).FirstOrDefault();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dtItems.Dispose();
            dtBulkInsert.Dispose();
            dtPartCodes.Dispose();
            dtProjects.Dispose();
            dtProducts.Dispose();
            ClsProperties.isClosed = true;
        }

        int cstValTax = 0;
        int cetExciseDuty = 0;
        private void CalculateTaxes()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtCformNo.Text))
                {
                    txtTotal.Text = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 2) / 100)).ToString();
                    txtFinalTotal.Text = txtTotal.Text;
                    totalText = txtTotal.Text;
                    cstValTax = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 2) / 100));
                }
                else
                {
                    txtTotal.Text = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 14.5) / 100)).ToString();
                    txtFinalTotal.Text = txtTotal.Text;
                    totalText = txtTotal.Text;
                    cstValTax = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 14.5) / 100));
                }
                if (!(bool)isExciseExemptedYes.IsChecked)
                {
                    txtTotal.Text = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 12.5) / 100)).ToString();
                    txtFinalTotal.Text = txtTotal.Text;
                    totalText = txtTotal.Text;
                    cetExciseDuty = Convert.ToInt32(Convert.ToInt32(txtTotal.Text) + ((Convert.ToInt32(txtTotal.Text) * 12.5) / 100));
                }

                //Modifications made to update total price
                dgBOM.Items.OfType<DataRowView>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.Row["TotalUnitPrice"].ToString()))
                    {
                        txtTotal.Text = (Convert.ToInt32(txtTotal.Text) + Convert.ToInt32(x.Row["TotalUnitPrice"].ToString())).ToString();
                        txtFinalTotal.Text = txtTotal.Text;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Validations())

                using (var scn = new SqlConnection(_connectionString))
                {

                    CalculateTaxes();
                    scn.Open();
                    var query = "insert into SalesQuote(QuotationNo,QuotationRevNo,QuotationDate,CustomerCode,CustomerName,CustomerAddress,ProjectNo,ProjectName,CreatedDate,IsDeleted,SpecialDiscount,Total,FinalTotal) values (@QuotationNo,@QuotationRevNo,@QuotationDate,@CustomerCode,@CustomerName,@CustomerAddress,@ProjectNo,@ProjectName,@CreatedDate,@IsDeleted,@SpecialDiscount,@Total,@FinalTotal)";
                    BulkInsertToTable(scn);
                    scn.Close();
                    bool isGenerated = false;
                    bool isInternational = (bool)rbInternational.IsChecked;
                    if (!isInternational)
                        WriteSalesFormat(ref isGenerated);
                    else
                        WriteIntoExcel();


                    dtePicker.DisplayDate = DateTime.Now;
                    if (!isGenerated && !isInternational)
                    {
                        MessageBox.Show("Sales Quote has not been generated. Please check template file exists or check item purchase type value in database", "Sales Quote", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    };

                    txtQutationRevNo.Clear();
                    txtQutationNo.Clear();
                    dtePicker.SelectedDate = null;

                    MessageBox.Show("Sales Quote generated successfully.", "Sales Quote", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                    MessageBoxResult res = MessageBox.Show("BOQ Generated. Do you want to open the generated BOQ file?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        if (!isInternational)
                        {
                            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx")))
                            {
                                System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"));
                                if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf")))
                                    System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf"));
                            }
                        }
                        else
                        {
                            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx")))
                            {
                                System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx"));
                                if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf")))
                                    System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf"));
                            }
                        }

                    }
                    else
                    {
                        Close();
                    }
                    //}
                }
            else
                MessageBox.Show("Please fill all the mandatory details.", "Sales Quote", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BulkInsertToTable(SqlConnection scn)
        {
            try
            {
                dtBulkInsert = new DataTable();
                dtBulkInsert.Columns.Add("SlNo");
                dtBulkInsert.Columns.Add("QuotationNo");
                dtBulkInsert.Columns.Add("QuotationRevNo");
                dtBulkInsert.Columns.Add("QuotationDate");
                dtBulkInsert.Columns.Add("CustomerCode");
                dtBulkInsert.Columns.Add("CustomerName");
                dtBulkInsert.Columns.Add("CustomerAddress");
                dtBulkInsert.Columns.Add("ProjectNo");
                dtBulkInsert.Columns.Add("ProjectName");
                dtBulkInsert.Columns.Add("SpecialDiscount");
                dtBulkInsert.Columns.Add("Total");
                dtBulkInsert.Columns.Add("FinalTotal");
                dtBulkInsert.Columns.Add("CreatedDate");
                dtBulkInsert.Columns.Add("IsDeleted");
                dtBulkInsert.Columns.Add("ItemCode");
                dtBulkInsert.Columns.Add("VariantCode");
                dtBulkInsert.Columns.Add("ItemVariantDescription");
                dtBulkInsert.Columns.Add("Qty");
                dtBulkInsert.Columns.Add("UnitOfMeas");
                dtBulkInsert.Columns.Add("UnitPrice");
                dtBulkInsert.Columns.Add("Discount");
                dtBulkInsert.Columns.Add("DiscountedUnitPrice");
                dtBulkInsert.Columns.Add("TotalUnitPrice");
                dtBulkInsert.Columns.Add("SpecialNotes");
                dtBulkInsert.Columns.Add("IsInternational");
                dtBulkInsert.Columns.Add("IsCustomerOutsideState");
                dtBulkInsert.Columns.Add("CFormNo");
                dtBulkInsert.Columns.Add("IsVariantDisplay");
                dtBulkInsert.Columns.Add("IsExciseExempted");
                dtBulkInsert.Columns.Add("IsImportDutyExempted");
                dtBulkInsert.Columns.Add("IsSEZandGovOrg");
                dtBulkInsert.Columns.Add("Installation");
                dtBulkInsert.Columns.Add("FreightCharges");
                dtBulkInsert.Columns.Add("OtherCharges");
                dtBulkInsert.Columns.Add("PFCharges");

                int i = 0;
                foreach (DataRowView dr in dgBOM.Items)
                {
                    i++;
                    //dtBulkInsert.Rows.Add(dr.Row.ItemArray);
                    dtBulkInsert.Rows.Add(i.ToString(), txtQutationNo.Text, txtQutationRevNo.Text, Convert.ToDateTime(dtePicker.SelectedDate.ToString()), txtCode.Text, txtName.Text, txtAddress.Text, cmbProjectNo.SelectedItem.ToString(), txtProjectName.Text, txtSpecialDiscount.Text, txtTotal.Text, txtFinalTotal.Text, DateTime.Now, false, dr.Row["ItemCode"].ToString(), dr.Row["VariantCode"].ToString(), dr.Row["ItemVariantDescription"].ToString(), dr.Row["Qty"].ToString(), dr.Row["UnitOfMeas"].ToString(), dr.Row["UnitPrice"].ToString(), dr.Row["Discount"].ToString(), dr.Row["DiscountedUnitPrice"].ToString(), dr.Row["TotalUnitPrice"].ToString(), dr.Row["SpecialNotes"].ToString(), (bool)rbInternational.IsChecked, (bool)cbIsCustomer.IsChecked, txtCformNo.Text, (bool)isVariantDetailsYes.IsChecked, (bool)isExciseExemptedYes.IsChecked, (bool)isImportDutyExemptedYes.IsChecked, (bool)isSEZCentralGovorgYes.IsChecked, txtInstallationCharges.Text, txtfreightCharges.Text, txtOtherCharges.Text, txtpf.Text);
                }
                DeleteFromSalesQuoteIfProjExists(txtProjectName.Text);
                SqlBulkCopy obj = new SqlBulkCopy(scn);
                obj.DestinationTableName = "SalesQuote";
                obj.ColumnMappings.Add("ItemCode", "ItemCode");
                obj.ColumnMappings.Add("VariantCode", "VariantCode");
                obj.ColumnMappings.Add("ItemVariantDescription", "ItemVariantDescription");
                obj.ColumnMappings.Add("Qty", "Qty");
                obj.ColumnMappings.Add("UnitOfMeas", "UnitOfMeas");
                obj.ColumnMappings.Add("UnitPrice", "UnitPrice");
                obj.ColumnMappings.Add("Discount", "Discount");
                obj.ColumnMappings.Add("DiscountedUnitPrice", "DiscountedUnitPrice");
                obj.ColumnMappings.Add("TotalUnitPrice", "TotalUnitPrice");
                obj.ColumnMappings.Add("SpecialNotes", "SpecialNotes");
                obj.ColumnMappings.Add("QuotationRevNo", "QuotationRevNo");
                obj.ColumnMappings.Add("QuotationNo", "QuotationNo");
                obj.ColumnMappings.Add("QuotationDate", "QuotationDate");
                obj.ColumnMappings.Add("CustomerCode", "CustomerCode");
                obj.ColumnMappings.Add("CustomerName", "CustomerName");
                obj.ColumnMappings.Add("CustomerAddress", "CustomerAddress");
                obj.ColumnMappings.Add("ProjectNo", "ProjectNo");
                obj.ColumnMappings.Add("ProjectName", "ProjectName");
                obj.ColumnMappings.Add("SpecialDiscount", "SpecialDiscount");
                obj.ColumnMappings.Add("Total", "Total");
                obj.ColumnMappings.Add("FinalTotal", "FinalTotal");
                obj.ColumnMappings.Add("CreatedDate", "CreatedDate");
                obj.ColumnMappings.Add("IsDeleted", "IsDeleted");
                obj.ColumnMappings.Add("IsInternational", "IsInternational");
                obj.ColumnMappings.Add("IsCustomerOutsideState", "IsCustomerOutsideState");
                obj.ColumnMappings.Add("CFormNo", "CFormNo");
                obj.ColumnMappings.Add("IsVariantDisplay", "IsVariantDisplay");
                obj.ColumnMappings.Add("IsExciseExempted", "IsExciseExempted");
                obj.ColumnMappings.Add("IsImportDutyExempted", "IsImportDutyExempted");
                obj.ColumnMappings.Add("IsSEZandGovOrg", "IsSEZandGovOrg");
                obj.ColumnMappings.Add("Installation", "Installation");
                obj.ColumnMappings.Add("FreightCharges", "FreightCharges");
                obj.ColumnMappings.Add("OtherCharges", "OtherCharges");
                obj.ColumnMappings.Add("PFCharges", "PFCharges");
                obj.WriteToServer(dtBulkInsert);
            }
            catch (Exception ex)
            {


            }
        }

        private void DeleteFromSalesQuoteIfProjExists(string projectName)
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "delete from SalesQuote where ProjectName = '" + projectName + "' and ProjectNo='" + cmbProjectNo.SelectedItem.ToString() + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                }
            }
        }


        private string GetSalesTemplatePath()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exePath = exePath.Substring(0, exePath.LastIndexOf(@"\"));
            if (dtItems.Rows.Count == 0)
                return string.Empty;
            string path = dtItems.Rows.OfType<DataRow>().ToList().Any(x => !string.IsNullOrWhiteSpace(x["ItemPurchaseType"].ToString()) && x["ItemPurchaseType"].ToString().Equals("Import")) ? System.IO.Path.Combine(exePath, @"Kewaunee\Revit\IMPORTED  OPTION FORMAT.xlsx") : dtItems.Rows.OfType<DataRow>().ToList().Any(x => !string.IsNullOrWhiteSpace(x["ItemPurchaseType"].ToString()) && x["ItemPurchaseType"].ToString().Equals("Indigenous")) ? System.IO.Path.Combine(exePath, @"Kewaunee\Revit\INDIGENOUS OPTION FORMAT.xlsx") : string.Empty;
            isImported = path.ToLower().Contains("import") ? true : false;
            return path;
        }

        private void WriteSalesFormat(ref bool isGenerated)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains("EXCEL"))
                {
                    clsProcess.CloseMainWindow();
                    clsProcess.Kill();
                    break;
                }
            }

            string path = GetSalesTemplatePath();
            if (string.IsNullOrWhiteSpace(path)) { isGenerated = false; return; }
            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx")))
                System.IO.File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"));

            if (string.IsNullOrWhiteSpace(path))
                MessageBox.Show("Excel template does not exists");

            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            xlApp = new Excel.Application();

            xlWorkBook = xlApp.Workbooks.Open(path, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            Excel.Worksheet xlTotWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlTotWorkSheet.Cells[1, 3] = txtQutationNo.Text;
            xlTotWorkSheet.Cells[2, 3] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["ProjectNo"].ToString()).First();
            xlTotWorkSheet.Cells[3, 3] = dtePicker.Text;
            xlTotWorkSheet.Cells[4, 3] = txtName.Text;

            double importedTot = 0, indengnsTot = 0;
            int val = 10;
            int annexureId = 1;
            int sheetId = 2;
            AnnexureProcess:
            var annextureList = dtItems.Rows.OfType<DataRow>().ToList().Where(x => x["AnnexureId"] != null && !string.IsNullOrEmpty(x["AnnexureId"].ToString()) && Convert.ToInt32(x["AnnexureId"].ToString()) == annexureId).Select(x => x).ToList();
            xlWorkSheet = null;
            if (annextureList.Count > 0)
            {
                FillAnnexureDetails(annextureList, sheetId, xlWorkSheet, xlWorkBook, xlApp, ref importedTot, ref indengnsTot, annexureId);
                if (isImported)
                {
                    xlTotWorkSheet.Cells[val, 4] = importedTot;
                    if ((bool)isExciseExemptedYes.IsChecked)
                    {
                        xlTotWorkSheet.Cells[val, 7] = indengnsTot;
                    }
                    else
                    {
                        xlTotWorkSheet.Cells[val, 6] = indengnsTot;
                    }
                }
                else
                {
                    xlTotWorkSheet.Cells[val - 1, 4] = indengnsTot;
                }
                annexureId++;
                sheetId++;
                val++;
                annextureList.Clear();
                if (annexureId <= 5)
                    goto AnnexureProcess;
            }
            else
            {
                annexureId++;
                sheetId++;
                if (annexureId <= 5)
                    goto AnnexureProcess;
            }
            if (isImported)
            {
                xlTotWorkSheet.Cells[22, 7] = txtfreightCharges.Text;
                xlTotWorkSheet.Cells[16, 7] = txtpf.Text;
                xlTotWorkSheet.Cells[26, 7] = txtInstallationCharges.Text;
                xlTotWorkSheet.Cells[18, 7] = cetExciseDuty != 0 ? cetExciseDuty : 0;
                xlTotWorkSheet.Cells[20, 7] = cstValTax != 0 ? cstValTax : 0;
            }
            else
            {
                xlTotWorkSheet.Cells[15, 4] = txtInstallationCharges.Text;
                xlTotWorkSheet.Cells[14, 4] = txtpf.Text;
                xlTotWorkSheet.Cells[16, 4] = cetExciseDuty != 0 ? cetExciseDuty : 0;
                xlTotWorkSheet.Cells[18, 4] = cstValTax != 0 ? cstValTax : 0;
                xlTotWorkSheet.Cells[20, 4] = txtfreightCharges.Text;
            }

            xlWorkBook.SaveAs(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"));

            //xlWorkBook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"), Type.Missing, true, false, Type.Missing, Type.Missing, false, Type.Missing);

            xlWorkBook.Close();
            xlApp.Quit();

            //PdfReader reader = new PdfReader(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"));
            //PdfStamper stamper = new PdfStamper(reader, new System.IO.FileStream(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf"), System.IO.FileMode.Create));
            //PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, reader.GetPageSize(1).Height, 0.75f);
            //PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, stamper.Writer);
            //stamper.Writer.SetOpenAction(action);
            //stamper.Close();
            //reader.Close();

            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf")))
                System.IO.File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"));
            isGenerated = true;

        }

        bool isImported = false;
        private void FillAnnexureDetails(List<DataRow> lstDataRow, int i, Excel.Worksheet xlWorkSheet, Excel.Workbook xlWorkBook, Excel.Application xlApp, ref double importedTot, ref double idengeniousTot, int annexureId)
        {
            try
            {
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(i);

                int k = 0;
                if (isImported)
                {
                    k = 5;
                    if (i == 2 || i == 3)
                    {
                        //xlWorkSheet.Cells[6, 3] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["LabName"].ToString()).First();
                        //xlWorkSheet.Cells[5, 3] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["FloorNo"].ToString()).First();
                        //k = 7;
                    }

                }
                else
                {
                    if (i == 2 || i == 3)
                        k = 4;
                    else
                        k = 5;
                }
                double tot = 0;
                double indenTot = 0;
                bool isImport = false;
                int sno = 0;
                if (isImported)
                {
                    var groupNames = new List<string>();

                    var lstGroup1 = lstDataRow.GroupBy(y => y["Group 1"]).Select(y => y).ToList();
                    if (lstGroup1.Any())
                    {

                        foreach (var grp1 in lstGroup1)
                        {
                            // if (!string.IsNullOrWhiteSpace(grp1.First()["Group 1"].ToString()))
                            {
                                xlWorkSheet.Cells[k, 3] = !string.IsNullOrWhiteSpace(grp1.First()["Group 1"].ToString()) ? grp1.First()["Group 1"].ToString() : "UnGrouped";
                                xlWorkSheet.Cells[k, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                            }
                            var lstGroup2 = grp1.GroupBy(x => x["Group 2"]).Select(x => x).ToList();
                            if (lstGroup2.Any())
                            {
                                foreach (var grp2 in lstGroup2)
                                {
                                    // if (!string.IsNullOrWhiteSpace(grp2.First()["Group 2"].ToString()))
                                    {
                                        xlWorkSheet.Cells[k + 1, 3] = !string.IsNullOrWhiteSpace(grp2.First()["Group 2"].ToString()) ? grp2.First()["Group 2"].ToString() : "UnGrouped";
                                        xlWorkSheet.Cells[k + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                                    }
                                    var lstGroup3 = grp2.GroupBy(x => x["Group 3"]).Select(x => x).ToList();
                                    if (lstGroup3.Any())
                                    {

                                        foreach (var grp3 in lstGroup3)
                                        {
                                            //if (!string.IsNullOrWhiteSpace(grp3.First()["Group 3"].ToString()))
                                            {
                                                xlWorkSheet.Cells[k + 2, 3] = !string.IsNullOrWhiteSpace(grp3.First()["Group 3"].ToString()) ? grp3.First()["Group 3"].ToString() : "UnGrouped";
                                                xlWorkSheet.Cells[k + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                                            }
                                            int z = k + 3;

                                            List<string> addedItem = new List<string>();
                                            foreach (var dr in grp3)
                                            {
                                                //Newly added by chocka
                                                double count = 0;
                                                double TotalUnitPrice = 0;
                                                if ((bool)isVariantDetailsYes.IsChecked && addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                //if (addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                {
                                                    continue;
                                                }
                                                else if (!(bool)isVariantDetailsYes.IsChecked && addedItem.Contains(dr["ItemCode"].ToString()))
                                                    continue;
                                                else
                                                {
                                                    foreach (var drInfo in grp3)
                                                    {
                                                        if ((bool)isVariantDetailsYes.IsChecked)
                                                        {
                                                            if (drInfo["ItemCode"].ToString().Equals(dr["ItemCode"].ToString()) && drInfo["ItemVariantDescription"].ToString().Equals(dr["ItemVariantDescription"]))
                                                            {
                                                                count += Convert.ToDouble(drInfo["Qty"].ToString());
                                                                TotalUnitPrice += Convert.ToDouble(drInfo["TotalUnitPrice"].ToString());
                                                            }
                                                            if (!addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                                addedItem.Add(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString());
                                                        }
                                                        else
                                                        {
                                                            if (drInfo["ItemCode"].ToString().Equals(dr["ItemCode"].ToString()))
                                                            {
                                                                count += Convert.ToDouble(drInfo["Qty"].ToString());
                                                                TotalUnitPrice += Convert.ToDouble(drInfo["TotalUnitPrice"].ToString());
                                                            }
                                                            if (!addedItem.Contains(dr["ItemCode"].ToString()))
                                                                addedItem.Add(dr["ItemCode"].ToString());
                                                        }
                                                    }

                                                }

                                                sno++;
                                                xlWorkSheet.Cells[z, 1] = sno;
                                                xlWorkSheet.Cells[z, 2] = dr["ItemCode"].ToString();
                                                if ((bool)isVariantDetailsYes.IsChecked)
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    var latChar = partDes.Substring(partDes.Length - 1);
                                                    if (latChar == "/")
                                                        partDes = partDes.Remove(partDes.Length - 1);
                                                    //xlWorkSheet.Cells[z, 3] = dr["ItemVariantDescription"].ToString();
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }
                                                else
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    if (partDes.Trim().StartsWith("18/", StringComparison.InvariantCultureIgnoreCase) || partDes.StartsWith("18 /"))
                                                    {
                                                        partDes = partDes.Substring(partDes.IndexOf("/") + 1).Contains("/") ? partDes.Substring(0, partDes.IndexOf("/", partDes.IndexOf("/") + 1)) : partDes;
                                                    }
                                                    else
                                                    {
                                                        partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;
                                                    }
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }
                                                xlWorkSheet.Cells[z, 6] = dr["UnitPrice"].ToString();
                                                if (dr["ItemPurchaseType"].ToString().Equals("Import"))
                                                {
                                                    /*xlWorkSheet.Cells[z, 6] = TotalUnitPrice.ToString();*/ //dr["TotalUnitPrice"].ToString(); isImport = true;
                                                    xlWorkSheet.Cells[z, 7] = TotalUnitPrice.ToString();
                                                    tot = tot + Convert.ToDouble(TotalUnitPrice.ToString()); //dr["TotalUnitPrice"].ToString());
                                                }
                                                else
                                                {
                                                    if ((bool)isExciseExemptedYes.IsChecked)
                                                    {
                                                        if (annexureId == 1)
                                                        {
                                                            xlWorkSheet.Cells[z, 10] = TotalUnitPrice.ToString();// dr["TotalUnitPrice"].ToString();
                                                            indenTot = indenTot + Convert.ToDouble(TotalUnitPrice);// dr["TotalUnitPrice"].ToString());
                                                        }
                                                        else
                                                        {
                                                            xlWorkSheet.Cells[z, 11] = TotalUnitPrice.ToString();//  dr["TotalUnitPrice"].ToString();
                                                            indenTot = indenTot + Convert.ToDouble(TotalUnitPrice);//dr["TotalUnitPrice"].ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        xlWorkSheet.Cells[z, 10] = TotalUnitPrice.ToString();//dr["TotalUnitPrice"].ToString();
                                                        indenTot = indenTot + Convert.ToDouble(TotalUnitPrice);//dr["TotalUnitPrice"].ToString());
                                                    }
                                                }
                                                xlWorkSheet.Cells[z, 5] = count.ToString();// dr["Qty"].ToString();
                                                xlWorkSheet.Cells[z, 4] = dr["UnitOfMeas"].ToString();
                                                z++;
                                            }
                                            k = z + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #region Working - Commented
                    //lstDataRow.ForEach(x =>
                    //{
                    //    if (!string.IsNullOrWhiteSpace(x["Group 1"].ToString()) && !groupNames.Contains(x["Group 1"].ToString()))
                    //    {
                    //        var lst = lstDataRow.Where(y => y["Group 1"].ToString().Equals(x["Group 1"].ToString())).Select(y => y).ToList();
                    //        if (lst.Any())
                    //        {
                    //            if (!string.IsNullOrWhiteSpace(x["Group 1"].ToString()))
                    //            {
                    //                xlWorkSheet.Cells[k, 3] = x["Group 1"].ToString();
                    //                xlWorkSheet.Cells[k + 1, 3] = x["Group 2"].ToString();
                    //                xlWorkSheet.Cells[k + 2, 3] = x["Group 3"].ToString();
                    //                xlWorkSheet.Cells[k, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                    //                xlWorkSheet.Cells[k + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                    //                xlWorkSheet.Cells[k + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                    //            }

                    //            int z = k + 2;
                    //            lst.ForEach(y =>
                    //            {
                    //                z++;
                    //                sno++;
                    //                xlWorkSheet.Cells[z, 1] = sno; //xlWorkSheet.Cells[k, 7] = x["UnitPrice"].ToString();
                    //                xlWorkSheet.Cells[z, 2] = y["ItemCode"].ToString(); //xlWorkSheet.Cells[k, 8] = x["Discount"].ToString();
                    //                //xlWorkSheet.Cells[k, 9] = x["DiscountedUnitPrice"].ToString(); //xlWorkSheet.Cells[k, 3] = x["VariantCode"].ToString();
                    //                xlWorkSheet.Cells[z, 3] = y["ItemVariantDescription"].ToString();
                    //                if (x["ItemPurchaseType"].ToString().Equals("Import"))
                    //                {
                    //                    xlWorkSheet.Cells[z, 6] = y["TotalUnitPrice"].ToString(); isImport = true;
                    //                    tot = tot + Convert.ToInt32(y["TotalUnitPrice"].ToString());
                    //                }
                    //                else
                    //                {
                    //                    xlWorkSheet.Cells[z, 8] = y["TotalUnitPrice"].ToString();
                    //                    indenTot = indenTot + Convert.ToInt32(y["TotalUnitPrice"].ToString());
                    //                }
                    //                xlWorkSheet.Cells[z, 5] = y["Qty"].ToString();// xlWorkSheet.Cells[k, 11] = x["SpecialNotes"].ToString();
                    //                xlWorkSheet.Cells[z, 4] = y["UnitOfMeas"].ToString();
                    //                //xlWorkSheet.Range["D" + k].Style.WrapText = true;
                    //                //    k++;
                    //            });
                    //            k = z + 2;
                    //        }
                    //        groupNames.Add(x["Group 1"].ToString());

                    //    }
                    //});
                    #endregion
                    #region Commented
                    //lstDataRow.ForEach(x =>
                    //   {
                    //       sno++;
                    //       xlWorkSheet.Cells[k, 1] = sno; //xlWorkSheet.Cells[k, 7] = x["UnitPrice"].ToString();
                    //       xlWorkSheet.Cells[k, 2] = x["ItemCode"].ToString(); //xlWorkSheet.Cells[k, 8] = x["Discount"].ToString();
                    //       //xlWorkSheet.Cells[k, 9] = x["DiscountedUnitPrice"].ToString(); //xlWorkSheet.Cells[k, 3] = x["VariantCode"].ToString();
                    //       xlWorkSheet.Cells[k, 3] = x["ItemVariantDescription"].ToString();
                    //       if (x["ItemPurchaseType"].ToString().Equals("Import"))
                    //       {
                    //           xlWorkSheet.Cells[k, 6] = x["TotalUnitPrice"].ToString(); isImport = true;
                    //           tot = tot + Convert.ToInt32(x["TotalUnitPrice"].ToString());
                    //       }
                    //       else
                    //       {
                    //           xlWorkSheet.Cells[k, 8] = x["TotalUnitPrice"].ToString();
                    //           indenTot = indenTot + Convert.ToInt32(x["TotalUnitPrice"].ToString());
                    //       }
                    //       xlWorkSheet.Cells[k, 5] = x["Qty"].ToString();// xlWorkSheet.Cells[k, 11] = x["SpecialNotes"].ToString();
                    //       xlWorkSheet.Cells[k, 4] = x["UnitOfMeas"].ToString();
                    //       //xlWorkSheet.Range["D" + k].Style.WrapText = true;
                    //       k++;
                    //   });
                    #endregion

                    //xlWorkSheet.Cells[k + 2, 6] = tot;
                    //xlWorkSheet.Cells[k + 2, 7] = tot;
                    if ((bool)isExciseExemptedYes.IsChecked)
                    {
                        if (annexureId == 1)
                        {
                            xlWorkSheet.Cells[k + 2, 10] = indenTot;
                        }
                        else
                        {
                            xlWorkSheet.Cells[k + 2, 11] = indenTot;
                        }
                    }
                    else
                    {
                        xlWorkSheet.Cells[k + 2, 10] = indenTot;
                    }
                    //xlWorkSheet.Cells[k + 2, 3] = "Total Net Price";
                    importedTot = tot;
                    idengeniousTot = indenTot;
                    int rowEnd = k + 2;
                    int rowCount = xlWorkSheet.UsedRange.Rows.Count;
                    int columnCounnt = xlWorkSheet.Columns.Count;
                    Excel.Range range;
                    switch (annexureId)
                    {
                        case 1:
                            columnCounnt = 10;
                            break;
                        case 2:
                            columnCounnt = 11;
                            break;
                        case 3:
                            columnCounnt = 11;
                            break;
                        default:
                            break;
                    }
                    for (int r = 5; r <= rowEnd; r++)
                    {
                        range = xlWorkSheet.Range[xlWorkSheet.Cells[r, 1], xlWorkSheet.Cells[r, columnCounnt]];
                        Microsoft.Office.Interop.Excel.Borders borders = range.Borders;
                        borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                    }
                }
                else
                {

                    var lstGroup1 = lstDataRow.GroupBy(y => y["Group 1"]).Select(y => y).ToList();
                    sno = 1;
                    if (lstGroup1.Any())
                    {

                        foreach (var grp1 in lstGroup1)
                        {
                            if (!string.IsNullOrWhiteSpace(grp1.First()["Group 1"].ToString()))
                            {
                                xlWorkSheet.Cells[k, 3] = grp1.First()["Group 1"].ToString();
                                xlWorkSheet.Cells[k, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                            }
                            var lstGroup2 = grp1.GroupBy(x => x["Group 2"]).Select(x => x).ToList();
                            if (lstGroup2.Any())
                            {
                                foreach (var grp2 in lstGroup2)
                                {
                                    if (!string.IsNullOrWhiteSpace(grp2.First()["Group 2"].ToString()))
                                    {
                                        xlWorkSheet.Cells[k + 1, 3] = grp2.First()["Group 2"].ToString();
                                        xlWorkSheet.Cells[k + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                                    }
                                    var lstGroup3 = grp2.GroupBy(x => x["Group 3"]).Select(x => x).ToList();
                                    if (lstGroup3.Any())
                                    {
                                        foreach (var grp3 in lstGroup3)
                                        {
                                            if (!string.IsNullOrWhiteSpace(grp3.First()["Group 3"].ToString()))
                                            {
                                                xlWorkSheet.Cells[k + 2, 3] = grp3.First()["Group 3"].ToString();
                                                xlWorkSheet.Cells[k + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                                            }
                                            int z = k + 3;
                                            List<string> addedItem = new List<string>();
                                            foreach (var dr in grp3)
                                            {
                                                //Newly added by chocka
                                                double count = 0;
                                                double TotalUnitPrice = 0;
                                                if (addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    foreach (var drInfo in grp3)
                                                    {
                                                        if (drInfo["ItemCode"].ToString().Equals(dr["ItemCode"].ToString()) && drInfo["ItemVariantDescription"].ToString().Equals(dr["ItemVariantDescription"]))
                                                        {
                                                            count += Convert.ToDouble(dr["Qty"].ToString());
                                                            TotalUnitPrice += Convert.ToDouble(dr["TotalUnitPrice"].ToString());
                                                        }
                                                    }
                                                    addedItem.Add(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString());
                                                }



                                                xlWorkSheet.Cells[z, 1] = sno;
                                                xlWorkSheet.Cells[z, 2] = dr["ItemCode"].ToString();
                                                if ((bool)isVariantDetailsYes.IsChecked)
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    var latChar = partDes.Substring(partDes.Length - 1);
                                                    if (latChar == "/")
                                                        partDes = partDes.Remove(partDes.Length - 1);
                                                    //xlWorkSheet.Cells[z, 3] = dr["ItemVariantDescription"].ToString();
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }
                                                else
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    if (partDes.Trim().StartsWith("18/", StringComparison.InvariantCultureIgnoreCase) || partDes.StartsWith("18 /"))
                                                    {
                                                        partDes = partDes.Substring(partDes.IndexOf("/") + 1).Contains("/") ? partDes.Substring(0, partDes.IndexOf("/", partDes.IndexOf("/") + 1)) : partDes;
                                                    }
                                                    else
                                                    {
                                                        partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;
                                                    }
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }
                                                if ((bool)isExciseExemptedYes.IsChecked)
                                                {
                                                    xlWorkSheet.Cells[z, 9] = TotalUnitPrice.ToString(); //dr["TotalUnitPrice"].ToString();
                                                }
                                                else
                                                {
                                                    xlWorkSheet.Cells[z, 8] = TotalUnitPrice.ToString(); //dr["TotalUnitPrice"].ToString();
                                                }
                                                xlWorkSheet.Cells[z, 5] = count.ToString();// dr["Qty"].ToString();
                                                xlWorkSheet.Cells[z, 4] = dr["UnitOfMeas"].ToString();
                                                tot = tot + Convert.ToDouble(TotalUnitPrice.ToString());// dr["TotalUnitPrice"].ToString());
                                                z++;
                                                sno++;
                                            }
                                            k = z + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //var grps = lstDataRow.GroupBy(x => x["Group 1"].ToString()).Select(x => x).ToList();
                    //sno = 1;
                    //foreach (var drList in grps)
                    //{

                    //    if (!string.IsNullOrWhiteSpace(drList.First()["Group 1"].ToString()))
                    //    {
                    //        xlWorkSheet.Cells[k, 4] = drList.First()["Group 1"].ToString();
                    //        xlWorkSheet.Cells[k + 1, 4] = drList.First()["Group 2"].ToString();
                    //        xlWorkSheet.Cells[k + 2, 4] = drList.First()["Group 3"].ToString();
                    //        xlWorkSheet.Cells[k, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                    //        xlWorkSheet.Cells[k + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                    //        xlWorkSheet.Cells[k + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                    //    }
                    //    k = k + 3;
                    //    foreach (DataRow dr in drList)
                    //    {
                    //        xlWorkSheet.Cells[k, 1] = sno;
                    //        xlWorkSheet.Cells[k, 2] = dr["ItemCode"].ToString();
                    //        xlWorkSheet.Cells[k, 3] = dr["ItemVariantDescription"].ToString();
                    //        xlWorkSheet.Cells[k, 6] = dr["TotalUnitPrice"].ToString();
                    //        xlWorkSheet.Cells[k, 5] = dr["Qty"].ToString();
                    //        xlWorkSheet.Cells[k, 4] = dr["UnitOfMeas"].ToString();
                    //        tot = tot + Convert.ToInt32(dr["TotalUnitPrice"].ToString());
                    //        sno++;
                    //        k++;
                    //    }
                    //    k = k + 1;


                    //}

                    //xlWorkSheet.Cells[k + 2, 3] = "Total Net Price";
                    if ((bool)isExciseExemptedYes.IsChecked)
                    {
                        xlWorkSheet.Cells[k + 2, 9] = tot;
                    }
                    else
                    {
                        xlWorkSheet.Cells[k + 2, 8] = tot;
                    }
                    idengeniousTot = tot;
                    int rowEnd = k + 2;
                    int columnCounnt = 0;
                    switch (annexureId)
                    {
                        case 1:
                            columnCounnt = 9;
                            break;
                        case 2:
                            columnCounnt = 9;
                            break;
                        case 3:
                            columnCounnt = 9;
                            break;
                        default:
                            break;
                    }
                    Excel.Range range;
                    for (int r = 4; r <= rowEnd; r++)
                    {
                        range = xlWorkSheet.Range[xlWorkSheet.Cells[r, 1], xlWorkSheet.Cells[r, columnCounnt]];
                        Microsoft.Office.Interop.Excel.Borders borders = range.Borders;
                        borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sales Quote", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void WriteIntoExcel()
        {
            try
            {
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.Contains("EXCEL"))
                    {
                        clsProcess.CloseMainWindow();
                        clsProcess.Kill();
                        break;
                    }
                }


                if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf")))
                    System.IO.File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf"));
                if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx")))
                    System.IO.File.Delete(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx"));


                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                Excel.Range range;

                xlApp = new Excel.Application();

                var excelPath = (bool)rbInternational.IsChecked == false ? System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\")), @"Kewaunee\Revit\Kewaunee_BOQ.xlsx") : System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\")), @"Kewaunee\Revit\Kewaunee-International-BOQ.xlsx");

                xlWorkBook = xlApp.Workbooks.Open(excelPath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                if ((bool)rbInternational.IsChecked == false)
                {
                    #region No needed
                    //xlWorkSheet.Cells[7, 3] = cmbProjectNo.SelectedItem.ToString();
                    //xlWorkSheet.Cells[8, 3] = txtProjectName.Text;
                    //xlWorkSheet.Cells[9, 3] = txtName.Text;
                    //xlWorkSheet.Cells[10, 3] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["BuildingName"].ToString()).First();
                    //if (!(bool)isVariantDetailsYes.IsChecked)
                    //    xlWorkSheet.get_Range("C:C", Type.Missing).EntireColumn.Hidden = true;
                    //else
                    //    xlWorkSheet.get_Range("C:C", Type.Missing).EntireColumn.Hidden = false;
                    //xlWorkSheet.Cells[7, 10] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["LabName"].ToString()).First();
                    //xlWorkSheet.Cells[8, 10] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["FloorNo"].ToString()).First();
                    //xlWorkSheet.Cells[9, 10] = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].Equals(txtProjectName.Text)).Select(x => x["RoomNo"].ToString()).First();
                    //xlWorkSheet.Cells[10, 10] = "Test";



                    //int i = 14;
                    //foreach (DataRow dr in dtBulkInsert.Rows)
                    //{
                    //    xlWorkSheet.Cells[i, 1] = dr["SlNo"].ToString(); xlWorkSheet.Cells[i, 7] = dr["UnitPrice"].ToString();
                    //    xlWorkSheet.Cells[i, 2] = dr["ItemCode"].ToString(); xlWorkSheet.Cells[i, 8] = dr["Discount"].ToString();
                    //    xlWorkSheet.Cells[i, 3] = dr["VariantCode"].ToString(); xlWorkSheet.Cells[i, 9] = dr["DiscountedUnitPrice"].ToString();
                    //    xlWorkSheet.Cells[i, 4] = dr["ItemVariantDescription"].ToString(); xlWorkSheet.Cells[i, 10] = dr["TotalUnitPrice"].ToString();
                    //    xlWorkSheet.Cells[i, 5] = dr["Qty"].ToString(); xlWorkSheet.Cells[i, 11] = dr["SpecialNotes"].ToString();
                    //    xlWorkSheet.Cells[i, 6] = dr["UnitOfMeas"].ToString();
                    //    xlWorkSheet.Range["D" + i].Style.WrapText = true;
                    //    i++;
                    //}

                    //try
                    //{
                    //    xlWorkSheet.Cells[i + 2, 8] = "Sub Total";
                    //    xlWorkSheet.Cells[i + 3, 8] = "Discount";
                    //    xlWorkSheet.Cells[i + 4, 8] = "Grand Total";
                    //    xlWorkSheet.Cells[i + 5, 8] = "P&F";
                    //    xlWorkSheet.Cells[i + 6, 8] = "Freight Charges";
                    //    xlWorkSheet.Cells[i + 7, 8] = "Grand Total";

                    //    var discount = dtItems.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["Discount"].ToString())).Sum(x => Convert.ToInt32(x["Discount"].ToString()));
                    //    xlWorkSheet.Cells[i + 2, 9] = dtItems.Rows.OfType<DataRow>().ToList().Sum(x => Convert.ToInt32(x["TotalUnitPrice"].ToString()));
                    //    xlWorkSheet.Cells[i + 3, 9] = discount.ToString();
                    //    xlWorkSheet.Cells[i + 4, 9] = txtFinalTotal.Text;
                    //    xlWorkSheet.Cells[i + 5, 9] = txtpf.Text;
                    //    xlWorkSheet.Cells[i + 6, 9] = txtfreightCharges.Text;
                    //    xlWorkSheet.Cells[i + 7, 9] = txtFinalTotal.Text;
                    //}
                    //catch
                    //{

                    //} 
                    #endregion
                }
                else
                {
                    //Changed on 09-02-2018
                    int i = 12;// 2;
                    double totalPrice = 0;
                    int sno = 1;
                    xlWorkSheet.Cells[7, 3] = cmbProjectNo.SelectedItem.ToString();
                    xlWorkSheet.Cells[8, 3] = txtProjectName.Text.ToString();
                    xlWorkSheet.Cells[9, 3] = txtName.Text;

                    xlWorkSheet.Cells[7, 7] = txtQutationNo.Text.ToString();
                    xlWorkSheet.Cells[8, 7] = txtQutationRevNo.Text.ToString();
                    xlWorkSheet.Cells[9, 7] = dtePicker.Text;

                    // var lstGroups = dtItems.Rows.OfType<DataRow>().ToList().GroupBy(x => x["Group 1"]).Select(x => x).ToList();

                    var lstGroup1 = dtItems.Rows.OfType<DataRow>().ToList().GroupBy(y => y["Group 1"]).Select(y => y).ToList();
                    if (lstGroup1.Any())
                    {
                        foreach (var grp1 in lstGroup1)
                        {
                            //if (!string.IsNullOrWhiteSpace(grp1.First()["Group 1"].ToString()))
                            {
                                xlWorkSheet.Cells[i, 3] = !string.IsNullOrWhiteSpace(grp1.First()["Group 1"].ToString()) ? grp1.First()["Group 1"].ToString() : "UnGrouped";
                                xlWorkSheet.Cells[i, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                            }
                            var lstGroup2 = grp1.GroupBy(x => x["Group 2"]).Select(x => x).ToList();
                            if (lstGroup2.Any())
                            {
                                foreach (var grp2 in lstGroup2)
                                {
                                    //if (!string.IsNullOrWhiteSpace(grp2.First()["Group 2"].ToString()))
                                    {
                                        xlWorkSheet.Cells[i + 1, 3] = !string.IsNullOrWhiteSpace(grp2.First()["Group 2"].ToString()) ? grp2.First()["Group 2"].ToString() : "UnGrouped";
                                        xlWorkSheet.Cells[i + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                                    }
                                    var lstGroup3 = grp2.GroupBy(x => x["Group 3"]).Select(x => x).ToList();
                                    if (lstGroup3.Any())
                                    {
                                        foreach (var grp3 in lstGroup3)
                                        {
                                            //if (!string.IsNullOrWhiteSpace(grp3.First()["Group 3"].ToString()))
                                            {
                                                xlWorkSheet.Cells[i + 2, 3] = !string.IsNullOrWhiteSpace(grp3.First()["Group 3"].ToString()) ? grp3.First()["Group 3"].ToString() : "UnGrouped";
                                                xlWorkSheet.Cells[i + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                                            }
                                            int z = i + 3;
                                            List<string> addedItem = new List<string>();
                                            foreach (var dr in grp3)
                                            {
                                                double count = 0;
                                                double TotalUnitPrice = 0;
                                                if ((bool)isVariantDetailsYes.IsChecked && addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                //if (addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                {
                                                    continue;
                                                }
                                                else if (!(bool)isVariantDetailsYes.IsChecked && addedItem.Contains(dr["ItemCode"].ToString()))
                                                    continue;
                                                else
                                                {
                                                    foreach (var drInfo in grp3)
                                                    {
                                                        if ((bool)isVariantDetailsYes.IsChecked)
                                                        {
                                                            if (drInfo["ItemCode"].ToString().Equals(dr["ItemCode"].ToString()) && drInfo["ItemVariantDescription"].ToString().Equals(dr["ItemVariantDescription"]))
                                                            {
                                                                count += Convert.ToDouble(drInfo["Qty"].ToString());
                                                                TotalUnitPrice += Convert.ToDouble(drInfo["TotalUnitPrice"].ToString());
                                                            }
                                                            if (!addedItem.Contains(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString()))
                                                                addedItem.Add(dr["ItemCode"].ToString() + ";" + dr["ItemVariantDescription"].ToString());
                                                        }
                                                        else
                                                        {
                                                            if (drInfo["ItemCode"].ToString().Equals(dr["ItemCode"].ToString()))
                                                            {
                                                                count += Convert.ToDouble(drInfo["Qty"].ToString());
                                                                TotalUnitPrice += Convert.ToDouble(drInfo["TotalUnitPrice"].ToString());
                                                            }
                                                            if (!addedItem.Contains(dr["ItemCode"].ToString()))
                                                                addedItem.Add(dr["ItemCode"].ToString());
                                                        }
                                                    }

                                                }


                                                xlWorkSheet.Cells[z, 6] = dr["UnitPrice"].ToString(); xlWorkSheet.Cells[z, 1] = sno;
                                                xlWorkSheet.Cells[z, 2] = dr["ItemCode"].ToString(); xlWorkSheet.Cells[z, 7] = TotalUnitPrice;
                                                //var partDes = dr["ItemVariantDescription"].ToString();
                                                //partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;

                                                if ((bool)isVariantDetailsYes.IsChecked)
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    var latChar = partDes.Substring(partDes.Length - 1);
                                                    if (latChar == "/")
                                                        partDes = partDes.Remove(partDes.Length - 1);
                                                    //xlWorkSheet.Cells[z, 3] = dr["ItemVariantDescription"].ToString();
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }
                                                else
                                                {
                                                    var partDes = dr["ItemVariantDescription"].ToString();
                                                    if (partDes.Trim().StartsWith("18/", StringComparison.InvariantCultureIgnoreCase) || partDes.StartsWith("18 /"))
                                                    {
                                                        partDes = partDes.Substring(partDes.IndexOf("/") + 1).Contains("/") ? partDes.Substring(0, partDes.IndexOf("/", partDes.IndexOf("/") + 1)) : partDes;
                                                    }
                                                    else
                                                    {
                                                        partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;
                                                    }
                                                    xlWorkSheet.Cells[z, 3] = partDes;
                                                }

                                                //xlWorkSheet.Cells[z, 3] = partDes;
                                                xlWorkSheet.Cells[z, 5] = count.ToString(); ;
                                                xlWorkSheet.Cells[z, 4] = dr["UnitOfMeas"].ToString(); ;
                                                xlWorkSheet.Range["C" + z].Style.WrapText = true;
                                                try
                                                {
                                                    //if (!(string.IsNullOrWhiteSpace(dr["TotalUnitPrice"].ToString())))
                                                    //{
                                                    //    totalPrice = totalPrice + Convert.ToInt32(dr["TotalUnitPrice"].ToString());
                                                    //}
                                                    totalPrice = totalPrice + TotalUnitPrice;
                                                }
                                                catch
                                                {
                                                }
                                                z++;
                                                sno++;
                                            }
                                            i = z + 1;

                                        }
                                    }
                                }
                            }
                        }
                    }
                   
                    //int sno = 1;
                    //foreach (var drList in lstGroups)
                    //{
                    //    if (!string.IsNullOrWhiteSpace(drList.First()["Group 1"].ToString()))
                    //    {
                    //        xlWorkSheet.Cells[i, 3] = drList.First()["Group 1"].ToString();
                    //        xlWorkSheet.Cells[i + 1, 3] = drList.First()["Group 2"].ToString();
                    //        xlWorkSheet.Cells[i + 2, 3] = drList.First()["Group 3"].ToString();
                    //        xlWorkSheet.Cells[i, 3].Interior.Color = ColorTranslator.FromHtml("#FFC000");
                    //        xlWorkSheet.Cells[i + 1, 3].Interior.Color = ColorTranslator.FromHtml("#A5A5A5");
                    //        xlWorkSheet.Cells[i + 2, 3].Interior.Color = ColorTranslator.FromHtml("#95B3D7");
                    //    }
                    //    i = i + 3;
                    //    foreach (DataRow dr in drList)
                    //    {

                    //        xlWorkSheet.Cells[i, 6] = dr["UnitPrice"].ToString(); xlWorkSheet.Cells[i, 1] = sno;
                    //        xlWorkSheet.Cells[i, 2] = dr["ItemCode"].ToString(); xlWorkSheet.Cells[i, 7] = dr["TotalUnitPrice"].ToString();
                    //        var partDes = dr["ItemVariantDescription"].ToString();
                    //        partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;
                    //        xlWorkSheet.Cells[i, 3] = partDes;
                    //        xlWorkSheet.Cells[i, 5] = dr["Qty"].ToString();
                    //        xlWorkSheet.Cells[i, 4] = "Nos";
                    //        //xlWorkSheet.Cells[i, 2] = dtItems.Rows.OfType<DataRow>().ToList().Where(z => z["ItemCode"].Equals(dr["ItemCode"].ToString())).Select(z => z["Category"].ToString()).FirstOrDefault();
                    //        xlWorkSheet.Range["C" + i].Style.WrapText = true;

                    //        try
                    //        {
                    //            if (!(string.IsNullOrWhiteSpace(dr["TotalUnitPrice"].ToString())))
                    //            {
                    //                totalPrice = totalPrice + Convert.ToInt32(dr["TotalUnitPrice"].ToString());
                    //            }
                    //        }
                    //        catch
                    //        {
                    //        }

                    //        sno++;
                    //        i++;

                    //    }
                    //    i++;
                    //}

                    #region Commented
                    //foreach (DataRow dr in dtBulkInsert.Rows)
                    //{
                    //    var groupNames = new List<string>();
                    //    var lst = dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(dr["ItemCode"].ToString())).Select(y => y).ToList();

                    //    if (lst.Any())
                    //    {
                    //        lst.ForEach(x =>
                    //        {
                    //            if (!string.IsNullOrWhiteSpace(x["Group 1"].ToString()) && !groupNames.Contains(x["Group 1"].ToString()))
                    //            {
                    //                var groupList = lst.Where(y => y["Group 1"].ToString().Equals(x["Group 1"].ToString())).Select(y => y).ToList();
                    //                if (!string.IsNullOrWhiteSpace(x["Group 1"].ToString()) && !groupNames.Contains(x["Group 1"].ToString()))
                    //                {

                    //                    groupList.ForEach(y =>
                    //                    {
                    //                        xlWorkSheet.Cells[i, 4] = x["Group 1"].ToString();
                    //                        xlWorkSheet.Cells[i + 1, 5] = x["Group 2"].ToString();

                    //                        i++;

                    //                        xlWorkSheet.Cells[i, 7] = y["UnitPrice"].ToString();//xlWorkSheet.Cells[i, 1] = dr["SlNo"].ToString();
                    //                        xlWorkSheet.Cells[i, 3] = y["ItemCode"].ToString(); xlWorkSheet.Cells[i, 8] = y["TotalUnitPrice"].ToString();
                    //                        var partDes = y["ItemVariantDescription"].ToString();
                    //                        partDes = partDes.Contains("/") ? partDes.Substring(0, partDes.IndexOf("/")).Replace("/", "") : partDes;
                    //                        xlWorkSheet.Cells[i, 4] = partDes;
                    //                        xlWorkSheet.Cells[i, 6] = dr["Qty"].ToString();
                    //                        xlWorkSheet.Cells[i, 5] = "Nos";
                    //                        xlWorkSheet.Cells[i, 2] = dtItems.Rows.OfType<DataRow>().ToList().Where(z => z["ItemCode"].Equals(y["ItemCode"].ToString())).Select(z => z["Category"].ToString()).FirstOrDefault();
                    //                        xlWorkSheet.Range["C" + i].Style.WrapText = true;

                    //                        try
                    //                        {
                    //                            if (!(string.IsNullOrWhiteSpace(y["TotalUnitPrice"].ToString())))
                    //                            {
                    //                                totalPrice = totalPrice + Convert.ToInt32(y["TotalUnitPrice"].ToString());
                    //                            }
                    //                        }
                    //                        catch
                    //                        {
                    //                        }
                    //                        if (!groupNames.Contains(y["Group 1"].ToString()))
                    //                            groupNames.Add(y["Group 1"].ToString());

                    //                    });
                    //                    i++;
                    //                    if (!groupNames.Contains(x["Group 1"].ToString()))
                    //                        groupNames.Add(x["Group 1"].ToString());
                    //                }
                    //            }
                    //        });
                    //    }

                    //}
                    //range = xlWorkSheet.UsedRange;
                    #endregion
                    xlWorkSheet.Cells[i + 2, 3] = "Total Net Price in USD";
                    xlWorkSheet.Cells[i + 2, 7] = totalPrice;

                    int rowend = i + 2;

                    for (int r = 2; r <= rowend; r++)
                    {
                        range = xlWorkSheet.Range[xlWorkSheet.Cells[r, 1], xlWorkSheet.Cells[r, 7]];
                        Microsoft.Office.Interop.Excel.Borders borders = range.Borders;
                        borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                    }
                    //Microsoft.Office.Interop.Excel.Range chartRange;
                    //chartRange = xlWorkSheet.get_Range("b1", "i"+i);
                    //foreach (Microsoft.Office.Interop.Excel.Range cell in chartRange.Cells)
                    //{
                    //    cell.BorderAround2();
                    //}
                    //range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    //range.Borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);

                    xlWorkSheet.Cells[i + 5, 2] = "Payment Terms and Conditions";

                    xlWorkSheet.Cells[i + 12, 2] = "Delivery Terms and Conditions";
                }
                if ((bool)rbInternational.IsChecked == false)
                    xlWorkBook.SaveAs(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"));
                else
                    xlWorkBook.SaveAs(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx"));
                //xlWorkBook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf"));


                Wsh_Print_Setting_OnePage(xlWorkSheet, Excel.XlPaperSize.xlPaperA4, xlApp);

                if ((bool)rbInternational.IsChecked == false)
                { }  //xlWorkBook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"), Type.Missing, true, false, Type.Missing, Type.Missing, false, Type.Missing);
                else { }
                    //xlWorkBook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ1.pdf"), Type.Missing, true, false, Type.Missing, Type.Missing, false, Type.Missing);

                xlWorkBook.Save();
                xlWorkBook.Close();

                xlApp.Quit();

                string path = string.Empty;
                string destPath = string.Empty;
                if ((bool)rbInternational.IsChecked == false)
                {
                    //path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf");
                    //destPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf");
                }
                else
                {
                    //path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ1.pdf");
                    //destPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf");
                }

                //PdfReader reader = new PdfReader(path);
                //PdfStamper stamper = new PdfStamper(reader, new System.IO.FileStream(destPath, System.IO.FileMode.Create));
                //PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, reader.GetPageSize(1).Height, 0.75f);
                //PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, stamper.Writer);
                //stamper.Writer.SetOpenAction(action);
                //stamper.Close();
                //reader.Close();

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                #region Commented

                //SautinSoft.ExcelToPdf stinSoft = new SautinSoft.ExcelToPdf();
                //if ((bool)rbInternational.IsChecked == false)
                //{
                //    if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx")))
                //        stinSoft.ConvertFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf"));
                //}
                //else
                //    if (System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx")))
                //        stinSoft.ConvertFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.xlsx"), System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee-International-BOQ.pdf"));

                //Workbook workBook = new Workbook();
                //workBook.LoadFromFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.xlsx"));
                ////workBook.SaveToFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"),Spire.Xls.FileFormat.PDF);
                //workBook.SaveToPdf(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ1.pdf"));


                // Save and preview PDF
                //pdfDocument.SaveToFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Kewaunee_BOQ.pdf"));
                //workbook.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sales Quote", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

        public void Wsh_Print_Setting_OnePage(Microsoft.Office.Interop.Excel.Worksheet WshTrg, Excel.XlPaperSize ePaperSize, Excel.Application xlApp)
        {
            // ERROR: Not supported in C#: OnErrorStatement

            // xlApp.PrintCommunication = false;
            var _with1 = WshTrg.PageSetup;
            _with1.LeftMargin = xlApp.InchesToPoints(0);
            _with1.RightMargin = xlApp.InchesToPoints(0);
            _with1.TopMargin = xlApp.InchesToPoints(0);
            _with1.BottomMargin = xlApp.InchesToPoints(0);
            _with1.HeaderMargin = xlApp.InchesToPoints(0);
            _with1.FooterMargin = xlApp.InchesToPoints(0);
            //.Orientation = xlLandscape
            _with1.Orientation = Excel.XlPageOrientation.xlPortrait;
            _with1.PaperSize = ePaperSize;
            _with1.Zoom = false;
            _with1.FitToPagesWide = 1;
            _with1.FitToPagesTall = 1;
            //xlApp.ActiveWindow.Zoom = 60;
            // xlApp.PrintCommunication = true;
        }

        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtQutationNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtQutationRevNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(dtePicker.SelectedDate.ToString()))
                return false;
            return true;
        }

        private void txtSpecialDiscount_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSpecialDiscount.Text))
            {
                int total = Convert.ToInt32(totalText) - Convert.ToInt32((Convert.ToInt32(totalText) * Convert.ToInt32(txtSpecialDiscount.Text)) / 100);
                txtTotal.Text = total.ToString();
                txtFinalTotal.Text = txtTotal.Text;
            }
            else
            {
                txtTotal.Text = totalText;
                txtFinalTotal.Text = totalText;
            }
        }

        private void rbInternational_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)rbInternational.IsChecked)
            {
                cbIsCustomer.IsChecked = false;
                cbIsCustomer.IsEnabled = false;
                txtCformNo.Clear();
                txtCformNo.IsEnabled = false;
                ChangePricesBasedOnSelection(true);

            }
        }

        private void ChangePricesBasedOnSelection(bool isInternational)
        {
            double unitPrice = 0;
            double totUnitPrice = 0;
            if (isInternational)
            {
                dtItems.Rows.OfType<DataRow>().ToList().ForEach(x =>
                {
                    var dr = dtProducts.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Trim().Equals(x["ItemCode"].ToString().Trim())).Select(y => y).FirstOrDefault();
                    if (dr != null)
                    {
                        if (dr["ItemCode"].ToString().Contains("JBG-"))
                        { }
                        else if (dr["ItemCode"].ToString().Contains("SK-"))
                        { }
                        else
                        {
                            x["UnitPrice"] = dr["Price in USD - P.Y"].ToString();
                            unitPrice = Convert.ToDouble(x["UnitPrice"].ToString());
                            totUnitPrice = (Convert.ToDouble(x["Qty"].ToString()) * unitPrice);
                            x["TotalUnitPrice"] = totUnitPrice.ToString();
                        }
                    }
                });
            }
            else
            {
                dtItems.Rows.OfType<DataRow>().ToList().ForEach(x =>
                {
                    var dr = dtProducts.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Trim().Equals(x["ItemCode"].ToString().Trim())).Select(y => y).FirstOrDefault();
                    if (dr != null)
                    {
                        x["UnitPrice"] = dr["Price in INR - P.Y"].ToString();
                        unitPrice = Convert.ToDouble(x["UnitPrice"].ToString());
                        totUnitPrice = (Convert.ToDouble(x["Qty"].ToString()) * unitPrice);
                        x["TotalUnitPrice"] = totUnitPrice.ToString();
                    }
                });
            }
            LoadTotalValues();
        }

        int calTedTax = 0;
        private void rbDomestic_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)rbDomestic.IsChecked)
            {
                cbIsCustomer.IsEnabled = true;
                ChangePricesBasedOnSelection(false);
            }
        }

        private void cbIsCustomer_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)cbIsCustomer.IsChecked)
            {
                txtCformNo.IsEnabled = true;
            }
            else
            {
                txtCformNo.Clear();
                txtCformNo.IsEnabled = false;
            }
        }


        private bool isDisplayVariant;
        private void isVariantDetailsYes_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)isVariantDetailsYes.IsChecked)
                isDisplayVariant = true;
        }

        private void isVariantDetailsNo_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)isVariantDetailsNo.IsChecked)
                isDisplayVariant = false;
        }

        private bool isCalculateExciseExempt = true;
        private void isExciseExemptedYes_Checked_1(object sender, RoutedEventArgs e)
        {
            //txtTotal.Text = totalText;
            //txtFinalTotal.Text = totalText;
        }

        int exciseTax = 0;
        private void isExciseExemptedYes_Unchecked_1(object sender, RoutedEventArgs e)
        {

        }

        private bool isCalculateImportDutyExempt;
        private void isImportDutyExemptedYes_Checked_1(object sender, RoutedEventArgs e)
        {
            isCalculateImportDutyExempt = true;
        }

        private void isImportDutyExemptedYes_Unchecked_1(object sender, RoutedEventArgs e)
        {
            isCalculateImportDutyExempt = false;
        }

        private void LoadSearchComboBoxValues()
        {
            var listItemType = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["ItemPurchaseType"].ToString())).Select(x => x["ItemPurchaseType"].ToString()).Distinct().ToList();
            ////MessageBox.Show(listItemType.Count.ToString());
            if (listItemType != null && listItemType.Count > 1)
                listItemType.Insert(2, "Show All");
            else if (listItemType != null && listItemType.Count > 0)
                listItemType.Insert(1, "Show All");
            else
            {
                listItemType = new List<string>();
                listItemType.Insert(0, "Show All");
            }
            cmbItemType.ItemsSource = listItemType;

            var listPackageType = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["PackageType"].ToString())).Select(x => x["PackageType"].ToString()).Distinct().ToList();

            if (listPackageType != null && listPackageType.Count > 0)
                listPackageType.Insert(listPackageType.Count, "Show All");// listPackageType.Insert(5, "Show All");
            else
            {
                listPackageType = new List<string>();
                listPackageType.Insert(0, "Show All");
            }
            cmbPackageType.ItemsSource = listPackageType;

        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (cmbItemType.SelectedIndex != -1 && cmbPackageType.SelectedIndex != -1)
            {
                if (cmbPackageType.SelectedItem.ToString().Equals("Show All") && cmbItemType.SelectedItem.ToString().Equals("Show All"))
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.ItemsSource = dtItems.DefaultView;
                    return;
                }

                if (cmbItemType.SelectedIndex != -1 && cmbPackageType.SelectedItem.ToString().Equals("Show All"))
                {
                    var drListProducts = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemPurchaseType"].ToString().Equals(cmbItemType.SelectedItem.ToString())).Select(x => x).ToList();
                    var drPartCodeList = new List<DataRow>();
                    var drList = new List<DataRow>();

                    drListProducts.ForEach(x =>
                    {
                        DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["ItemCode"].ToString())).Select(y => y).FirstOrDefault();
                        if (newDr != null)
                            drList.Add(newDr);
                    });


                    if (drList.Count > 0)
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.ItemsSource = drList.CopyToDataTable().DefaultView;
                    }
                    else
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.Items.Clear();
                    }

                    return;
                }
                else if (cmbItemType.SelectedItem.ToString().Equals("Show All") && cmbPackageType.SelectedIndex != -1)
                {
                    var drListProducts = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["PackageType"].ToString().Equals(cmbPackageType.SelectedItem.ToString())).Select(x => x).ToList();
                    var drPartCodeList = new List<DataRow>();
                    var drList = new List<DataRow>();

                    drListProducts.ForEach(x =>
                    {
                        DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["ItemCode"].ToString())).Select(y => y).FirstOrDefault();
                        if (newDr != null)
                            drList.Add(newDr);
                    });


                    if (drList.Count > 0)
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.ItemsSource = drList.CopyToDataTable().DefaultView;
                    }
                    else
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.Items.Clear();
                    }
                    return;
                }
                else
                {
                    //
                    var drListProducts = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemPurchaseType"].ToString().Equals(cmbItemType.SelectedItem.ToString()) && x["PackageType"].ToString().Equals(cmbPackageType.SelectedItem.ToString())).Select(x => x).ToList();
                    var drPartCodeList = new List<DataRow>();
                    var drList = new List<DataRow>();

                    drListProducts.ForEach(x =>
                    {
                        DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["ItemCode"].ToString())).Select(y => y).FirstOrDefault();
                        if (newDr != null)
                            drList.Add(newDr);
                    });


                    if (drList.Count > 0)
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.ItemsSource = drList.CopyToDataTable().DefaultView;
                    }
                    else
                    {
                        dgBOM.ItemsSource = null;
                        dgBOM.Items.Clear();
                    }

                    return;
                }
            }
            else if (cmbItemType.SelectedIndex != -1)
            {
                if (cmbItemType.SelectedItem.ToString().Equals("Show All"))
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.ItemsSource = dtItems.DefaultView;
                    //cmbItemType.SelectedIndex = -1;
                    return;
                }

                var drListProducts = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemPurchaseType"].ToString().Equals(cmbItemType.SelectedItem.ToString())).Select(x => x).ToList();
                var drPartCodeList = new List<DataRow>();
                var drList = new List<DataRow>();

                drListProducts.ForEach(x =>
                {
                    DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["ItemCode"].ToString())).Select(y => y).FirstOrDefault();
                    if (newDr != null)
                        drList.Add(newDr);
                });


                if (drList.Count > 0)
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.ItemsSource = drList.CopyToDataTable().DefaultView;
                    //cmbItemType.SelectedIndex = -1;
                }
                else
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.Items.Clear();
                }
            }
            else if (cmbPackageType.SelectedIndex != -1)
            {
                if (cmbPackageType.SelectedItem.ToString().Equals("Show All"))
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.ItemsSource = dtItems.DefaultView;
                    //cmbPackageType.SelectedIndex = -1;
                    return;
                }

                var drListProducts = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["PackageType"].ToString().Equals(cmbPackageType.SelectedItem.ToString())).Select(x => x).ToList();
                var drPartCodeList = new List<DataRow>();
                var drList = new List<DataRow>();

                drListProducts.ForEach(x =>
                {
                    DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["ItemCode"].ToString())).Select(y => y).FirstOrDefault();
                    if (newDr != null)
                        drList.Add(newDr);
                });

                //drPartCodeList.ForEach(x =>
                //{
                //    DataRow newDr = (DataRow)dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().Equals(x["PartCode"].ToString())).Select(y => y).FirstOrDefault();
                //    drList.Add(newDr);
                //});
                if (drList.Count > 0)
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.ItemsSource = drList.CopyToDataTable().DefaultView;
                    //cmbPackageType.SelectedIndex = -1;
                }
                else
                {
                    dgBOM.ItemsSource = null;
                    dgBOM.Items.Clear();
                }
            }
        }

        private void dgBOM_CellEditEnding_1(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataRowView drView = (DataRowView)e.Row.Item;
                if (e.Column.Header.ToString().Equals("Discount %") || e.Column.Header.ToString().Equals("Special Notes"))
                    if (drView != null)
                    {
                        var editingElement = ((System.Windows.Controls.TextBox)(e.EditingElement));
                        if (editingElement != null)
                        {
                            if (e.Column.Header.ToString().Equals("Discount %"))
                            {
                                int discnt = !string.IsNullOrWhiteSpace(editingElement.Text) ? Convert.ToInt32(editingElement.Text) : 0;
                                int disCountedUnit = Convert.ToInt32(drView["TotalUnitPrice"].ToString()) - Convert.ToInt32((Convert.ToInt32(drView["TotalUnitPrice"].ToString()) * discnt / 100));
                                drView["DiscountedUnitPrice"] = disCountedUnit.ToString();
                                drView["TotalUnitPrice"] = disCountedUnit.ToString();
                            }
                            else if (e.Column.Header.ToString().Equals("Special Notes"))
                            {
                                string notes = editingElement.Text;
                                drView["SpecialNotes"] = notes;
                            }
                            //dgBOM.Items.OfType<DataRowView>().ToList().ForEach(x =>
                            //{
                            //    if (!string.IsNullOrWhiteSpace(x.Row["TotalUnitPrice"].ToString()))
                            //    {
                            //        txtTotal.Text = (Convert.ToInt32(txtTotal.Text) + Convert.ToInt32(x.Row["TotalUnitPrice"].ToString())).ToString();
                            //        txtFinalTotal.Text = txtTotal.Text;
                            //    }
                            //});
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void rbInternational_Unchecked_1(object sender, RoutedEventArgs e)
        {

        }

        int installCharges = 0;
        private void txtInstallationCharges_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtInstallationCharges.Text))
            {
                txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - (installCharges - Convert.ToInt32(txtInstallationCharges.Text))).ToString() : (Convert.ToInt32(txtTotal.Text) + (Convert.ToInt32(txtInstallationCharges.Text)) - installCharges).ToString();
                totalText = txtTotal.Text;
                txtFinalTotal.Text = totalText;
                installCharges = Convert.ToInt32(txtInstallationCharges.Text);
                isKeyDown = false;
            }
            else
            {
                txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - installCharges).ToString() : txtTotal.Text;
                totalText = txtTotal.Text;
                txtFinalTotal.Text = totalText;
                installCharges = 0;
                isKeyDown = false;
            }

        }
        int frghtCharges = 0;
        private void txtfreightCharges_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtfreightCharges.Text))
            {
                txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - (frghtCharges - Convert.ToInt32(txtfreightCharges.Text))).ToString() : (Convert.ToInt32(txtTotal.Text) + (Convert.ToInt32(txtfreightCharges.Text)) - frghtCharges).ToString();
                totalText = txtTotal.Text;
                txtFinalTotal.Text = totalText;
                frghtCharges = Convert.ToInt32(txtfreightCharges.Text);
                isKeyDown = false;
            }
            else
            {
                txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - frghtCharges).ToString() : txtTotal.Text;
                totalText = txtTotal.Text;
                txtFinalTotal.Text = totalText;
                frghtCharges = 0;
                isKeyDown = false;
            }
        }
        int otherCharges = 0;
        private void txtOtherCharges_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtOtherCharges.Text))
                {
                    txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - (otherCharges - Convert.ToInt32(txtOtherCharges.Text))).ToString() : (Convert.ToInt32(txtTotal.Text) + (Convert.ToInt32(txtOtherCharges.Text)) - otherCharges).ToString();
                    totalText = txtTotal.Text;
                    txtFinalTotal.Text = totalText;
                    otherCharges = Convert.ToInt32(txtOtherCharges.Text);
                    isKeyDown = false;
                }
                else
                {
                    txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - otherCharges).ToString() : txtTotal.Text;
                    totalText = txtTotal.Text;
                    txtFinalTotal.Text = totalText;
                    otherCharges = 0;
                    isKeyDown = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        bool isKeyDown = false;
        private void txtInstallationCharges_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            int val = 0;
            if (!string.IsNullOrWhiteSpace((sender as TextBox).Text))
            {
                if (e.Key == Key.Back)
                {
                    switch ((sender as TextBox).Name)
                    {
                        case "txtInstallationCharges":
                            installCharges = Convert.ToInt32(txtInstallationCharges.Text);
                            isKeyDown = true;
                            break;
                        case "txtfreightCharges":
                            frghtCharges = Convert.ToInt32(txtfreightCharges.Text);
                            isKeyDown = true;
                            break;
                        case "txtOtherCharges":
                            otherCharges = Convert.ToInt32(txtOtherCharges.Text);
                            isKeyDown = true;
                            break;
                        default:
                            break;
                    }

                }

            }
        }

        private void TaxProcess()
        {
            rbInternational.IsChecked = true;
            isVariantDetailsYes.IsChecked = true;
            isExciseExemptedYes.IsChecked = true;
            isImportDutyExemptedYes.IsChecked = true;
            isSEZCentralGovorgYes.IsChecked = true;
        }

        bool isCformEntered = false;
        int calculatedTax = 0;
        bool isTaxCalculatedWithoutText = false;
        bool isTaxCalculatedText = false;
        bool isTaxAlreadyCalculatedFromInternational = true;
        private void txtCformNo_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void btnUpdate_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                dgBOM.Items.OfType<DataRowView>().ToList().Where(x => !string.IsNullOrWhiteSpace(x.Row["Discount"].ToString())).Select(x =>
                   {
                       if (!string.IsNullOrWhiteSpace(x.Row["Discount"].ToString()))
                       {
                           int discnt = Convert.ToInt32(x.Row["Discount"].ToString());
                           int disCountedUnit = Convert.ToInt32(x["TotalUnitPrice"].ToString()) - Convert.ToInt32((Convert.ToInt32(x["TotalUnitPrice"].ToString()) * discnt / 100));
                           x["DiscountedUnitPrice"] = disCountedUnit.ToString();
                           x["TotalUnitPrice"] = Convert.ToInt32(disCountedUnit * Convert.ToInt32(x["Qty"].ToString())).ToString();
                       }
                       return x;
                   }).ToList();
                dgBOM.Items.OfType<DataRowView>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.Row["TotalUnitPrice"].ToString()))
                    {
                        txtTotal.Text = (Convert.ToInt32(txtTotal.Text) + Convert.ToInt32(x.Row["TotalUnitPrice"].ToString())).ToString();
                        txtFinalTotal.Text = txtTotal.Text;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void txtpf_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            int val = 0;
            if (!string.IsNullOrWhiteSpace((sender as TextBox).Text))
            {
                if (e.Key == Key.Back)
                {
                    pf = Convert.ToInt32(txtpf.Text);
                    isKeyDown = true;
                }
            }
        }

        int pf = 0;

        private void txtpf_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtpf.Text))
                {
                    txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - (pf - Convert.ToInt32(txtpf.Text))).ToString() : (Convert.ToInt32(txtTotal.Text) + (Convert.ToInt32(txtpf.Text)) - pf).ToString();
                    totalText = txtTotal.Text;
                    txtFinalTotal.Text = totalText;
                    pf = Convert.ToInt32(txtpf.Text);
                    isKeyDown = false;
                }
                else
                {
                    txtTotal.Text = isKeyDown == true ? (Convert.ToInt32(txtTotal.Text) - pf).ToString() : txtTotal.Text;
                    totalText = txtTotal.Text;
                    txtFinalTotal.Text = totalText;
                    pf = 0;
                    isKeyDown = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
