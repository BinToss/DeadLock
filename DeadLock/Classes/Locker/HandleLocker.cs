using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;

namespace DeadLock.Classes.Locker
{
    /// <inheritdoc />
    /// <summary>
    /// This class represents the details of a process or handle that is currently locking another file
    /// </summary>
    internal sealed class HandleLocker : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        /// The event that will be called when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _fileName;
        /// <summary>
        /// The name of the file that is locking a file or folder including the extension
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set
            {
                if (value == _fileName) return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        private string _filePath;
        /// <summary>
        /// The full path of the file that is locking a file or folder
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (value == _filePath) return;
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private string _lockedPath;
        /// <summary>
        /// The path to the handle that is currently being locked
        /// </summary>
        public string LockedPath
        {
            get => _lockedPath;
            set
            {
                if (value == _lockedPath) return;
                _lockedPath = value;
                OnPropertyChanged();
            }
        }

        private int _processId;
        /// <summary>
        /// The process ID of the Process that is locking a file or folder
        /// </summary>
        public int ProcessId
        {
            get => _processId;
            set
            {
                if (value == _processId) return;
                _processId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The Process that is locking a file or folder
        /// </summary>
        private readonly Process _process;

        /// <summary>
        /// Initialize a new HandleLocker
        /// </summary>
        /// <param name="p">The Process that is locking a file or folder</param>
        /// <param name="lockedPath">The path to the handle that is currently being locked</param>
        internal HandleLocker(Process p, string lockedPath)
        {
            _process = p;
            FilePath = GetMainModuleFilepath(p.Id);
            FileName = Path.GetFileName(FilePath);
            LockedPath = lockedPath;
            ProcessId = p.Id;
        }

        /// <summary>
        /// Get the Process that is locking a file or folder
        /// </summary>
        /// <returns>The Process that is locking a file or folder</returns>
        internal Process GetProcess()
        {
            return _process;
        }

        /// <summary>
        /// Get the file path of the Process that is associated with the HandleLocker. Warning: Might require a lot of system resources
        /// </summary>
        /// <param name="processId">The process ID of the Process that is associated with the HandleLocker</param>
        /// <returns>The file path that is associated with the Process of the HandleLocker</returns>
        private static string GetMainModuleFilepath(int processId)
        {
            string filepath = "";
            try
            {
                string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQueryString))
                {
                    using (ManagementObjectCollection results = searcher.Get())
                    {
                        ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                        if (mo != null)
                        {
                            filepath = (string)mo["ExecutablePath"];
                        }
                    }
                }
            }
            catch (Win32Exception) { }
            return filepath;
        }

        /// <summary>
        /// The method that will be called when a property has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
