using KewauneeTaskAssigner;
using System;
using System.Collections.Generic;
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
using System.IO;
using System.Data.SqlClient;
using System.Data;
namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for DockablePage.xaml
    /// </summary>
    public partial class DockablePage : Page
    {
        List<BindingListItem> imagescoll { get; set; }
        private string _connectionString = string.Empty;
        public DataTable dtProducts = new DataTable();
        private DataTable dtPaths = new DataTable();
        private string[] imageFiles = default(string[]);
        public static string SubGroup1 { get; set; }
        private string dirPath = string.Empty;
        private static DockablePage _dockablePageInstance = null;

        public static DockablePage SingleTonDockablePageInstance
        {
            get
            {
                if (_dockablePageInstance == null)
                {
                    _dockablePageInstance = new DockablePage();
                }
                return _dockablePageInstance;
            }
        }

        public DockablePage()
        {
            InitializeComponent();
            _connectionString = Properties.Settings.Default.ConnectionString;
            dirPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dirPath = dirPath.Substring(0, dirPath.LastIndexOf(@"\"));
            dirPath = System.IO.Path.Combine(dirPath, "Kewaunee\\Revit\\Resources\\NoImageAvailable.jpg");
            GetPaths();
            LoadData();
            //txtSearch.IsEnabled = false;
        }

        public void LoadData()
        {
            dtProducts = new DataTable();
            GetProducts();
            LoadListViewItems();
            LoadCategories();
        }

        private void LoadCategories()
        {
            if (dtProducts != null && dtProducts.Rows.Count > 0)
            {
                var lstCategories = new List<string>();
                lstCategories = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["SubGroup1"].ToString())).Select(x => x["SubGroup1"].ToString()).Distinct().ToList();
                if (lstCategories.Count > 0)
                    lstCategories.ForEach(x => { cmboCategory.Items.Add(x); });
                cmboCategory.Items.Insert(0, "All");
            }
        }

        private void GetPaths()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from ProjectPath";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtPaths = new System.Data.DataTable();
                    dtPaths.Load(scmd.ExecuteReader());
                }
            }
        }

        private void GetProducts()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from ProductMasterNew where IsDeleted != '1'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtProducts.Load(scmd.ExecuteReader());
                    ClsProperties.dtProducts = dtProducts;
                }
            }
        }

        private void Image_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ClsProperties.isLoginSuccess)
            {
                if (lstImages.SelectedItem == null)
                    return;
                string hostname = dtPaths.Rows[0]["HostName"].ToString();
                string hostfile = dtPaths.Rows[0]["HostFilePath"].ToString();
                string path = System.IO.Path.Combine(hostname, hostfile);
                var bindingItem = (BindingListItem)lstImages.SelectedItem;
                ClsProperties.IsMouseDragged = true;

                //ClsProperties.FamilyPath = (sender as Image).Tag.ToString();
                ClsProperties.FamilyPath = bindingItem.picturestring == dirPath ? path + @"\" + dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().Equals(bindingItem.partCode)).Select(x => x["FamilyName"].ToString().Replace(".jpg", ".rfa")).FirstOrDefault() : bindingItem.picturestring.Replace("jpg", "rfa");
                //ClsProperties.FamilyPath = bindingItem.picturestring;
                ClsProperties.PartCode = bindingItem.partCode;
                ClsProperties.PlacedFamilyCategory = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["FamilyName"].ToString().Equals(System.IO.Path.GetFileName(ClsProperties.FamilyPath.Replace(".jpg", ".rfa")))).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                string fileName = System.IO.Path.GetFileNameWithoutExtension(ClsProperties.FamilyPath);
                ClsProperties.FamilyPath = System.IO.Path.Combine(path, fileName + ".rfa");
                SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(fileName + ".rfa")).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                TaskAssigner.HideDockPanel(ClsProperties.UIApplication, ClsProperties.DockablePaneGuid);
            }
            else
            {
                if (!ClsProperties.isLoginSuccess)
                    MessageBox.Show("Please login before loading families into project.", "Kewaunee", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadListViewItems()
        {
            //imageFiles = Directory.GetFiles(@"C:\Kewaunee\JPG\");
            string hostname = dtPaths.Rows[0]["HostName"].ToString();
            string hostfile = dtPaths.Rows[0]["ImageFilePath"].ToString();
            imageFiles = Directory.GetFiles(System.IO.Path.Combine(hostname, hostfile));
            imagescoll = new List<BindingListItem>();
            //foreach (string path in imageFiles)
            //{
            //    BindingListItem obj = new BindingListItem();
            //    obj.picturestring = path;

            //    obj.partCode = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["FamilyName"].ToString().Equals(System.IO.Path.GetFileName(path).Replace("jpg", "rfa"))).Select(x => x["ItemCode"].ToString()).FirstOrDefault();
            //    imagescoll.Add(obj);
            //}
            
            dtProducts.Rows.OfType<DataRow>().ToList().ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x["FamilyName"].ToString()) && string.IsNullOrWhiteSpace(x["BelongsTo"].ToString()))
                {
                    var fileName = x["FamilyName"].ToString();
                    fileName = fileName.Replace(".rfa", ".jpg");
                    BindingListItem obj = new BindingListItem();
                    bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                    obj.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                    obj.partCode = x["ItemCode"].ToString();
                    imagescoll.Add(obj);
                }
            });
            lstImages.ItemsSource = null;
            lstImages.ItemsSource = imagescoll;
        }

        private void btnsearch_Click_1(object sender, RoutedEventArgs e)
        {
            if (dtProducts.Rows.Count > 0 && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                bordImages.Margin = new Thickness(4);
                var searchItem = string.Empty;
                var drList = new List<DataRow>();
                drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemDescription"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ProductFamily"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup2"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup3"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup4"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup5"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup6"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup7"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count == 0)
                    drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup8"].ToString().ToUpper().Contains(txtSearch.Text.ToUpper())).Select(x => x).ToList();
                if (drList.Count > 0)
                {
                    imagescoll = new List<BindingListItem>();



                    foreach (DataRow dr in drList)
                    {
                        if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()))
                        {
                            var fileName = dr["FamilyName"].ToString();
                            fileName = fileName.Replace(".rfa", ".jpg");
                            searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            BindingListItem bndg = new BindingListItem();
                            bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                            bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                            bndg.partCode = dr["ItemCode"].ToString();
                            bndg.FamilyName = searchItem;
                            imagescoll.Add(bndg);
                        }
                    }
                    lstImages.ItemsSource = null;
                    lstImages.ItemsSource = imagescoll;
                    lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                    //txtSearch.Clear();
                    // uniformGrid.Visiblity = Visibility.Collapsed;
                }
            }
        }

        private void lblSuggestion_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (!isArrowKeyused)
                if (lblSuggestion.ItemsSource != null)
                {
                    lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                    txtSearch.TextChanged -= new TextChangedEventHandler(txtSearch_SelectionChanged_1);
                    if (lblSuggestion.SelectedIndex != -1)
                    {
                        txtSearch.Text = lblSuggestion.SelectedItem.ToString();
                        bordImages.Margin = new Thickness(4);
                        lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    txtSearch.TextChanged += new TextChangedEventHandler(txtSearch_SelectionChanged_1);
                }
        }

        private void txtSearch_SelectionChanged_1(object sender, RoutedEventArgs e)
        {
            string typedString = txtSearch.Text;
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                SearchProcess(typedString);
            }
            else
            {
                lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                lblSuggestion.ItemsSource = null;
                bordImages.Margin = new Thickness(4);
            }
        }

        private void SearchProcessBasedCategory(string typedString)
        {
            var drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup2"].ToString().Equals(typedString)).Select(x => x).ToList();
            var searchItem = string.Empty;
            imagescoll = new List<BindingListItem>();

            foreach (DataRow dr in drList)
            {
                if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()))
                {
                    var fileName = dr["FamilyName"].ToString();
                    fileName = fileName.Replace(".rfa", ".jpg");
                    searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                    SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                    var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                    BindingListItem bndg = new BindingListItem();
                    bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                    bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                    bndg.FamilyName = searchItem;
                    bndg.partCode = dr["ItemCode"].ToString();
                    imagescoll.Add(bndg);
                }
            }
            lstImages.ItemsSource = null;
            lstImages.ItemsSource = imagescoll;
            lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
        }

        //private void SearchProcess(string typedString)
        //{
        //    try
        //    {
        //        List<string> autoList = new List<string>();

        //        if (cmboCategory.SelectedItem.ToString() == "All")
        //            autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup1"].ToString()).Distinct().ToList();
        //        else
        //            autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup1"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup1"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemCode"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["ItemCode"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemCode"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ProductFamily"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ProductFamily"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["ProductFamily"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ProductFamily"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemDescription"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemDescription"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["ItemDescription"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemDescription"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup2"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup2"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup2"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup2"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup3"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup3"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup3"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup3"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup4"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup4"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup4"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup4"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup5"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup5"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup5"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup5"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup6"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup6"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup6"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup6"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup7"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup7"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup7"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup7"].ToString()).Distinct().ToList();

        //        if (autoList.Count == 0)
        //            if (cmboCategory.SelectedItem.ToString() == "All")
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup8"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup8"].ToString()).Distinct().ToList();
        //            else
        //                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Contains(cmboCategory.SelectedItem.ToString()) && x["SubGroup2"].ToString().Contains(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup3"].ToString().Contains(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup4"].ToString().Contains(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup8"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup8"].ToString()).Distinct().ToList();

        //        if (autoList.Count > 0)
        //        {
        //            lblSuggestion.ItemsSource = null;
        //            lblSuggestion.ItemsSource = autoList;
        //            lblSuggestion.Visibility = System.Windows.Visibility.Visible;
        //            bordImages.Margin = new Thickness(4, 100, 4, 4);
        //        }
        //        else if (string.IsNullOrWhiteSpace(txtSearch.Text))
        //        {
        //            lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
        //            lblSuggestion.ItemsSource = null;
        //            bordImages.Margin = new Thickness(4);
        //        }
        //        else
        //        {
        //            lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
        //            lblSuggestion.ItemsSource = null;
        //            bordImages.Margin = new Thickness(4);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}


        private void SearchProcess(string typedString)
        {
            try
            {
                List<string> autoList = new List<string>();


                autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup1"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemCode"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ProductFamily"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ProductFamily"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemDescription"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["ItemDescription"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup2"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup2"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup3"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup3"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup4"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup4"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup5"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup5"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup6"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup6"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup7"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup7"].ToString()).Distinct().ToList();

                if (autoList.Count == 0)

                    autoList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup8"].ToString().ToUpper().Contains(typedString.ToUpper())).Select(x => x["SubGroup8"].ToString()).Distinct().ToList();

                if (autoList.Count > 0)
                {
                    lblSuggestion.ItemsSource = null;
                    lblSuggestion.ItemsSource = autoList;
                    lblSuggestion.Visibility = System.Windows.Visibility.Visible;
                    bordImages.Margin = new Thickness(4, 100, 4, 4);
                }
                else if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                    lblSuggestion.ItemsSource = null;
                    bordImages.Margin = new Thickness(4);
                }
                else
                {
                    lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                    lblSuggestion.ItemsSource = null;
                    bordImages.Margin = new Thickness(4);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmboCategory.SelectedIndex = -1;
            cmbSubGroup2.SelectedIndex = -1;
            cmbSubGroup3.SelectedIndex = -1;
            cmbSubGroup4.SelectedIndex = -1;
            //txtSearch.IsEnabled = false;
            LoadListViewItems();
            return;

        }

        private void btnCategorySearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            if (cmboCategory.SelectedItem.ToString() == "All")
            {
                LoadListViewItems();
                return;
            }
            SearchProcessBasedCategory(cmboCategory.SelectedItem.ToString());
        }

        private void cmboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                if (cmboCategory.SelectedItem.ToString() == "All")
                {
                    LoadListViewItems();
                    return;
                }

                var lstCategories = new List<string>();
                lstCategories = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["SubGroup1"].ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x["SubGroup2"].ToString()).Distinct().ToList();
                if (lstCategories.Count > 0)
                {
                    cmbSubGroup2.Items.Clear();
                    cmbSubGroup3.Items.Clear();
                    cmbSubGroup4.Items.Clear();
                    txtSearch.Clear();
                    lstCategories.ForEach(x => { cmbSubGroup2.Items.Add(x); });
                }

                var drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x).ToList();
                var searchItem = string.Empty;
                imagescoll = new List<BindingListItem>();
                foreach (DataRow dr in drList)
                {
                    //if (string.IsNullOrWhiteSpace(dr["BelongsTo"].ToString()))
                        if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()) && dr["FamilyName"].ToString().Contains("."))
                        {
                            //searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            //SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            //var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            //BindingListItem bndg = new BindingListItem();
                            //bndg.picturestring = familyImage;
                            //bndg.FamilyName = searchItem;
                            //bndg.partCode = dr["ItemCode"].ToString();
                            //imagescoll.Add(bndg);
                            var fileName = dr["FamilyName"].ToString();
                            fileName = fileName.Replace(".rfa", ".jpg");
                            searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            BindingListItem bndg = new BindingListItem();
                            bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                            bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                            bndg.FamilyName = searchItem;
                            bndg.partCode = dr["ItemCode"].ToString();
                            imagescoll.Add(bndg);
                        }
                }
                lstImages.ItemsSource = null;
                lstImages.ItemsSource = imagescoll;
                lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void cmbSubGroup2_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                var lstCategories = new List<string>();
                lstCategories = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["SubGroup2"].ToString()) && x["SubGroup2"].ToString().Equals(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x["SubGroup3"].ToString()).Distinct().ToList();
                if (lstCategories.Count > 0)
                {
                    cmbSubGroup3.Items.Clear();
                    lstCategories.ForEach(x => { cmbSubGroup3.Items.Add(x); });
                }

                var drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["SubGroup2"].ToString()) && x["SubGroup2"].ToString().Equals(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x).ToList();
                var searchItem = string.Empty;
                imagescoll = new List<BindingListItem>();
                foreach (DataRow dr in drList)
                {
                    //if (string.IsNullOrWhiteSpace(dr["BelongsTo"].ToString()))
                        if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()))
                        {
                            //searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            //SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            //var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            //BindingListItem bndg = new BindingListItem();
                            //bndg.picturestring = familyImage;
                            //bndg.FamilyName = searchItem;
                            //bndg.partCode = dr["ItemCode"].ToString();
                            //imagescoll.Add(bndg);
                            var fileName = dr["FamilyName"].ToString();
                            fileName = fileName.Replace(".rfa", ".jpg");
                            searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            BindingListItem bndg = new BindingListItem();
                            bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                            bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                            bndg.FamilyName = searchItem;
                            bndg.partCode = dr["ItemCode"].ToString();
                            imagescoll.Add(bndg);
                        }
                }
                lstImages.ItemsSource = null;
                lstImages.ItemsSource = imagescoll;
                lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
            }
            // cmboCategory.Items.Insert(0, "All");
        }

        private void cmbSubGroup3_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                var lstCategories = new List<string>();
                lstCategories = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => !string.IsNullOrWhiteSpace(x["SubGroup3"].ToString()) && x["SubGroup3"].ToString().Equals(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup2"].ToString().Equals(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x["SubGroup4"].ToString()).Distinct().ToList();
                if (lstCategories.Count > 0)
                {
                    cmbSubGroup4.Items.Clear();
                    lstCategories.ForEach(x => { cmbSubGroup4.Items.Add(x); });
                }

                var drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup3"].ToString().Equals(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup2"].ToString().Equals(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x).ToList();
                var searchItem = string.Empty;
                imagescoll = new List<BindingListItem>();
                foreach (DataRow dr in drList)
                {
                    //if (string.IsNullOrWhiteSpace(dr["BelongsTo"].ToString()))
                        if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()))
                        {
                            //searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            //SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            //var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            //BindingListItem bndg = new BindingListItem();
                            //bndg.picturestring = familyImage;
                            //bndg.FamilyName = searchItem;
                            //bndg.partCode = dr["ItemCode"].ToString();
                            //imagescoll.Add(bndg);
                            var fileName = dr["FamilyName"].ToString();
                            fileName = fileName.Replace(".rfa", ".jpg");
                            searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            BindingListItem bndg = new BindingListItem();
                            bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                            bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                            bndg.FamilyName = searchItem;
                            bndg.partCode = dr["ItemCode"].ToString();
                            imagescoll.Add(bndg);
                        }
                }
                lstImages.ItemsSource = null;
                lstImages.ItemsSource = imagescoll;
                lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void cmbSubGroup4_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                var drList = dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["SubGroup4"].ToString().Equals(cmbSubGroup4.SelectedItem.ToString()) && x["SubGroup3"].ToString().Equals(cmbSubGroup3.SelectedItem.ToString()) && x["SubGroup2"].ToString().Equals(cmbSubGroup2.SelectedItem.ToString()) && x["SubGroup1"].ToString().Equals(cmboCategory.SelectedItem.ToString())).Select(x => x).ToList();
                var searchItem = string.Empty;
                imagescoll = new List<BindingListItem>();
                foreach (DataRow dr in drList)
                {
                    //if (string.IsNullOrWhiteSpace(dr["BelongsTo"].ToString()))
                        if (!string.IsNullOrWhiteSpace(dr["FamilyName"].ToString()))
                        {
                            //searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            //SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            //var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            //BindingListItem bndg = new BindingListItem();
                            //bndg.picturestring = familyImage;
                            //bndg.FamilyName = searchItem;
                            //bndg.partCode = dr["ItemCode"].ToString();
                            //imagescoll.Add(bndg);
                            var fileName = dr["FamilyName"].ToString();
                            fileName = fileName.Replace(".rfa", ".jpg");
                            searchItem = dr["FamilyName"].ToString().Substring(0, dr["FamilyName"].ToString().LastIndexOf("."));
                            SubGroup1 = dtProducts.Rows.OfType<DataRow>().ToList().Where(X => X["FamilyName"].ToString().Equals(dr["FamilyName"].ToString())).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                            var familyImage = imageFiles.Where(x => x.Contains(searchItem + ".jpg")).Select(x => x.ToString()).FirstOrDefault();
                            BindingListItem bndg = new BindingListItem();
                            bool isImageExists = imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() != null ? true : false;
                            bndg.picturestring = isImageExists ? imageFiles.Where(y => y.Contains(fileName)).Select(y => y).FirstOrDefault() : dirPath;
                            bndg.FamilyName = searchItem;
                            bndg.partCode = dr["ItemCode"].ToString();
                            imagescoll.Add(bndg);
                        }
                }
                lstImages.ItemsSource = null;
                lstImages.ItemsSource = imagescoll;
                lblSuggestion.Visibility = System.Windows.Visibility.Collapsed;
                txtSearch.IsEnabled = true;
            }
        }
        bool isArrowKeyused = false;
        private void txtSearch_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnsearch_Click_1(null, null);
            switch (e.Key)
            {
                case Key.Up:

                    ((UIElement)lblSuggestion).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    e.Handled = true;
                    isArrowKeyused = true;
                    break;
                case Key.Down:
                    ((UIElement)lblSuggestion).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                    isArrowKeyused = true;
                    break;
            }
        }

        private void lblSuggestion_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                isArrowKeyused = false;
                lblSuggestion_SelectionChanged_1(null, null);
            }
        }

        private void lstImages_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
