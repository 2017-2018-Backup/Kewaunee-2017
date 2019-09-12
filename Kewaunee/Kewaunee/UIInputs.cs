using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kewaunee
{
    public class UIInputs
    {
        public static int WallLength { get; set; }
        public static int WallWidth { get; set; }
        public static int TrueCeilingHeight { get; set; }
        public static int FalseCielingHeight { get; set; }

        public static string ProjectPath { get; set; }
        public static string ProjectName { get; set; }
        public static string ProjectNo { get; set; }

        public static string ProjectDrawingRevNo { get; set; }


        public static string ProjectNameBeforeRevNo { get; set; }

        public static string ProjectNoBeforeRevNo { get; set; }

        public static string Username { get; set; }
    }

    public class ClsProperties
    {
        public static object UIApplication { get; set; }

        public static object commandData { get; set; }

        public static Guid DockablePaneGuid { get; set; }

        public static bool IsMouseDragged { get; set; }

        public static string FamilyPath { get; set; }

        public static bool isLoginSuccess { get; set; }

        public static bool isProjectRevised { get; set; }

        public static ProjectsCatalogue ProjectCatalog { get; set; }

        public static bool isProjectUpdated { get; set; }

        private static string _connectionstr;
        public static string connectionString
        {
            get { return _connectionstr; }
            set
            {
                _connectionstr = value;
                Properties.Settings.Default.ConnectionString = _connectionstr;
            }
        }

        public static string PlacedFamilyCategory { get; set; }

        public static bool isClosed { get; set; }

        public static string DocumentName { get; set; }

        public static bool isSalesClosed { get; set; }

        public static object familySymbol { get; set; }

        public static DataTable dtProducts { get; set; }

        public static List<string> LstUpdatedParamList;

        public static List<Autodesk.Revit.DB.ElementId> LstElementIds { get; set; }

        public static string PartCode { get; set; }

        public static Dictionary<string, Dictionary<string,List<string>>> dictFumeHoodFamilies { get; set; }
    }

    public class ProfileDescription
    {
        public static int Profile { get; set; }
        public static List<Features> FeaturesName { get; set; }
    }

    public class Features
    {
        public string FeaturesName { get; set; }
        public bool isRead { get; set; }
        public bool isEdit { get; set; }
        public bool isDelete { get; set; }
    }
}
