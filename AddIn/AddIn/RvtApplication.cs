using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kewaunee;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace AddIn
{
    public class RvtApplication : IExternalApplication
    {
        public static string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string assyPath = Path.Combine(dir, "AddIn.dll");
        private UIControlledApplication uicontrolledApp;
        public Result OnShutdown(UIControlledApplication application)
        {
            ClsProperties.isClosed = false;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            FileInfo finfo = new FileInfo(assyPath);
            //DateTime createdTime = finfo. ;
            DateTime createdTime = DateTime.Now;
            DateTime validityDate = new DateTime(2019, 1, 31);//new DateTime(2017, 12, 18);//new DateTime(2017, 10, 24);
            // DateTime.ParseExact(validityTime, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            if (createdTime < validityDate)
            {
                Properties.showhide = false;
                Properties.isFirstLogin = true;
                ClsProperties.isClosed = true;
                ClsProperties.LstUpdatedParamList = new List<string>();
                uicontrolledApp = application;
                Properties.uiControlledApp = application;
                application.Idling += application_Idling;
                application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
                application.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;
                string connectionString = System.IO.Path.Combine(assyPath.Substring(0, assyPath.LastIndexOf(@"\")), @"Kewaunee\Revit\Settings.txt");
                if (File.Exists(connectionString))
                {
                    using (StreamReader sw = new StreamReader(connectionString))
                    {
                        string connstr = sw.ReadLine();
                        ClsProperties.connectionString = connstr;
                    }
                }
                var dPid = new DockablePaneId(DockConstants.Id);
                if (!DockablePane.PaneIsRegistered(dPid))
                {
                    var state = new DockablePaneState { DockPosition = DockPosition.Left };
                    var element = DockablePage.SingleTonDockablePageInstance;
                    application.RegisterDockablePane(DockConstants.Id, DockConstants.Name, element, state);
                }

                CreatePushButtons(application);
            }
            return Result.Succeeded;
        }

        void ControlledApplication_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            try
            {
                if (e.GetAddedElementIds().Any())
                {
                    if (Properties.AddedElementIds == null)
                        Properties.AddedElementIds = new List<ElementId>();
                    Properties.AddedElementIds.AddRange(e.GetAddedElementIds());
                }
            }
            catch (Exception ex)
            {

            }
        }

        void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            ClsProperties.DocumentName = System.IO.Path.GetFileNameWithoutExtension(e.Document.PathName);
            var dPid = new DockablePaneId(DockConstants.Id);
            DockablePane pane = uicontrolledApp.GetDockablePane(dPid);
            pane.Hide();
            Properties.showhide = false;

        }


        private void application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            if (ClsProperties.IsMouseDragged == true)
            {

                string tempmsg = string.Empty;
                DownloadFamily dwnld = new DownloadFamily();
                dwnld.Execute(Properties.ExternalCommandData, ref tempmsg, Properties.ElementSet);
                ClsProperties.IsMouseDragged = false;
                Properties.showhide = false;

            }
            if (ClsProperties.isProjectRevised)
            {
                var uiapp = Properties.ExternalCommandData.Application;
                SaveDocument(Properties.doc, uiapp);
                ClsProperties.isProjectRevised = false;
            }

        }

        private void SaveDocument(Document doc, UIApplication uiapp)
        {
            if (UIInputs.ProjectPath.Contains(".rvt"))
            {
                UIInputs.ProjectPath = UIInputs.ProjectPath.Substring(0, UIInputs.ProjectPath.LastIndexOf(@"\"));
                Properties.doc.SaveAs(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + "_" + UIInputs.ProjectDrawingRevNo + ".rvt"));
                uiapp.OpenAndActivateDocument(Path.Combine(UIInputs.ProjectPath, UIInputs.ProjectName + "_" + UIInputs.ProjectNo + "_" + UIInputs.ProjectDrawingRevNo + ".rvt"));
            }

            UIInputs.ProjectName = string.Empty;
            UIInputs.ProjectPath = string.Empty;
            //UIInputs.ProjectNo = string.Empty;
            UIInputs.ProjectNameBeforeRevNo = string.Empty;
            UIInputs.ProjectNoBeforeRevNo = string.Empty;
            UIInputs.ProjectDrawingRevNo = string.Empty;
        }

        private void CreatePushButtons(UIControlledApplication application)
        {
            Properties.controlledApp = application;

            application.CreateRibbonTab("Kewaunee");
            var ribbonPanel = application.CreateRibbonPanel("Kewaunee", "Login");
            Properties.ribbonPanel = ribbonPanel;


            var pushButtonLogin = ribbonPanel.AddItem(new PushButtonData("Login", "Login", assyPath, "AddIn.UserLogin")) as PushButton;
            pushButtonLogin.ToolTip = "Show/Hide Panel";
            Uri shwimg1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/login64x64.ico");
            BitmapImage shwlrg1 = new BitmapImage(shwimg1);
            Uri shwsml1 = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/login32x32.ico");
            BitmapImage showsmall1 = new BitmapImage(shwsml1);
            pushButtonLogin.LargeImage = showsmall1;
            pushButtonLogin.ToolTipImage = shwlrg1;
            Properties.pushButtonLogin = pushButtonLogin;

            //ribbonPanel.AddSeparator();



        }


    }
}
