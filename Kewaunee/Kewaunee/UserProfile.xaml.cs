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
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        List<SavedProfile> _lstSavedProfiles = new List<SavedProfile>();
        List<SavedProfile> _lstSerachProfiles = new List<SavedProfile>();
        Dictionary<string, List<SavedProfile>> dictSavedProfiles = new Dictionary<string, List<SavedProfile>>();
        DataTable dtFeatures = new DataTable();
        DataTable dtProfiles = new DataTable();
        private string _connectionString;
        private bool isTabChangedFromSearch = false;
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public UserProfile(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            //dgUserProfiles.DataContext = _lstSavedProfiles;
            _connectionString = Properties.Settings.Default.ConnectionString;
            SetFeatures();
            cmboFeatures.SelectedIndex = 0;
            GetUserProfiles();
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
                tbTabUser.IsEnabled = false;
                tbUserProfile.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbTabUser.IsEnabled = false;
                tbUserProfile.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbTabUser.IsEnabled = true;
                tbUserProfile.SelectedIndex = 0;
            }
        }

        private void btnSave_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProfileName.Text) || string.IsNullOrWhiteSpace(cmboFeatures.SelectedItem.ToString()) || cmboFeatures.SelectedItem.Equals("--Select--") || !CheckAccessLevel())
                    MessageBox.Show("Please fill all mandatory details.", "User Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    if (btnSave.Content.Equals("Add"))
                    {
                        AddToDictionary();

                    }
                    else
                    {
                        if (dictSavedProfiles.ContainsKey(txtProfileName.Text))
                        {
                            _lstSavedProfiles.Where(sProfile => sProfile.ProfileName.Equals(staticProfile.ProfileName) && sProfile.Feature.Equals(staticProfile.Feature) && sProfile.Delete == staticProfile.Delete && sProfile.Read == staticProfile.Read && sProfile.Edit == staticProfile.Edit).Select(x => { x.ProfileName = txtProfileName.Text; x.AccessLevel = GenerateAccessLevel(); x.Feature = cmboFeatures.SelectedItem.ToString(); x.Edit = (bool)chkEdit.IsChecked; x.Delete = (bool)chkDelete.IsChecked; x.Read = (bool)chkRead.IsChecked; return x; }).ToList();

                            dictSavedProfiles[txtProfileName.Text] = _lstSavedProfiles;
                            ClearValues();
                            dgUserProfiles.ItemsSource = null;
                            dgUserProfiles.ItemsSource = _lstSavedProfiles;
                            btnSave.Content = "Add";

                        }
                    }
                }
                staticProfile = null;
            }
            catch (Exception)
            {

            }
        }

        private void AddToDictionary()
        {
            if (dictSavedProfiles.ContainsKey(txtProfileName.Text))
            {
                dictSavedProfiles[txtProfileName.Text].Add(new SavedProfile { ProfileName = txtProfileName.Text, Feature = cmboFeatures.SelectedItem.ToString(), Read = (bool)chkRead.IsChecked, Edit = (bool)chkEdit.IsChecked, Delete = (bool)chkDelete.IsChecked, AccessLevel = GenerateAccessLevel() });
                ClearValues();
                dgUserProfiles.ItemsSource = null;
                dgUserProfiles.ItemsSource = _lstSavedProfiles;
            }
            else
            {
                SavedProfile svdProfile = new SavedProfile();
                svdProfile.ProfileName = txtProfileName.Text;
                svdProfile.Feature = cmboFeatures.SelectedItem.ToString();
                svdProfile.Read = (bool)chkRead.IsChecked;
                svdProfile.Edit = (bool)chkEdit.IsChecked;
                svdProfile.Delete = (bool)chkDelete.IsChecked;
                svdProfile.AccessLevel = GenerateAccessLevel();
                _lstSavedProfiles.Add(svdProfile);
                dictSavedProfiles.Add(txtProfileName.Text, _lstSavedProfiles);
                ClearValues();
                dgUserProfiles.ItemsSource = null;
                dgUserProfiles.ItemsSource = _lstSavedProfiles;
            }
        }

        private int GetProfile(string ProfileDescription)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select ProfileId from Profile where ProfileDescription ='" + ProfileDescription + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dtProfileID = new DataTable();
                    dtProfileID.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dtProfileID.Rows[0][0].ToString());
                }
            }
        }

        private int GetProfileId(SavedProfile svdProfile)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select ProfileId from Profile where ProfileDescription ='" + svdProfile.ProfileName + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dtProfileID = new DataTable();
                    dtProfileID.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dtProfileID.Rows[0][0].ToString());
                }
            }
        }

        private void UpdateSavedProfile(SavedProfile svdProfile, string accessLevel, int featureId, int profileId)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "update Profile SET ProfileDescription='" + txtProfileName.Text + "',FeatureId=" + featureId + ",AccessLevel='" + accessLevel + "' where ProfileId=" + profileId + "";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                    MessageBox.Show("Profile details updated successfully.", "User Profile", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ClearValues()
        {
            // txtProfileName.Clear();
            cmboFeatures.SelectedIndex = 0;
            chkDelete.IsChecked = false;
            chkEdit.IsChecked = false;
            chkRead.IsChecked = false;
            dgUserProfiles.ItemsSource = null;
            dgUserProfiles.Items.Clear();
        }

        private SavedProfile staticProfile;

        private void SetFeatures()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                using (SqlCommand scmd = new SqlCommand("select * from Features", scn))
                {
                    dtFeatures = new DataTable();
                    dtFeatures.Load(scmd.ExecuteReader());
                    foreach (DataRow dr in dtFeatures.Rows)
                    {
                        cmboFeatures.Items.Add(dr["Feature"].ToString());
                    }
                    cmboFeatures.Items.Insert(0, "--Select--");
                }
            }
        }

        private string GenerateAccessLevel()
        {
            string accessLevel = string.Empty;
            accessLevel = chkRead.IsChecked == true && chkEdit.IsChecked == true && chkDelete.IsChecked == true ? "Read,Edit,Delete" : chkRead.IsChecked == true && chkEdit.IsChecked == true ? "Read,Edit" : chkDelete.IsChecked == true && chkEdit.IsChecked == true ? "Edit,Delete" : chkRead.IsChecked == true && chkDelete.IsChecked == true ? "Read,Delete" : chkRead.IsChecked == true ? "Read" : chkEdit.IsChecked == true ? "Edit" : "Delete";
            return accessLevel;
        }

        private bool CheckAccessLevel()
        {
            if (chkRead.IsChecked == false && chkEdit.IsChecked == false && chkDelete.IsChecked == false)
                return false;
            return true;
        }

        private void btnAdd_Click_1(object sender, RoutedEventArgs e)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                if (btnAdd.Content.Equals("Save"))
                {
                    AddIntoDatabase(scn);
                    dgUserProfiles.ItemsSource = null;
                    dgUserProfiles.Items.Clear();
                    dictSavedProfiles.Clear();
                    _lstSavedProfiles.Clear();
                }
                else
                {
                    UpdateIntoDatabase(scn);
                    txtProfileName.IsEnabled = true;
                    btnAdd.Content = "Save";
                    dgUserProfiles.ItemsSource = null;
                    dgUserProfiles.Items.Clear();
                    dictSavedProfiles.Clear();
                    _lstSavedProfiles.Clear();
                }

                using (SqlCommand scmd = new SqlCommand("select * from Profile where IsDeleted != '1'", scn))
                {
                    dtProfiles = new DataTable();
                    dtProfiles.Load(scmd.ExecuteReader());
                    dgSearch.ItemsSource = null;
                    dgSearch.ItemsSource = dtProfiles.DefaultView;
                }
                staticProfile = null;
                txtProfileName.Clear();
                txtProfileName.IsEnabled = true;
            }
        }

        private bool CheckProfileExists(SqlConnection scn)
        {
            var query = "select * from Profile where ProfileDescription = '" + txtProfileName.Text + "'";
            using (SqlCommand scmd = new SqlCommand(query, scn))
            {
                var dt = new DataTable();
                dt.Load(scmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                    return true;
                return false;
            }
        }

        private void UpdateIntoDatabase(SqlConnection scn)
        {
            foreach (var keys in dictSavedProfiles.Keys)
            {
                var query = string.Empty;
                if (!CheckProfileExists(scn))
                {
                    query = "insert into Profile(ProfileDescription,CreatedDate,IsDeleted) values ('" + keys.ToString() + "','" + DateTime.Now + "','0')";
                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.ExecuteNonQuery();
                    }
                }
                query = "delete from SavedProfiles where ProfileId=" + GetProfile(keys) + "";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                }
                foreach (SavedProfile svdProfile in dictSavedProfiles[keys])
                {

                    int profileId = GetProfileId(svdProfile);
                    int featureId = GetFeatureID(svdProfile.Feature);

                    //if (!CheckSavedProfilesExists(scn, svdProfile))
                    //{
                    query = "insert into SavedProfiles(ProfileId,FeatureId,[Read],Edit,[Delete]) values (@ProfileId,@FeatureId,@Read,@Edit,@Delete)";
                    //}
                    //else
                    //{
                    //    query = "update SavedProfiles SET ProfileId=@ProfileId,FeatureId=@FeatureId,[Read]=@Read,Edit=@Edit,[Delete]=@Delete where ProfileId=" + profileId + " and FeatureId = " + featureId + "";
                    //}

                    //if (!isExists)
                    //{
                    string read = svdProfile.Read ? "1" : "0";
                    string edit = svdProfile.Edit ? "1" : "0";
                    string delete = svdProfile.Delete ? "1" : "0";

                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = CommandType.Text;
                        scmd.Parameters.Add(new SqlParameter("@ProfileId", profileId));
                        scmd.Parameters.Add(new SqlParameter("@FeatureId", featureId));
                        if (read.Equals("0"))
                            scmd.Parameters.Add(new SqlParameter("@Read", '0'));
                        else
                            scmd.Parameters.Add(new SqlParameter("@Read", '1'));
                        if (edit.Equals("0"))
                            scmd.Parameters.Add(new SqlParameter("@Edit", '0'));
                        else
                            scmd.Parameters.Add(new SqlParameter("@Edit", '1'));
                        if (delete.Equals("0"))
                            scmd.Parameters.Add(new SqlParameter("@Delete", '0'));
                        else
                            scmd.Parameters.Add(new SqlParameter("@Delete", '1'));

                        scmd.ExecuteNonQuery();
                    }
                    //}
                    txtProfileName.Clear();

                }
                MessageBox.Show("Profiles updated successfully.", "User Profile", MessageBoxButton.OK, MessageBoxImage.Information);
                Login log = new Login();
                log.GetSavedProfilesDetails(ProfileDescription.Profile);
            }
        }

        private bool CheckSavedProfilesExists(SqlConnection scn, SavedProfile svdProfile)
        {
            var query = "select * from SavedProfiles where ProfileId = '" + GetProfile(svdProfile.ProfileName) + "' and FeatureId =" + GetFeatureID(svdProfile.Feature) + "";
            using (SqlCommand scmd = new SqlCommand(query, scn))
            {
                var dt = new DataTable();
                dt.Load(scmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                    return true;
                return false;
            }
        }

        private void AddIntoDatabase(SqlConnection scn)
        {
            foreach (var keys in dictSavedProfiles.Keys)
            {
                if (!CheckProfileExists(scn))
                {
                    var query = "insert into Profile(ProfileDescription,CreatedDate,IsDeleted) values ('" + keys.ToString() + "','" + DateTime.Now + "','0')";
                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.ExecuteNonQuery();
                    }
                }
                foreach (SavedProfile svdProfile in dictSavedProfiles[keys])
                {
                    if (!CheckSavedProfilesExists(scn, svdProfile))
                    {
                        int profileId = GetProfileId(svdProfile);
                        int featureId = GetFeatureID(svdProfile.Feature);
                        //if (!isExists)
                        //{
                        string read = svdProfile.Read ? "1" : "0";
                        string edit = svdProfile.Edit ? "1" : "0";
                        string delete = svdProfile.Delete ? "1" : "0";
                        var query = "insert into SavedProfiles(ProfileId,FeatureId,[Read],Edit,[Delete]) values (@ProfileId,@FeatureId,@Read,@Edit,@Delete)";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            scmd.CommandType = CommandType.Text;
                            scmd.Parameters.Add(new SqlParameter("@ProfileId", profileId));
                            scmd.Parameters.Add(new SqlParameter("@FeatureId", featureId));
                            if (read.Equals("0"))
                                scmd.Parameters.Add(new SqlParameter("@Read", '0'));
                            else
                                scmd.Parameters.Add(new SqlParameter("@Read", '1'));
                            if (edit.Equals("0"))
                                scmd.Parameters.Add(new SqlParameter("@Edit", '0'));
                            else
                                scmd.Parameters.Add(new SqlParameter("@Edit", '1'));
                            if (delete.Equals("0"))
                                scmd.Parameters.Add(new SqlParameter("@Delete", '0'));
                            else
                                scmd.Parameters.Add(new SqlParameter("@Delete", '1'));

                            scmd.ExecuteNonQuery();
                        }
                        //}
                        txtProfileName.Clear();
                    }
                }
                Login log = new Login();
                log.GetSavedProfilesDetails(ProfileDescription.Profile);
            }
            MessageBox.Show("Added profiles saved successfully into database.", "User Profile", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private int GetFeatureID(string selectedFeature)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select FeatureId from Features where Feature='" + selectedFeature + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dtFetre = new DataTable();
                    dtFetre.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dtFetre.Rows[0][0].ToString());
                }
            }
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to edit the profile?", "User Profile", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            SavedProfile selectedRow = (SavedProfile)dgUserProfiles.SelectedItem;
            DisplayFromCreateProfile(selectedRow);

        }

        private void DisplayFromCreateProfile(SavedProfile svdProfile, bool isFromSearch = false)
        {
            if (svdProfile != null)
            {
                staticProfile = svdProfile;
                txtProfileName.Text = svdProfile.ProfileName;
                cmboFeatures.SelectedIndex = cmboFeatures.Items.IndexOf(svdProfile.Feature);
                chkDelete.IsChecked = svdProfile.Delete;
                chkEdit.IsChecked = svdProfile.Edit;
                chkRead.IsChecked = svdProfile.Read;
                btnSave.Content = "Update";
                txtProfileName.IsEnabled = false;
                if (isFromSearch)
                {
                    isTabChangedFromSearch = true;
                    tbUserProfile.SelectedIndex = 0;
                }
            }
        }

        private void GetUserProfiles()
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Profile where IsDeleted != '1'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    dtProfiles = new DataTable();
                    dtProfiles.Load(scmd.ExecuteReader());
                    dgSearch.ItemsSource = null;
                    dgSearch.ItemsSource = dtProfiles.DefaultView;
                }
            }
        }

        private void CheckAccessLevel(string accessLevel)
        {
            switch (accessLevel)
            {
                //         string accessLevel = string.Empty;
                //accessLevel = chkRead.IsChecked == true && chkEdit.IsChecked == true && chkDelete.IsChecked == true ? "Read,Edit,Delete" : chkRead.IsChecked == true && chkEdit.IsChecked == true ? "Read,Edit" : chkDelete.IsChecked == true && chkEdit.IsChecked == true ? "Edit,Delete" : chkRead.IsChecked == true && chkDelete.IsChecked == true ? "Read,Delete" : chkRead.IsChecked == true ? "Read" : chkEdit.IsChecked == true ? "Edit" : "Delete";
                //return accessLevel;

                case "Read,Edit,Delete":
                    chkDelete.IsChecked = true;
                    chkRead.IsChecked = true;
                    chkEdit.IsChecked = true;
                    break;
                case "Read,Edit":
                    chkEdit.IsChecked = true;
                    chkRead.IsChecked = true;
                    break;
                case "Edit,Delete":
                    chkEdit.IsChecked = true;
                    chkDelete.IsChecked = true;
                    break;
                case "Read,Delete":
                    chkRead.IsChecked = true;
                    chkDelete.IsChecked = true;
                    break;
                case "Read":
                    chkRead.IsChecked = true;
                    break;
                case "Edit":
                    chkEdit.IsChecked = true;
                    break;
                case "Delete":
                    chkDelete.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to delete the user profile?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SavedProfile selectedRow = (SavedProfile)dgUserProfiles.SelectedItem;
                int profileId = GetProfileId(selectedRow);
                DeleteUserProfile(selectedRow, profileId);

            }
        }

        private void DeleteUserProfile(SavedProfile selectedRow, int profileId)
        {
            //using (SqlConnection scn = new SqlConnection(_connectionString))
            // {
            // scn.Open();
            //var query = "update Profile SET DeletedDate ='" + DateTime.Now + "',IsDeleted='1' where ProfileID=" + profileId + "";
            // using (SqlCommand scmd = new SqlCommand(query, scn))
            //{
            //scmd.ExecuteNonQuery();
            _lstSavedProfiles.Remove(selectedRow);
            //_lstSerachProfiles.Remove(selectedRow);
            //dgSearch.ItemsSource = null;
            //dgSearch.ItemsSource = _lstSerachProfiles;
            dgUserProfiles.ItemsSource = null;
            dgUserProfiles.ItemsSource = _lstSavedProfiles;

            //}
            // }
        }

        private void btnCancel_Click_1(object sender, RoutedEventArgs e)
        {
            btnSave.Content = "Add";
            ClearValues();
            dictSavedProfiles.Clear();
            _lstSavedProfiles.Clear();
            if (isTabChangedFromSearch)
            {
                txtProfileName.IsEnabled = true;
                txtProfileName.Clear();
                tbUserProfile.SelectedIndex = 1;
                btnAdd.Content = "Save";
                isTabChangedFromSearch = false;
            }
            else
            {
                Close();
            }
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgSearch.ItemsSource = null;
                dgSearch.ItemsSource = _lstSerachProfiles;
            }
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                GetUserProfiles();
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
                    var query = "select * from Profile where ProfileDescription Like '%" + text + "%' and IsDeleted != '1'";
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

                }
                if (dtSearch.Rows.Count > 0)
                {
                    dgSearch.ItemsSource = null;
                    var lst = new List<SavedProfile>();
                    //foreach (DataRow dr in dtSearch.Rows)
                    //{
                    //    SavedProfile svdProfile = new SavedProfile();
                    //    svdProfile.ProfileName = dr["ProfileDescription"].ToString();
                    //    svdProfile.Feature = (from dataRow in dtFeatures.AsEnumerable()
                    //                          where dataRow["FeatureId"].ToString().Equals(dr["FeatureId"].ToString())
                    //                          select dataRow["Feature"].ToString()).FirstOrDefault();
                    //    svdProfile.AccessLevel = dr["AccessLevel"].ToString();
                    //    svdProfile.createdTime = Convert.ToDateTime(dr["CreatedDate"].ToString());
                    //    lst.Add(svdProfile);
                    //}
                    //dgSearch.ItemsSource = lst;
                    dgSearch.ItemsSource = null;
                    dgSearch.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgSearch.ItemsSource = null;
                    dgSearch.Items.Clear();
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
            GetUserProfiles();
        }

        private void hyperEditSearch_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to edit the profile?", "User Profile", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            DataRowView selectedRow = (DataRowView)dgSearch.SelectedItem;
            //DisplayFromCreateProfile(selectedRow, true);
            DisplayData(selectedRow);
        }

        private void DisplayData(DataRowView selectedRow)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                dictSavedProfiles = new Dictionary<string, List<SavedProfile>>();
                _lstSavedProfiles.Clear();
                string profileDesc = (from dataRow in dtProfiles.AsEnumerable()
                                      where dataRow["ProfileId"].ToString().Equals(selectedRow["ProfileId"].ToString())
                                      select dataRow["ProfileDescription"].ToString()).FirstOrDefault();
                var query = "select * from SavedProfiles where ProfileId =" + selectedRow["ProfileId"] + "";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            SavedProfile profile = new SavedProfile();
                            profile.ProfileName = profileDesc;
                            profile.Feature = (from dataRow in dtFeatures.AsEnumerable()
                                               where dataRow["FeatureId"].ToString().Equals(dr["FeatureId"].ToString())
                                               select dataRow["Feature"].ToString()).FirstOrDefault();
                            profile.Read = (bool)(dr["Read"]) == true ? true : false;
                            profile.Edit = (bool)(dr["Edit"]) == true ? true : false;
                            profile.Delete = (bool)(dr["Delete"]) == true ? true : false;

                            if (dictSavedProfiles.ContainsKey(profileDesc))
                            {
                                _lstSavedProfiles.Add(profile);
                                dictSavedProfiles[txtProfileName.Text] = _lstSavedProfiles;
                            }
                            else
                            {
                                _lstSavedProfiles.Add(profile);
                                dictSavedProfiles.Add(profileDesc, _lstSavedProfiles);
                            }
                            dgUserProfiles.ItemsSource = null;
                            dgUserProfiles.ItemsSource = _lstSavedProfiles;
                            txtProfileName.Text = profileDesc;
                            txtProfileName.IsEnabled = false;
                        }
                        tbUserProfile.SelectedIndex = 0;
                        isTabChangedFromSearch = true;
                        btnAdd.Content = "Update";
                    }
                }
            }
        }



        private void hyperDeleteSearch_Click_1(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to delete the user profile?", "Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DataRowView selectedRow = (DataRowView)dgSearch.SelectedItem;
                if (DeleteProfile(Convert.ToInt32(selectedRow["ProfileId"])))
                    GetUserProfiles();
            }
        }

        private bool DeleteProfile(int profileId)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "update Profile SET DeletedDate ='" + DateTime.Now + "',IsDeleted='1' where ProfileID=" + profileId + "";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
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

    public class SavedProfile
    {
        public string ProfileName { get; set; }
        public string Feature { get; set; }
        public string AccessLevel { get; set; }
        public bool Read { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        public DateTime createdTime { get; set; }
        public bool isNew { get; set; }
    }
}
