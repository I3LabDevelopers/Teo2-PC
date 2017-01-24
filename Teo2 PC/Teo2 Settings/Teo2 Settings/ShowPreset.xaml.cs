using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Teo2_Settings
{
    /// <summary>
    /// Logica di interazione per CustomPreset.xaml
    /// </summary>
    public partial class ShowPreset : Page
    {
        public ShowPreset()
        {
            InitializeComponent();

            // Setting the source for the image selected in PresetSelection
            var uri = new Uri("pack://application:,,,/Teo2 Settings;component/images/" + PresetSelection.preset + ".png");
            var bitmap = new BitmapImage(uri);
            
            image.Source = bitmap;
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Sei sicuro di selezionare il preset corrente?", "Selezione del preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                MainWindow.SetPreset(PresetSelection.preset + ".xml");
                MessageBox.Show("Il preset attuale è stato selezionato");

                confirm.IsEnabled = false;

                // This avoids the user to navigate back.
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
