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
    /// Interaction logic for ProjectsCatalogue.xaml
    /// </summary>
    public partial class ProjectsCatalogue : Window
    {
        private string _connectionString;
        private DataTable dtProjects = new DataTable();
        private DataTable dtCustomer = new DataTable();
        private bool _read = false;
        private bool _edit = false;
        private bool _delete = false;
        public ProjectsCatalogue(bool isRead, bool isEdit, bool isDelete)
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetProjects();
            GetCustomerNames();
            _read = isRead;
            _edit = isEdit;
            _delete = isDelete;
            UserAccessValidations(); ClsProperties.isClosed = false;
        }

        private void UserAccessValidations()
        {
            if (!_read)
            {
                tbProjects.IsEnabled = false;
                tbProjectsCatalogue.SelectedIndex = 1;
            }
            if (!_delete)
            {
                tbProjects.IsEnabled = false;
                tbProjectsCatalogue.SelectedIndex = 1;
            }
            if (_edit)
            {
                tbProjects.IsEnabled = true;
                tbProjectsCatalogue.SelectedIndex = 0;
            }
        }

        private void GetProjects()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Projects where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProjects = new DataTable();
                    lstProjects.Items.Clear();
                    lstRevisions.Items.Clear();
                    ClearValues();
                    dtProjects.Load(scmd.ExecuteReader());
                    bool isExists = false;
                    foreach (DataRow dr in dtProjects.Rows)
                    {
                        var itm = (from object item in lstProjects.Items
                                   where item.Equals(dr["ProjectName"].ToString())
                                   select item).FirstOrDefault();
                        isExists = itm == null ? false : true;
                        if (!isExists)
                            lstProjects.Items.Add(dr["ProjectName"].ToString());
                    }
                    //lstProjects.SelectedIndex = lstProjects.Items.Count > 0 ? 0 : -1;
                }
                dgProjectsCatalogue.ItemsSource = null;
                dgProjectsCatalogue.ItemsSource = dtProjects.DefaultView;

            }
        }

        private void lstProjects_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (lstProjects.SelectedItem != null)
            {
                //string projectNo = GetProjectName(lstProjects.SelectedItem.ToString());
                //DataRow dr = (from drow in dtProjects.AsEnumerable()
                //              where drow["ProjectNo"].ToString().Equals(projectNo) && drow["ProjectId"].ToString().Equals(GetIdentityId(projectNo, lstProjects.SelectedItem.ToString()))
                //              select drow).FirstOrDefault();
                //if (dr != null)
                //{
                //    gbProjectDetails.IsEnabled = true;
                //    lblNameAddres.IsEnabled = true;
                //    txtlabname.Text = dr["LabName"].ToString();
                //    txtProjectNo.Text = dr["ProjectNo"].ToString();
                //    txtProjectName.Text = dr["ProjectName"].ToString();
                //    txtBuildingName.Text = dr["BuildingName"].ToString();
                //    txtFloorNo.Text = dr["FloorNo"].ToString();
                //    txtRoomNo.Text = dr["RoomNo"].ToString();
                //    txtdrawngRevNo.Text = dr["DrawingRevNo"].ToString();
                //    string[] custNameAddress = dr["CustomerNameAddress"].ToString().Split('\r', '\n');
                //    cmbCustNames.SelectedIndex = cmbCustNames.Items.IndexOf(custNameAddress[0].ToString());
                //    txtCustAddress.Text = dr["CustomerNameAddress"].ToString();

                //}

                string name = lstProjects.SelectedItem.ToString();
                LoadRevisions(name);
            }
        }

        private int lstRevisionSelectedItem = -1;

        private void LoadRevisions(string name)
        {
            lstRevisions.Items.Clear();
            List<DataRow> drlist = (from drow in dtProjects.AsEnumerable()
                                    where drow["ProjectName"].ToString().Equals(name)
                                    select drow).ToList();
            foreach (DataRow dr in drlist)
            {
                lstRevisions.Items.Add(dr["DrawingRevNo"].ToString());
            }
            if (lstRevisionSelectedItem != -1)
                lstRevisions.SelectedIndex = lstRevisionSelectedItem;
            else
                lstRevisions.SelectedIndex = 0;
        }

        private string GetProjectName(string name, string revisionName)
        {
            //return dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].ToString().Equals(name)).Select(x => x["ProjectNo"].ToString()).FirstOrDefault();
            return dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectName"].ToString().Equals(lstProjects.SelectedItem.ToString()) && x["DrawingRevNo"].ToString().Equals(revisionName)).Select(x => x["ProjectNo"].ToString()).FirstOrDefault();
        }

        private string GetIdentityId(string no, string name, string revisonName)
        {
            return dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(no) && x["ProjectName"].ToString().Equals(name) && x["DrawingRevNo"].ToString().Equals(revisonName)).Select(x => x["ProjectId"].ToString()).FirstOrDefault();
        }

        private void GetCustomerNames()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from Customer where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtCustomer.Load(scmd.ExecuteReader());
                }
                if (dtCustomer.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtCustomer.Rows)

                        cmbCustNames.Items.Add(dr["Name"].ToString());
                }
            }
        }

        private void hyperEdit_Click_1(object sender, RoutedEventArgs e)
        {
            if (_edit)
            {
                var result = MessageBox.Show("Do you want to edit the project details?", "Projects Catalogue", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    isFromEdit = true;
                    btnClose.Content = "Cancel";
                    DataRowView selectedRow = (DataRowView)dgProjectsCatalogue.SelectedItem;
                    tbProjectsCatalogue.SelectedIndex = 0;
                    lstRevisionSelectedItem = lstRevisions.Items.IndexOf(selectedRow["DrawingRevNo"].ToString());
                    lstProjects.SelectedIndex = lstProjects.Items.IndexOf(selectedRow.Row["ProjectName"].ToString());
                    lstProjects_SelectionChanged_1(null, null);
                }
            }
        }

        private void hyperDelete_Click_1(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                var result = MessageBox.Show("Do you want to delete the project?", "Projects Catalogue", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataRowView selectedRow = (DataRowView)dgProjectsCatalogue.SelectedItem;
                    if (DeleteProject(selectedRow["DrawingRevNo"].ToString(), selectedRow["ProjectNo"].ToString()))
                    {
                        MessageBox.Show("Project revision deleted successfully.", "Projects Catalogue", MessageBoxButton.OK, MessageBoxImage.Information);
                        GetProjects();
                    }
                }
            }
        }

        private bool DeleteProject(string id, string no)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                //var query = "delete from Customer where Code =" + customerID + "";
                var query = "update Projects SET IsDeleted='1',DeletedDate = '" + DateTime.Now + "' where DrawingRevNo = '" + id + "' and ProjectNo='" + no + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetProjects();
        }

        private void btnSearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                GetProjects();
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
                    var query = "select * from Projects where ProjectNo = '" + text + "' and IsDeleted != '1'";
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
                        query = "select * from Projects where ProjectName Like '%" + text + "%' and IsDeleted != '1'";
                        using (SqlCommand scmd = new SqlCommand(query, scn))
                        {
                            dtSearch.Load(scmd.ExecuteReader());
                        }
                    }
                }
                if (dtSearch.Rows.Count > 0)
                {

                    dgProjectsCatalogue.ItemsSource = null;
                    dgProjectsCatalogue.ItemsSource = dtSearch.DefaultView;

                    //dgCustomer.ItemsSource = null;
                    //dgCustomer.ItemsSource = dtSearch.DefaultView;
                    txtSearch.TextChanged -= txtSearch_TextChanged_1;
                    txtSearch.Clear();
                    txtSearch.TextChanged += txtSearch_TextChanged_1;
                }
                else
                {
                    MessageBox.Show("No results found.", "Customer", MessageBoxButton.OK, MessageBoxImage.Information);
                    dgProjectsCatalogue.ItemsSource = null;
                    dgProjectsCatalogue.Items.Clear();
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
                dgProjectsCatalogue.ItemsSource = null;
                GetProjects();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "Update Projects SET ProjectName=@ProjectName,CustomerNameAddress=@CustomerNameAddress,BuildingName=@BuildingName,FloorNo=@FloorNo,RoomNo=@RoomNo,LabName=@LabName,DrawingRevNo=@DrawingRevNo where ProjectId=@ProjectId";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    UIInputs.ProjectNo = txtProjectNo.Text;
                    scmd.CommandType = CommandType.Text;
                    scmd.Parameters.Add(new SqlParameter("@ProjectName", txtProjectName.Text));
                    scmd.Parameters.Add(new SqlParameter("@CustomerNameAddress", txtCustAddress.Text));
                    scmd.Parameters.Add(new SqlParameter("@BuildingName", txtBuildingName.Text));
                    scmd.Parameters.Add(new SqlParameter("@FloorNo", txtFloorNo.Text));
                    scmd.Parameters.Add(new SqlParameter("@RoomNo", txtRoomNo.Text));
                    scmd.Parameters.Add(new SqlParameter("@LabName", txtlabname.Text));
                    scmd.Parameters.Add(new SqlParameter("@DrawingRevNo", txtdrawngRevNo.Text));
                    scmd.Parameters.Add(new SqlParameter("@ProjectId", GetIdentityId(txtProjectNo.Text, txtProjectName.Text, lstRevisions.SelectedItem.ToString())));
                    scmd.ExecuteNonQuery();
                    MessageBox.Show("Project updated successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    UIInputs.ProjectPath = dtProjects.Rows.OfType<DataRow>().ToList().Where(x => x["ProjectNo"].ToString().Equals(txtProjectNo.Text) && x["DrawingRevNo"].ToString().Equals(lstRevisions.SelectedItem.ToString())).Select(x => x["ProjectPath"].ToString()).FirstOrDefault();
                    GetProjects();
                    KewauneeTaskAssigner.TaskAssigner.OpenDocument(ClsProperties.UIApplication, UIInputs.ProjectPath);
                    ClsProperties.isClosed = true;
                    this.Hide();
                }
            }
        }



        private void ClearValues()
        {
            txtProjectName.Clear();
            txtProjectNo.Clear();
            txtCustAddress.Clear();
            txtBuildingName.Clear();
            txtRoomNo.Clear();
            txtFloorNo.Clear();
            txtlabname.Clear();
            txtdrawngRevNo.Clear();
            cmbCustNames.SelectedIndex = -1;
            txtCustAddress.Clear();
        }

        private void cmbCustNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addr = (from drCust in dtCustomer.AsEnumerable()
                        where drCust["Name"].Equals(cmbCustNames.SelectedItem)
                        select drCust["Address"].ToString()).FirstOrDefault();
            txtCustAddress.Text = string.Empty;
            txtCustAddress.Text = cmbCustNames.SelectedItem + System.Environment.NewLine + addr;
        }

        private void tbProjectsCatalogue_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        bool isFromEdit = false;
        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            if (isFromEdit)
            {
                btnClose.Content = "Close";
                tbProjectsCatalogue.SelectedIndex = 1;
            }
            else
            {
                Close();
            }
        }

        private void btnRevision_Click_1(object sender, RoutedEventArgs e)
        {
            if (Validations())
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    UIInputs.ProjectNo = txtProjectNo.Text;
                    UIInputs.ProjectName = txtProjectName.Text;

                    DataRow dr = (from drow in dtProjects.AsEnumerable()
                                  where drow["ProjectNo"].ToString().Equals(txtProjectNo.Text)
                                  select drow).FirstOrDefault();

                    UIInputs.ProjectPath = dr["ProjectPath"].ToString();
                    UIInputs.ProjectDrawingRevNo = txtdrawngRevNo.Text;
                    UIInputs.ProjectNameBeforeRevNo = dr["ProjectName"].ToString();
                    UIInputs.ProjectNoBeforeRevNo = dr["ProjectNo"].ToString();

                    var query = Title.Equals("New Project") ? "insert into Projects (ProjectName,ProjectNo,CustomerNameAddress,BuildingName,FloorNo,RoomNo,LabName,DrawingRevNo,LabLength,LabWidth,TrueCeilingHeight,FalseCeilingHeight,isOpenProject,CreatedDate,IsDeleted,ProjectPath) values (@ProjectName,@ProjectNo,@CustomerNameAddress,@BuildingName,@FloorNo,@RoomNo,@LabName,@DrawingRevNo,@LabLength,@LabWidth,@TrueCeilingHeight,@FalseCeilingHeight,@isOpenProject,@CreatedDate,@IsDeleted,@ProjectPath)" : "insert into Projects (ProjectName,ProjectNo,CustomerNameAddress,BuildingName,FloorNo,RoomNo,LabName,DrawingRevNo,ProjectPath,isOpenProject,CreatedDate,IsDeleted) values (@ProjectName,@ProjectNo,@CustomerNameAddress,@BuildingName,@FloorNo,@RoomNo,@LabName,@DrawingRevNo,@ProjectPath,@isOpenProject,@CreatedDate,@IsDeleted)";
                    using (SqlCommand scmd = new SqlCommand(query, scn))
                    {
                        scmd.CommandType = CommandType.Text;
                        scmd.Parameters.Add(new SqlParameter("@ProjectName", txtProjectName.Text));
                        scmd.Parameters.Add(new SqlParameter("@ProjectNo", txtProjectNo.Text));
                        scmd.Parameters.Add(new SqlParameter("@CustomerNameAddress", dr["CustomerNameAddress"].ToString()));
                        scmd.Parameters.Add(new SqlParameter("@BuildingName", txtBuildingName.Text));
                        scmd.Parameters.Add(new SqlParameter("@FloorNo", txtFloorNo.Text));
                        scmd.Parameters.Add(new SqlParameter("@RoomNo", txtRoomNo.Text));
                        scmd.Parameters.Add(new SqlParameter("@LabName", dr["LabName"].ToString()));
                        scmd.Parameters.Add(new SqlParameter("@DrawingRevNo", txtdrawngRevNo.Text));
                        if (!string.IsNullOrWhiteSpace(dr["LabLength"].ToString()))
                        {
                            scmd.Parameters.Add(new SqlParameter("@LabLength", Convert.ToInt32(dr["LabLength"].ToString())));
                            scmd.Parameters.Add(new SqlParameter("@LabWidth", Convert.ToInt32(dr["LabWidth"].ToString())));
                            scmd.Parameters.Add(new SqlParameter("@TrueCeilingHeight", Convert.ToInt32(dr["TrueCeilingHeight"].ToString())));
                            scmd.Parameters.Add(new SqlParameter("@FalseCeilingHeight", Convert.ToInt32(dr["FalseCeilingHeight"].ToString())));
                        }
                        string projectPath = dr["ProjectPath"].ToString();
                        projectPath = projectPath.Substring(0, projectPath.LastIndexOf(@"\"));
                        scmd.Parameters.Add(new SqlParameter("@ProjectPath", System.IO.Path.Combine(projectPath, txtProjectName.Text + "_" + txtProjectNo.Text + "_" + txtdrawngRevNo.Text + ".rvt")));
                        scmd.Parameters.Add(new SqlParameter("@isOpenProject", '0'));
                        scmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                        scmd.Parameters.Add(new SqlParameter("@IsDeleted", '0'));
                        scmd.ExecuteNonQuery();
                        if (!string.IsNullOrWhiteSpace(dr["LabLength"].ToString()))
                        {
                            UIInputs.WallLength = Convert.ToInt32(dr["LabLength"].ToString());
                            UIInputs.WallWidth = Convert.ToInt32(dr["LabWidth"].ToString());
                            UIInputs.TrueCeilingHeight = Convert.ToInt32(dr["TrueCeilingHeight"].ToString());
                            UIInputs.FalseCielingHeight = Convert.ToInt32(dr["FalseCeilingHeight"].ToString());
                        }
                        MessageBox.Show("Project revised successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        ClsProperties.isProjectRevised = true;
                        ClsProperties.ProjectCatalog = this;
                        GetProjects();
                        ClsProperties.isClosed = true;
                        this.Hide();
                    }
                }
            }
        }

        private bool Validations()
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtProjectNo.Text))
                return false;

            if (string.IsNullOrWhiteSpace(txtBuildingName.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtRoomNo.Text))
                return false;
            if (string.IsNullOrWhiteSpace(txtFloorNo.Text))
                return false;
            return true;
        }

        private void lstRevisions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (lstRevisions.SelectedItem != null)
            {
                string projectNo = GetProjectName(lstProjects.SelectedItem.ToString(), lstRevisions.SelectedItem.ToString());
                DataRow dr = (from drow in dtProjects.AsEnumerable()
                              where drow["ProjectNo"].ToString().Equals(projectNo) && drow["ProjectId"].ToString().Equals(GetIdentityId(projectNo, lstProjects.SelectedItem.ToString(), lstRevisions.SelectedItem.ToString()))
                              select drow).FirstOrDefault();
                if (dr != null)
                {
                    gbProjectDetails.IsEnabled = true;
                    lblNameAddres.IsEnabled = true;
                    txtlabname.Text = dr["LabName"].ToString();
                    txtProjectNo.Text = dr["ProjectNo"].ToString();
                    txtProjectName.Text = dr["ProjectName"].ToString();
                    txtBuildingName.Text = dr["BuildingName"].ToString();
                    txtFloorNo.Text = dr["FloorNo"].ToString();
                    txtRoomNo.Text = dr["RoomNo"].ToString();
                    txtdrawngRevNo.Text = dr["DrawingRevNo"].ToString();
                    string[] custNameAddress = dr["CustomerNameAddress"].ToString().Split('\r', '\n');
                    cmbCustNames.SelectedIndex = cmbCustNames.Items.IndexOf(custNameAddress[0].ToString());
                    txtCustAddress.Text = dr["CustomerNameAddress"].ToString();

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
}
