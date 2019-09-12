using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Products : Window
    {
        private string _connectionString;
        private DataTable dtProducts = new DataTable();
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public Products(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            GetProducts();
            LoadFilterValues();
            UserAccessValidations();
            LoadUnitOfMeasures();
            ClsProperties.isClosed = false;
        }

        private void LoadUnitOfMeasures()
        {
            cmbunitOfMeasures.Items.Clear();
            cmbunitOfMeasures.Items.Add("Nos");
            cmbunitOfMeasures.Items.Add("Sqft");
            cmbunitOfMeasures.Items.Add("Mtrs");
            cmbunitOfMeasures.Items.Add("RFT");
        }

        private void LoadFilterValues()
        {
            cmbItemPurchaseType.Items.Clear();
            cmbPackageType.Items.Clear();

            cmbPackageType.Items.Add("Furniture");
            cmbPackageType.Items.Add("Fume Hood");
            cmbPackageType.Items.Add("Exhaust");
            cmbPackageType.Items.Add("GDS");
            cmbPackageType.Items.Add("Drain");

            cmbItemPurchaseType.Items.Add("Indigenous ");
            cmbItemPurchaseType.Items.Add("Import");

        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbProductsAdd.IsEnabled = false;
                tbProducts.SelectedIndex = 0;
            }
            if (!_delete)
            {
                tbProductsAdd.IsEnabled = false;
                tbProducts.SelectedIndex = 0;
            }
            if (_edit)
            {
                tbProductsAdd.IsEnabled = true;
                tbProducts.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (btnAdd.Content.Equals("Add"))
            { InsertIntoProductMaster(); }
            else
            { }
        }

        private bool CheckProductInserted()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select GroupHeader,MainGroup,SubGroup1,SubGroup2,SubGroup3,SubGroup4,SubGroup5,SubGroup6,SubGroup7,SubGroup8,ProductFamily,ProductId,FamilyName from ProductMasterNew where GroupHeader=@GroupHeader and MainGroup=@MainGroup and SubGroup1=@SubGroup1 and SubGroup2=@SubGroup2 and SubGroup3=@SubGroup3 and SubGroup4=@SubGroup4 and SubGroup5=@SubGroup5 and SubGroup6=@SubGroup6 and SubGroup7=@SubGroup7 and SubGroup8=@SubGroup8 and ProductFamily=@ProductFamily and FamilyName = @FamilyName";

                using (var scmd = new SqlCommand(query, scn))
                {
                    scmd.CommandType = CommandType.Text;
                    scmd.Parameters.Add(new SqlParameter("@GroupHeader", cmbGroupHeader.Text));
                    scmd.Parameters.Add(new SqlParameter("@MainGroup", cmbMainProductGroup.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup1", sbGroup1.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup2", sbGroup2.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup3", sbGroup3.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup4", sbGroup4.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup5", sbGroup5.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup6", sbGroup6.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup7", sbGroup7.Text));
                    scmd.Parameters.Add(new SqlParameter("@SubGroup8", sbGroup8.Text));
                    scmd.Parameters.Add(new SqlParameter("@ProductFamily", txtProdtFamily.Text));
                    scmd.Parameters.Add(new SqlParameter("@FamilyName", txtFamilyName.Text));
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        ProductId = Convert.ToInt32(dt.Rows[0]["ProductId"]);
                        return true;
                    }
                    return false;
                }

            }
        }

        private string GetCommand(SqlConnection scn)
        {
            var query = @"update ProductMasterNew SET ItemCode=@ItemCode,ItemDescription=@ItemDescription,GroupHeader=@GroupHeader,MainGroup=@MainGroup,SubGroup1=@SubGroup1,SubGroup2=@SubGroup2,SubGroup3=@SubGroup3,SubGroup4=@SubGroup4,SubGroup5=@SubGroup5,SubGroup6=@SubGroup6,SubGroup7=@SubGroup7,SubGroup8=@SubGroup8,ProductFamily=@ProductFamily,ItemPurchaseType=@ItemPurchaseType,[Price in USD - P.Y]=@PriceinUSDPY,[Price in USD - C.Y]=@PriceinUSDCY,[Price in INR - P.Y]=@PriceinINRPY,[Price in INR - C.Y]=@PriceinINRCY,UnitofMeasures=@UnitofMeasures,EDApplicable=@EDApplicable,VATApplicable=@VATApplicable,PackageType=@PackageType,CreatedDate=@CreatedDate,IsDeleted=@IsDeleted,FamilyName=@FamilyName,CalculateBy=@CalculateBy where ProductId=@ProductId";
            return query;
        }

        private void InsertIntoProductMaster()
        {
            if (Validations())
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    var query = string.Empty;
                    bool isUpdate = false;
                    if (CheckProductInserted())
                    {
                        isUpdate = true;
                        query = GetCommand(scn);
                    }
                    else
                        query = "insert into ProductMasterNew(ItemCode,ItemDescription,GroupHeader,MainGroup,SubGroup1,SubGroup2,SubGroup3,SubGroup4,SubGroup5,SubGroup6,SubGroup7,SubGroup8,ProductFamily,ItemPurchaseType,[Price in USD - P.Y],[Price in USD - C.Y],[Price in INR - P.Y],[Price in INR - C.Y],UnitofMeasures,EDApplicable,VATApplicable,PackageType,CreatedDate,IsDeleted,FamilyName) values (@ItemCode,@ItemDescription,@GroupHeader,@MainGroup,@SubGroup1,@SubGroup2,@SubGroup3,@SubGroup4,@SubGroup5,@SubGroup6,@SubGroup7,@SubGroup8,@ProductFamily,@ItemPurchaseType,@PriceinUSDPY,@PriceinUSDCY,@PriceinINRPY,@PriceinINRCY,@UnitofMeasures,@EDApplicable,@VATApplicable,@PackageType,@CreatedDate,@IsDeleted,@FamilyName)";

                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = CommandType.Text;
                        scmd.Parameters.Add(new SqlParameter("@ItemCode", txtItemCode.Text));
                        scmd.Parameters.Add(new SqlParameter("@ItemDescription", txtItemDescription.Text));
                        scmd.Parameters.Add(new SqlParameter("@GroupHeader", cmbGroupHeader.Text));
                        scmd.Parameters.Add(new SqlParameter("@MainGroup", cmbMainProductGroup.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup1", sbGroup1.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup2", sbGroup2.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup3", sbGroup3.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup4", sbGroup4.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup5", sbGroup5.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup6", sbGroup6.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup7", sbGroup7.Text));
                        scmd.Parameters.Add(new SqlParameter("@SubGroup8", sbGroup8.Text));
                        scmd.Parameters.Add(new SqlParameter("@ProductFamily", txtProdtFamily.Text));
                        scmd.Parameters.Add(new SqlParameter("@ItemPurchaseType", cmbItemPurchaseType.Text));
                        scmd.Parameters.Add(new SqlParameter("@PriceinUSDPY", txtUSDPY.Text));
                        scmd.Parameters.Add(new SqlParameter("@PriceinUSDCY", txtUSDCY.Text));
                        scmd.Parameters.Add(new SqlParameter("@PriceinINRPY", txtINRPY.Text));
                        scmd.Parameters.Add(new SqlParameter("@PriceinINRCY", txtINRCY.Text));
                        scmd.Parameters.Add(new SqlParameter("@UnitofMeasures", !string.IsNullOrWhiteSpace(cmbunitOfMeasures.SelectedItem.ToString()) ? cmbunitOfMeasures.SelectedItem.ToString() : string.Empty));
                        scmd.Parameters.Add(new SqlParameter("@EDApplicable", txtedapplicable.Text));
                        scmd.Parameters.Add(new SqlParameter("@VATApplicable", txtVatApplicable.Text));
                        scmd.Parameters.Add(new SqlParameter("@PackageType", cmbPackageType.Text));
                        scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now.ToString()));
                        scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                        scmd.Parameters.Add(new SqlParameter("@FamilyName", txtFamilyName.Text));
                        scmd.Parameters.Add(new SqlParameter("@CalculateBy", calculateBy));
                        if (isUpdate)
                            scmd.Parameters.Add(new SqlParameter("@ProductId", ProductId));
                        scmd.ExecuteNonQuery();
                        MessageBox.Show("Product added successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearValues();
                        DockablePage.SingleTonDockablePageInstance.LoadData();
                    }
                }
            }
        }

        private bool CheckCodeExists()
        {
            string code = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].Equals(txtItemCode.Text)).Select(x => x["ItemCode"].ToString()).FirstOrDefault();
            bool retValue = string.IsNullOrWhiteSpace(code) ? false : true;
            return retValue;
        }

        private void ClearValues()
        {
            txtItemCode.Clear();
            txtItemDescription.Clear();
            cmbGroupHeader.SelectedIndex = -1;
            cmbMainProductGroup.SelectedIndex = -1;
            sbGroup1.Text = string.Empty;
            sbGroup2.Text = string.Empty;
            sbGroup3.Text = string.Empty;
            sbGroup4.Text = string.Empty;
            sbGroup5.Text = string.Empty;
            sbGroup6.Text = string.Empty;
            sbGroup7.Text = string.Empty;
            sbGroup8.Text = string.Empty;
            cmbPackageType.Text = string.Empty;
            cmbItemPurchaseType.Text = string.Empty;
            cmbGroupHeader.Text = string.Empty;
            cmbMainProductGroup.Text = string.Empty;
            sbGroup1.Text = string.Empty;
            sbGroup1.SelectedIndex = -1;
            sbGroup2.SelectedIndex = -1;
            sbGroup3.SelectedIndex = -1;
            sbGroup4.SelectedIndex = -1;
            sbGroup5.SelectedIndex = -1;
            sbGroup6.SelectedIndex = -1;
            sbGroup7.SelectedIndex = -1;
            sbGroup8.SelectedIndex = -1;
            txtProdtFamily.Clear();
            cmbItemPurchaseType.SelectedIndex = -1;
            txtUSDPY.Clear();
            txtUSDCY.Clear();
            txtINRPY.Clear();
            txtINRCY.Clear();
            cmbunitOfMeasures.SelectedIndex = -1;
            txtedapplicable.Clear();
            txtVatApplicable.Clear();
            cmbPackageType.SelectedIndex = -1;
            txtFamilyName.Clear();
            rb1.IsChecked = true;
            rb1.IsChecked = false;
            rb1.IsChecked = false;
            calculateBy = 0;
        }



        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtItemCode.Text))
                return false;
            if (CheckCodeExists())
            {
                MessageBox.Show("Item Code already exists.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private void GetProducts()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                cmbMainProductGroup.Items.Clear();
                cmbGroupHeader.Items.Clear();
                sbGroup1.Items.Clear();
                sbGroup2.Items.Clear();
                sbGroup3.Items.Clear();
                sbGroup4.Items.Clear();
                sbGroup5.Items.Clear();
                sbGroup6.Items.Clear();
                sbGroup7.Items.Clear();
                sbGroup8.Items.Clear();
                cmbPackageType.Items.Clear();
                cmbItemPurchaseType.Items.Clear();
                dtProducts = new DataTable();
                var query = "select * from ProductMasterNew where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProducts.Load(scmd.ExecuteReader());
                }
                if (dtProducts.Rows.Count > 0)
                {
                    LoadComboBoxValues();
                }
            }
        }

        private void LoadComboBoxValues()
        {
            foreach (DataRow dr in dtProducts.Rows)
            {
                if (!string.IsNullOrWhiteSpace(dr["GroupHeader"].ToString()))
                {
                    if (!cmbGroupHeader.Items.Contains(dr["GroupHeader"].ToString()))
                        cmbGroupHeader.Items.Add(dr["GroupHeader"].ToString());
                }
                if (!string.IsNullOrWhiteSpace(dr["MainGroup"].ToString()))
                    if (!cmbMainProductGroup.Items.Contains(dr["MainGroup"].ToString()))
                        cmbMainProductGroup.Items.Add(dr["MainGroup"].ToString());

                if (!string.IsNullOrWhiteSpace(dr["SubGroup1"].ToString()))
                    if (!sbGroup1.Items.Contains(dr["SubGroup1"].ToString()))
                        sbGroup1.Items.Add(dr["SubGroup1"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup2"].ToString()))
                    if (!sbGroup2.Items.Contains(dr["SubGroup2"].ToString()))
                        sbGroup2.Items.Add(dr["SubGroup2"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup3"].ToString()))
                    if (!sbGroup3.Items.Contains(dr["SubGroup3"].ToString()))
                        sbGroup3.Items.Add(dr["SubGroup3"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup4"].ToString()))
                    if (!sbGroup4.Items.Contains(dr["SubGroup4"].ToString()))
                        sbGroup4.Items.Add(dr["SubGroup4"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup5"].ToString()))
                    if (!sbGroup5.Items.Contains(dr["SubGroup5"].ToString()))
                        sbGroup5.Items.Add(dr["SubGroup5"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup6"].ToString()))
                    if (!sbGroup6.Items.Contains(dr["SubGroup6"].ToString()))
                        sbGroup6.Items.Add(dr["SubGroup6"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup7"].ToString()))
                    if (!sbGroup7.Items.Contains(dr["SubGroup7"].ToString()))
                        sbGroup7.Items.Add(dr["SubGroup7"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["SubGroup8"].ToString()))
                    if (!sbGroup8.Items.Contains(dr["SubGroup8"].ToString()))
                        sbGroup8.Items.Add(dr["SubGroup8"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["ItemPurchaseType"].ToString()))
                    if (!cmbItemPurchaseType.Items.Contains(dr["ItemPurchaseType"].ToString()))
                        cmbItemPurchaseType.Items.Add(dr["ItemPurchaseType"].ToString());
                if (!string.IsNullOrWhiteSpace(dr["PackageType"].ToString()))
                    if (!cmbPackageType.Items.Contains(dr["PackageType"].ToString()))
                        cmbPackageType.Items.Add(dr["PackageType"].ToString());
            }
        }

        private void btnCancel_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cmbGroupHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null)
                (sender as ComboBox).Text = (sender as ComboBox).SelectedItem.ToString();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        public int ProductId { get; set; }

        private int calculateBy = 0;
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var content = (sender as RadioButton).Content.ToString();
            switch (content)
            {
                case "Number":
                    calculateBy = 1;
                    break;
                case "Square Feet":
                    calculateBy = 2;
                    break;
                case "Running Feet":
                    calculateBy = 3;
                    break;
                default:
                    break;
            }
        }
    }
}
