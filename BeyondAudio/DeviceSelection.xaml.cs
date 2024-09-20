using System;
using System.Linq;
using System.Windows;

namespace BeyondAudio
{
    public partial class DeviceSelection : Window
    {
        public bool StartBatc { get; protected set; } = true;

        public DeviceSelection()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ListDevices.ItemsSource = App.DeviceEnumeration.Select(d => d.DeviceFriendlyName).ToList();
            ListDevices.SelectedIndex = 0;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration appConfig = new Configuration
                {
                    DeviceName = ListDevices.SelectedValue.ToString(),
                };
                appConfig.Save();
                Tools.PlaceDesktopLink();
                StartBatc = CheckboxStartBatc.IsChecked == true;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception '{ex.GetType()}' during ButtonSave_Click", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
