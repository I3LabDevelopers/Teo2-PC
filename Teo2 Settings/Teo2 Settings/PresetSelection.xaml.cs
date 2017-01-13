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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Teo2_Settings
{
    /// <summary>
    /// Logica di interazione per PresetSelection.xaml
    /// </summary>
    public partial class PresetSelection : Page
    {
        public PresetSelection()
        {
            InitializeComponent();
        }
        

        private void button_left_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetPreset("greetings.xml");
            MainWindow.ExitSettings("Greetings");
        }

        private void button_center_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetPreset("default.xml");
            MainWindow.ExitSettings("Default");
        }
        private void button_right_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetPreset("more_emotions.xml");
            MainWindow.ExitSettings("More Emotions");
        }

        private void custom_preset_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("CustomPreset.xaml", UriKind.Relative));
        }


    }
}
