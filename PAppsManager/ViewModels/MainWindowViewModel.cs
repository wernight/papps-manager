﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using PAppsManager.Core.PApps;

namespace PAppsManager.ViewModels
{
    internal class MainWindowViewModel : PropertyChangedBase
    {
        private readonly PortableApplicationManager _manager;

        public MainWindowViewModel(PortableApplicationManager manager)
        {
            _manager = manager;
        }

        public void Install()
        {
            Process.Start("http://compareason.com");
        }

        public void InstallApplication(string url)
        {
            // Change protocol (if necessary).
            url = Regex.Replace(url, @"^papp://", "http://");

            try
            {
                // Load the application info.
                PortableApplication application = PortableApplication.LoadFromUrl(url);

                // Confirm installation.
                if (MessageBox.Show(string.Format("Do you want to the install the portable application {0}?", application.Name), "PApps Manager - Install Applicaiton", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Install the application.
                    IoC.Get<PortableApplicationManager>().Install(application);

                    MessageBox.Show(
                        string.Format("The portable application {0} has been install successfully. You can now use it like a regular application.", application.Name),
                        "PApps Manager - Install Applicaiton");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to install the application: " + e.Message, "PApps Manager - Install Applicaiton",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Uninstall()
        {
            MessageBox.Show("Simply delete the portable application directory.");

            Process.Start(new ProcessStartInfo("explorer.exe", "/n, /e, " + _manager.BaseDirectory));
        }

        public void Exit()
        {
            App.Current.Shutdown();
        }
    }
}