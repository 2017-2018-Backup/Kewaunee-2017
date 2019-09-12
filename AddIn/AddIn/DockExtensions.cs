using System;
using System.Windows;
using Autodesk.Revit.UI;

namespace AddIn
{
    public static class DockExtensions
    {
        public static bool PaneExists(this UIControlledApplication application, Guid id, out DockablePane pane)
        {
            var dPid = new DockablePaneId(id);
            if (DockablePane.PaneExists(dPid))
            {
                pane = application.GetDockablePane(dPid);
                return true;
            }
            pane = null;
            return false;
        }

        public static bool PaneExists(this UIApplication application, Guid id, out DockablePane pane)
        {
            var dPid = new DockablePaneId(id);
            if (DockablePane.PaneExists(dPid))
            {
                pane = application.GetDockablePane(dPid);
                return true;
            }
            pane = null;
            return false;
        }

        public static void RegisterDockablePane(this UIControlledApplication application,
            Guid id, string title, FrameworkElement dockElement, DockablePaneState state)
        {
            var dPid = new DockablePaneId(id);

            var dataProvider = new DockablePaneProviderData { FrameworkElement = dockElement, InitialState = state };
            IDockablePaneProvider provider = new DockPageProvider(dataProvider);
            application.RegisterDockablePane(dPid, title, provider);

        }




        public class DockPageProvider : IDockablePaneProvider
        {
            private readonly DockablePaneProviderData _data;

            public DockPageProvider(DockablePaneProviderData data)
            {
                _data = data;
            }

            public void SetupDockablePane(DockablePaneProviderData data)
            {
                data.FrameworkElement = _data.FrameworkElement;
                data.InitialState = _data.InitialState;
            }
        }
    }
}