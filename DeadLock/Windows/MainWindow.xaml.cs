using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DeadLock.Classes.GUI;
using DeadLock.Classes.Locker;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace DeadLock.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variables
        private readonly UpdateManager.UpdateManager _updateManager;
        private readonly ObservableCollection<QuestionablePath> _pathList;
        private readonly ObservableCollection<HandleLocker> _detailList;
        private readonly DataTemplate _defaultColumnTemplate;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _updateManager = new UpdateManager.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/DeadLock/update.xml", "DeadLock");
            _pathList = new ObservableCollection<QuestionablePath>();
            _detailList = new ObservableCollection<HandleLocker>();
            _defaultColumnTemplate = GvcLockedPath.CellTemplate;

            LsvFiles.ItemsSource = _pathList;
            LsvDetails.ItemsSource = _detailList;

            LoadTheme();
            LoadSettings();
            LoadArguments();

            LblVersion.Content += Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        /// <summary>
        /// Load all variables as file paths and load them into the GUI
        /// </summary>
        private void LoadArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                AddPath(args[i]);
            }
        }

        /// <summary>
        /// Check whether the current user is running DeadLock using administrative rights or not
        /// </summary>
        /// <returns>A boolean to represent whether the current user is an administrator or not</returns>
        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Load the user settings
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (Properties.Settings.Default.AdminWarning && !IsAdministrator())
                {
                    MessageBox.Show("DeadLock might not function correctly without administrative rights!", "DeadLock", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                if (Properties.Settings.Default.AutoUpdate)
                {
                    Update(true, false);
                }
                if (Properties.Settings.Default.StartMinimized)
                {
                    WindowState = WindowState.Minimized;
                }
                LoadGlobalSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load settings that can be accessed by other objects and modified at run-time
        /// </summary>
        internal void LoadGlobalSettings()
        {
            try
            {
                AllowDrop = Properties.Settings.Default.AllowDragDrop;
                MniDetails.IsChecked = Properties.Settings.Default.ShowDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change the visual style of the window, depending on user settings
        /// </summary>
        internal void LoadTheme()
        {
            StyleManager.ChangeStyle(this);
        }

        /// <summary>
        /// Check for application updates
        /// </summary>
        /// <param name="showErrors">Show a message if an error occurs</param>
        /// <param name="showNoUpdate">Show a message if no updates are available</param>
        private void Update(bool showErrors, bool showNoUpdate)
        {
            _updateManager.CheckForUpdate(showErrors, showNoUpdate);
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != true) return;
            foreach (string s in ofd.FileNames)
            {
                AddPath(s);
            }
        }

        private void DetailsItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (LsvDetails == null) return;
            LsvDetails.Visibility = MniDetails.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            //SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void RestartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\help.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WebsiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://codedead.com/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LicenseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\gpl.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Update(true, true);
        }

        /// <summary>
        /// Add a file or folder to the GUI
        /// </summary>
        /// <param name="path">The path of the file or folder that should be added to the GUI</param>
        private void AddPath(string path)
        {
            bool already = false;
            foreach (QuestionablePath hl in LsvFiles.Items)
            {
                if (hl.ActualPath != path) continue;
                already = true;
                break;
            }

            if (already) return;
            {
                try
                {
                    QuestionablePath hl = new QuestionablePath(path);
                    _pathList.Add(hl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null) return;

            foreach (string s in files)
            {
                AddPath(s);
            }
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            try
            {
                foreach (string s in Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories))
                {
                    AddPath(s);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open all files in the directory! Please make sure that you have the appropriate rights to access this folder.", "DeadLock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LsvFiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(((FrameworkElement) e.OriginalSource).DataContext is QuestionablePath item)) return;
            item.HasOwnership();

            List<HandleLocker> details = await item.GetHandleLockers();
            _detailList.Clear();
            foreach (HandleLocker detail in details)
            {
                _detailList.Add(detail);
            }
        }

        private void MniLockedPath_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (GvcLockedPath == null) return;
            GvcLockedPath.Width = MniLockedPath.IsChecked ? 100 : 0;
        }
    }
}
