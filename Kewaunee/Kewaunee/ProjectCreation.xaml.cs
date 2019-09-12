using Microsoft.Win32;
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
    /// Interaction logic for ProjectCreation.xaml
    /// </summary>
    public partial class ProjectCreation : Window
    {
        private string _connectionString;
        private DataTable dtCustomer = new DataTable();
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public ProjectCreation(bool isNewProject, bool read, bool edit, bool delete)//bool isNewProject - To be passed via constructor
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            Title = isNewProject ? "New Project" : "Open Project";
            btnCreateProject.Content = isNewProject ? "Create Project" : "Import Project";
            //Title = "Open Project";
            SetProperties();
            SetCustomerName();
            GetProjectPath();
            cmboCustomerName.SelectedIndex = 0;
            _read = read;
            _edit = edit;
            _delete = delete;
            ClsProperties.isClosed = false;
        }


        private void GetProjectPath()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Generic";
                using (var scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    UIInputs.ProjectPath = dt.Rows[1]["ValueStr"].ToString();
                }
            }
        }

        private void SetCustomerName()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Customer where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtCustomer = new DataTable();
                    dtCustomer.Load(scmd.ExecuteReader());
                    foreach (DataRow dr in dtCustomer.Rows)
                    {
                        cmboCustomerName.Items.Add(dr["Name"].ToString());
                    }
                    cmboCustomerName.Items.Insert(0, "--Select--");
                }
            }
        }

        private bool IsProjectIdExists(string text)
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select ProjectNo from Projects where ProjectNo='" + text + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                        return true;
                    return false;
                }
            }
        }

        private bool IsProjectNameExists(string text)
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select ProjectName from Projects where ProjectName='" + text + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                        return true;
                    return false;
                }
            }
        }

        private void SetProperties()
        {
            if (Title.Equals("New Project"))
            {
                gbBuildingDetails.Visibility = Visibility.Visible;
                gbBrowse.Visibility = Visibility.Collapsed;
                txtLabLength.TabIndex = 9;
                txttruecielingHieght.TabIndex = 10;
                txtlabwidth.TabIndex = 11;
                txtfalsecielingHieght.TabIndex = 12;
                btnCreateProject.TabIndex = 13;
                btnClose.TabIndex = 14;
                Height = 590;
            }
            else
            {
                gbBuildingDetails.Visibility = Visibility.Collapsed;
                gbBrowse.Visibility = Visibility.Visible;
                btnBrowse.TabIndex = 9;
                btnCreateProject.TabIndex = 10;
                btnClose.TabIndex = 11;
                Height = 550;
            }
        }

        private void txtSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void btnCreateProject_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Validations())
                {
                    using (var scn = new SqlConnection(_connectionString))
                    {
                        scn.Open();
                        UIInputs.ProjectNo = txtProjectNo.Text;
                        UIInputs.ProjectName = txtProjectName.Text;
                        var query = Title.Equals("New Project") ? "insert into Projects (ProjectName,ProjectNo,CustomerNameAddress,BuildingName,FloorNo,RoomNo,LabName,DrawingRevNo,LabLength,LabWidth,TrueCeilingHeight,FalseCeilingHeight,isOpenProject,CreatedDate,IsDeleted,ProjectPath) values (@ProjectName,@ProjectNo,@CustomerNameAddress,@BuildingName,@FloorNo,@RoomNo,@LabName,@DrawingRevNo,@LabLength,@LabWidth,@TrueCeilingHeight,@FalseCeilingHeight,@isOpenProject,@CreatedDate,@IsDeleted,@ProjectPath)" : "insert into Projects (ProjectName,ProjectNo,CustomerNameAddress,BuildingName,FloorNo,RoomNo,LabName,DrawingRevNo,ProjectPath,isOpenProject,CreatedDate,IsDeleted) values (@ProjectName,@ProjectNo,@CustomerNameAddress,@BuildingName,@FloorNo,@RoomNo,@LabName,@DrawingRevNo,@ProjectPath,@isOpenProject,@CreatedDate,@IsDeleted)";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            scmd.CommandType = CommandType.Text;
                            scmd.Parameters.Add(new SqlParameter("@ProjectName", txtProjectName.Text));
                            scmd.Parameters.Add(new SqlParameter("@ProjectNo", txtProjectNo.Text));
                            scmd.Parameters.Add(new SqlParameter("@CustomerNameAddress", txtCustNameAddress.Text));
                            scmd.Parameters.Add(new SqlParameter("@BuildingName", txtBuildingName.Text));
                            scmd.Parameters.Add(new SqlParameter("@FloorNo", txtFloorNo.Text));
                            scmd.Parameters.Add(new SqlParameter("@RoomNo", txtRoomNo.Text));
                            scmd.Parameters.Add(new SqlParameter("@LabName", txtLabName.Text));
                            scmd.Parameters.Add(new SqlParameter("@DrawingRevNo", txtDrawingRevNo.Text));
                            if (Title.Equals("New Project"))
                            {
                                scmd.Parameters.Add(new SqlParameter("@LabLength", Convert.ToInt32(txtLabLength.Text)));
                                scmd.Parameters.Add(new SqlParameter("@LabWidth", Convert.ToInt32(txtlabwidth.Text)));
                                scmd.Parameters.Add(new SqlParameter("@TrueCeilingHeight", !string.IsNullOrWhiteSpace(txttruecielingHieght.Text) ? Convert.ToInt32(txttruecielingHieght.Text) : 0));
                                scmd.Parameters.Add(new SqlParameter("@FalseCeilingHeight", !string.IsNullOrWhiteSpace(txtfalsecielingHieght.Text) ? Convert.ToInt32(txtfalsecielingHieght.Text) : 0));
                                scmd.Parameters.Add(new SqlParameter("@ProjectPath", System.IO.Path.Combine(UIInputs.ProjectPath, txtProjectName.Text + "_" + txtProjectNo.Text + ".rvt")));
                            }
                            else
                            {
                                scmd.Parameters.Add(new SqlParameter("@ProjectPath", txtBrowsePath.Text));
                            }
                            scmd.Parameters.Add(new SqlParameter("@isOpenProject", Title.Equals("New Project") ? '0' : '1'));
                            scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                            scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                            scmd.ExecuteNonQuery();
                            if (Title.Equals("New Project"))
                            {
                                UIInputs.WallLength = Convert.ToInt32(txtLabLength.Text);
                                UIInputs.WallWidth = Convert.ToInt32(txtlabwidth.Text);
                                UIInputs.TrueCeilingHeight = !string.IsNullOrWhiteSpace(txttruecielingHieght.Text) ? Convert.ToInt32(txttruecielingHieght.Text) : 0;
                                UIInputs.FalseCielingHeight = !string.IsNullOrWhiteSpace(txtfalsecielingHieght.Text) ? Convert.ToInt32(txtfalsecielingHieght.Text) : 0;
                            }
                            MessageBox.Show("Project created successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                            if (btnCreateProject.Content.ToString().Equals("Import Project",StringComparison.InvariantCultureIgnoreCase))
                            {
                                UIInputs.ProjectPath = txtBrowsePath.Text;
                            }
                            ClearValues();
                            Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("All fields are mandatory.", "Efficax", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearValues()
        {
            txtProjectName.Clear();
            txtProjectNo.Clear();
            txtCustNameAddress.Clear();
            txtBuildingName.Clear();
            txtRoomNo.Clear();
            txtFloorNo.Clear();
            txtLabName.Clear();
            txtDrawingRevNo.Clear();
            txtLabLength.Clear();
            txtlabwidth.Clear();
            txttruecielingHieght.Clear();
            txtfalsecielingHieght.Clear();
            txtBrowsePath.Clear();
            cmboCustomerName.SelectedIndex = 0;
        }

        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtProjectNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(cmboCustomerName.SelectedItem.ToString()) || cmboCustomerName.SelectedItem.Equals("--Select--"))
                return false;
            if (string.IsNullOrWhiteSpace(txtBuildingName.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtRoomNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtFloorNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtLabName.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtDrawingRevNo.Text))
                return false;
            if (Title.Equals("New Project"))
            {
                if (string.IsNullOrWhiteSpace(txtlabwidth.Text))
                    return false;
                if (string.IsNullOrWhiteSpace(txttruecielingHieght.Text))
                    return false;
                //if (string.IsNullOrWhiteSpace(txtfalsecielingHieght.Text))
                //    return false;
                if (string.IsNullOrWhiteSpace(txtLabLength.Text))
                    return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtBrowsePath.Text))
                    return false;
            }
            if (IsProjectIdExists(txtProjectNo.Text))
            {
                MessageBox.Show("Project number exists already. Please enter another number.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (IsProjectNameExists(txtProjectName.Text))
            {
                MessageBox.Show("Project Name exists already. Please enter another name.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void btnBrowse_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opnFileDialog = new OpenFileDialog();
            opnFileDialog.Multiselect = false;
            opnFileDialog.Filter = "RVT (*.rvt,*.rvt)|*.rvt;*.rvt";
            opnFileDialog.ShowDialog();
            if (opnFileDialog.FileName != null && !string.IsNullOrWhiteSpace(opnFileDialog.FileName))
            {
                txtBrowsePath.Text = opnFileDialog.FileName;
            }
        }

        private void cmboCustomerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmboCustomerName.SelectedItem.Equals("--Select--")) return;
            SetCustomerNameAddress(cmboCustomerName.SelectedItem.ToString());
        }

        private void SetCustomerNameAddress(string selectedItem)
        {
            var address = (from dr in dtCustomer.AsEnumerable()
                           where dr["Name"].ToString().Equals(selectedItem)
                           select dr["Address"].ToString()).FirstOrDefault();
            if (address != null)
            {
                string details = selectedItem + Environment.NewLine + address;
                txtCustNameAddress.Text = details;
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnBrowseProjectPath_Click_1(object sender, RoutedEventArgs e)
        {
            var formDialog = new System.Windows.Forms.FolderBrowserDialog();
            formDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(formDialog.SelectedPath))
            {
                // txtprojectPath.Text = formDialog.SelectedPath;
            }
        }

        private void txtProjectNo_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (IsProjectIdExists(txtProjectNo.Text))
            //{
            //    MessageBox.Show("Project number exists already. Please enter another number.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            ClsProperties.isClosed = true;
        }

        private void txtLabLength_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            //var senderText = (sender as TextBox);
            //if (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)))
            //{
            //    e.Handled = true;
            //}
        }

        private void txtfalsecielingHieght_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsNumber(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }
    }
}
