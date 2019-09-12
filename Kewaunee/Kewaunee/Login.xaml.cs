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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private string _connectionString;
        public Login()
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            ClsProperties.isClosed = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClsProperties.isLoginSuccess = false;
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Please fill all mandatory feilds.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var profileId = 0;
                var query = "select * from LoginCreation where Username='" + txtUsername.Text + "' and Password='" + txtPassword.Password + "' and IsDeleted != '1'";
                UIInputs.Username = txtUsername.Text;
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        ClsProperties.isLoginSuccess = true;
                        profileId = Convert.ToInt32(dt.Rows[0]["ProfileId"].ToString());
                        GetSavedProfilesDetails(profileId);
                        ClsProperties.isClosed = true;
                        this.Hide();
                    }
                    else { MessageBox.Show("Invalid login or User account may be deleted. Please contact administrator.", Title, MessageBoxButton.OK, MessageBoxImage.Information); }
                }
            }
        }



        public void GetSavedProfilesDetails(int ProfileId)
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from SavedProfiles where ProfileId='" + ProfileId + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        ProfileDescription.Profile = ProfileId;
                        ProfileDescription.FeaturesName = new List<Features>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Features features = new Features();
                            int featureId = Convert.ToInt32(dr["FeatureId"].ToString());
                            features.FeaturesName = GetFeatures(featureId);
                            features.isRead = (bool)dr["Read"];
                            features.isEdit = (bool)dr["Edit"];
                            features.isDelete = (bool)dr["Delete"];
                            ProfileDescription.FeaturesName.Add(features);
                        }
                    }
                }
            }
        }

        private string GetFeatures(int featureId)
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Features where FeatureId='" + featureId + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0]["Feature"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Button_Click_1(null, null);
        }


    }
}
