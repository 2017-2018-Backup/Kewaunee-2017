using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kewaunee
{
    public class CreateParameter
    {
        private string _group1, _group2, _group3;
        private List<ElementId> _lstElementID;
        private Document _doc = default(Document);
        private List<string> familyNames = new List<string>();
        bool isGrp2Disabled = false;
        bool isGrp3Disabled = false;
        public CreateParameter(object lstElementIds, object doc, string group1, string group2, string group3, bool isGroup2Disabled, bool isGroup3Disabled)
        {
            _lstElementID = (List<ElementId>)lstElementIds;
            _group1 = group1;
            _group2 = group2;
            _group3 = group3;
            isGrp2Disabled = isGroup2Disabled;
            isGrp3Disabled = isGroup3Disabled;
            _doc = (Document)doc;
        }

        public void FamilyParameterCreation()
        {
            bool isExists = false;
            UpdateInstanceParamValueIfExists(ref isExists);

            //_lstElementID.OfType<ElementId>().ToList().ForEach(x =>
            //{
            //    var element = _doc.GetElement(x);
            //    if (element != null && element is FamilyInstance)
            //    {

            //    }
            //});
            if (!isExists)
                ClearParameterValues(familyNames);
        }

        public void CreateAccessoriesParameter(List<string> lstAccessories, string partCode)
        {
            foreach (var eleId in ClsProperties.LstElementIds)
            {
                var element = _doc.GetElement(eleId);
                if (element == null || !(element is FamilyInstance)) continue;
                var familyInsatnce = (FamilyInstance)element;
                if (!familyInsatnce.Name.Equals(partCode)) continue;
                var family = familyInsatnce.Symbol.Family;
                string value = string.Empty;
                lstAccessories.ForEach(x => { if (!string.IsNullOrWhiteSpace(value)) value = value + "," + x; else value = x; });

                var param = familyInsatnce.Parameters.OfType<Parameter>().ToList().Where(x => x.Definition.Name.Equals("Accessories")).Select(x => x).FirstOrDefault();

                if (param != null)
                {
                    var trns = new Transaction(_doc);
                    trns.Start("Update Value");
                    param.Set(value);
                    trns.Commit();
                }
                else
                {
                    var editDoc = _doc.EditFamily(family);


                    var trans = new Transaction(editDoc);
                    trans.Start("Add Paramter");
                    var familyManager = editDoc.FamilyManager;
                    var familyParam = familyManager.get_Parameter("Accessories") == null ? familyManager.AddParameter("Accessories", BuiltInParameterGroup.PG_GENERAL, ParameterType.Text, true) : null;
                    if (familyParam != null)
                        familyManager.Set(familyParam, value);
                    trans.Commit();
                    if (familyParam != null)
                    {
                        editDoc.Save();
                        trans = new Transaction(_doc);
                        trans.Start("Load Family");
                        LoadFamilyOption loadOptions = new LoadFamilyOption();
                        var fam = default(Family);
                        _doc.LoadFamily(editDoc.PathName, loadOptions, out fam);
                        trans.Commit();
                    }

                    editDoc.Close();



                    var filteredElementCollector = new FilteredElementCollector(_doc);

                    trans = new Transaction(_doc, "Clear Value");
                    trans.Start();
                    var collection = filteredElementCollector.OfClass(typeof(FamilyInstance)).Where(x => (x is FamilyInstance) && !(x as FamilyInstance).Name.Equals(familyInsatnce.Name)).Select(x => x).ToList();
                    if (collection.Any())
                    {
                        collection.ForEach(x =>
                        {
                            var famParam = (x as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Accessories")).Select(y => y).FirstOrDefault();
                            if (famParam != null && famParam.AsString().Equals(value))
                                famParam.Set("");
                        });
                    }
                    trans.Commit();
                }
            }
        }


        private void CreateParamterForFamily(Element element)
        {
            try
            {
                var familyInsatnce = (FamilyInstance)element;
                var family = familyInsatnce.Symbol.Family;

                if (!familyNames.Contains(family.Name))
                {
                    familyNames.Add(family.Name);

                    var editDoc = _doc.EditFamily(family);

                    var trans = new Transaction(editDoc);
                    trans.Start("Add Paramter");
                    var familyManager = editDoc.FamilyManager;


                    var familyParam = familyManager.get_Parameter("Group 1") == null ? familyManager.AddParameter("Group 1", BuiltInParameterGroup.PG_GENERAL, ParameterType.Text, true) : null;

                    if (familyParam != null)
                        familyManager.Set(familyParam, _group1);
                    //else
                    //{
                    //    familyParam = familyManager.get_Parameter("Group 1");
                    //    familyManager.Set(familyParam, _group1);
                    //}

                    var familyParam1 = familyManager.get_Parameter("Group 2") == null ? familyManager.AddParameter("Group 2", BuiltInParameterGroup.PG_GENERAL, ParameterType.Text, true) : null;

                    if (familyParam1 != null)
                        familyManager.Set(familyParam1, _group2);
                    //else
                    //{
                    //    familyParam1 = familyManager.get_Parameter("Group 2");
                    //    familyManager.Set(familyParam1, _group2);
                    //}

                    var familyParam2 = familyManager.get_Parameter("Group 3") == null ? familyManager.AddParameter("Group 3", BuiltInParameterGroup.PG_GENERAL, ParameterType.Text, true) : null;

                    if (familyParam2 != null)
                        familyManager.Set(familyParam2, _group3);
                    //else
                    //{
                    //    familyParam2 = familyManager.get_Parameter("Group 3");
                    //    familyManager.Set(familyParam2, _group3);
                    //}

                    trans.Commit();
                    if (familyParam != null || familyParam2 != null || familyParam1 != null)
                    {
                        editDoc.Save();
                        trans = new Transaction(_doc);
                        trans.Start("Load Family");
                        LoadFamilyOption loadOptions = new LoadFamilyOption();
                        var fam = default(Family);
                        _doc.LoadFamily(editDoc.PathName, loadOptions, out fam);
                        trans.Commit();
                    }

                    editDoc.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateInstanceParamValueIfExists(ref bool isGroupExists)
        {

            bool isTransClosed = false;
            foreach (ElementId x in _lstElementID)
            {
                var element = _doc.GetElement(x);
                if (element != null && element is FamilyInstance)
                {
                    if (!familyNames.Contains((element as FamilyInstance).Symbol.FamilyName))
                    {
                        Transaction trans = new Transaction(_doc, "Update Value");
                        trans.Start();
                        var param = (element as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 1")).Select(y => y).FirstOrDefault();
                        if (param != null)
                        {

                            param.Set(_group1);
                            isGroupExists = true;
                        }
                        var param1 = (element as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 2")).Select(y => y).FirstOrDefault();
                        if (param1 != null)
                        {
                            if (isGrp2Disabled)
                                param1.Set(_group2);
                            isGroupExists = true;
                        }
                        var param2 = (element as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 3")).Select(y => y).FirstOrDefault();
                        if (param2 != null)
                        {
                            if (isGrp3Disabled)
                                param2.Set(_group3);
                            isGroupExists = true;
                        }

                        trans.Commit();
                        if (!isGroupExists)
                        {
                            //isTransClosed = true;
                            CreateParamterForFamily(element);
                        }

                        isGroupExists = false;

                    }
                }
            }
            //if (!isTransClosed)
            //    trans.Commit();
        }

        private void ClearParameterValues(List<string> familyNames)
        {
            if (familyNames.Any())
            {
                var filteredElementCollector = new FilteredElementCollector(_doc);

                Transaction trans = new Transaction(_doc, "Clear Value");
                trans.Start();
                var collection = filteredElementCollector.OfClass(typeof(FamilyInstance)).Where(x => (x is FamilyInstance) && familyNames.Contains((x as FamilyInstance).Symbol.FamilyName)).Select(x => x).ToList();
                if (collection.Any())
                {
                    collection.ForEach(x =>
                    {
                        if (!_lstElementID.Contains(x.Id))
                        {
                            var param = (x as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 1")).Select(y => y).FirstOrDefault();
                            if (param != null && param.AsString().Equals(_group1))
                                param.Set("");

                            var param1 = (x as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 2")).Select(y => y).FirstOrDefault();
                            if (param1 != null && param1.AsString().Equals(_group2))
                                param1.Set("");

                            var param2 = (x as FamilyInstance).Parameters.OfType<Parameter>().ToList().Where(y => y.Definition.Name.Equals("Group 3")).Select(y => y).FirstOrDefault();
                            if (param2 != null && param2.AsString().Equals(_group3))
                                param2.Set("");
                        }
                    });
                }
                trans.Commit();
            }
        }
    }

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
