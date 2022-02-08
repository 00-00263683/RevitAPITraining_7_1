using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary_7_1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITraining_7_1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;

        public List<FamilySymbolWrapper> SheetTemplate { get; } = new List<FamilySymbolWrapper>();
        public List<ViewPlan> ViewTypes { get; } = new List<ViewPlan>();
        public ViewPlan SelectedViewTypes { get; set; }
        public DelegateCommand CreateSheets { get; }
        public int SheetCount { get; set; }
        public string DesignBy { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            SheetTemplate = FamilyInstanceUtils.GetSheetTemplate(commandData).Select(s => new FamilySymbolWrapper(s)).ToList();
            ViewTypes = ViewPortUtils.GetViewPorts(_commandData);
            CreateSheets = new DelegateCommand(OnCreateSheets);
            SheetCount = 1;
        }

        private void OnCreateSheets()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<FamilySymbolWrapper> selectedFamilySymbol = SheetTemplate.Where(s => s.IsSelected).ToList();
            if (selectedFamilySymbol == null)
                return;

            using (var ts = new Transaction(doc, "Create sheet"))
            {
                ts.Start();

                foreach (var familysymbol in selectedFamilySymbol)
                {
                    for (int i = 1; i <= SheetCount; i++)
                    {
                        ViewSheet viewSheet = ViewSheet.Create(doc, familysymbol.FamilySymbol.Id);
                        viewSheet.SheetNumber = $"{i}";

                        Parameter designByParam = viewSheet.get_Parameter(BuiltInParameter.SHEET_DESIGNED_BY);
                        designByParam.Set(DesignBy);

                        Viewport viewport = Viewport.Create(doc, viewSheet.Id, SelectedViewTypes.Id, new XYZ(0, 0, 0));
                    }
                }

                ts.Commit();
            }
            RaiseCloseRequest();
        }
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
