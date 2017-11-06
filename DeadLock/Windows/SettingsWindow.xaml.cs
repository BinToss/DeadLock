using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using DeadLock.Classes;
using Microsoft.Win32;

namespace DeadLock.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        #region Variables
        private readonly MainWindow _mw;
        private bool _originalIntegration;
        private bool _originalStartup;
        #endregion

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mw = mainWindow;

            LoadTheme();
            LoadSettings();
        }

        /// <summary>
        /// Change the visual style of the window, depending on user settings
        /// </summary>
        private void LoadTheme()
        {
            StyleManager.ChangeStyle(this);
        }

        /// <summary>
        /// Check if DeadLock starts automatically.
        /// </summary>
        /// <returns>A boolean to represent whether DeadLock starts automatically or not.</returns>
        private static bool AutoStartUp()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "DeadLock", "").ToString() == System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Check if Windows Explorer integration is enabled for the program.
        /// </summary>
        /// <returns>A boolean to represent whether Windows Explorer integration is enabled for the program.</returns>
        private static bool WindowsExplorerIntegration()
        {
            bool fileExplorerIntegration = false;
            bool folderExplorerIntegration = false;
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"*\shell\DeadLock\command"))
            {
                if (key != null && key.GetValue("", "").ToString() == System.Reflection.Assembly.GetExecutingAssembly().Location + " %1")
                {
                    fileExplorerIntegration = true;
                }
            }

            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\DeadLock\command"))
            {
                if (key != null && key.GetValue("", "").ToString() == System.Reflection.Assembly.GetExecutingAssembly().Location + " %1")
                {
                    folderExplorerIntegration = true;
                }
            }

            return folderExplorerIntegration && fileExplorerIntegration;
        }

        /// <summary>
        /// Change the GUI elements to represent the latest settings
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                ChbAutoUpdate.IsChecked = Properties.Settings.Default.AutoUpdate;
                ChbAdminWarning.IsChecked = Properties.Settings.Default.AdminWarning;
                ChbStartMinimized.IsChecked = Properties.Settings.Default.StartMinimized;
                ChbDragDrop.IsChecked = Properties.Settings.Default.AllowDragDrop;
                ChbShowDetails.IsChecked = Properties.Settings.Default.ShowDetails;

                CboStyle.SelectedValue = Properties.Settings.Default.VisualStyle;
                CpMetroBrush.Color = Properties.Settings.Default.MetroColor;
                IntBorderThickness.Value = Properties.Settings.Default.BorderThickness;

                ChbAutoOwnership.IsChecked = Properties.Settings.Default.AutoOwnership;

                ChbAutoRun.IsChecked = AutoStartUp();
                _originalStartup = (bool) ChbAutoRun.IsChecked;
                ChbExplorerIntegration.IsChecked = WindowsExplorerIntegration();
                _originalIntegration = (bool) ChbExplorerIntegration.IsChecked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure that you want to reset all settings?", "DeadLock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            try
            {
                List<string> args = new List<string>();
                if (WindowsExplorerIntegration())
                {
                    args.Add("3");
                }
                if (AutoStartUp())
                {
                    args.Add("1");
                }
                if (args.Count != 0)
                {
                    StartRegManager(args);
                }

                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                LoadSettings();
                LoadTheme();

                _mw.LoadTheme();
                _mw.LoadGlobalSettings();

                MessageBox.Show("All settings have been reset!", "DeadLock", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChbAutoUpdate.IsChecked != null) Properties.Settings.Default.AutoUpdate = (bool)ChbAutoUpdate.IsChecked;
                if (ChbAdminWarning.IsChecked != null) Properties.Settings.Default.AdminWarning = (bool)ChbAdminWarning.IsChecked;
                if (ChbStartMinimized.IsChecked != null) Properties.Settings.Default.StartMinimized = (bool)ChbStartMinimized.IsChecked;
                if (ChbDragDrop.IsChecked != null) Properties.Settings.Default.AllowDragDrop = (bool) ChbDragDrop.IsChecked;
                if (ChbShowDetails.IsChecked != null) Properties.Settings.Default.ShowDetails = (bool)ChbShowDetails.IsChecked;

                Properties.Settings.Default.VisualStyle = (string)CboStyle.SelectedValue;
                Properties.Settings.Default.MetroColor = CpMetroBrush.Color;
                if (IntBorderThickness.Value != null) Properties.Settings.Default.BorderThickness = (int)IntBorderThickness.Value;
                if (ChbAutoOwnership.IsChecked != null) Properties.Settings.Default.AutoOwnership = (bool)ChbAutoOwnership.IsChecked;

                List<string> args = new List<string>();
                if (_originalStartup != (ChbAutoRun.IsChecked == true))
                {
                    _originalStartup = ChbAutoRun.IsChecked == true;
                    args.Add(_originalStartup ? "0" : "1");
                }

                if (_originalIntegration != (ChbExplorerIntegration.IsChecked == true))
                {
                    _originalIntegration = ChbExplorerIntegration.IsChecked == true;
                    args.Add(_originalIntegration ? "2" : "3");
                }
                if (args.Count != 0)
                {
                    args.Add("\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                    StartRegManager(args);
                }

                Properties.Settings.Default.Save();

                LoadTheme();
                _mw.LoadTheme();
                _mw.LoadGlobalSettings();

                MessageBox.Show("All settings have been saved!", "DeadLock", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Start the RegManager with a list of arguments.
        /// </summary>
        /// <param name="args">A list of arguments.</param>
        private static void StartRegManager(IEnumerable<string> args)
        {
            string a = string.Join(" ", args);
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = AppDomain.CurrentDomain.BaseDirectory + "\\RegManager.exe",
                    Arguments = a,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
