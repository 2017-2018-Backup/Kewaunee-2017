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
    /// Interaction logic for Currency.xaml
    /// </summary>
    public partial class Currency : Window
    {
        private string _connectionString;
        private string query;
        private string _taxDescription;
        private string _taxValue;
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        private DataTable  dtCntry = new DataTable();
        public Currency(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
             _connectionString = Properties.Settings.Default.ConnectionString;
            GetCurrency();
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            UserAccessValidations();
            GetCountry();
            ClsProperties.isClosed = false;
        }

        
        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbAddCurrency.IsEnabled = false;
                tbCurreny.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbAddCurrency.IsEnabled = false;
                tbCurreny.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbAddCurrency.IsEnabled = true;
                tbCurreny.SelectedIndex = 0;
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

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the Currency?", "Currency", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgCurrency.SelectedItem;
                _taxDescription = selectedRow.Row["Country"].ToString();
                _taxValue = selectedRow.Row["Currency"].ToString();
                DisplayTaxDetails(_taxDescription, _taxValue);
            }
        }

        private void DisplayTaxDetails(string taxDescription, string taxValue)
        {
            cmbCountry.SelectedIndex = cmbCountry.Items.IndexOf(taxDescription);
            txtCurrency.Text = taxValue;
            btnAdd.Content = "Update";
            btnClose.Content = "Cancel";
            tbCurreny.SelectedIndex = 0;
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the currency?", "Currency", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgCurrency.SelectedItem;
               
                if (DeleteTaxFromTaxMaster(selectedRow.Row["Country"].ToString(), selectedRow.Row["Currency"].ToString()))
                    GetCurrency(); ;
            }

        }

        private bool DeleteTaxFromTaxMaster(string taxDescription, string taxValue)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "update Currency SET DeletedDate = '" + DateTime.Now + "',IsDeleted='1' where Country='" + taxDescription + "' and Currency=" + taxValue + "";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        private void GetCurrency()
        {
            DataTable dt = new DataTable();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "select * from Currency where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    dt.Load(scmd.ExecuteReader());
                }
                if (dt.Rows.Count > 0)
                {
                    dgCurrency.ItemsSource = null;
                    dgCurrency.ItemsSource = dt.DefaultView;
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
                dgCurrency.ItemsSource = null;
                GetCurrency();
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgCurrency.ItemsSource = null;
                GetCurrency();
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
                    var query = "select * from Currency where Country Like '%" + text + "%' and IsDeleted != '1'";
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
                        query = "select * from Currency where Currency Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgCurrency.ItemsSource = null;
                    dgCurrency.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgCurrency.ItemsSource = null;
                    dgCurrency.Items.Clear();
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
            GetCurrency();
        }

        private void btnAddTax_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(cmbCountry.Text)) && !(string.IsNullOrWhiteSpace(txtCurrency.Text)))
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    if (btnAdd.Content.Equals("Add"))
                    {
                        query = "insert into Currency(Country,Currency,CreatedDate,IsDeleted) values (@Country,@Currency,@CreatedDate,@IsDeleted)";
                    }
                    else
                    {
                        query = "update Currency SET Country=@Country,Currency=@Currency where Country='" + cmbCountry.Text + "' and Currency='" + txtCurrency.Text + "'";
                    }
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = System.Data.CommandType.Text;
                        if (btnAdd.Content.Equals("Add"))
                        {
                            scmd.Parameters.Add(new SqlParameter("@Country", cmbCountry.Text));
                            scmd.Parameters.Add(new SqlParameter("@Currency", txtCurrency.Text));
                            scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                        }
                        else
                        {
                            scmd.Parameters.Add(new SqlParameter("@Country", cmbCountry.Text));
                            scmd.Parameters.Add(new SqlParameter("@Currency", txtCurrency.Text));
                        }
                        scmd.ExecuteNonQuery();
                        if (btnAdd.Content.Equals("Add"))
                        {
                            MessageBox.Show("Currency inserted successfully.", "Currency", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Currency updated successfully.", "Currency", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                            btnAdd.Content = "Add";
                            btnClose.Content = "Close";
                            tbCurreny.SelectedIndex = 1;
                        }
                    }
                }
                GetCurrency();
            }
            else
            {
                MessageBox.Show("Field values should not be empty.", "Currency", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnClose.Content.Equals("Close"))
                Close();
            else
            {
                ClearFields();
                tbCurreny.SelectedIndex = 1;
                btnAdd.Content = "Add";
                btnClose.Content = "Close";
                cmbCountry.SelectedIndex = 0;
            }
        }

        private void ClearFields()
        {
            cmbCountry.SelectedIndex = 0;
            txtCurrency.Clear();
        }

        private void tbTaxMaster_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCountry.SelectedIndex == -1 || cmbCountry.SelectedIndex == 0)
            {
                return;
            }
            txtCurrency.Text = (from dr in dtCntry.AsEnumerable()
                               where dr["CountryName"].Equals(cmbCountry.SelectedItem.ToString())
                               select dr["Currency"].ToString()).FirstOrDefault();
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
