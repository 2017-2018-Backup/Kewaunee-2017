using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{
    public class Properties
    {
        public static bool showhide { get; set; }

        public static ExternalCommandData ExternalCommandData { get; set; }

        public static ElementSet ElementSet { get; set; }


        public static PushButton pushButtonDockPanel { get; set; }

        public static PushButton pushButtonCreateProject { get; set; }

        public static PushButton pushButtonOpenProject { get; set; }

        public static PushButton pushButtonCustomerMaster { get; set; }

        public static PushButton pushButtonLoginMaster { get; set; }

        public static PushButton pushButtonUserProfile { get; set; }

        public static PushButton pushButtonTaxMaster { get; set; }

        public static PushButton pushButtonPrpjCatalog { get; set; }

        public static PushButton pushButtonLogin { get; set; }

        public static RibbonPanel ribbonPanel { get; set; }

        public static bool isFirstLogin { get; set; }

        public static UIControlledApplication controlledApp { get; set; }

        public static RibbonPanel ribbonPanelProjects { get; set; }

        public static PushButton pushButtonCurrencyCatalog { get; set; }

        public static PushButton pushButtonVariants { get; set; }

        public static PushButton pushButtonPricing { get; set; }

        public static Document doc { get; set; }

        public static PushButton pushButtonProductMaster { get; set; }

        public static PushButton pushButtonSalesQuoteMaster { get; set; }

        public static string DocumentName { get; set; }

        public static List<ElementId> AddedElementIds { get; set; }

        public static PushButton pushButtonTagging { get; set; }

        public static Dictionary<string, List<string>> dictFumeHoodFamilies { get; set; }


        public static UIControlledApplication uiControlledApp { get; set; }
    }
}
