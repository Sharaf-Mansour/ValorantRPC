using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
namespace ValorantDRPC;
public partial class MainWindow : Window
{
    public MainWindow()=>  InitializeComponent();
    
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
        catch when (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
        }
        catch when (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        catch when (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        catch
        {
            throw;
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
        } 
        else
        {
            toggleApp.IsChecked = false;
            MessageBox.Show("Valorant Is not running, \nplease launch the game and re-enable the application.");
        }
        DiscordName.Text = $"Discord: {VDRPC.Client?.CurrentUser.Username}#{VDRPC.Client?.CurrentUser.Discriminator}";
        launchOS.IsChecked = true;
    }

    private void launchOS_Checked(object sender, RoutedEventArgs e)
    {
        bool lOSC = launchOS.IsChecked ?? true;
        Startup.SetStartup(lOSC);
    }
}
