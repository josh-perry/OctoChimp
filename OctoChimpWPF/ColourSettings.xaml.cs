using System.Windows;
using System.Windows.Media;

namespace OctoChimpWPF
{
    /// <summary>
    /// Interaction logic for ColourSettings.xaml
    /// </summary>
    public partial class ColourSettings : Window
    {
        public ColourSettings()
        {
            InitializeComponent();

            var foregroundColour = new Color
            {
                R = Properties.Settings.Default.ForegroundR,
                G = Properties.Settings.Default.ForegroundG,
                B = Properties.Settings.Default.ForegroundB,
                A = 255
            };

            ForegroundColourPicker.SelectedColor = foregroundColour;

            var backgroundColour = new Color
            {
                R = Properties.Settings.Default.BackgroundR,
                G = Properties.Settings.Default.BackgroundG,
                B = Properties.Settings.Default.BackgroundB,
                A = 255
            };

            BackgroundColourPicker.SelectedColor = backgroundColour;
        }

        private void OkButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (ForegroundColourPicker.SelectedColor != null)
            {
                Properties.Settings.Default.ForegroundR = ForegroundColourPicker.SelectedColor.Value.R;
                Properties.Settings.Default.ForegroundG = ForegroundColourPicker.SelectedColor.Value.G;
                Properties.Settings.Default.ForegroundB = ForegroundColourPicker.SelectedColor.Value.B;
            }

            if (BackgroundColourPicker.SelectedColor != null)
            {
                Properties.Settings.Default.BackgroundR = BackgroundColourPicker.SelectedColor.Value.R;
                Properties.Settings.Default.BackgroundG = BackgroundColourPicker.SelectedColor.Value.G;
                Properties.Settings.Default.BackgroundB = BackgroundColourPicker.SelectedColor.Value.B;
            }

            Properties.Settings.Default.Save();

            Close();
        }
    }
}
