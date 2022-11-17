using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using ValAPINet;

namespace ValorantDRPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void toggleApp_Checked(object sender, RoutedEventArgs e)
        {

        }

        private async void toggleApp_Click(object sender, RoutedEventArgs e)
        {
            if (VDRPC.IsValorantNotRunning)
            {
                MessageBox.Show("Valorant Is not running, \nplease launch the game and re-enable the application.");
                toggleApp.IsChecked = false;
                return;
            }
            statusBox.Text = toggleApp.IsChecked ?? false ? "Status : Running" : "Status : Disabled";
            toggleApp.IsEnabled = false;
            await VDRPC.InitApp();
        }
        
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Crv5heR/ValorantRPC");
            DiscordName.Text = "Discord: " + VDRPC.DUsername;
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VDRPC.PrepareDiscordRPC();
            if (!VDRPC.IsValorantNotRunning)
            {
                toggleApp.IsChecked = true;
                statusBox.Text = toggleApp.IsChecked ?? false ? "Status : Running" : "Status : Disabled";
                toggleApp.IsEnabled = false;
                
                
                await VDRPC.InitApp();
            } else
            {
                MessageBox.Show("Valorant Is not running, \nplease launch the game and re-enable the application.");
                toggleApp.IsChecked = false;
            }
            DiscordName.Text = "Discord: " + VDRPC.Client.CurrentUser.Username+"#"+ VDRPC.Client.CurrentUser.Discriminator;
            launchOS.IsChecked = true;
        }

        private void launchOS_Checked(object sender, RoutedEventArgs e)
        {
            bool lOSC = launchOS.IsChecked ?? true;
            Startup.SetStartup(lOSC);
        }
    }
}
