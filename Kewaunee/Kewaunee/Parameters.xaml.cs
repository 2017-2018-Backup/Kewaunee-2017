using Microsoft.Windows.Controls;
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
    /// Interaction logic for Parameters.xaml
    /// </summary>
    public partial class Parameters : Window
    {
        public static DataTable dtVariants = new DataTable();
        private string _connectionString = string.Empty;
        public static DataTable dtAccessories = new DataTable();
        private object doc = null;
        public Parameters(object _doc)
        {
            InitializeComponent();
            doc = _doc;
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetVariants();
            LoadDetails();
            //GetAccessoires();
        }

        private void GetVariants()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from VariantDetails";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtVariants = new DataTable();
                    dtVariants.Load(scmd.ExecuteReader());
                }
            }
        }

        private void GetAccessoires()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from ProductMasterNew where BelongsTo IS NOT NULL and BelongsTo='Furniture Accessories'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    dtAccessories = new DataTable();
                    dtAccessories.Load(scmd.ExecuteReader());
                }

                foreach (DataRow drObj in dtAccessories.Rows)
                {
                    CheckListBoxItem cbItem = new CheckListBoxItem();
                    cbItem.IsChecked = false;
                    cbItem.Content = drObj["ItemDescription"].ToString();
                    cbItem.Tag = drObj["ItemCode"].ToString();
                    checkbxLstAccessories.Items.Add(cbItem);
                }
            }

        }

        private void LoadDetails()
        {
            cmbCbnetStyle.Items.Clear();
            cmbHandleStyle.Items.Clear();
            cmbDoorStyles.Items.Clear();
            cmbMoc.Items.Clear();
            cmbOtherVariants.Items.Clear();
            if (dtVariants.Rows.Count > 0)
            {
                //dtVariants.Rows.OfType<DataRow>().ToList().ForEach(x =>
                //    {
                //        if (x["Category"].ToString().Equals("Handle Style"))
                //        {
                //            cmbHandleStyle.Items.Add(x["VariantDescription"].ToString());
                //        }
                //        if (x["Category"].ToString().Equals("MOC"))
                //        {
                //            cmbMoc.Items.Add(x["VariantDescription"].ToString());
                //        }
                //        if (x["Category"].ToString().Equals("Cabinet Styles"))
                //        {
                //            cmbCbnetStyle.Items.Add(x["VariantDescription"].ToString());
                //        }
                //        if (x["Category"].ToString().Equals("Door & Drawer Styles"))
                //        {
                //            cmbDoorStyles.Items.Add(x["VariantDescription"].ToString());
                //        }
                //        if (x["Category"].ToString().Equals("Cabinets-Other Variants"))
                //        {
                //            cmbOtherVariants.Items.Add(x["VariantDescription"].ToString());
                //        }
                //    });
                var drlist1 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("Handle Style")).Select(x => x).ToList();
                if (drlist1.Count > 0)
                {
                    cmbHandleStyle.DisplayMemberPath = "VariantDisplayName";
                    cmbHandleStyle.SelectedValuePath = "VariantDescription";
                    cmbHandleStyle.ItemsSource = drlist1.CopyToDataTable().DefaultView;
                }
                var drlist2 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("MOC")).Select(x => x).ToList();
                if (drlist2.Count > 0)
                {
                    cmbMoc.DisplayMemberPath = "VariantDisplayName";
                    cmbMoc.SelectedValuePath = "VariantDescription";
                    cmbMoc.ItemsSource = drlist2.CopyToDataTable().DefaultView;
                }
                var drlist3 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("Cabinet Styles")).Select(x => x).ToList();
                if (drlist3.Count > 0)
                {
                    cmbCbnetStyle.DisplayMemberPath = "VariantDisplayName";
                    cmbCbnetStyle.SelectedValuePath = "VariantDescription";
                    cmbCbnetStyle.ItemsSource = drlist3.CopyToDataTable().DefaultView;
                }
                var drlist4 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("Door & Drawer Styles")).Select(x => x).ToList();
                if (drlist4.Count > 0)
                {
                    cmbDoorStyles.DisplayMemberPath = "VariantDisplayName";
                    cmbDoorStyles.SelectedValuePath = "VariantDescription";
                    cmbDoorStyles.ItemsSource = drlist4.CopyToDataTable().DefaultView;
                }
                var drlist5 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("Cabinets-Other Variants")).Select(x => x).ToList();
                if (drlist5.Count > 0)
                {
                    //cmbOtherVariants.DisplayMemberPath = "VariantDisplayName";
                    //cmbOtherVariants.SelectedValuePath = "VariantDescription";
                    foreach (DataRow drObj in drlist5)
                    {
                        CheckListBoxItem cbItem = new CheckListBoxItem();
                        cbItem.IsChecked = false;
                        cbItem.Content = drObj["VariantDisplayName"].ToString();
                        cbItem.Tag = drObj["VariantDescription"].ToString();
                        cmbOtherVariants.Items.Add(cbItem);
                    }
                }

            }
        }
        private string variantCode = string.Empty;
        private string variantDes = string.Empty;
        private string itemcode = string.Empty;
        private string price = string.Empty;
        private void btnUpdate_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
            string otherVariants = string.Empty;
            foreach (CheckListBoxItem obj in cmbOtherVariants.Items)
            {
                if (obj.IsChecked)
                {
                    if (otherVariants == string.Empty)
                    {
                        otherVariants = obj.Tag.ToString();

                    }
                    else
                    {
                        otherVariants = otherVariants + "," + obj.Tag.ToString();

                    }
                }
            }
            string hod = cmbHandleStyle.SelectedValue != null && !string.IsNullOrWhiteSpace(cmbHandleStyle.SelectedValue.ToString()) ? cmbHandleStyle.SelectedValue.ToString() : string.Empty;
            string mod = cmbMoc.SelectedValue != null && !string.IsNullOrWhiteSpace(cmbMoc.SelectedValue.ToString()) ? cmbMoc.SelectedValue.ToString() : string.Empty;
            string dod = cmbDoorStyles.SelectedValue != null && !string.IsNullOrWhiteSpace(cmbDoorStyles.SelectedValue.ToString()) ? cmbDoorStyles.SelectedValue.ToString() : string.Empty;
            string cod = cmbCbnetStyle.SelectedValue != null && !string.IsNullOrWhiteSpace(cmbCbnetStyle.SelectedValue.ToString()) ? cmbCbnetStyle.SelectedValue.ToString() : string.Empty;
            string od = !string.IsNullOrWhiteSpace(otherVariants) ? otherVariants : string.Empty;
            string varCode = string.Empty;
            KewauneeTaskAssigner.TaskAssigner.UpdateParameters(hod, mod, dod, cod, od, dtVariants, out variantCode, out itemcode, out variantDes, ClsProperties.LstElementIds, _connectionString, ref varCode);

            var lst = new List<string>();

            foreach (CheckListBoxItem item in checkbxLstAccessories.Items)
            {
                if (item.IsChecked)
                {
                    lst.Add(item.Tag.ToString());
                }
            }
            CreateParameter createParam = new CreateParameter(ClsProperties.LstElementIds, doc, string.Empty, string.Empty, string.Empty, false, false);

            createParam.CreateAccessoriesParameter(lst, ClsProperties.PartCode);
        }
    }
}
