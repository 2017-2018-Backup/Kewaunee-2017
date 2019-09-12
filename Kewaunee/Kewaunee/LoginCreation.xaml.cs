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
    /// Interaction logic for LoginCreation.xaml
    /// </summary>
    public partial class LoginCreation : Window
    {
        private string _connectionString;
        DataTable dt = new DataTable();
        DataTable dtProfiles = new DataTable();
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public LoginCreation(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetProfiles();
            GetMaxCustomerId();
            GetUserLogins();
            cmboProfiles.SelectedIndex = 0;
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
                tbCreation.IsEnabled = false;
                tbControl.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbCreation.IsEnabled = false;
                tbControl.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbCreation.IsEnabled = true;
                tbControl.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Validations())
            {
                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();
                    var query = string.Empty;
                    var profileId = GetProfileID(cmboProfiles.SelectedItem.ToString());
                    if (TxtCreate.Text.Equals("Create"))
                        query = "insert into LoginCreation(Username,Designation,Department,ProfileId,CreatedDate,IsDeleted,Password) values (@Username,@Designation,@Department,@ProfileId,@CreatedDate,@IsDeleted,@Password)";
                    else
                        query = "update LoginCreation SET Username=@Username,Designation=@Designation,Department=@Department,ProfileId=@ProfileId,Password=@Password where UserID=@UserId";
                    using (var scmd = new SqlCommand(query, sqlConnection))
                    {
                        scmd.CommandType = System.Data.CommandType.Text;
                        if (TxtCreate.Text.Equals("Create"))
                        {
                            scmd.Parameters.Add(new SqlParameter("@Username", txtUserame.Text));
                            scmd.Parameters.Add(new SqlParameter("@Designation", txtDescription.Text));
                            scmd.Parameters.Add(new SqlParameter("@Department", txtDepartment.Text));
                            scmd.Parameters.Add(new SqlParameter("@ProfileId", profileId));
                            scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                            scmd.Parameters.Add(new SqlParameter("@Password", txtPassword.Password));
                            if (TxtCreate.Text.Equals("Create"))
                            {
                                if (CheckUserNameAlreadyExists())
                                { }
                                else
                                { scmd.ExecuteNonQuery(); }
                            }
                            else
                            {
                                scmd.ExecuteNonQuery();
                                tbControl.SelectedIndex = 1;
                            }
                            ClearValues();
                            MessageBox.Show("User login created successfully.", "Login Creation", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        else
                        {
                            scmd.Parameters.Add(new SqlParameter("@Username", txtUserame.Text));
                            scmd.Parameters.Add(new SqlParameter("@Designation", txtDescription.Text));
                            scmd.Parameters.Add(new SqlParameter("@Department", txtDepartment.Text));
                            scmd.Parameters.Add(new SqlParameter("@ProfileId", profileId));
                            scmd.Parameters.Add(new SqlParameter("@Password", txtPassword.Password));
                            scmd.Parameters.Add(new SqlParameter("@UserID", ReArrangeCode(txtUserId.Text)));
                            scmd.ExecuteNonQuery();
                            ClearValues();
                            MessageBox.Show("User login updated successfully.", "Login Creation", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        GetMaxCustomerId();
                        GetUserLogins();
                    }
                }
            }
        }



        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (TxtCreate.Text.Equals("Create"))
            {
                TxtCancel.Text = "Close";
                Close();
            }
            else
            {
                TxtCancel.Text = "Cancel";
                TxtCreate.Text = "Create";
                ClearValues();
                GetMaxCustomerId();
                tbControl.SelectedIndex = 1;
            }
        }

        private void GetUserLogins()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                using (SqlCommand scmd = new SqlCommand("select * from LoginCreation where IsDeleted != '1'", scn))
                {
                    dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());

                    var dtCloned = dt.Clone();
                    dtCloned.Columns[0].DataType = typeof(string);
                    dtCloned.Columns[0].ReadOnly = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        dtCloned.Rows.Add(dr.ItemArray);
                    }

                    foreach (DataRow dr in dtCloned.Rows)
                    {

                        string str = dr["UserID"].ToString();
                        str = GenerateCustomerId(str);
                        dr["UserID"] = str;
                        dtCloned.AcceptChanges();
                    }
                    dgCustomer.ItemsSource = null;
                    dgCustomer.ItemsSource = dtCloned.DefaultView;
                }
            }
        }

        private static string GenerateCustomerId(string str)
        {
            str = str.Length == 1 ? "00" + str : str.Length == 2 ? "0" + str : str;
            return str;
        }



        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtUserame.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtDepartment.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
                return false;
            if (string.IsNullOrWhiteSpace(cmboProfiles.SelectedItem.ToString()) && !cmboProfiles.SelectedItem.ToString().Equals("--Select--"))
                return false;

            return true;
        }

        private bool CheckUserNameAlreadyExists()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select Username from LoginCreation where Username='" + txtUserame.Text + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                        return true;
                    return false;
                }
            }
        }

        private int GetProfileID(string selectedProfile)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select ProfileId from Profile where ProfileDescription='" + selectedProfile + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dt.Rows[0][0].ToString());
                }
            }
        }

        private void GetProfiles()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Profile";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProfiles = new DataTable();
                    dtProfiles.Load(scmd.ExecuteReader());

                    foreach (DataRow dr in dtProfiles.Rows)
                    {
                        cmboProfiles.Items.Add(dr["ProfileDescription"].ToString());
                    }
                    cmboProfiles.Items.Insert(0, "--Select--");
                }
            }
        }

        private void GetMaxCustomerId()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                using (SqlCommand scmd = new SqlCommand("select Max(UserID) as UserId from LoginCreation", scn))
                {
                    var dtMaxCode = new DataTable();
                    dtMaxCode.Load(scmd.ExecuteReader());
                    if (string.IsNullOrWhiteSpace(dtMaxCode.Rows[0][0].ToString()))
                    {
                        txtUserId.Text = "001";
                    }
                    else
                    {
                        var id = dtMaxCode.Rows[0][0].ToString();
                        FrameUserId(id);
                    }
                }
            }
        }

        private void ClearValues()
        {
            txtUserame.Clear();
            txtDepartment.Clear();
            txtDescription.Clear();
            txtPassword.Clear();
            cmboProfiles.SelectedIndex = 0;

        }

        private void FrameUserId(string id)
        {
            var newID = Convert.ToInt32(id);
            newID = newID + 1;
            txtUserId.Text = id.Length == 1 ? "00" + newID : id.Length == 2 ? "0" + newID.ToString() : newID.ToString();
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the login details?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataRowView selectedRow = (DataRowView)dgCustomer.SelectedItem;
                    var code = selectedRow.Row[0].ToString();
                    code = ReArrangeCode(code);
                    DataRow selectedCustomer = (from DataRow dr in dt.AsEnumerable()
                                                where dr["UserID"].ToString().Equals(code)
                                                select dr).FirstOrDefault();
                    DisplayCustomerDetails(selectedCustomer);
                }
            }
        }

        private void DisplayCustomerDetails(DataRow selectedCustomer)
        {
            try
            {
                tbControl.SelectedIndex = 0;
                var str = GenerateCustomerId(selectedCustomer["UserID"].ToString());
                txtUserId.Text = str;
                txtUserame.Text = selectedCustomer["Username"].ToString();
                txtDescription.Text = selectedCustomer["Designation"].ToString();
                txtDepartment.Text = selectedCustomer["Department"].ToString();
                txtPassword.Password = selectedCustomer["Password"].ToString();
                string value = (from dr in dtProfiles.AsEnumerable()
                                where dr["ProfileId"].ToString().Equals(selectedCustomer["ProfileId"].ToString(), StringComparison.InvariantCultureIgnoreCase)
                                select dr["ProfileDescription"].ToString()).FirstOrDefault();
                cmboProfiles.SelectedIndex = cmboProfiles.Items.IndexOf(value);
                TxtCreate.Text = "Update";
                TxtCancel.Text = "Cancel";

            }
            catch (Exception)
            {

            }
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the user login?", "Login Creation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataRowView selectedRow = (DataRowView)dgCustomer.SelectedItem;
                    var code = selectedRow.Row[0].ToString();
                    code = ReArrangeCode(code);
                    string selectedCustomer = (from DataRow dr in dt.AsEnumerable()
                                               where dr["UserID"].ToString().Equals(code)
                                               select dr["UserID"].ToString()).FirstOrDefault();
                    if (DeleteCustomer(selectedCustomer))
                        GetUserLogins();
                }
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
                    var query = "update LoginCreation SET IsDeleted='1',DeletedDate = '" + DateTime.Now + "' where UserID = " + customerID + "";
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


        private static string ReArrangeCode(string code)
        {
            code = code.Contains("00") ? code.Replace("00", "") : code.Contains("0") ? code.Replace("0", "") : code;
            return code;
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgCustomer.ItemsSource = null;
                GetUserLogins();
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                GetUserLogins();
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
                    var query = "select * from LoginCreation where UserId = " + text + " and IsDeleted != '1'";
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
                        query = "select * from LoginCreation where Username Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgCustomer.ItemsSource = null;
                    dgCustomer.ItemsSource = dtSearch.DefaultView;
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetUserLogins();
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
