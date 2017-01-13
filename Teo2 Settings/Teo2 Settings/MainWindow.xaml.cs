using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;

namespace Teo2_Settings
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        }

        // This method modifies the config.xml file setting the desired value for the selected preset
        public static void SetPreset(string preset)
        {
            XmlDocument config = new XmlDocument();

            config.Load("../Teo2 Control/XML/config.xml");

            config.SelectSingleNode("//config/selected_preset").InnerText = preset;

            config.Save("../Teo2 Control/XML/config.xml");
        }

        public static void ExitSettings(string preset)
        {
            MessageBox.Show(preset + " Preset has been set!");
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Do you want to change settings?", "Exit Settings", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
                Application.Current.Shutdown();
        }
    }
}
