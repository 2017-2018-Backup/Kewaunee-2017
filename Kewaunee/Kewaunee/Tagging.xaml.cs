using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kewaunee
{
    /// <summary>
    /// Interaction logic for Tagging.xaml
    /// </summary>
    public partial class Tagging : Window
    {
        private object _lstElementIds = null;
        private object _doc = null;
        public Tagging(object lstElementIDs, object doc, string grp1, string grp2, string grp3, bool isEnableGroup2, bool isEnableGroup3)
        {
            InitializeComponent();
            _lstElementIds = lstElementIDs;
            _doc = doc;
            txtGroup1.Text = grp1;
            txtGroup2.Text = grp2;
            txtGroup3.Text = grp3;
            txtGroup2.IsEnabled = isEnableGroup2;
            txtGroup3.IsEnabled = isEnableGroup3;
            txtGroup2.Text = txtGroup2.IsEnabled ? txtGroup2.Text : string.Empty;
            txtGroup3.Text = txtGroup3.IsEnabled ? txtGroup3.Text : string.Empty;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CreateParameter createParameter = new CreateParameter(_lstElementIds, _doc, txtGroup1.Text, txtGroup2.Text, txtGroup3.Text, txtGroup2.IsEnabled, txtGroup3.IsEnabled);
            createParameter.FamilyParameterCreation();
            Close();
        }


    }
}
