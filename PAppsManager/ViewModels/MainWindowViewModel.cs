﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using PAppsManager.Core.PApps;
using PAppsManager.Properties;

namespace PAppsManager.ViewModels
{
    internal class MainWindowViewModel : PropertyChangedBase
    {
        private readonly PortableEnvironment _portableEnvironment;
        private static readonly FileInfo EnvironmentJson = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Environment.json"));
        private readonly DispatcherTimer _autoUpdateTime = new DispatcherTimer();

        public MainWindowViewModel()
        {
            _portableEnvironment = SetUpPortableEnvironment();
        }

        public void Install()
        {
            Process.Start("http://compareason.com");
        }

        public void InstallApplication(string url)
        {
            try
            {
                // Load the application info.
                PortableApplication application;
                try
                {
                    using (var webClient = new WebClient())
                        application = PortableApplication.LoadFromUrl(url, webClient.DownloadString);
                }
                catch (Exception e)
                {
                    throw new Exception("Couldn't load the application's information:" + Environment.NewLine
                                        + e.Message + Environment.NewLine
                                        + Environment.NewLine
                                        + "You may have to upgrade your PApps Manager to a newer version.", e);
                }

                // Confirm installation.
                string message;
                if (!_portableEnvironment.Applications.Contains(application))
                    message = string.Format("Do you want to the add the portable application {0}?", application.Name);
                else
                    message = string.Format("You already have the portable application {0}." + Environment.NewLine + "Do you want to reinstall it?", application.Name);

                if (MessageBox.Show(message, "PApps Manager - Install Application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Install the application.
                    _portableEnvironment.Applications.Add(application);

                    MessageBox.Show(
                        string.Format("The portable application {0} has been install successfully. You can now use it like a regular application.", application.Name),
                        "PApps Manager - Install Application");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to install the application:" + Environment.NewLine + e.Message, "PApps Manager - Install Application",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Update()
        {
            // Avoid spamming the server, there is no point checking that often.
            if ((DateTime.UtcNow - Settings.Default.LastUpdateCheckTime).TotalMinutes < 1)
                return;

            // Retrieve a list of updates available.
            IEnumerable<PortableApplication> updates = _portableEnvironment.Applications.Updates("http://compareason.com/update");

            // Upgrade them all.
            foreach (PortableApplication application in updates)
            {
                _portableEnvironment.Applications.Add(application);
            }

            // Update last update time.
            Settings.Default.LastUpdateCheckTime = DateTime.UtcNow;

            // Schedule next auto-update check.
            _autoUpdateTime.Stop();
            if (Settings.Default.UpdateCheckInterval.TotalMilliseconds > 0)
            {
                if (Settings.Default.UpdateCheckInterval.TotalDays < 6)
                    Settings.Default.UpdateCheckInterval = new TimeSpan(6, 0, 0);
                _autoUpdateTime.Interval = Settings.Default.UpdateCheckInterval;
                _autoUpdateTime.Stop();
                _autoUpdateTime.Tick += (e, a) => Update();
            }

            Settings.Default.Save();
        }

        public void Uninstall()
        {
            MessageBox.Show("Simply delete the portable application directory.");

            Directory.CreateDirectory(_portableEnvironment.Applications.InstallationBaseDirectory);
            Process.Start(new ProcessStartInfo("explorer.exe", "/n, /e, " + _portableEnvironment.Applications.InstallationBaseDirectory));
        }

        public void Exit()
        {
            TearDownPortableEnvironment();
            App.Current.Shutdown();
        }

        private PortableEnvironment SetUpPortableEnvironment()
        {
            PortableEnvironment portableEnvironment = null;
            try
            {
                if (EnvironmentJson.Exists)
                {
                    try
                    {
                        using (var reader = new StreamReader(EnvironmentJson.OpenRead()))
                            portableEnvironment = PortableEnvironment.Load(reader);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to load " + EnvironmentJson + ": " + ex.Message, ex);
                    }
                }

                if (portableEnvironment == null)
                {
                    portableEnvironment = new PortableEnvironment();
                }

                // Create ShellLink from the shortcut info.
                portableEnvironment.Shortcuts.Add(new Shortcut
                {
                    FileName = @"%PAppsStartMenuDir%\Find more applications.lnk",
                    Target = "http://compareason.com/",
                    IconPath = Assembly.GetExecutingAssembly().Location,
                    Description = "Find more applications",
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to restore the current environment (shortcuts, environement, registry...): " + ex.Message,
                    "PApps Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            return portableEnvironment;
        }

        public void TearDownPortableEnvironment()
        {
            // Save the current portable environment configuration.
            try
            {
                if (_portableEnvironment != null)
                    using (var writer = new StreamWriter(EnvironmentJson.OpenWrite()))
                        _portableEnvironment.Save(writer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WARNING: Failed to save the current environment (shortcuts, environement, registry...): " + ex.Message, "PApps Manager",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Restore original configuration.
            try
            {
                if (_portableEnvironment != null)
                    _portableEnvironment.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("WARNING: Failed to clean-up and restore the original configuration (shortcuts, environement, registry...): " + ex.Message, "PApps Manager",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}