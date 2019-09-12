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
    /// Interaction logic for Pricing.xaml
    /// </summary>
    public partial class Pricing : Window
    {
        private string _connectionString;
        private string query;
        private string _taxDescription;
        private string _taxValue;
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        private DataTable dtCntry = new DataTable();
        private DataTable dtPricing = new DataTable();
        private DataTable dtProducts = new DataTable();
        private int countryID = 0;
        private int priceid = 0;
        private int ProductID = 0;
        public Pricing(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetPricing();
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            UserAccessValidations();
            GetCountry();
            GetProducts();
            ClsProperties.isClosed = false;
        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbAddPricing.IsEnabled = false;
                tbPricing.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbAddPricing.IsEnabled = false;
                tbPricing.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbAddPricing.IsEnabled = true;
                tbPricing.SelectedIndex = 0;
            }
        }

        private void GetCountry()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "select * from Country";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    cmbCountry.Items.Clear();
                    dtCntry = new DataTable();
                    dtCntry.Load(scmd.ExecuteReader());
                    if (dtCntry.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtCntry.Rows)
                        {
                            cmbCountry.Items.Add(dr["CountryName"].ToString());
                        }
                        cmbCountry.Items.Insert(0, "--Select--");
                        cmbCountry.SelectedIndex = 0;
                    }
                }
            }

        }

        private void GetProducts()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "select * from ProductMasterNew where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    cmboProduct.Items.Clear();
                    dtProducts = new DataTable();
                    dtProducts.Load(scmd.ExecuteReader());
                    if (dtProducts.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtProducts.Rows)
                        {
                            cmboProduct.Items.Add(dr["ProductFamily"].ToString());
                        }
                        cmboProduct.Items.Insert(0, "--Select--");
                        cmboProduct.SelectedIndex = 0;
                    }
                }
            }

        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the Price?", "Pricing", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgPricing.SelectedItem;
                cmbCountry.SelectedIndex = dtCntry.Rows.OfType<DataRow>().ToList().Where(x => x["Id"].Equals(selectedRow["CountryId"])).Select(x => Convert.ToInt32(x["Id"].ToString())).FirstOrDefault();
                cmboProduct.SelectedIndex = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ProductId"].ToString().Equals(selectedRow["ProductId"].ToString())).Select(x => Convert.ToInt32(x["ProductId"].ToString())).FirstOrDefault();
                txtPrice.Text = selectedRow.Row["Price"].ToString();
                priceid = Convert.ToInt32(selectedRow["PriceId"].ToString());
                DisplayTaxDetails();
            }
        }

        private void DisplayTaxDetails()
        {
            btnAdd.Content = "Update";
            btnClose.Content = "Cancel";
            tbPricing.SelectedIndex = 0;
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the Price?", "Pricing", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgPricing.SelectedItem;

                if (DeleteTaxFromTaxMaster(Convert.ToInt32(selectedRow["PriceId"].ToString())))
                    GetPricing(); ;
            }

        }

        private bool DeleteTaxFromTaxMaster(int priceID)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "update Pricing SET DeletedDate = '" + DateTime.Now + "',IsDeleted='1' where PriceId =" + priceID + "";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        private void GetPricing()
        {
            dtPricing = new DataTable();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                //var query = "select * from Pricing where IsDeleted != '1'";
                var query = @"select p1.PriceId,p1.Price,p1.CountryId,p1.ProductId,c.CountryName as Country,pm.ProductFamily as Product from Pricing p1 inner join Country c on c.Id = p1.CountryId
inner join ProductMasterNew pm on pm.ProductId = p1.ProductId where p1.IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    dtPricing.Load(scmd.ExecuteReader());
                }
                if (dtPricing.Rows.Count > 0)
                {
                    dgPricing.ItemsSource = null;
                    dgPricing.ItemsSource = dtPricing.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
            }
        }



        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgPricing.ItemsSource = null;
                GetPricing();
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgPricing.ItemsSource = null;
                GetPricing();
            }
            else
            {
                Search(txtSearch.Text);
            }
        }

        private void Search(string text)
        {
            try
            {
                DataTable dtSearch = new DataTable();
                using (SqlConnection scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    var query = "select * from Pricing where ProductId Like '%" + text + "%' and IsDeleted != '1'";
                    try
                    {
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                    catch
                    {
                    }
                    if (dtSearch.Rows.Count == 0)
                    {
                        dtSearch = new DataTable();
                        query = "select * from Pricing where Price Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgPricing.ItemsSource = null;
                    dgPricing.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgPricing.ItemsSource = null;
                    dgPricing.Items.Clear();
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
            }
            catch (Exception)
            {

            }
        }


        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetPricing();
        }

        private void btnAddTax_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(cmbCountry.SelectedItem.ToString())) && !(string.IsNullOrWhiteSpace(cmboProduct.SelectedItem.ToString())) && !(string.IsNullOrWhiteSpace(txtPrice.Text)))
            {
                using (var scn = new SqlConnection(_connectionString))
                {

                    scn.Open();
                    if (btnAdd.Content.Equals("Add"))
                    {
                        GetCountryId(cmbCountry.SelectedItem.ToString());
                        GetProductId(cmboProduct.SelectedItem.ToString());
                        GetCountryId(cmbCountry.Text);
                        query = "insert into Pricing(CountryId,ProductId,Price,CreatedDate,IsDeleted) values (@CountryId,@ProductId,@Price,@CreatedDate,@IsDeleted)";
                    }
                    else
                    {
                        query = "update Pricing SET CountryId=@CountryId,ProductId=@ProductId,Price=@Price where PriceId=" + priceid + "";
                    }
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = System.Data.CommandType.Text;
                        if (btnAdd.Content.Equals("Add"))
                        {
                            scmd.Parameters.Add(new SqlParameter("@CountryId", countryID));
                            scmd.Parameters.Add(new SqlParameter("@ProductId", ProductID));
                            scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            scmd.Parameters.Add(new SqlParameter("@Price", txtPrice.Text));
                            scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                        }
                        else
                        {
                            scmd.Parameters.Add(new SqlParameter("@CountryId", countryID));
                            scmd.Parameters.Add(new SqlParameter("@ProductId", ProductID));
                            scmd.Parameters.Add(new SqlParameter("@Price", txtPrice.Text));
                        }
                        scmd.ExecuteNonQuery();
                        if (btnAdd.Content.Equals("Add"))
                        {
                            MessageBox.Show("Price inserted successfully.", "Pricing", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Price updated successfully.", "Pricing", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                            btnAdd.Content = "Add";
                            btnClose.Content = "Close";
                            tbPricing.SelectedIndex = 1;
                        }
                    }
                }
                GetPricing();
            }
            else
            {
                MessageBox.Show("Field values should not be empty.", "Pricing", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GetCountryId(string text)
        {
            countryID = dtCntry.Rows.OfType<DataRow>().ToList().Where(x => x["CountryName"].Equals(text)).Select(x => Convert.ToInt32(x["Id"].ToString())).FirstOrDefault();
        }

        private void GetProductId(string text)
        {
            ProductID = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ProductFamily"].Equals(text)).Select(x => Convert.ToInt32(x["ProductId"].ToString())).FirstOrDefault();
        }

        private void GetPriceId(string text)
        {
            priceid = Convert.ToInt32(text);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnClose.Content.Equals("Close"))
                Close();
            else
            {
                ClearFields();
                tbPricing.SelectedIndex = 1;
                btnAdd.Content = "Add";
                btnClose.Content = "Close";
                cmbCountry.SelectedIndex = 0;
            }
        }

        private void ClearFields()
        {
            cmbCountry.SelectedIndex = 0;
            cmboProduct.SelectedIndex = 0;
            txtPrice.Clear();
        }

        private void tbTaxMaster_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClsProperties.isClosed = true;
        }
    }
}
