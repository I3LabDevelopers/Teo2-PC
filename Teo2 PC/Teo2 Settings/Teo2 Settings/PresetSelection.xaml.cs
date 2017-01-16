using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Teo2_Settings
{
    /// <summary>
    /// Logica di interazione per PresetSelection.xaml
    /// </summary>
    public partial class PresetSelection : Page
    {
        public static string preset { get; internal set; }

        public PresetSelection()
        {
            InitializeComponent();
        }
        

        private void button_left_Click(object sender, RoutedEventArgs e)
        {
            preset = "greetings";
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("ShowPreset.xaml", UriKind.Relative));
        }

        private void button_center_Click(object sender, RoutedEventArgs e)
        {
            preset = "default";
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("ShowPreset.xaml", UriKind.Relative));
        }
        private void button_right_Click(object sender, RoutedEventArgs e)
        {
            preset = "more_emotions";
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("ShowPreset.xaml", UriKind.Relative));
        }

        private void custom_preset_Click(object sender, RoutedEventArgs e)
        {
            NavigationService ns = NavigationService.GetNavigationService(this);
            ns.Navigate(new Uri("CustomPreset.xaml", UriKind.Relative));
        }


    }
}
