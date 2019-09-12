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
    /// Interaction logic for FumeHoodVariants.xaml
    /// </summary>
    public partial class FumeHoodVariants : Window
    {
        public static DataTable dtVariants = new DataTable();
        private string _connectionString = string.Empty;
        public static DataTable dtAccessories = new DataTable();
        private object doc;
        public FumeHoodVariants(object _doc)
        {
            InitializeComponent();
            doc = _doc;
            _connectionString = Properties.Settings.Default.ConnectionString;
            GetVariants();
            LoadDetails();
            GetAccessoires();
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

        private void LoadDetails()
        {

            cmbOtherVariants.Items.Clear();
            if (dtVariants.Rows.Count > 0)
            {
                var drlist1 = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["Category"].ToString().Equals("Fume Hood")).Select(x => x).ToList();
                if (drlist1.Count > 0)
                {
                    //cmbOtherVariants.DisplayMemberPath = "VariantDisplayName";
                    //cmbOtherVariants.SelectedValuePath = "VariantDescription";
                    foreach (DataRow drObj in drlist1)
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

        private void GetAccessoires()
        {
            using (var scn = new SqlConnection(_connectionString))
            {
                scn.Open();
                var query = "select * from ProductMasterNew where BelongsTo IS NOT NULL and BelongsTo='Fumehood Accessories'";
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
                    cbxAccessories.Items.Add(cbItem);
                }
            }

        }

        private string variantCode = string.Empty;
        private string variantDes = string.Empty;
        private string itemcode = string.Empty;
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
                        if (!ClsProperties.LstUpdatedParamList.Contains(obj.Tag.ToString()))
                            ClsProperties.LstUpdatedParamList.Add(obj.Tag.ToString());
                    }
                    else
                    {
                        otherVariants = otherVariants + "," + obj.Tag.ToString();
                        if (!ClsProperties.LstUpdatedParamList.Contains(obj.Tag.ToString()))
                            ClsProperties.LstUpdatedParamList.Add(obj.Tag.ToString());
                    }
                }
            }

            string od = !string.IsNullOrWhiteSpace(otherVariants) ? otherVariants : string.Empty;
            string varCode = string.Empty;
            KewauneeTaskAssigner.TaskAssigner.UpdateParameters(od, dtVariants, out variantCode, out itemcode, out variantDes, ClsProperties.LstElementIds, _connectionString,ref varCode);

            if (ClsProperties.dictFumeHoodFamilies == null)
                ClsProperties.dictFumeHoodFamilies = new Dictionary<string, Dictionary<string, List<string>>>();

            if (!ClsProperties.dictFumeHoodFamilies.ContainsKey(ClsProperties.PartCode))
                ClsProperties.dictFumeHoodFamilies.Add(ClsProperties.PartCode, new Dictionary<string, List<string>>());

            var lst = new List<string>();

            foreach (CheckListBoxItem item in cbxAccessories.Items)
            {
                if (item.IsChecked)
                {
                    lst.Add(item.Tag.ToString());
                }
            }
            CreateParameter createParam = new CreateParameter(ClsProperties.LstElementIds, doc, string.Empty, string.Empty, string.Empty,false,false);

            createParam.CreateAccessoriesParameter(lst, ClsProperties.PartCode);

            //if (!string.IsNullOrWhiteSpace(variantCode))
            //{
            //    using (var scn = new SqlConnection(_connectionString))
            //    {
            //        scn.Open();
            //        var query = "update ProductMaster SET VariantCode ='" + variantCode + "',VariantDescription='" + variantDes + "' where ItemCode='" + itemcode + "'";
            //        using (var scmd = new SqlCommand(query, scn))
            //        {
            //            scmd.ExecuteNonQuery();
            //        }
            //    }
            //}
        }
    }
}
