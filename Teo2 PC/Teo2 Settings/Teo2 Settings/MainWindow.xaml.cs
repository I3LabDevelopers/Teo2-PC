using System;
using System.Collections.Generic;
using System.Windows;
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
        }

        // This method modifies the config.xml file setting the desired value for the selected preset
        public static void SetPreset(string preset)
        {
            XmlDocument config = new XmlDocument();

            try
            {
                config.Load("../Teo2 Control/XML/config.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Error: " + ex.Message + "\nImpossibile caricare il file config.xml");
            }

            config.SelectSingleNode("//config/selected_preset").InnerText = preset;

            try
            {
                config.Save("../Teo2 Control/XML/config.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Error: " + ex.Message + "\nImpossibile salvare il file config.xml");
            }
        }

    }
}
