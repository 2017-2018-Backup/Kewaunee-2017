using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for TaxMaster.xaml
    /// </summary>
    public partial class TaxMaster : Window
    {
        private string _connectionString;
        private string query;
        private string _taxDescription;
        private string _taxValue;
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        private DataTable dtTax = new DataTable();
        public TaxMaster(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetTaxFromTaxMaster();
            txtTaxDescription.SelectedIndex = 0;
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            UserAccessValidations();
            ClsProperties.isClosed = false;
        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbAddTax.IsEnabled = false;
                tbTaxMaster.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbAddTax.IsEnabled = false;
                tbTaxMaster.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbAddTax.IsEnabled = true;
                tbTaxMaster.SelectedIndex = 0;
            }
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the tax?", "Tax Master", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgTax.SelectedItem;
                bool isDepreciated = (bool)selectedRow.Row["IsDepreciated"];
                if (isDepreciated)
                {
                    MessageBox.Show("Editing cannot be done for depreciated items.", "Tax Master", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _taxDescription = selectedRow.Row["TaxDescription"].ToString();
                _taxValue = selectedRow.Row["TaxValue"].ToString();
                DisplayTaxDetails(_taxDescription, _taxValue);
            }
        }

        private void DisplayTaxDetails(string taxDescription, string taxValue)
        {
            txtTaxDescription.Text = taxDescription;
            txtTaxValue.Text = taxValue;
            btnAddTax.Content = "Update";
            btnCancel.Content = "Cancel";
            tbTaxMaster.SelectedIndex = 0;
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the tax?", "Tax Master", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgTax.SelectedItem;
                bool isDepreciated = (bool)selectedRow.Row["IsDepreciated"];
                if (isDepreciated)
                {
                    MessageBox.Show("Delete cannot be done for depreciated items.", "Tax Master", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (DeleteTaxFromTaxMaster(selectedRow.Row["TaxDescription"].ToString(), selectedRow.Row["TaxValue"].ToString()))
                    GetTaxFromTaxMaster(true); 
            }

        }

        private bool DeleteTaxFromTaxMaster(string taxDescription, string taxValue)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "update TaxMaster SET DeletedDate = '" + DateTime.Now + "',IsDeleted='1' where TaxDescription='" + taxDescription + "' and TaxValue=" + taxValue + "";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        private void GetTaxFromTaxMaster(bool isFromDelete = false)
        {
            dtTax = new DataTable();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "select * from TaxMaster where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    dtTax.Load(scmd.ExecuteReader());
                }
                if (dtTax.Rows.Count > 0)
                {
                    dgTax.ItemsSource = null;
                    dgTax.ItemsSource = dtTax.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                if (isFromDelete && dtTax.Rows.Count == 0)
                {
                    dgTax.ItemsSource = null;
                    dgTax.Items.Clear();
                }
            }
        }

        private void hyperInActive_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to depreciate the tax?", "Tax Master", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            DataRowView selectedRow = (DataRowView)dgTax.SelectedItem;
            DepreciateTax((selectedRow.Row["TaxDescription"].ToString()), selectedRow.Row["TaxValue"].ToString());
            foreach (DataGridCellInfo cell in dgTax.SelectedCells)
            {
                var cellContent = cell.Column.GetCellContent(cell.Item);
                if (cellContent != null)
                {
                    DataGridCell dgCell = (DataGridCell)cellContent.Parent;
                    dgCell.IsEnabled = false;
                }
            }

        }

        private void DepreciateTax(string taxDescription, string taxValue)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "update TaxMaster SET DepreciatedDate = '" + DateTime.Now + "',IsDepreciated='1' where TaxDescription='" + taxDescription + "' and TaxValue=" + taxValue + "";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    scmd.ExecuteNonQuery();
                }
            }
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgTax.ItemsSource = null;
                GetTaxFromTaxMaster();
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgTax.ItemsSource = null;
                GetTaxFromTaxMaster();
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
                    var query = "select * from TaxMaster where TaxDescription Like '%" + text + "%' and IsDeleted != '1'";
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
                        query = "select * from TaxMaster where TaxValue Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgTax.ItemsSource = null;
                    dgTax.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgTax.ItemsSource = null;
                    dgTax.Items.Clear();
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
            GetTaxFromTaxMaster();
        }

        private void btnAddTax_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(txtTaxDescription.Text)) && !(string.IsNullOrWhiteSpace(txtTaxValue.Text)))
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    if (btnAddTax.Content.Equals("Add"))
                    {
                        query = "insert into TaxMaster(TaxDescription,TaxValue,CreatedDate,IsDeleted,IsDepreciated) values (@TaxDescription,@TaxValue,@CreatedDate,@IsDeleted,@IsDepreciated)";
                    }
                    else
                    {
                        query = "update TaxMaster SET TaxDescription=@TaxDescription,TaxValue=@TaxValue where TaxDescription='" + _taxDescription + "' and TaxValue=" + _taxValue + "";
                    }
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = System.Data.CommandType.Text;
                        if (btnAddTax.Content.Equals("Add"))
                        {
                            scmd.Parameters.Add(new SqlParameter("@TaxDescription", txtTaxDescription.Text));
                            scmd.Parameters.Add(new SqlParameter("@TaxValue", txtTaxValue.Text));
                            scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                            scmd.Parameters.Add(new SqlParameter("@IsDepreciated", '0'));
                        }
                        else
                        {
                            scmd.Parameters.Add(new SqlParameter("@TaxDescription", txtTaxDescription.Text));
                            scmd.Parameters.Add(new SqlParameter("@TaxValue", txtTaxValue.Text));
                        }
                        scmd.ExecuteNonQuery();
                        if (btnAddTax.Content.Equals("Add"))
                        {
                            MessageBox.Show("Tax inserted successfully.", "Tax Master", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Tax updated successfully.", "Tax Master", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                            btnAddTax.Content = "Add";
                            btnCancel.Content = "Close";
                            tbTaxMaster.SelectedIndex = 1;
                        }
                    }
                }
                GetTaxFromTaxMaster();
            }
            else
            {
                MessageBox.Show("Field values should not be empty.", "Tax Master", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnCancel.Content.Equals("Close"))
                Close();
            else
            {
                ClearFields();
                tbTaxMaster.SelectedIndex = 1;
            }
        }

        private void ClearFields()
        {
            txtTaxDescription.SelectedIndex = 0;
            txtTaxValue.Clear();
        }

        private void tbTaxMaster_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
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

        private void txtTaxDescription_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (txtTaxDescription.SelectedIndex == 0)
            {
                txtTaxValue.Clear();
                return;
            }
            var value = dtTax.Rows.OfType<DataRow>().ToList().Where(x => x["TaxDescription"].ToString().Equals((txtTaxDescription.Items[txtTaxDescription.SelectedIndex] as ComboBoxItem).Content)).Select(x => x["TaxValue"].ToString()).FirstOrDefault();
            txtTaxValue.Text = value;
        }


    }
}
