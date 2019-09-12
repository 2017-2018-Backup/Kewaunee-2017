using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kewaunee;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
namespace AddIn
{

    class RvtCommands
    {
    }



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateProject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonCreateProject.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null || !fetre.isEdit)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }

            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;
            ClsProperties.UIApplication = uiapp;
            if (!string.IsNullOrWhiteSpace(doc.PathName))
            {
                MessageBox.Show("Please close the existing project and open the new project template", "Create Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                var projectCreation = new ProjectCreation(true, fetre.isRead, fetre.isEdit, fetre.isDelete);
                projectCreation.ShowDialog();

                if (UIInputs.WallLength != 0 || UIInputs.WallWidth != 0 || UIInputs.TrueCeilingHeight != 0 || UIInputs.FalseCielingHeight != 0)
                {
                    CreateWall(app, doc, UIInputs.WallLength, UIInputs.WallWidth, UIInputs.TrueCeilingHeight, UIInputs.FalseCielingHeight);
                    UIInputs.WallLength = 0;
                    UIInputs.WallWidth = 0;
                    UIInputs.TrueCeilingHeight = 0;
                    UIInputs.FalseCielingHeight = 0;
                }
                if (!string.IsNullOrWhiteSpace(UIInputs.ProjectPath) && !string.IsNullOrWhiteSpace(UIInputs.ProjectName))
                    SaveDocument(doc, uiapp);
            }
            return Result.Succeeded;
        }

        private void SaveDocument(Document doc, UIApplication uiapp)
        {
            string path = doc.PathName;
            if (File.Exists(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + ".rvt")))
                File.Delete(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + ".rvt"));
            if (Directory.Exists(UIInputs.ProjectPath))
                doc.SaveAs(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + ".rvt"));
            else
            {
                Directory.CreateDirectory(UIInputs.ProjectPath);
                doc.SaveAs(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + ".rvt"));
            }
            uiapp.OpenAndActivateDocument(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + ".rvt"));
            UIInputs.ProjectName = string.Empty;
            UIInputs.ProjectPath = string.Empty;
            // UIInputs.ProjectNo = string.Empty;
        }

        private void CreateWall(Autodesk.Revit.ApplicationServices.Application app, Document doc, int wallLength, int wallWidth, int trueHeight, int falseHeight)
        {
            if (wallLength == 0) return;
            var filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(WallType));
            IList<WallType> m_wallTypeCollection = filteredElementCollector.Cast<WallType>().ToList<WallType>();

            var collector = new FilteredElementCollector(doc);
            ICollection<Element> collection = collector.OfClass(typeof(Level)).ToElements();

            // Get the level which will be used in create method
            var m_level = collector.OfClass(typeof(Level)).FirstElement() as Level;

            Line l1 = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(wallWidth, 0, 0));
            Line l2 = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, wallLength, 0));
            Line l3 = Line.CreateBound(new XYZ(wallWidth, 0, 0), new XYZ(wallWidth, wallLength, 0));
            Line l4 = Line.CreateBound(new XYZ(0, wallLength, 0), new XYZ(wallWidth, wallLength, 0));
            double offset = 0;
            WallType wtype = null;
            foreach (WallType wallType in m_wallTypeCollection)
            {
                if (wallType.FamilyName.Equals("Basic Wall") && wallType.Name.Equals("Generic - 200mm"))
                {
                    wtype = wallType;
                }
            }
            var trans = new Transaction(doc, "Create Wall");
            trans.Start();
            wtype = wtype == null ? m_wallTypeCollection[0] : wtype;
            // Wall wall = Wall.Create(doc, baseline, m_wallTypeCollection[0].Id, m_level.Id, wallWidth, 0, false, false);
            Wall.Create(doc, l1, wtype.Id, m_level.Id, trueHeight, offset, true, true);
            Wall.Create(doc, l2, wtype.Id, m_level.Id, trueHeight, offset, true, true);
            Wall.Create(doc, l3, wtype.Id, m_level.Id, trueHeight, offset, true, true);
            Wall.Create(doc, l4, wtype.Id, m_level.Id, trueHeight, offset, true, true);
            trans.Commit();
        }


    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OpenProject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonOpenProject.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null || !fetre.isEdit)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                ProjectCreation projectCreation = new ProjectCreation(false, fetre.isRead, fetre.isEdit, fetre.isDelete);
                projectCreation.ShowDialog();
                if (!string.IsNullOrWhiteSpace(UIInputs.ProjectPath))
                {
                    if (File.Exists(UIInputs.ProjectPath))
                    {
                        var uiapp = commandData.Application;
                        var uidoc = uiapp.ActiveUIDocument;
                        var app = uiapp.Application;
                        var doc = uidoc.Document;
                        Properties.ElementSet = elements;
                        Properties.ExternalCommandData = commandData;
                        Properties.doc = doc;
                        ClsProperties.UIApplication = uiapp;
                        uiapp.OpenAndActivateDocument(UIInputs.ProjectPath);
                    }
                }
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class CustomerMaster : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;
            ClsProperties.UIApplication = uiapp;
            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonCustomerMaster.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                Customer customer = new Customer(fetre.isRead, fetre.isEdit, fetre.isDelete);
                customer.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class UserProfileMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonUserProfile.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                UserProfile userProfile = new UserProfile(fetre.isRead, fetre.isEdit, fetre.isDelete);
                userProfile.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class LoginMaster : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonLoginMaster.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                LoginCreation LoginCreation = new LoginCreation(fetre.isRead, fetre.isEdit, fetre.isDelete);
                LoginCreation.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class TaxCreation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonTaxMaster.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                TaxMaster taxMaster = new TaxMaster(fetre.isRead, fetre.isEdit, fetre.isDelete);
                taxMaster.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowHide : IExternalCommand
    {

        #region Show or Hide Dockable Pane
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            ClsProperties.UIApplication = uiapp;
            ClsProperties.DockablePaneGuid = DockConstants.Id;
            Properties.ExternalCommandData = commandData;
            Properties.ElementSet = elements;
            if (Properties.showhide == true)
            {
                var dPid = new DockablePaneId(DockConstants.Id);
                DockablePane pane = uiapp.GetDockablePane(dPid);
                pane.Hide();
                Properties.showhide = false;
            }
            else
            {
                var dPid = new DockablePaneId(DockConstants.Id);
                DockablePane pane = uiapp.GetDockablePane(dPid);
                pane.Show();
                Properties.showhide = true;
            }
            return Result.Succeeded;
        }
        #endregion
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ProjectsCatalog : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonPrpjCatalog.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                ProjectsCatalogue catalog = new ProjectsCatalogue(fetre.isRead, fetre.isEdit, fetre.isDelete);
                catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CurrencyMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonCurrencyCatalog.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                Currency catalog = new Currency(fetre.isRead, fetre.isEdit, fetre.isDelete);
                catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class VariantsMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonVariants.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                VariantMaster catalog = new VariantMaster(fetre.isRead, fetre.isEdit, fetre.isDelete);
                catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PricingMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonPricing.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                Pricing catalog = new Pricing(fetre.isRead, fetre.isEdit, fetre.isDelete);
                catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ProductMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonProductMaster.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                Products catalog = new Products(fetre.isRead, fetre.isEdit, fetre.isDelete);
                catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SalesQuoteMaster : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;

            Features fetre = (from ftre in ProfileDescription.FeaturesName
                              where ftre.FeaturesName.Equals(Properties.pushButtonSalesQuoteMaster.Name)
                              select ftre).FirstOrDefault();
            if (fetre == null)
            {
                MessageBox.Show("User does not have access. Please contact administrator.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return Result.Succeeded;
            }
            if (ClsProperties.isClosed)
            {
                ClsProperties.isSalesClosed = false;
                SalesQuote catalog = new SalesQuote(fetre.isRead, fetre.isEdit, fetre.isDelete);
                if (!ClsProperties.isSalesClosed)
                    catalog.ShowDialog();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UserLogin : IExternalCommand
    {
        public static string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string assyPath = Path.Combine(dir, "AddIn.dll");
        private static int id { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;
            Properties.ElementSet = elements;
            Properties.ExternalCommandData = commandData;
            Properties.doc = doc;
            ClsProperties.UIApplication = uiapp;
            ClsProperties.commandData = commandData;
            ProfileDescription.Profile = 0;
            ProfileDescription.FeaturesName = new List<Features>();
            ClsProperties clsProp = new ClsProperties();
            UIInputs uiinputs = new UIInputs();

            if (Properties.pushButtonLogin.ItemText.Equals("Login"))
            {

                Login login = new Login();
                login.ShowDialog();
                if (ClsProperties.isLoginSuccess)
                {
                    /*id = InsertLogInfromation(GetUserId(UIInputs.Username), (new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day)).ToString(),"")*/
                    ;
                    id = InsertLogInfromation(GetUserId(UIInputs.Username), DateTime.Now.ToString("yyyy/MM/dd HH:mm"), "");
                    Properties.ribbonPanel.Title = "Logout";
                    CreateCommandButtons();
                    SetPropertiesToCommandButtons(true);
                    Properties.pushButtonLogin.ItemText = "Logout";
                    Uri shwimg1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/logout64x64.ico");
                    BitmapImage shwlrg1 = new BitmapImage(shwimg1);
                    Uri shwsml1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/logout32x32.ico");
                    BitmapImage showsmall1 = new BitmapImage(shwsml1);
                    Properties.pushButtonLogin.LargeImage = showsmall1;
                    Properties.pushButtonLogin.ToolTipImage = shwlrg1;
                    Properties.ribbonPanelProjects.Visible = true;

                }
            }
            else
            {
                UpdateLogoutTime(id);
                Properties.ribbonPanel.Title = "Login";
                SetPropertiesToCommandButtons(false);
                Properties.pushButtonLogin.ItemText = "Login";
                Uri shwimg1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/login64x64.ico");
                BitmapImage shwlrg1 = new BitmapImage(shwimg1);
                Uri shwsml1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/login32x32.ico");
                BitmapImage showsmall1 = new BitmapImage(shwsml1);
                Properties.pushButtonLogin.LargeImage = showsmall1;
                Properties.pushButtonLogin.ToolTipImage = shwlrg1;
                IList<RibbonItem> ribbonItems = Properties.ribbonPanel.GetItems();
                Properties.ribbonPanelProjects.Visible = false;
                ClsProperties.isLoginSuccess = false;
            }

            var dPid = new DockablePaneId(DockConstants.Id);
            DockablePane pane = Properties.uiControlledApp.GetDockablePane(dPid);
            pane.Hide();
            Properties.showhide = false;

            return Result.Succeeded;
        }

        private int GetUserId(string username)
        {
            using (var scn = new SqlConnection(ClsProperties.connectionString))
            {
                scn.Open();
                var query = "select UserId from LoginCreation where Username ='" + username + "'";
                using (SqlCommand scmd = new SqlCommand(query, scn))
                {
                    var dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dt.Rows[0][0].ToString());
                }
            }
        }

        private int InsertLogInfromation(int userId, string loginTime, string logoutTime)
        {
            using (var scn = new SqlConnection(ClsProperties.connectionString))
            {
                scn.Open();
                var query = "insert into LogInformation(UserId,LoginTime,LogoutTime) values (" + userId + ",'" + loginTime + "','" + logoutTime + "') select Id from LogInformation where UserId = " + userId + " and LoginTime='" + loginTime + "'";
                using (var scmd = new SqlCommand(query, scn))
                {
                    DataTable dt = new DataTable();
                    dt.Load(scmd.ExecuteReader());
                    return Convert.ToInt32(dt.Rows[0]["Id"].ToString());
                }
            }
        }

        private void UpdateLogoutTime(int id)
        {
            using (var scn = new SqlConnection(ClsProperties.connectionString))
            {
                scn.Open();
                //var query = "update LogInformation SET LogoutTime='" + (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).ToString() + "' where Id =" + id + "";
                var query = "update LogInformation SET LogoutTime='getdate()' where Id =" + id + "";
                using (var scmd = new SqlCommand(query, scn))
                {
                    scmd.ExecuteNonQuery();
                }
            }
        }

        private static void SetPropertiesToCommandButtons(bool isVisible)
        {
            Properties.pushButtonDockPanel.Visible = isVisible;
            Properties.pushButtonCreateProject.Visible = isVisible;
            Properties.pushButtonCustomerMaster.Visible = isVisible;
            Properties.pushButtonLoginMaster.Visible = isVisible;
            Properties.pushButtonOpenProject.Visible = isVisible;
            Properties.pushButtonPrpjCatalog.Visible = isVisible;
            Properties.pushButtonUserProfile.Visible = isVisible;
            Properties.pushButtonTaxMaster.Visible = isVisible;
            Properties.pushButtonCurrencyCatalog.Visible = isVisible;
            Properties.pushButtonVariants.Visible = isVisible;
            Properties.pushButtonPricing.Visible = isVisible;
            Properties.pushButtonProductMaster.Visible = isVisible;
            Properties.pushButtonSalesQuoteMaster.Visible = isVisible;
            Properties.pushButtonTagging.Visible = isVisible;
        }

        private void CreateCommandButtons()
        {
            if (Properties.isFirstLogin)
            {
                var ribbonPanelProjects = Properties.controlledApp.CreateRibbonPanel("Kewaunee", "Features");
                Properties.ribbonPanelProjects = ribbonPanelProjects;

                var pushButtonUserProfile = Properties.ribbonPanelProjects.AddItem(new PushButtonData("UserProfile", "User Profile", assyPath, "AddIn.UserProfileMaster")) as PushButton;
                pushButtonUserProfile.ToolTip = "User Profile";
                Uri newuriprof = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/user_access64.ico");
                BitmapImage newlrg1prof = new BitmapImage(newuriprof);
                Uri newsml1prof = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/user_access32.ico");
                BitmapImage newsmallprof = new BitmapImage(newsml1prof);
                pushButtonUserProfile.LargeImage = newsmallprof;
                pushButtonUserProfile.ToolTipImage = newlrg1prof;
                Properties.pushButtonUserProfile = pushButtonUserProfile;
                pushButtonUserProfile.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonDockPanel = Properties.ribbonPanelProjects.AddItem(new PushButtonData("ShowHide", "Show/Hide" + Environment.NewLine + "Panel", assyPath, "AddIn.ShowHide")) as PushButton;
                pushButtonDockPanel.ToolTip = "Show/Hide Panel";
                Uri shwimg = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/64k.ico");
                BitmapImage shwlrg = new BitmapImage(shwimg);
                Uri shwsml = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/32k.ico");
                BitmapImage showsmall = new BitmapImage(shwsml);
                pushButtonDockPanel.LargeImage = showsmall;
                pushButtonDockPanel.ToolTipImage = shwlrg;
                pushButtonDockPanel.Visible = false;
                Properties.pushButtonDockPanel = pushButtonDockPanel;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonCreateProject = Properties.ribbonPanelProjects.AddItem(new PushButtonData("CreateProject", "New Project", assyPath, "AddIn.CreateProject")) as PushButton;
                pushButtonCreateProject.ToolTip = "Create Project";
                Uri newuri1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/64newprj.ico");
                BitmapImage newlrg1 = new BitmapImage(newuri1);
                Uri newsml1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/32newprj.ico");
                BitmapImage newsmall = new BitmapImage(newsml1);
                pushButtonCreateProject.LargeImage = newsmall;
                pushButtonCreateProject.ToolTipImage = newlrg1;
                pushButtonCreateProject.Visible = false;
                Properties.pushButtonCreateProject = pushButtonCreateProject;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonOpenProject = Properties.ribbonPanelProjects.AddItem(new PushButtonData("OpenProject", "Open Project", assyPath, "AddIn.OpenProject")) as PushButton;
                pushButtonOpenProject.ToolTip = "Open Project";
                Uri exsuri1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/64exsprj.ico");
                BitmapImage exslrg1 = new BitmapImage(exsuri1);
                Uri exssml1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/32exsprj.ico");
                BitmapImage exssmall = new BitmapImage(exssml1);
                pushButtonOpenProject.LargeImage = exssmall;
                pushButtonOpenProject.ToolTipImage = exslrg1;
                pushButtonOpenProject.Visible = false;
                Properties.pushButtonOpenProject = pushButtonOpenProject;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonCustomerMaster = Properties.ribbonPanelProjects.AddItem(new PushButtonData("CustomerMaster", "Customer Master", assyPath, "AddIn.CustomerMaster")) as PushButton;
                pushButtonCustomerMaster.ToolTip = "Customer Master";
                Uri uriCustMast = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/customers64.ico");
                BitmapImage bmpCustLarge = new BitmapImage(uriCustMast);
                Uri uriCustMast1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/customers32.ico");
                BitmapImage bmpCustMaster = new BitmapImage(uriCustMast1);
                pushButtonCustomerMaster.LargeImage = bmpCustMaster;
                pushButtonCustomerMaster.ToolTipImage = bmpCustLarge;
                Properties.pushButtonCustomerMaster = pushButtonCustomerMaster;
                pushButtonCustomerMaster.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonLoginMaster = Properties.ribbonPanelProjects.AddItem(new PushButtonData("LoginMaster", "Login Master", assyPath, "AddIn.LoginMaster")) as PushButton;
                pushButtonLoginMaster.ToolTip = "Login Creation";
                Uri uriLoginMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/Login64.ico");
                BitmapImage bmpLogLarge = new BitmapImage(uriLoginMaster);
                Uri uriLogLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/Login32.ico");
                BitmapImage bmpLogMaster = new BitmapImage(uriLogLarge);
                pushButtonLoginMaster.LargeImage = bmpLogMaster;
                pushButtonLoginMaster.ToolTipImage = bmpLogLarge;
                Properties.pushButtonLoginMaster = pushButtonLoginMaster;
                pushButtonLoginMaster.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonTaxMaster = Properties.ribbonPanelProjects.AddItem(new PushButtonData("TaxMaster", "Tax Master", assyPath, "AddIn.TaxCreation")) as PushButton;
                pushButtonTaxMaster.ToolTip = "Tax Master";
                Uri uriTaxMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/tax-icon-25.ico");
                BitmapImage bmpTaxLarge = new BitmapImage(uriTaxMaster);
                Uri uriTaxLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/tax-icon-25.ico");
                BitmapImage bmpTaxMaster = new BitmapImage(uriTaxLarge);
                pushButtonTaxMaster.LargeImage = bmpTaxMaster;
                pushButtonTaxMaster.ToolTipImage = bmpTaxLarge;
                Properties.pushButtonTaxMaster = pushButtonTaxMaster; pushButtonTaxMaster.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonPrpjCatalog = Properties.ribbonPanelProjects.AddItem(new PushButtonData("ProjectsCatalog", "Projects Catalog", assyPath, "AddIn.ProjectsCatalog")) as PushButton;
                pushButtonPrpjCatalog.ToolTip = "Projects Catalog";
                Uri uriProjMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/projects-catalog64.ico");
                BitmapImage bmpProjLarge = new BitmapImage(uriProjMaster);
                Uri uriProjLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/projects-catalog32.ico");
                BitmapImage bmpProjMaster = new BitmapImage(uriProjLarge);
                pushButtonPrpjCatalog.LargeImage = bmpProjMaster;
                pushButtonPrpjCatalog.ToolTipImage = bmpProjLarge;
                Properties.pushButtonPrpjCatalog = pushButtonPrpjCatalog;
                pushButtonPrpjCatalog.Visible = false;
                Properties.isFirstLogin = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonCurrencyCatalog = Properties.ribbonPanelProjects.AddItem(new PushButtonData("CurrencyMaster", "Currency Master", assyPath, "AddIn.CurrencyMaster")) as PushButton;
                pushButtonPrpjCatalog.ToolTip = "Currency";
                Uri uriCurrencyMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/pricing64.ico");
                BitmapImage bmpCurrencyLarge = new BitmapImage(uriCurrencyMaster);
                Uri uriCurrencyLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/pricing32.ico");
                BitmapImage bmpCurrencyMaster = new BitmapImage(uriCurrencyLarge);
                pushButtonCurrencyCatalog.LargeImage = bmpCurrencyMaster;
                pushButtonCurrencyCatalog.ToolTipImage = bmpCurrencyLarge;
                Properties.pushButtonCurrencyCatalog = pushButtonCurrencyCatalog;
                pushButtonCurrencyCatalog.Visible = false;
                Properties.isFirstLogin = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonVariants = Properties.ribbonPanelProjects.AddItem(new PushButtonData("VariantsMaster", "Variants Master", assyPath, "AddIn.VariantsMaster")) as PushButton;
                pushButtonVariants.ToolTip = "Variants";
                Uri uriVarMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/tax-icon-25.ico");
                BitmapImage bmpVarLarge = new BitmapImage(uriVarMaster);
                Uri uriVarLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/tax-icon-25.ico");
                BitmapImage bmpVarMaster = new BitmapImage(uriVarLarge);
                pushButtonVariants.LargeImage = bmpVarMaster;
                pushButtonVariants.ToolTipImage = bmpVarLarge;
                Properties.pushButtonVariants = pushButtonVariants;
                pushButtonVariants.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonPricing = Properties.ribbonPanelProjects.AddItem(new PushButtonData("PricingMaster", "Pricing Master", assyPath, "AddIn.PricingMaster")) as PushButton;
                pushButtonPricing.ToolTip = "Pricing";
                Uri uriPricingMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/pricing64.ico");
                BitmapImage bmpPricingLarge = new BitmapImage(uriPricingMaster);
                Uri uriPricingLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/pricing32.ico");
                BitmapImage bmpPricingMaster = new BitmapImage(uriPricingLarge);
                pushButtonPricing.LargeImage = bmpPricingMaster;
                pushButtonPricing.ToolTipImage = bmpPricingLarge;
                Properties.pushButtonPricing = pushButtonPricing;
                pushButtonPricing.Visible = false;
                pushButtonPricing.Enabled = false;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonProductMaster = Properties.ribbonPanelProjects.AddItem(new PushButtonData("ProductMaster", "Products Master", assyPath, "AddIn.ProductMaster")) as PushButton;
                pushButtonProductMaster.ToolTip = "Product Master";
                Uri uriProductMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/products64.ico");
                BitmapImage bmpProductLarge = new BitmapImage(uriProductMaster);
                Uri uriProductLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/products32.ico");
                BitmapImage bmpProductMaster = new BitmapImage(uriProductLarge);
                pushButtonProductMaster.LargeImage = bmpProductMaster;
                pushButtonProductMaster.ToolTipImage = bmpProductLarge;
                Properties.pushButtonProductMaster = pushButtonProductMaster;
                pushButtonPricing.Visible = false;

                Properties.ribbonPanelProjects.AddSeparator();


                var pushButtonTagging = Properties.ribbonPanelProjects.AddItem(new PushButtonData("Tagging", "Tagging", assyPath, "AddIn.ElementTagging")) as PushButton;
                pushButtonTagging.ToolTip = "Tagging";
                Uri uriTagging = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/Tag64.ico");
                BitmapImage bmpTagging = new BitmapImage(uriTagging);
                Uri uriLargeTagging = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/Tag32.ico");
                BitmapImage bmpLargeTag = new BitmapImage(uriLargeTagging);
                pushButtonTagging.LargeImage = bmpLargeTag;
                pushButtonTagging.ToolTipImage = bmpTagging;
                Properties.pushButtonTagging = pushButtonTagging;

                Properties.ribbonPanelProjects.AddSeparator();

                var pushButtonSalesQuoteMaster = Properties.ribbonPanelProjects.AddItem(new PushButtonData("SalesQuoteMaster", "Sales Quote", assyPath, "AddIn.SalesQuoteMaster")) as PushButton;
                pushButtonSalesQuoteMaster.ToolTip = "Sales Quote";
                Uri urisalesMaster = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/sales64.ico");
                BitmapImage bmpsalesLarge = new BitmapImage(urisalesMaster);
                Uri urisalesLarge = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Kewaunee/Revit/Resources/sales32.ico");
                BitmapImage bmpsalesMaster = new BitmapImage(urisalesLarge);
                pushButtonSalesQuoteMaster.LargeImage = bmpsalesMaster;
                pushButtonSalesQuoteMaster.ToolTipImage = bmpsalesLarge;
                Properties.pushButtonSalesQuoteMaster = pushButtonSalesQuoteMaster;





                Properties.isFirstLogin = false;
            }
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DownloadFamily : IExternalCommand
    {
        FamilySymbol symbol1;
        string FamilyPath = ClsProperties.FamilyPath;
        string Familyname = System.IO.Path.GetFileNameWithoutExtension(ClsProperties.FamilyPath);
        string fhoodcode = string.Empty;
        Family familynew;
        #region Loading Families and Architecture Items
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            FilteredElementCollector a = new FilteredElementCollector(doc).OfClass(typeof(Family));

            Family family = a.FirstOrDefault<Element>(e => e.Name.Equals(Familyname)) as Family;




            //Modified by Balaji A on 01 Jun, 2015 - Only for 2016 version 
            try
            {
                if (null == family)
                {

                    if (!File.Exists(FamilyPath))
                    {
                        MessageBox.Show("Selected family not exists in Library.", "Efficax", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return Result.Failed;
                    }

                    // Load family from file:

                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Load Family");
                        var loadOptions = new LoadFamilyOption();
                        doc.LoadFamily(FamilyPath, loadOptions, out family);
                        tx.Commit();
                    }

                }


                FamilySymbol symbol;

                symbol = null;


                int k = 0;
                string append = "";
                foreach (ElementId eleid in family.GetFamilySymbolIds())
                {
                    Element ele = doc.GetElement(eleid);
                    FamilySymbol s = ele as FamilySymbol;
                    append = append + ";" + s.Name;
                    if (s.Name.Trim() == ClsProperties.PartCode.Trim())
                    {
                        symbol = s;

                        break;
                    }
                }

                // MessageBox.Show(append);
                // MessageBox.Show(Familyname);
                if (symbol == null)
                {
                    MessageBox.Show("Selected type was not available in family.", "Kewaunee", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Succeeded;
                }
                uidoc.PromptForFamilyInstancePlacement(symbol);


                var lstFamilyInstance = new List<Element>();
                if (Properties.AddedElementIds.Any())
                {
                    Properties.AddedElementIds.ForEach(x =>
                     {
                         var element = doc.GetElement(x);
                         var pc = string.Empty;
                         if (element != null && !string.IsNullOrWhiteSpace(element.Name))
                         {
                             try
                             {
                                 pc = element.Name.Substring(0, 4);
                             }
                             catch
                             {
                                 pc = string.Empty;
                             }

                             var familyCategory = ClsProperties.dtProducts.Rows.OfType<DataRow>().ToList().Where(y => y["ItemCode"].ToString().StartsWith(pc)).Select(y => y["SubGroup1"].ToString()).FirstOrDefault();

                             if (!string.IsNullOrWhiteSpace(familyCategory) && (familyCategory.Equals("Bench Hoods") || familyCategory.Equals("Canopy Hoods")))
                             {
                                 lstFamilyInstance.Add(element);
                             }
                         }
                     });
                }

                if (Properties.AddedElementIds.Any())
                {
                    Properties.AddedElementIds.ForEach(x =>
                     {
                         Transaction transUpdateParam = new Transaction(doc, "Upate Param");
                         transUpdateParam.Start();
                         var element = doc.GetElement(x);
                         if (element != null)
                         {
                             var accessories = element.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Accessories")).Select(y => y).FirstOrDefault();
                             if (accessories != null)
                                 accessories.Set("");

                             var grp1 = element.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 1")).Select(y => y).FirstOrDefault();
                             if (grp1 != null)
                                 grp1.Set("");

                             var grp2 = element.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 2")).Select(y => y).FirstOrDefault();
                             if (grp2 != null)
                                 grp2.Set("");

                             var grp3 = element.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 3")).Select(y => y).FirstOrDefault();
                             if (grp3 != null)
                                 grp3.Set("");
                         }
                         transUpdateParam.Commit();
                     });
                }


                if (lstFamilyInstance != null)
                {
                    lstFamilyInstance.ForEach(x =>
                    {
                        if ((x is FamilyInstance))
                        {
                            FamilyInstance famInst = (FamilyInstance)x;
                            if (famInst != null)
                            {
                                var param = famInst.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Offset")).Select(y => y).FirstOrDefault();
                                if (param != null)
                                {
                                    StorageType type = param.StorageType;
                                    if (type == StorageType.Double)
                                    {
                                        using (Transaction tx = new Transaction(doc))
                                        {
                                            tx.Start("Change Offset");
                                            double doffset = UnitUtils.Convert(35.748, DisplayUnitType.DUT_DECIMAL_INCHES, DisplayUnitType.DUT_DECIMAL_FEET);
                                            param.Set(doffset);
                                            tx.Commit();

                                        }
                                    }
                                }
                            }
                        }
                    });


                }
                ClsProperties.LstElementIds = new List<ElementId>();
                ClsProperties.LstElementIds = Properties.AddedElementIds;
                FilteredElementCollector ftElements = new FilteredElementCollector(doc);
                Element elem = ftElements.OfClass(typeof(FamilyInstance)).Where(X => X.Name.Equals(symbol.Name)).Select(X => X).FirstOrDefault();
                if (elem != null)
                {
                    FamilyInstance famInst = (FamilyInstance)elem;
                    ClsProperties.familySymbol = famInst;
                    if (famInst != null)
                    {
                        //var isFumeHoodOrCabinet = ClsProperties.dtProducts.Rows.OfType<DataRow>().ToList().Where(x => x["ItemCode"].ToString().Equals(famInst.Name)).Select(x => x["SubGroup1"].ToString()).FirstOrDefault();
                        var isFumeHoodOrCabinet = Kewaunee.DockablePage.SubGroup1;
                        if (isFumeHoodOrCabinet.Contains("Bench Hoods") || isFumeHoodOrCabinet.Contains("Walk-in Hoods") || isFumeHoodOrCabinet.Contains("Fume Hood Accessories") || isFumeHoodOrCabinet.Contains("Fumehood Accessories") || isFumeHoodOrCabinet.Contains("Canopy Hoods") || isFumeHoodOrCabinet.Contains("Miscellaneous Hoods") || isFumeHoodOrCabinet.Contains("Fans & Fan Accessories") || isFumeHoodOrCabinet.Contains("Distillation Hood"))
                        {

                            ClsProperties.LstElementIds = new List<ElementId>();
                            lstFamilyInstance.ForEach(x => { ClsProperties.LstElementIds.Add(x.Id); });
                            Kewaunee.FumeHoodVariants obj = new FumeHoodVariants(doc);
                            obj.ShowDialog();
                        }
                        else
                        {
                            Kewaunee.Parameters obj = new Kewaunee.Parameters(doc);
                            obj.ShowDialog();
                        }
                    }
                }
                Properties.AddedElementIds.Clear();

            }
            catch (Exception ex)
            {
                Properties.AddedElementIds.Clear();
            }
            return Result.Succeeded;
        }
        #endregion
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ElementTagging : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            var selectedElementIDs = uidoc.Selection.GetElementIds();
            var dictGroup1 = new Dictionary<string, ElementId>();
            var dictGroup2 = new Dictionary<string, ElementId>();
            var dictGroup3 = new Dictionary<string, ElementId>();
            if (selectedElementIDs.Any())
            {
                string grp1 = string.Empty, grp2 = string.Empty, grp3 = string.Empty;
                selectedElementIDs.OfType<ElementId>().ToList().ForEach(x =>
                {
                    var element = doc.GetElement(x);
                    if (element != null && element is FamilyInstance)
                    {
                        var famInst = (FamilyInstance)element;
                        var value = famInst.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 1")).Select(y => y.AsString()).FirstOrDefault();
                        value = value == null ? "Empty" : value;
                        if (!dictGroup1.ContainsKey(value.ToString()))
                        {
                            dictGroup1.Add(value, x);
                        }
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (string.IsNullOrWhiteSpace(grp1))
                            { grp1 = value; }
                            else
                            {
                                if (grp1 == value) { }
                                else { grp1 = string.Empty; }
                            }
                        }
                        var value1 = famInst.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 2")).Select(y => y.AsString()).FirstOrDefault();
                        value1 = value1 == null ? "Empty" : value1;
                        if (!dictGroup2.ContainsKey(value1.ToString()))
                        {
                            dictGroup2.Add(value1, x);
                        }
                        if (!string.IsNullOrWhiteSpace(value1))
                        {

                            if (string.IsNullOrWhiteSpace(grp2))
                            { grp2 = value1; }
                            else
                            {
                                if (grp2 == value1) { }
                                else { grp2 = string.Empty; }
                            }
                        }
                        var value2 = famInst.Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 3")).Select(y => y.AsString()).FirstOrDefault();
                        value2 = value2 == null ? "Empty" : value2;
                        if (!dictGroup3.ContainsKey(value2.ToString()))
                        {
                            dictGroup3.Add(value2, x);
                        }
                        if (!string.IsNullOrWhiteSpace(value2))
                        {

                            if (string.IsNullOrWhiteSpace(grp3))
                            { grp3 = value2; }
                            else
                            {
                                if (grp3 == value2) { }
                                else { grp3 = string.Empty; }
                            }
                        }
                    }
                });

                grp1 = dictGroup1.Count == 0 ? string.Empty : dictGroup1.ContainsKey("Empty") ? string.Empty : dictGroup1.Keys.Count == 1 ? dictGroup1.Keys.First() : string.Empty;
                grp2 = dictGroup2.Count == 0 ? string.Empty : dictGroup2.ContainsKey("Empty") ? string.Empty : dictGroup2.Keys.Count == 1 ? dictGroup2.Keys.First() : string.Empty;
                grp3 = dictGroup3.Count == 0 ? string.Empty : dictGroup2.ContainsKey("Empty") ? string.Empty : dictGroup3.Keys.Count == 1 ? dictGroup3.Keys.First() : string.Empty;

                bool isEnableGroup2 = dictGroup1.Count > 1 ? false : dictGroup1.ContainsKey("Empty") ? true : dictGroup1.Count == 0 || dictGroup1.Keys.Count == 1 ? true : false;
                bool isEnableGroup3 = dictGroup2.Count > 1 ? false : dictGroup2.ContainsKey("Empty") ? true : dictGroup2.Count == 0 || dictGroup2.Keys.Count == 1 ? true : false;
                isEnableGroup3 = !isEnableGroup2 ? false : isEnableGroup3;
                Tagging tagging = new Tagging(selectedElementIDs, doc, grp1, grp2, grp3, isEnableGroup2, isEnableGroup3);
                tagging.ShowDialog();


            }
            else
                TaskDialog.Show("Kewaunee", "Please select products to tag.");
            return Result.Succeeded;
        }
    }




    public class DockConstants
    {
        public static readonly Guid Id = new Guid("{F75C6446-58E3-40D3-8071-46F87BEDE928}");
        public const string Name = "KEWAUNEE";
    }
}
