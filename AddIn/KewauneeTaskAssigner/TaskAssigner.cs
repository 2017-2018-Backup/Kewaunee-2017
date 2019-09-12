using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KewauneeTaskAssigner
{
    public class TaskAssigner
    {
        static UIApplication uiapp = default(UIApplication);
        private static string _connectionString = string.Empty;



        public static void HideDockPanel(object uiApplication, Guid guid)
        {
            if (uiApplication != null)
            {
                uiapp = (UIApplication)uiApplication;
                var dPid = new DockablePaneId(guid);
                DockablePane dockPanel = uiapp.GetDockablePane(dPid);
                dockPanel.Hide();
            }
        }

        public static void ShowDockPanel(object uiApplication, Guid guid)
        {
            if (uiApplication != null)
            {
                uiapp = (UIApplication)uiApplication;
                var dPid = new DockablePaneId(guid);
                DockablePane dockPanel = uiapp.GetDockablePane(dPid);
                dockPanel.Hide();
            }
        }

        public static void OpenDocument(object uiAppln, string path)
        {
            if (System.IO.File.Exists(path))
            {
                uiapp = uiAppln as UIApplication;
                uiapp.OpenAndActivateDocument(path);
            }
        }

        public static DataTable GetLoadedItems(object cmndData, DataTable dtProducts, DataTable dtPartCodes, DataTable dtVariants, Dictionary<string, Dictionary<string, List<string>>> dictAccessories)
        {
            try
            {
                DataTable dtItems = new DataTable();
                ExternalCommandData commandData = default(ExternalCommandData);
                if (cmndData != null)
                    commandData = (ExternalCommandData)cmndData;
                string itemDescription = string.Empty;
                string group1 = string.Empty, group2 = string.Empty, group3 = string.Empty;
                uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Application app = uiapp.Application;
                Document doc = uidoc.Document;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> elements = collector.OfClass(typeof(FamilyInstance)).ToElements();

                dtItems.Columns.Add("SlNo");
                dtItems.Columns.Add("ItemCode");
                dtItems.Columns.Add("VariantCode");
                dtItems.Columns.Add("Group 1");
                dtItems.Columns.Add("Group 2");
                dtItems.Columns.Add("Group 3");
                dtItems.Columns.Add("ItemVariantDescription");
                dtItems.Columns.Add("Qty");
                dtItems.Columns.Add("UnitOfMeas");
                dtItems.Columns.Add("UnitPrice");
                dtItems.Columns.Add("Discount");
                dtItems.Columns.Add("DiscountedUnitPrice");
                dtItems.Columns.Add("TotalUnitPrice");
                dtItems.Columns.Add("SpecialNotes");
                dtItems.Columns.Add("ItemPurchaseType");
                dtItems.Columns.Add("AnnexureId");
                dtItems.Columns.Add("Category");
                dtItems.Columns.Add("VariantDescription");

                int i = 0;
                int cnt = 0;
                foreach (var famInst in elements)
                {
                    try
                    {
                        cnt = 1;
                        FamilyInstance finst = (FamilyInstance)famInst;
                        if (finst != null)
                        {
                            string itemName = finst.Name;
                            vdes = string.Empty;
                            var famType = finst.GetType();
                            var familyName = (from drow in dtProducts.AsEnumerable()
                                              where drow["ItemCode"].ToString().Trim().Equals(itemName.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                              select drow["FamilyName"]).FirstOrDefault();
                            if (familyName != null && !string.IsNullOrWhiteSpace(familyName.ToString()))
                            {
                                DataRow dr = (from drow in dtProducts.AsEnumerable()
                                              where drow["FamilyName"].ToString().Contains(finst.Symbol.FamilyName)
                                              select drow).FirstOrDefault();
                                //if (dr != null)
                                //{
                                i++;

                                DataRow drItemCode = (from drow in dtItems.AsEnumerable()
                                                      where drow["ItemCode"].ToString().Trim().Contains(itemName.Trim())
                                                      select drow).FirstOrDefault();

                                hd = string.Empty; md = string.Empty; dd = string.Empty; cd = string.Empty; od = string.Empty; fd = string.Empty; vdes = string.Empty;
                                if (drItemCode != null)
                                {
                                    i--;

                                    double price = 0; bool isNewSet = true;
                                    double ftCalc = 0.0;
                                    var lstVariantDetails = new List<string>();
                                    famInst.Parameters.OfType<Parameter>().ToList().ForEach(x =>
                                    {
                                        string str = x.AsValueString();
                                        bool isExists = dtVariants.Rows.OfType<DataRow>().Any(y => y["VariantDescription"].Equals(x.Definition.Name));
                                        if (str != null && isExists && str.Equals("Yes"))
                                        {
                                            isNewSet = false;
                                            price = Convert.ToInt32(price + Convert.ToInt32(dtVariants.Rows.OfType<DataRow>().Where(y => y["VariantDescription"].Equals(x.Definition.Name)).Select(y => y["Price"].ToString()).FirstOrDefault()));
                                            GenerateCode(x.Definition.Name, dtVariants);
                                            string varDescription = dtVariants.Rows.OfType<DataRow>().ToList().Where(y => y["VariantDescription"].ToString().Equals(x.Definition.Name)).Select(y => y["VariantDisplayName"].ToString()).FirstOrDefault().Replace("/", "-");
                                            //vdes = vdes + "/" + varDescription;
                                            if (!lstVariantDetails.Contains(varDescription))
                                                lstVariantDetails.Add(varDescription);
                                        }
                                    });
                                    lstVariantDetails.Sort();
                                    vdes = vdes + (vdes.Length > 0 ? "/" : string.Empty) + string.Join("/", lstVariantDetails);
                                    itemDescription = string.Empty;
                                    itemDescription = finst.Symbol.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Description")).Select(x => x.AsString()).FirstOrDefault().Replace("/", "-");
                                    group1 = string.Empty;
                                    var grp1 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 1", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group1 = grp1 != null ? grp1 : string.Empty;

                                    group2 = string.Empty;
                                    var grp2 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 2", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group2 = grp2 != null ? grp2 : string.Empty;

                                    group3 = string.Empty;
                                    var grp3 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 3", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group3 = grp3 != null ? grp3 : string.Empty;


                                    if (price == 0)
                                        price = Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
                                    else
                                        price = price + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());

                                    string variantCode = string.Empty;
                                    if (!string.IsNullOrWhiteSpace(fd))
                                        variantCode = fd;
                                    else
                                    {
                                        variantCode = md + hd + cd + dd + od;
                                    }
                                    bool isNew = false;
                                    if (string.IsNullOrWhiteSpace(variantCode))
                                    {
                                        isNew = dtItems.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().Trim().Equals(itemName.Trim(), StringComparison.InvariantCultureIgnoreCase) && x["VariantCode"].ToString().Trim().Equals(variantCode.Trim(), StringComparison.InvariantCultureIgnoreCase)).Select(x => x).ToList().Count == 0 ? true : false;
                                    }
                                    else
                                    {
                                        var drList = dtItems.Rows.OfType<DataRow>().Where(x => x["ItemCode"].ToString().Trim().Equals(itemName.Trim(), StringComparison.InvariantCultureIgnoreCase) && x["VariantCode"].ToString().Trim().Equals(variantCode.Trim(), StringComparison.InvariantCultureIgnoreCase)).Select(x => x).ToList();
                                        //isNew = Extensions.CheckValidVariants(vdes, drList);
                                        isNew = drList.Count == 0 ? true : false;
                                    }


                                    double length = 0, depth = 0;
                                    if (itemName.Contains("JBG-"))
                                    {

                                        var parameter = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Length", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();

                                        var typeDepth = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Depth", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();

                                        if (parameter != null)
                                            length = Convert.ToDouble(parameter.AsValueString());
                                        //var depthParam = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Depth", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();
                                        //if (depthParam != null)
                                        //    depth = Convert.ToInt32(depthParam.AsValueString());


                                        //Element e = doc.GetElement(finst.Id);
                                        //ElementType type = doc.GetElement(e.GetTypeId()) as ElementType;
                                        //to get height of section
                                        //Parameter h = type.LookupParameter("Depth");
                                        if (typeDepth != null)
                                            depth = Convert.ToDouble(typeDepth.AsValueString());

                                        ftCalc = CalculateStandardFeet(length, depth, 1);
                                        price = price * CalculateStandardFeet(length, depth, 1);
                                        //System.Windows.Forms.MessageBox.Show("JBG-" + ftCalc.ToString() + ";" + length.ToString() + ";" + depth.ToString());
                                        //price = CalculateStandardFeet(length, depth, 1);
                                    }
                                    else if (itemName.Contains("SK-"))
                                    {
                                        var depthParam = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Length", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();
                                        if (depthParam != null)
                                            depth = Convert.ToInt32(depthParam.AsValueString());


                                        ftCalc = CalculateRunningFeet(depth, 1);
                                        price = price * CalculateRunningFeet(depth, 1);
                                        //System.Windows.Forms.MessageBox.Show("SK - " + depth.ToString() + ";" + ftCalc.ToString());
                                        //price = CalculateRunningFeet(depth, 1);
                                    }

                                    if (isNew)
                                    {
                                        AddRowForNewFamilySet(dtItems, itemDescription, group1, group2, group3, ref i, itemName, dr, ref price, vdes, (itemName.Contains("SK-")) || itemName.Contains("JBG-") ? ftCalc : cnt);

                                    }
                                    else
                                    {
                                        // IncreaseQuantityForExistingFamilySet(dtItems, cnt, itemName, dr, drItemCode, price, group1, group2, group3, ref i, itemDescription, variantCode, vdes);
                                        IncreaseQuantityForExistingFamilySet(dtItems, (itemName.Contains("SK-")) || itemName.Contains("JBG-") ? ftCalc : cnt, itemName, dr, drItemCode, price, group1, group2, group3, ref i, itemDescription, variantCode, vdes);
                                    }
                                    int sno = i;
                                    var param = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Accessories")).Select(x => x).FirstOrDefault();
                                    if (param != null)
                                    {

                                        var value = param.AsString();
                                        var values = value.Split(',');
                                        values.ToList().ForEach(x =>
                                        {
                                            // if (!x.Contains("JBG-") && !x.Contains("SK-"))
                                            // {
                                            var drExists = dtItems.Rows.OfType<DataRow>().ToList().Where(y => !string.IsNullOrWhiteSpace(x) && y["ItemCode"].ToString().Equals(x) && y["Group 1"].ToString().Equals(grp1) && y["Group 2"].ToString().Equals(grp2) && y["Group 3"].ToString().Equals(grp3)).Select(y => y).FirstOrDefault();
                                            if (drExists == null)
                                            {
                                                var drAccess = dtProducts.Rows.OfType<DataRow>().ToList().Where(y => !string.IsNullOrWhiteSpace(x) && y["ItemCode"].ToString().Equals(x)).Select(y => y).FirstOrDefault();
                                                if (drAccess != null)
                                                {
                                                    sno++;
                                                    dtItems.Rows.Add(sno, x, "", group1, group2, group3, drAccess["ItemDescription"].ToString().Replace("/", "-"), cnt, drAccess["UnitofMeasures"].ToString(), drAccess["Price in USD - P.Y"].ToString(), "", "", drAccess["Price in USD - P.Y"].ToString(), "", drAccess["ItemPurchaseType"].ToString(), drAccess["AnnexureId"].ToString(), "", vdes);
                                                }
                                            }
                                            else
                                            {
                                                cnt = Convert.ToInt32(drExists["Qty"].ToString());
                                                cnt = cnt + 1;
                                                dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].Equals(x) && y["Group 1"].ToString().Equals(grp1) && y["Group 2"].ToString().Equals(grp2) && y["Group 3"].ToString().Equals(grp3)).Select(y =>
                                                {
                                                    y["Qty"] = cnt.ToString();
                                                    return y;
                                                }).ToList();

                                                price = Convert.ToInt32(drExists["UnitPrice"].ToString());
                                                dtItems.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].Equals(x) && y["Group 1"].ToString().Equals(grp1) && y["Group 2"].ToString().Equals(grp2) && y["Group 3"].ToString().Equals(grp3)).Select(y =>
                                                {
                                                    int tot = Convert.ToInt32(drExists["TotalUnitPrice"].ToString());
                                                    //tot = tot + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
                                                    y["TotalUnitPrice"] = Convert.ToInt32(price + tot).ToString();
                                                    y["UnitPrice"] = Convert.ToInt32(price + tot).ToString();
                                                    return y;
                                                }).ToList();
                                            }
                                            // }
                                        });
                                    }
                                    i = sno++;
                                }
                                else if (dr != null)
                                {
                                    double price = 0;
                                    double ftCalc = 0.0;
                                    var lstVariantDetails = new List<string>();
                                    famInst.Parameters.OfType<Parameter>().ToList().ForEach(x =>
                                    {
                                        string str = x.AsValueString();
                                        bool isExists = dtVariants.Rows.OfType<DataRow>().Any(y => y["VariantDescription"].Equals(x.Definition.Name));
                                        if (str != null && isExists && str.Equals("Yes"))
                                        {
                                            price = Convert.ToInt32(price + Convert.ToInt32(dtVariants.Rows.OfType<DataRow>().Where(y => y["VariantDescription"].Equals(x.Definition.Name)).Select(y => y["Price"].ToString()).FirstOrDefault()));
                                            GenerateCode(x.Definition.Name, dtVariants);
                                            string varDescription = dtVariants.Rows.OfType<DataRow>().ToList().Where(y => y["VariantDescription"].ToString().Equals(x.Definition.Name)).Select(y => y["VariantDisplayName"].ToString()).FirstOrDefault().Replace("/", "-");
                                            //vdes = vdes + "/" + varDescription;
                                            if (!lstVariantDetails.Contains(varDescription))
                                                lstVariantDetails.Add(varDescription);
                                        }
                                    });
                                    lstVariantDetails.Sort();
                                    vdes = vdes + (vdes.Length > 0 ? "/" : string.Empty) + string.Join("/", lstVariantDetails);
                                    itemDescription = string.Empty;
                                    itemDescription = finst.Symbol.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Description")).Select(x => x.AsString()).FirstOrDefault().Replace("/", "-");

                                    group1 = string.Empty;
                                    var grp1 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 1", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group1 = grp1 != null ? grp1 : string.Empty;

                                    group2 = string.Empty;
                                    var grp2 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 2", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group2 = grp2 != null ? grp2 : string.Empty;

                                    group3 = string.Empty;
                                    var grp3 = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Group 3", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.AsString()).FirstOrDefault();
                                    group3 = grp3 != null ? grp3 : string.Empty;
                                    if (price == 0)
                                        price = Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
                                    else
                                        price = price + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
                                    double length = 0, depth = 0;
                                    if (itemName.Contains("JBG-"))
                                    {

                                        var parameter = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Length", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();

                                        var typeDepth = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Depth", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();

                                        if (parameter != null)
                                            length = Convert.ToDouble(parameter.AsValueString());

                                        //try
                                        //{
                                        //Element e = doc.GetElement(finst.Id);
                                        //ElementType type = doc.GetElement(e.GetTypeId()) as ElementType;
                                        //to get height of section
                                        //Parameter h = type.LookupParameter("Depth");
                                        if (typeDepth != null)
                                            depth = Convert.ToDouble(typeDepth.AsValueString());

                                        //}
                                        //catch { System.Windows.Forms.MessageBox.Show("Failed"); }
                                        //Family family = finst.Symbol.Family;
                                        //List<string> strL1 = family.Parameters.OfType<Parameter>().ToList().Select(x => x.Definition.Name).ToList();
                                        //System.Windows.Forms.MessageBox.Show(itemName + "-"+ string.Join(",", strL1));


                                        //var depthParam = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Depth", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();

                                        //if (depthParam != null)
                                        //    depth = Convert.ToInt32(depthParam.AsValueString());

                                        ftCalc = CalculateStandardFeet(length, depth, 1);
                                        price = price * CalculateStandardFeet(length, depth, 1);
                                        //cnt = Convert.ToInt32(ftCalc);

                                        //List<string> strL = finst.Parameters.OfType<Parameter>().ToList().Select(x => x.Definition.Name).ToList();
                                        //System.Windows.Forms.MessageBox.Show(itemName + "-" + length + "-" + depth + "-" + string.Join(",", strL));
                                    }
                                    else if (itemName.Contains("SK-"))
                                    {
                                        var depthParam = finst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Length", StringComparison.InvariantCultureIgnoreCase)).Select(x => x).FirstOrDefault();
                                        if (depthParam != null)
                                            depth = Convert.ToInt32(depthParam.AsValueString());

                                        ftCalc = CalculateRunningFeet(depth, 1);
                                        price = price * CalculateRunningFeet(depth, 1);
                                        //cnt = Convert.ToInt32(ftCalc);
                                    }
                                    if (!string.IsNullOrWhiteSpace(fd))
                                        dr["VariantCode"] = fd;
                                    else
                                    {
                                        //dr["VariantCode"] = cd + dd + md + hd + od;
                                        dr["VariantCode"] = md + hd + cd + dd + od;
                                    }
                                    if (!string.IsNullOrWhiteSpace(vdes) && (!string.IsNullOrWhiteSpace(hd) || !string.IsNullOrWhiteSpace(md) || !string.IsNullOrWhiteSpace(cd) || !string.IsNullOrWhiteSpace(dd) || !string.IsNullOrWhiteSpace(od) || !string.IsNullOrWhiteSpace(fd)))
                                        dr["VariantDescription"] = vdes;
                                    else
                                        dr["VariantDescription"] = string.Empty;


                                    //if (itemName.Contains("JBG-") || itemName.Contains("SK-"))
                                    //    System.Windows.Forms.MessageBox.Show(itemName + "-" + cnt + "-" + ftCalc);
                                    //familyName.ToString().Replace(".rfa", "") + @"/" +
                                    dtItems.Rows.Add(i.ToString(), itemName, dr["VariantCode"].ToString(), group1, group2, group3, itemDescription.Replace("/", "-") + @"/" + dr["VariantDescription"].ToString(), (itemName.Contains("SK-")) || itemName.Contains("JBG-") ? ftCalc : cnt, dr["UnitofMeasures"].ToString(), Convert.ToInt32(dr["Price in USD - P.Y"].ToString()), "", "", price.ToString(), "", dr["ItemPurchaseType"].ToString(), dr["AnnexureId"].ToString(), dr["SubGroup1"].ToString(), vdes);

                                    //dtItems.Rows.Add(i.ToString(), itemName, dr["VariantCode"].ToString(), group1, group2, group3, itemDescription + @"/" + dr["VariantDescription"].ToString(), cnt, dr["UnitofMeasures"].ToString(), price.ToString(), "", "", price.ToString(), "", dr["ItemPurchaseType"].ToString(), dr["AnnexureId"].ToString(), dr["SubGroup1"].ToString(), vdes);
                                    int sno = i;
                                    var param = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Accessories")).Select(x => x).FirstOrDefault();
                                    if (param != null)
                                    {

                                        var value = param.AsString();
                                        var values = value.Split(',');
                                        values.ToList().ForEach(x =>
                                        {
                                            //if (!x.Contains("JBG-") && !x.Contains("SK-"))
                                            //{
                                            var drAccess = dtProducts.Rows.OfType<DataRow>().ToList().Where(y => !string.IsNullOrWhiteSpace
                                            (x) && y["ItemCode"].ToString().Equals(x)).Select(y => y).FirstOrDefault();
                                            if (drAccess != null)
                                            {
                                                sno++;
                                                dtItems.Rows.Add(sno, x, "", group1, group2, group3, drAccess["ItemDescription"].ToString().Replace("/", "-"), cnt, drAccess["UnitofMeasures"].ToString(), drAccess["Price in USD - P.Y"].ToString(), "", "", drAccess["Price in USD - P.Y"].ToString(), "", drAccess["ItemPurchaseType"].ToString(), dr["AnnexureId"].ToString(), "");
                                            }
                                            // }
                                        });
                                    }

                                    hd = string.Empty; md = string.Empty; dd = string.Empty; cd = string.Empty; od = string.Empty; fd = string.Empty; vdes = string.Empty;
                                    i = sno;
                                }
                                //} // Cabinet Style-Door&Drawer style - MOC - HandleStyle - Other Variants

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
                return dtItems;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            return null;
        }

        private static double CalculateStandardFeet(double depth, double length, int noOfCount)
        {
            var resultTant = depth * length;
            resultTant = Math.Round(resultTant / 144, 2);
            resultTant = resultTant * noOfCount;
            return resultTant;
        }

        private static double CalculateRunningFeet(double length, int noOfCount)
        {
            var resultTant = Math.Round(length / 12, 2);
            resultTant = resultTant * noOfCount;
            return resultTant;
        }

        // private static void IncreaseQuantityForExistingFamilySet(DataTable dtItems, int cnt, string itemName, DataRow dr, DataRow drItemCode, int price, string grp1, string grp2, string grp3, ref int i, string itemDescription, string variantCode, string vdes)
        private static void IncreaseQuantityForExistingFamilySet(DataTable dtItems, double cnt, string itemName, DataRow dr, DataRow drItemCode, double price, string grp1, string grp2, string grp3, ref int i, string itemDescription, string variantCode, string vdes)
        {
            var drGroupExists = (from DataRow drGrp in dtItems.AsEnumerable()
                                 where drGrp["Group 1"].ToString().Equals(grp1) && drGrp["Group 2"].ToString().Equals(grp2) && drGrp["Group 3"].ToString().Equals(grp3) && drGrp["ItemCode"].ToString().Equals(itemName) && drGrp["VariantCode"].ToString().Equals(variantCode)
                                 select drGrp).FirstOrDefault();
            if (drGroupExists != null)
            {
                //'chocka - 06102017'
                //cnt = Convert.ToInt32(drGroupExists["Qty"].ToString());
                //cnt = cnt + 1;
                cnt += Convert.ToDouble(drGroupExists["Qty"].ToString());
                dtItems.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].Equals(itemName) && x["Group 1"].ToString().Equals(grp1) && x["Group 2"].ToString().Equals(grp2) && x["Group 3"].ToString().Equals(grp3) && x["VariantCode"].ToString().Equals(variantCode)).Select(x =>
                 {
                     bool isNotExisting = Extensions.CheckValidVariants(vdes, x);
                     //if (!isNotExisting)
                     {
                         //'chocka - 06102017'
                         //cnt = Convert.ToInt32(drGroupExists["Qty"].ToString());
                         //cnt = cnt + 1;
                         //cnt += Convert.ToInt32(drGroupExists["Qty"].ToString());
                         x["Qty"] = cnt.ToString();
                     }
                     return x;
                 }).ToList();
                //if (price == 0)
                //    price = price + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());//Convert.ToInt32(cnt * Convert.ToInt32(dr["Price in USD - P.Y"].ToString()));
                //else
                //    price = price + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());//Convert.ToInt32(cnt * Convert.ToInt32(dr["Price in USD - P.Y"].ToString()));


                //&& x["VariantCode"].Equals(variantCode)
                dtItems.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].Equals(itemName) && x["Group 1"].ToString().Equals(grp1) && x["Group 2"].ToString().Equals(grp2) && x["Group 3"].ToString().Equals(grp3)).Select(x =>
               {
                   bool isNotExisting = Extensions.CheckValidVariants(vdes, x);
                   //if (!isNotExisting)
                   {
                       int tot = Convert.ToInt32(drGroupExists["TotalUnitPrice"].ToString());
                       //tot = tot + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
                       x["TotalUnitPrice"] = Convert.ToInt32(price + tot).ToString();
                       //x["UnitPrice"] = Convert.ToInt32(price + tot).ToString();
                   }
                   return x;
               }).ToList();
                hd = string.Empty; md = string.Empty; dd = string.Empty; cd = string.Empty; od = string.Empty; fd = string.Empty;
            }
            else
            {
                AddRowForNewFamilySet(dtItems, itemDescription, grp1, grp2, grp3, ref i, itemName, dr, ref price, vdes, cnt);
            }
        }

        private static void AddRowForNewFamilySet(DataTable dtItems, string itemDescription, string group1, string group2, string group3, ref int i, string itemName, DataRow dr, ref double price, string vdes, double qty)
        {
            i++;
            //if (price == 0)
            //    price = Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
            //else
            //    price = Convert.ToInt32(qty) * Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
            //price = price * Convert.ToInt32(dr["Price in USD - P.Y"].ToString());

            //price = price + Convert.ToInt32(dr["Price in USD - P.Y"].ToString());
            if (!string.IsNullOrWhiteSpace(fd))
                dr["VariantCode"] = fd;
            else
            {
                dr["VariantCode"] = md + hd + cd + dd + od;
            }
            if (!string.IsNullOrWhiteSpace(vdes) && (!string.IsNullOrWhiteSpace(hd) || !string.IsNullOrWhiteSpace(md) || !string.IsNullOrWhiteSpace(cd) || !string.IsNullOrWhiteSpace(dd) || !string.IsNullOrWhiteSpace(od) || !string.IsNullOrWhiteSpace(fd)))
                dr["VariantDescription"] = vdes;
            else
                dr["VariantDescription"] = string.Empty;
            //familyName.ToString().Replace(".rfa", "") + @"/" +
            // dtItems.Rows.Add(i.ToString(), itemName, dr["VariantCode"].ToString(), group1, group2, group3, itemDescription + @"/" + dr["VariantDescription"].ToString(), 1, dr["UnitofMeasures"].ToString(), price.ToString(), "", "", price.ToString(), "", dr["ItemPurchaseType"].ToString(), dr["AnnexureId"].ToString(), dr["SubGroup1"].ToString(), vdes);

            dtItems.Rows.Add(i.ToString(), itemName, dr["VariantCode"].ToString(), group1, group2, group3, itemDescription.Replace("/", "-") + "/" + dr["VariantDescription"].ToString(), qty.ToString(), dr["UnitofMeasures"].ToString(), price.ToString(), "", "", price.ToString(), "", dr["ItemPurchaseType"].ToString(), dr["AnnexureId"].ToString(), dr["SubGroup1"].ToString(), vdes);
            hd = string.Empty; md = string.Empty; dd = string.Empty; cd = string.Empty; od = string.Empty; fd = string.Empty; vdes = string.Empty;
        }

        private static string hd, md, cd, dd, od;
        private static string fd;
        private static string vdes = string.Empty;
        private static void GenerateCode(string name, DataTable dtVar)
        {
            string category = dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["Category"].ToString()).FirstOrDefault();
            string variantCode = dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
            switch (category.ToLower())
            {
                case "handle style":
                    hd = hd + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    hd = string.Concat(hd.OrderBy(c => c));
                    break;
                case "moc":
                    md = md + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    md = string.Concat(md.OrderBy(c => c));
                    break;
                case "cabinet styles":
                    cd = cd + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    cd = string.Concat(cd.OrderBy(c => c));
                    break;
                case "door & drawer styles":
                    dd = dd + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    dd = string.Concat(dd.OrderBy(c => c));
                    break;
                case "cabinets-other variants":
                    od = od + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    od = string.Concat(od.OrderBy(c => c));
                    break;
                case "fume hood":
                    fd = fd + variantCode;// dtVar.Rows.OfType<DataRow>().Where(x => x["VariantDescription"].ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                    fd = string.Concat(fd.OrderBy(c => c));
                    break;
                default:
                    break;
            }
        }

        public static void UpdateParameters(string handleStyle, string moc, string door, string cbnt, string others, DataTable dtVariants, out string variantcode, out string itemcode, out string variantDes, List<ElementId> lstEleIds, string connstr, ref string varCode)
        {
            string mocCode = string.Empty;
            string handleCode = string.Empty;
            string cabCode = string.Empty;
            string DrawCode = string.Empty;
            string Lock = string.Empty;
            string otherCabinet = string.Empty;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            var otherVariants = others.Split(',').ToList();

            using (Transaction trans = new Transaction(doc, "Parameters Updation"))
            {
                trans.Start();
                foreach (ElementId eleId in lstEleIds)
                {
                    Element ele = doc.GetElement(eleId);
                    if (!(ele is FamilyInstance)) continue;
                    FamilyInstance famInst = (FamilyInstance)ele;
                    itemcode = famInst.Name;
                    Parameter handleParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Contains(handleStyle)).Select(x => x).FirstOrDefault();
                    if (handleParameter != null)
                    {
                        if (!handleParameter.IsReadOnly)
                        {
                            var values = handleParameter.AsValueString();
                            handleParameter.Set(1);
                            handleCode = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(handleStyle)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                        }

                    }
                    Parameter mocParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Contains(moc)).Select(x => x).FirstOrDefault();
                    if (mocParameter != null)
                    {
                        if (!mocParameter.IsReadOnly)
                        {
                            var values = mocParameter.AsValueString();
                            mocParameter.Set(1);
                            mocCode = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(moc)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                        }
                    }
                    Parameter doorParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Contains(door)).Select(x => x).FirstOrDefault();
                    if (doorParameter != null)
                    {
                        if (!doorParameter.IsReadOnly)
                        {
                            var values = doorParameter.AsValueString();
                            doorParameter.Set(1);
                            DrawCode = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(door)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                        }

                    }
                    Parameter cbntParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Contains(cbnt)).Select(x => x).FirstOrDefault();
                    if (cbntParameter != null)
                    {
                        if (!cbntParameter.IsReadOnly)
                        {
                            var values = cbntParameter.AsValueString();
                            cbntParameter.Set(1);
                            cabCode = dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(cbnt)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                        }
                    }
                    //Parameter otherParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Contains(others)).Select(x => x).FirstOrDefault();
                    otherVariants.ForEach(s =>
                    {
                        Parameter otherParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => s.Contains(x.Definition.Name)).Select(x => x).FirstOrDefault();
                        if (otherParameter != null)
                        {
                            if (!otherParameter.IsReadOnly)
                            {
                                var values = otherParameter.AsValueString();
                                otherParameter.Set(1);
                                otherCabinet = otherCabinet + dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(s)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                            }

                        }
                    });

                    if (!string.IsNullOrWhiteSpace(handleCode) || !string.IsNullOrWhiteSpace(mocCode) || !string.IsNullOrWhiteSpace(DrawCode) || !string.IsNullOrWhiteSpace(cabCode) || !string.IsNullOrWhiteSpace(otherCabinet))
                    {
                        variantcode = mocCode + handleCode + cabCode + DrawCode + otherCabinet;
                        variantDes = string.Empty;
                        variantDes += !string.IsNullOrWhiteSpace(mocCode) ? mocParameter.Definition.Name : string.Empty;
                        variantDes += "," + (!string.IsNullOrWhiteSpace(handleCode) ? handleParameter.Definition.Name : string.Empty);
                        variantDes += "," + (!string.IsNullOrWhiteSpace(cabCode) ? cbntParameter.Definition.Name : string.Empty);
                        variantDes += "," + (!string.IsNullOrWhiteSpace(DrawCode) ? doorParameter.Definition.Name : string.Empty);
                        variantDes += "," + (!string.IsNullOrWhiteSpace(otherCabinet) ? otherCabinet : string.Empty);
                    }
                    else
                    {
                        variantcode = string.Empty;
                        variantDes = string.Empty;
                    }
                    //UpdateVariantsForCabinets(variantcode, variantDes, itemcode, connstr);
                    varCode = varCode + variantcode;
                    variantDes = string.Empty;
                }
                trans.Commit();
                variantcode = string.Empty;
                variantDes = string.Empty;
                itemcode = string.Empty;
            }
        }

        private static void UpdateVariantsForCabinets(string variantCode, string variantDes, string itemCode, string connstr)
        {
            _connectionString = connstr;
            if (!string.IsNullOrWhiteSpace(variantCode))
            {
                using (var scn = new SqlConnection(_connectionString))
                {
                    scn.Open();
                    var query = "update ProductMasterNew SET VariantCode ='" + variantCode + "',VariantDescription='" + variantDes + "' where ItemCode='" + itemCode + "'";
                    using (var scmd = new SqlCommand(query, scn))
                    {
                        scmd.ExecuteNonQuery();
                    }
                }


            }
        }

        public static void UpdateParameters(string otherVariants, DataTable dtVariants, out string variantCode, out string itemcode, out string variantDes, List<ElementId> lstEleIds, string connstr, ref string varCode)
        {

            string vairantDescription = string.Empty;
            string otherCabinet = string.Empty;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            var otherVariants1 = otherVariants.Split(',').ToList();
            using (Transaction trans = new Transaction(doc, "Parameters Updation"))
            {
                trans.Start();
                foreach (ElementId eleId in lstEleIds)
                {
                    Element ele = doc.GetElement(eleId);
                    if (!(ele is FamilyInstance)) continue;
                    FamilyInstance famInst = (FamilyInstance)ele;
                    itemcode = famInst.Name;
                    otherVariants1.ForEach(s =>
                    {
                        Parameter otherParameter = famInst.Parameters.OfType<Parameter>().ToList().Where(x => s.Contains(x.Definition.Name)).Select(x => x).FirstOrDefault();
                        if (otherParameter != null)
                        {
                            if (!otherParameter.IsReadOnly)
                            {
                                var values = otherParameter.AsValueString();
                                try
                                {
                                    otherParameter.Set(1);
                                }
                                catch
                                {
                                }
                                otherCabinet = otherCabinet + dtVariants.Rows.OfType<DataRow>().ToList().Where(x => x["VariantDescription"].Equals(s)).Select(x => x["VariantCode"].ToString()).FirstOrDefault();
                                vairantDescription = vairantDescription + "," + otherParameter.Definition.Name;
                            }
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(otherCabinet))
                    {
                        variantCode = otherCabinet;
                        variantDes = string.Empty;
                        variantDes += vairantDescription;
                    }
                    else
                    {
                        variantCode = string.Empty;
                        variantDes = string.Empty;
                    }

                    //UpdateVariantsForCabinets(variantCode, variantDes, itemcode, connstr);
                    otherCabinet = string.Empty;
                    varCode = varCode + variantCode;
                }
                trans.Commit();

                variantCode = string.Empty;
                variantDes = string.Empty;
                itemcode = string.Empty;
            }
        }
    }

    public class Extensions
    {
        public static bool CheckValidVariants(string str1, List<DataRow> lstMatchingVariants)
        {
            var str1Array = str1.Split('/');
            var arrangedString = str1Array.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x).ToList();
            var lstStrings = new List<string>();
            foreach (DataRow dr in lstMatchingVariants)
            {
                if (string.IsNullOrWhiteSpace(dr["VariantDescription"].ToString()))
                    continue;
                var str2Array = dr["VariantDescription"].ToString().Split('/');
                arrangedString.ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x) && str2Array.Contains(x))
                    {
                        lstStrings.Add(x);
                    }
                });
                if (lstStrings.Count() == arrangedString.Count())
                    return false;
                lstStrings = new List<string>();
            }
            return true;
        }

        public static bool CheckValidVariants(string str1, DataRow dr)
        {
            if (string.IsNullOrWhiteSpace(str1))
                return false;
            var str1Array = str1.Split('/');
            var arrangedString = str1Array.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x).ToList();
            var lstStrings = new List<string>();

            if (!string.IsNullOrWhiteSpace(dr["VariantDescription"].ToString()))
            {
                var str2Array = dr["VariantDescription"].ToString().Split('/');
                arrangedString.ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x) && str2Array.Contains(x))
                    {
                        lstStrings.Add(x);
                    }
                });
                if (lstStrings.Count() == arrangedString.Count())
                    return false;
                lstStrings = new List<string>();
            }
            return true;
        }
    }
}
