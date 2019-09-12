using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{

    public class LoadFamilyOption : IFamilyLoadOptions
    {

        //public bool OnFamilyFound(bool familyInUse, ref bool overwriteParameterValues)
        //{

        //    if (familyInUse == true)
        //    {
        //        overwriteParameterValues = false;
        //        return true;
        //    }
        //    else
        //    {
        //        overwriteParameterValues = false;
        //        return true;
        //    }

        //}

        //public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, ref FamilySource source, ref bool overwriteParameterValues)
        //{

        //    return true;

        //}

        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            if (familyInUse == true)
            {
                overwriteParameterValues = false;
                return true;
            }
            else
            {
                overwriteParameterValues = false;
                return true;
            }
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }

}
