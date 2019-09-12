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
    /// Interaction logic for VariantMaster.xaml
    /// </summary>
    public partial class VariantMaster : Window
    {
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        private string _connectionString = string.Empty;
        private DataTable dtVariants = new DataTable();
        private int variantId;
        public VariantMaster(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetVariants();
            LoadVariantCategories();
            UserAccessValidations();
            ClsProperties.isClosed = false;
        }


        private void LoadVariantCategories()
        {
            var lstCategory = new List<string>();
            if (dtVariants.Rows.Count > 0)
            {
                dtVariants.Rows.OfType<DataRow>().ToList().ForEach(x => 
                {
                    if (!string.IsNullOrWhiteSpace(x["Category"].ToString()))
                    {
                        if (!lstCategory.Contains(x["Category"].ToString()))
                        {
                            lstCategory.Add(x["Category"].ToString());
                            txtVariantGroup.Items.Add(x["Category"].ToString());
                        }
                    }
                });
            }
        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbAddVariants.IsEnabled = false;
                tbVariantMaster.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbAddVariants.IsEnabled = false;
                tbVariantMaster.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbAddVariants.IsEnabled = true;
                tbVariantMaster.SelectedIndex = 0;
            }
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgVariants.ItemsSource = null;
                GetVariants();
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgVariants.ItemsSource = null;
                GetVariants();
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
                    var query = "select * from VariantDetails where VariantCode Like '%" + text + "%' and IsDeleted != '1'";
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
                        query = "select * from VariantDetails where VariantDescription Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgVariants.ItemsSource = null;
                    dgVariants.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgVariants.ItemsSource = null;
                    dgVariants.Items.Clear();
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
            txtSearch.TextChanged -= txtSearch_TextChanged_1;
            GetVariants();
            txtSearch.Clear();
            txtSearch.TextChanged += txtSearch_TextChanged_1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnCancel.Content == "Close")
                Close();
            else
            {
                ClearValues();
                btnAdd.Content = "Add";
                btnCancel.Content = "Close";
                tbVariantMaster.SelectedIndex = 1;
            }
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClsProperties.isClosed = true;
        }


        private void GetVariants()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                dtVariants = new DataTable();
                var query = "select * from VariantDetails where IsDeleted != 1";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    dtVariants.Load(scmd.ExecuteReader());
                }
            }
            if (dtVariants.Rows.Count > 0)
            {
                txtSearch.TextChanged -= txtSearch_TextChanged_1;
                dgVariants.ItemsSource = null;
                dgVariants.ItemsSource = dtVariants.DefaultView;
                txtSearch.Clear();
                txtSearch.TextChanged += txtSearch_TextChanged_1;
            }
        }

        private void DoUpdateProcess()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "update VariantDetails SET VariantCode=@VariantCode,VariantDescription=@VariantDescription,Category=@Category,VariantDisplayName=@VariantDisplayName,Price=@Price where Id=" + variantId + "";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.CommandType = CommandType.Text;
                    scmd.Parameters.Add(new SqlParameter("@VariantCode", txtVariantCode.Text));
                    scmd.Parameters.Add(new SqlParameter("@VariantDescription", txtVariantname.Text));
                    scmd.Parameters.Add(new SqlParameter("@Category", txtVariantGroup.Text));
                    scmd.Parameters.Add(new SqlParameter("@VariantDisplayName", txtVariantDisplayName.Text));
                    scmd.Parameters.Add(new SqlParameter("@Price", txtPrice.Text));
                    scmd.ExecuteNonQuery();
                    MessageBox.Show("Variant details updated successfully.", "Variant Master", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearValues();
                    btnAdd.Content = "Add";
                    btnCancel.Content = "Close";
                    tbVariantMaster.SelectedIndex = 1;
                    txtVariantCode.IsEnabled = true;
                }

            }
            GetVariants();
        }

        private void btnAdd_Click_1(object sender, RoutedEventArgs e)
        {
            if (btnAdd.Content.ToString().Equals("Update"))
            {
                DoUpdateProcess();
                return;
            }
            if (!Validations())
            {
                MessageBox.Show("Please fill all mandatory details to add variants.", "Variant Master", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var IsVariantCodeExists = !string.IsNullOrWhiteSpace((from drow in dtVariants.AsEnumerable()
                                                                  where drow["VariantCode"].ToString().Equals(txtVariantCode.Text)
                                                                  select drow["VariantCode"].ToString()).FirstOrDefault()) ? true : false;
            if (IsVariantCodeExists)
            {
                MessageBox.Show("Variant code already exists.", "Variant Master", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "insert into VariantDetails(VariantCode,VariantDescription,Category,VariantDisplayName,Price,CreatedDate,IsDeleted) values (@VariantCode,@VariantDescription,@Category,@VariantDisplayName,@Price,@CreatedDate,@IsDeleted)";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.CommandType = CommandType.Text;
                    scmd.Parameters.Add(new SqlParameter("@VariantCode", txtVariantCode.Text));
                    scmd.Parameters.Add(new SqlParameter("@VariantDescription", txtVariantname.Text));
                    scmd.Parameters.Add(new SqlParameter("@Category", txtVariantGroup.Text));
                    scmd.Parameters.Add(new SqlParameter("@VariantDisplayName", txtVariantDisplayName.Text));
                    scmd.Parameters.Add(new SqlParameter("@Price", txtPrice.Text));
                    scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                    scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                    scmd.ExecuteNonQuery();

                    MessageBox.Show("Variants has been added successfully.", "Variant Master", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearValues();
                }
            }
            GetVariants();

        }

        private void ClearValues()
        {
            txtVariantname.Clear();
            txtVariantGroup.SelectedIndex = -1;
            txtVariantDisplayName.Clear();
            txtPrice.Clear();
            txtVariantCode.Clear();
        }

        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtVariantname.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtVariantname.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtVariantGroup.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtVariantDisplayName.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                return false;
            }
            return true;
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the selected variant?", "Variant Master", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgVariants.SelectedItem;

                txtVariantCode.Text = selectedRow.Row["VariantCode"].ToString();
                txtVariantname.Text = selectedRow.Row["VariantDescription"].ToString();
                txtVariantGroup.Text = selectedRow.Row["Category"].ToString();
                txtVariantDisplayName.Text = selectedRow.Row["VariantDisplayName"].ToString();
                txtPrice.Text = selectedRow.Row["Price"].ToString();
                variantId = Convert.ToInt32(selectedRow["Id"].ToString());
                DisplayVariantDetails();
            }
        }

        private void DisplayVariantDetails()
        {
            txtVariantCode.IsEnabled = false;
            btnAdd.Content = "Update";
            btnCancel.Content = "Cancel";
            tbVariantMaster.SelectedIndex = 0;
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the selected variant?", "Variant Master", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
                DataRowView selectedRow = (DataRowView)dgVariants.SelectedItem;

                if (DeleteVariantFromMaster(Convert.ToInt32(selectedRow["Id"].ToString())))
                    GetVariants();
            }
        }

        private bool DeleteVariantFromMaster(int p)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                var query = "update VariantDetails SET DeletedDate = '" + DateTime.Now + "',IsDeleted='1' where Id =" + p + "";
                using (var scmd = new SqlCommand(query, sqlConnection))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
    }
}
