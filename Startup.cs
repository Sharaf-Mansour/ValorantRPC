using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ValorantDRPC
{
    public class Startup
    {
        static RegistryKey VDRPCAPP = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        public static void SetStartup(bool enabled)
        {
            if(!IsUserAdministrator())
            {
                MessageBox.Show("Please Launch the application in administrator mode to enable this feature.");
                return;
            }
            if (enabled)
            {
                VDRPCAPP.SetValue("ValorantRPC /ranFromStartup", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                VDRPCAPP.DeleteValue("ValorantRPC", false);
            }
        }

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {             
                throw ex;
            }
            return isAdmin;
        }
    }
}
