using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Customer : Window
    {
        private string _connectionString;
        private DataTable dt = new DataTable();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public Customer(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetDataFromCustomer();
            GetMaxCustomerCode();
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            UserAccessValidations(); ClsProperties.isClosed = false;
        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tabAddUser.IsEnabled = false;
                tbControl.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tabAddUser.IsEnabled = false;
                tbControl.SelectedIndex = 1;
            }
            if (_edit)
            {
                tabAddUser.IsEnabled = true;
                tbControl.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBoxResult.No;
                chkUpdateLogo.IsChecked = false;
                var query = string.Empty;
                if (Validations())
                {
                    byte[] imageArray;
                    if (string.IsNullOrWhiteSpace(filePath)) imageArray = new byte[0];
                    else
                        imageArray = GetVarBinary(filePath);
                    using (var sqlConnection = new SqlConnection(_connectionString))
                    {
                        sqlConnection.Open();
                        if (TxtAdd.Text.Equals("Add"))
                        {
                            messageBoxResult = MessageBoxResult.OK;
                            query = "insert into Customer (Name,Address,City,State,Country,PhoneNo,MobileNo,FaxNo,PrimaryContact,Logo,Designation,TinNo,CSTNo,VATNo,ServiceTaxRegistrationNo,CreatedDate,IsDeleted) values (@Name,@Address,@City,@State,@Country,@PhoneNo,@MobileNo,@FaxNo,@PrimaryContact,@Logo,@Designation,@TinNo,@CSTNo,@VATNo,@ServiceTaxRegistrationNo,@CreatedDate,@IsDeleted)";

                            //ImageConverter imgCon = new ImageConverter();
                            //return (byte[])imgCon.ConvertTo(inImg, typeof(byte[]));
                        }
                        else
                        {

                            if (string.IsNullOrWhiteSpace(txtLogoPath.Text))
                            {
                                messageBoxResult = MessageBox.Show("Do you want to retain the old logo?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            }
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                query = "update Customer SET Name=@Name,Address=@Address,City=@City,State=@State,Country=@Country,PhoneNo=@PhoneNo,MobileNo=@MobileNo,FaxNo=@FaxNo,PrimaryContact=@PrimaryContact,Designation=@Designation,TinNo=@TinNo,CSTNo=@CSTNo,VATNo=@VATNo,ServiceTaxRegistrationNo=@ServiceTaxRegistrationNo,CreatedDate=@CreatedDate,IsDeleted=@IsDeleted where Code =" + ReArrangeCode(txtCode.Text) + "";
                            }
                            else
                            {
                                query = "update Customer SET Name=@Name,Address=@Address,City=@City,State=@State,Country=@Country,PhoneNo=@PhoneNo,MobileNo=@MobileNo,FaxNo=@FaxNo,PrimaryContact=@PrimaryContact,Logo=@Logo,Designation=@Designation,TinNo=@TinNo,CSTNo=@CSTNo,VATNo=@VATNo,ServiceTaxRegistrationNo=@ServiceTaxRegistrationNo,CreatedDate=@CreatedDate,IsDeleted=@IsDeleted where Code =" + ReArrangeCode(txtCode.Text) + "";
                            }

                        }
                        using (var sqlCommand = new SqlCommand(query, sqlConnection))
                        {
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.Parameters.Add(new SqlParameter("@Name", txtName.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@Address", txtAddress.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@City", txtCity.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@State", txtState.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@Country", txtCountry.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@PhoneNo", txtPhoneNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@MobileNo", txtMobileNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@FaxNo", txtFaxNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@PrimaryContact", txtPrimaryContact.Text));
                            if (messageBoxResult == MessageBoxResult.OK)
                            {
                                sqlCommand.Parameters.Add(new SqlParameter("@Logo", imageArray));
                            }
                            sqlCommand.Parameters.Add(new SqlParameter("@Designation", txtDesignation.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@TinNo", txtPinNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@CSTNo", txtPSDNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@VATNo", txtWapNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@ServiceTaxRegistrationNo", txtServiceTaxNo.Text));
                            sqlCommand.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            sqlCommand.Parameters.Add(new SqlParameter("@IsDeleted", "0"));
                            sqlCommand.ExecuteNonQuery();
                            ClearValues();
                            if (TxtAdd.Text.Equals("Update"))
                            {
                                MessageBox.Show("Customer was successfully updated.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                                TxtAdd.Text = "Add";
                                TxtCancel.Text = "Close";
                                tbControl.SelectedIndex = 1;
                                chkUpdateLogo.IsChecked = false;
                                chkUpdateLogo.Visibility = Visibility.Collapsed;
                                txtLogoPath.Visibility = Visibility.Visible;
                                lblLogo.Visibility = Visibility.Visible;
                                btnBrowse.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("Customer was successfully added.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        GetMaxCustomerCode();
                        GetDataFromCustomer();
                    }
                }
                else
                {
                    MessageBox.Show("All fields are mandatory.", "Efficax", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                //ignored
            }
        }

        private byte[] GetVarBinary(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            byte[] image = br.ReadBytes((int)fs.Length);
            br.Close();
            fs.Close();
            return image;
        }

        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCity.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtState.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCountry.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhoneNo.Text))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtMobileNo.Text))
            {
                return false;
            }
            return true;
        }
        string filePath = string.Empty;
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "JPG (*.jpg,*.jpeg)|*.jpg;*.jpeg|PNG (*.png,*.png)|*.png;";
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != null)
                {
                    filePath = openFileDialog.FileName;
                    txtLogoPath.Text = filePath;
                }
            }
            catch (Exception)
            {

            }
        }

        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (((TabControl)sender).SelectedIndex == 0)
            {

            }
            else
            {
                //GetDataFromCustomer();
            }
        }

        private void GetDataFromCustomer()
        {

            try
            {
                dt.Clear();
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    //var query = "select Code,Name,Address,City,State,Country from Customer";
                    var query = "select * from Customer where IsDeleted != 1";
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        dt.Load(scmd.ExecuteReader());
                    }
                    if (dt == null) return;
                    DataTable dtCloned = dt.Clone();
                    dtCloned.Columns[0].DataType = typeof(string);
                    dtCloned.Columns[0].ReadOnly = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        dtCloned.Rows.Add(dr.ItemArray);
                    }

                    foreach (DataRow dr in dtCloned.Rows)
                    {

                        string str = dr["Code"].ToString();
                        str = GenerateCustomerId(str);
                        dr["Code"] = str;
                        dtCloned.AcceptChanges();
                    }
                    dgCustomer.ItemsSource = null;
                    dgCustomer.ItemsSource = dtCloned.DefaultView;
                }

            }
            catch (Exception)
            {

            }
        }

        private static string GenerateCustomerId(string str)
        {
            str = str.Length == 1 ? "CUST000" + str : str.Length == 2 ? "CUST00" + str : str.Length == 3 ? "CUST0" + str : str;
            return str;
        }

        private void GetMaxCustomerCode()
        {
            try
            {
                var dtTable = new DataTable();
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    var query = "select Max(Code) as Code from Customer";
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        dtTable.Load(scmd.ExecuteReader());
                    }
                    string str = string.Empty;
                    int value = 0;
                    int newValue = 0;
                    if (dtTable.Rows.Count == 0) str = "CUST0001";
                    else
                    {
                        value = !string.IsNullOrWhiteSpace(dtTable.Rows[0][0].ToString())? Convert.ToInt32(dtTable.Rows[0][0]) : 0 ;
                        newValue = value + 1;
                        string custCode = newValue.ToString().Length == 3 ? "CUST" + newValue : newValue.ToString().Length == 2 ? "CUST0" + newValue : newValue.ToString().Length == 1 ? "CUST00" + newValue : newValue.ToString();
                        //str = value.ToString().Length == 1 ? custCode : value.ToString().Length == 2 ? custCode : value.ToString().Length == 3 ? custCode : custCode;
                        str = custCode;
                    }
                    txtCode.Text = str;
                }
            }
            catch (Exception)
            {

            }
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to delete the Customer?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DataRowView selectedRow = (DataRowView)dgCustomer.SelectedItem;
                var code = selectedRow.Row[0].ToString();
                code = ReArrangeCode(code);
                string selectedCustomer = (from DataRow dr in dt.AsEnumerable()
                                           where dr["Code"].ToString().Equals(code)
                                           select dr["Code"].ToString()).FirstOrDefault();
                if (DeleteCustomer(selectedCustomer))
                    GetDataFromCustomer();
            }

        }

        private static string ReArrangeCode(string code)
        {
            code = code.Contains("CUST000") ? code.Replace("CUST000", "") : code.Contains("CUST00") ? code.Replace("CUST00", "") : code.Replace("CUST0", "");
            return code;
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to edit the Customer details?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DataRowView selectedRow = (DataRowView)dgCustomer.SelectedItem;
                var code = selectedRow.Row[0].ToString();
                code = ReArrangeCode(code);
                DataRow selectedCustomer = (from DataRow dr in dt.AsEnumerable()
                                            where dr["Code"].ToString().Equals(code)
                                            select dr).FirstOrDefault();
                DisplayCustomerDetails(selectedCustomer);
            }

        }

        private void DisplayCustomerDetails(DataRow selectedCustomer)
        {
            try
            {
                tbControl.SelectedIndex = 0;
                var str = GenerateCustomerId(selectedCustomer["Code"].ToString());
                txtCode.Text = str;
                txtName.Text = selectedCustomer["Name"].ToString();
                txtAddress.Text = selectedCustomer["Address"].ToString();
                txtAddress.ToString().Trim();
                txtCity.Text = selectedCustomer["City"].ToString();
                txtState.Text = selectedCustomer["State"].ToString();
                txtCountry.Text = selectedCustomer["Country"].ToString();
                txtPhoneNo.Text = selectedCustomer["PhoneNo"].ToString();
                txtMobileNo.Text = selectedCustomer["MobileNo"].ToString();
                txtFaxNo.Text = selectedCustomer["FaxNo"].ToString();
                txtPrimaryContact.Text = selectedCustomer["PrimaryContact"].ToString();
                txtDesignation.Text = selectedCustomer["Designation"].ToString();
                txtPinNo.Text = selectedCustomer["TinNo"].ToString();
                txtPSDNo.Text = selectedCustomer["CSTNo"].ToString();
                txtWapNo.Text = selectedCustomer["VATNo"].ToString();
                txtServiceTaxNo.Text = selectedCustomer["ServiceTaxRegistrationNo"].ToString();
                TxtAdd.Text = "Update";
                TxtCancel.Text = "Cancel";

                txtLogoPath.Visibility = Visibility.Collapsed;
                lblLogo.Visibility = Visibility.Collapsed;
                btnBrowse.Visibility = Visibility.Collapsed;
                chkUpdateLogo.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

            }
        }

        private bool DeleteCustomer(string customerId)
        {
            try
            {
                int customerID = Convert.ToInt32(customerId);
                using (SqlConnection scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    //var query = "delete from Customer where Code =" + customerID + "";
                    var query = "update Customer SET IsDeleted='1',DeletedDate = '" + DateTime.Now + "' where Code = " + customerID + "";
                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private void ClearValues()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtCity.Clear();
            txtState.Clear();
            txtCountry.Clear();
            txtPhoneNo.Clear();
            txtMobileNo.Clear();
            txtFaxNo.Clear();
            txtPrimaryContact.Clear();
            txtLogoPath.Clear();
            txtDesignation.Clear();
            txtPinNo.Clear();
            txtPSDNo.Clear();
            txtWapNo.Clear();
            txtServiceTaxNo.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (TxtAdd.Text.Equals("Add"))
            {
                TxtCancel.Text = "Close";
                Close();
            }
            else
            {
                TxtCancel.Text = "Cancel";
                ClearValues();
                GetMaxCustomerCode();
                tbControl.SelectedIndex = 1;
                TxtAdd.Text = "Add";
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                GetDataFromCustomer();
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
                    var query = "select * from Customer where Code = " + text + " and IsDeleted != '1'";
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
                        query = "select * from Customer where Name Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    if (dt == null) return;
                    DataTable dtCloned = dtSearch.Clone();
                    dtCloned.Columns[0].DataType = typeof(string);
                    dtCloned.Columns[0].ReadOnly = false;
                    foreach (DataRow dr in dtSearch.Rows)
                    {
                        dtCloned.Rows.Add(dr.ItemArray);
                    }

                    foreach (DataRow dr in dtCloned.Rows)
                    {

                        string str = dr["Code"].ToString();
                        str = GenerateCustomerId(str);
                        dr["Code"] = str;
                        dtCloned.AcceptChanges();
                    }
                    dgCustomer.ItemsSource = null;
                    dgCustomer.ItemsSource = dtCloned.DefaultView;

                    //dgCustomer.ItemsSource = null;
                    //dgCustomer.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgCustomer.ItemsSource = null;
                    dgCustomer.Items.Clear();
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                    //GetDataFromCustomer();
                }
            }
            catch (Exception)
            {

            }
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgCustomer.ItemsSource = null;
                GetDataFromCustomer();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromCustomer();
        }

        private void chkUpdateLogo_Checked_1(object sender, RoutedEventArgs e)
        {
            if (chkUpdateLogo.IsChecked == true)
            {
                txtLogoPath.Visibility = Visibility.Visible;
                lblLogo.Visibility = Visibility.Visible;
                btnBrowse.Visibility = Visibility.Visible;
                chkUpdateLogo.Visibility = Visibility.Collapsed;
            }
            else
            {
                //do nothing
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
    }
}
