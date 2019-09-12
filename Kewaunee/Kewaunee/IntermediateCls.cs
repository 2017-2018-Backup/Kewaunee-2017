using KewauneeTaskAssigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kewaunee
{
    public class IntermediateCls
    {
        public IntermediateCls()
        {
        }

        public static void DisplayDockPanel()
        {
            TaskAssigner.ShowDockPanel(ClsProperties.UIApplication, ClsProperties.DockablePaneGuid);
        }
    }
}
