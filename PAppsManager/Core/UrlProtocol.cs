using System;
using System.IO;
using Microsoft.Win32;

namespace PAppsManager.Core
{
    /// <summary>
    /// Associate a URL protocol, like "example://" with an executable.
    /// </summary>
    public class UrlProtocol
    {
        public static void Associate(string protocol, string exe, string commandLine = "\"%1\"")
        {
            try
            {
                var classesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
                if (classesKey == null)
                    throw new Exception("Failed to open user registry key.");

                // Software\Classes\PApps\
                var pappsKey = classesKey.CreateSubKey(protocol);
                if (pappsKey == null)
                    throw new Exception("Failed to create 'shell' registry key.");

                // Software\Classes\PApps\URL Protocol
                pappsKey.SetValue("URL Protocol", "");

                // Software\Classes\PApps\DefaultIcon\
                var defaultIconKey = pappsKey.CreateSubKey("DefaultIcon");
                if (defaultIconKey != null)
                    defaultIconKey.SetValue(null, string.Format("\"{0}\",0", exe));

                // Software\Classes\PApps\shell\
                var pappsShellKey = pappsKey.CreateSubKey(@"shell");
                if (pappsShellKey == null)
                    throw new Exception("Failed to create 'shell' registry key.");
                pappsShellKey.SetValue(null, "open");

                // Software\Classes\PApps\shell\open\command
                var pappsShellOpenCommandKey = pappsKey.CreateSubKey(@"shell\open\command");
                if (pappsShellOpenCommandKey == null)
                    throw new Exception(@"Failed to create 'shell\open\command' registry key.");
                pappsShellOpenCommandKey.SetValue(null, string.Format("\"{0}\" {1}", exe, commandLine));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to associate {0} with the {1} URL protocol: {2}.", Path.GetFileNameWithoutExtension(exe), protocol, ex.Message), ex);
            }
        }

        public static void Disassociate(string protocol)
        {
            try
            {
                var classesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
                if (classesKey == null)
                    throw new Exception("Failed to open user registry key.");
                classesKey.DeleteSubKeyTree(protocol);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to de-associate the {1} URL protocol.", protocol));
            }
        } 
    }
}