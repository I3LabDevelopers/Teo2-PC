using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Teo2_Settings
{
    /// <summary>
    /// Logica di interazione per CustomPreset.xaml
    /// </summary>
    public partial class CustomPreset : Page
    {

        Dictionary<string, string> command_map;

        public CustomPreset()
        {
            InitializeComponent();
            LoadCommands();
            SetComboBoxes();
        }

        // Load the set of available commands that can be mapped in each button
        // of the Xbox controller and put them in a Dictionary that associates
        // each display name of the command to the actual command fro Teo2, the 
        // map will be used in CreatePreset() to easily gain the command value 
        // assoiated to the selected command name in the combo boxes.
        private void LoadCommands()
        {
            command_map = new Dictionary<string, string>();
            var xDoc = XDocument.Load("../Teo2 Control/XML/commands.xml");

            var commands = xDoc.Descendants("command");
            foreach (var command in commands)
            {
                command_map.Add(command.Attribute("name").Value, command.Value);
            }

        }

        // Put in the combo boxes each available command.
        private void SetComboBoxes()
        {
            foreach (var key in command_map.Keys)
            {
                left_trigger_box.Items.Add(key);
                right_trigger_box.Items.Add(key);
                left_shoulder_box.Items.Add(key);
                right_shoulder_box.Items.Add(key);
                left_box.Items.Add(key);
                right_box.Items.Add(key);
                up_box.Items.Add(key);
                down_box.Items.Add(key);
            }

        }


        // Creates the preset according to the selected command in the combo boxes.
        private void CreatePreset()
        {
            XDocument preset_file = new XDocument();
            XElement preset = new XElement("preset");

            XElement left = new XElement("left");
            if ((string)left_box.SelectedValue != null)
                left.Value = command_map[(string)left_box.SelectedValue];
            else
                left.Value = " ";
            preset.Add(left);

            XElement right = new XElement("right");
            if ((string)right_box.SelectedValue != null)
                right.Value = command_map[(string)right_box.SelectedValue];
            else
                right.Value = " ";
            preset.Add(right);

            XElement up = new XElement("up");
            if ((string)up_box.SelectedValue != null)
                up.Value = command_map[(string)up_box.SelectedValue];
            else
                up.Value = " ";
            preset.Add(up);

            XElement down = new XElement("down");
            if ((string)down_box.SelectedValue != null)
                down.Value = command_map[(string)down_box.SelectedValue];
            else
                down.Value = " ";
            preset.Add(down);

            XElement r_trigger = new XElement("r_trigger");
            if ((string)right_trigger_box.SelectedValue != null)
                r_trigger.Value = command_map[(string)right_trigger_box.SelectedValue];
            else
                r_trigger.Value = " ";
            preset.Add(r_trigger);

            XElement l_trigger = new XElement("l_trigger");
            if ((string)left_trigger_box.SelectedValue != null)
                l_trigger.Value = command_map[(string)left_trigger_box.SelectedValue];
            else
                l_trigger.Value = " ";
            preset.Add(l_trigger);

            XElement r_shoulder = new XElement("r_shoulder");
            if ((string)right_shoulder_box.SelectedValue != null)
                r_shoulder.Value = command_map[(string)right_shoulder_box.SelectedValue];
            else
                r_shoulder.Value = " ";
            preset.Add(r_shoulder);

            XElement l_shoulder = new XElement("l_shoulder");
            if ((string)left_shoulder_box.SelectedValue != null)
                l_shoulder.Value = command_map[(string)left_shoulder_box.SelectedValue];
            else
                l_shoulder.Value = " ";
            preset.Add(l_shoulder);

            preset_file.Add(preset);
            try
            {
                preset_file.Save("../Teo2 Control/XML/custom.xml");
            }
            catch( Exception ex)
            {
                MessageBox.Show("XML Error: " + ex.Message + "\nImpossibile salvare il preset");
            }
        }

        // Upon click on the confirm button the preset will be created and custom preset
        // will be indicated as selected preset in config.xml.
        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sei sicuro di selezionare il preset corrente?", "Selezione del preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CreatePreset();
                MainWindow.SetPreset("custom.xml");
                MessageBox.Show("Il preset personalizzato è stato selezionato");

                left_trigger_box.IsEnabled = false;
                right_trigger_box.IsEnabled = false;
                left_shoulder_box.IsEnabled = false;
                right_shoulder_box.IsEnabled = false;
                left_box.IsEnabled = false;
                right_box.IsEnabled = false;
                up_box.IsEnabled = false;
                down_box.IsEnabled = false;
                confirm.IsEnabled = false;

                // This avoids the user to navigate back.
                NavigationService.RemoveBackEntry();
            }

        }
    }
}
