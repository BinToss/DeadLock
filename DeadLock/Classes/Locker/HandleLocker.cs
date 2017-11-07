using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using DeadLock.Classes.Native;

namespace DeadLock.Classes.Locker
{
    /// <inheritdoc />
    /// <summary>
    /// This class represents a file including details such as ownership and lock status
    /// </summary>
    internal sealed class HandleLocker : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        /// The event that will be called when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _actualPath;
        /// <summary>
        /// The full path of the file
        /// </summary>
        public string ActualPath
        {
            get => _actualPath;
            set
            {
                if (value == ActualPath) return;
                _actualPath = value;
                OnPropertyChanged();
            }
        }

        private string _status;
        /// <summary>
        /// The current status of the file
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _ownership;
        /// <summary>
        /// An indicator whether ownership of the file is valid or not
        /// </summary>
        public string Ownership
        {
            get => _ownership;
            set
            {
                if (value == _ownership) return;
                _ownership = value;
                OnPropertyChanged();
            }
        }

        private bool _isCancelled;

        /// <summary>
        /// A boolean indicating whether the operation is cancelled or not
        /// </summary>
        public bool IsCancelled
        {
            get => _isCancelled;
            set
            {
                if (value == _isCancelled) return;
                _isCancelled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initialize a new HandeLocker
        /// </summary>
        /// <param name="path">The path of the file</param>
        internal HandleLocker(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File does not exist!", path);
            }
            ActualPath = path;
            Status = "Unknown";
            Ownership = "Unknown";
            IsCancelled = false;
        }

        /// <summary>
        /// Check whether or not the operator has ownership rights to the file or folder that is associated with the HandleLocker
        /// </summary>
        internal void HasOwnership()
        {
            bool isWriteAccess = false;
            try
            {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(ActualPath).GetAccessRules(true, true, typeof(NTAccount));
                if (collection.Cast<FileSystemAccessRule>().Any(rule => rule.AccessControlType == AccessControlType.Allow))
                {
                    isWriteAccess = true;
                }
            }
            catch (Exception)
            {
                isWriteAccess = false;
            }

            Ownership = isWriteAccess ? "True" : "False";
        }

        /// <summary>
        /// Get a list of files inside a folder that are accessible to the operator
        /// </summary>
        /// <param name="rootPath">The root path of the folder</param>
        /// <param name="patternMatch">The pattern that should be used to find the files</param>
        /// <param name="searchOption">The SearchOption that should be used to find the files</param>
        /// <returns>A list of files inside a folder that are accessible to the operator</returns>
        private static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption)
        {
            List<string> foundFiles = new List<string>();

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foundFiles = subDirs.Aggregate(foundFiles, (current, dir) => current.Concat(GetDirectoryFiles(dir, patternMatch, searchOption)).ToList());
                }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            }

            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)).ToList(); // Add files from the current directory
            }
            catch (UnauthorizedAccessException)
            {
                
            }
            return foundFiles;
        }

        /// <summary>
        /// Retrieve the handles that are locking the file or folder
        /// </summary>
        /// <returns>A list of handles that are currently locking the file or folder</returns>
        internal async Task<List<HandleLockerDetail>> GetHandleLockers()
        {
            List<HandleLockerDetail> handleLockers;
            if (File.GetAttributes(ActualPath).HasFlag(FileAttributes.Directory))
            {
                handleLockers = await GetDirectoryLockers();
            }
            else
            {
                handleLockers = await GetFileLockers();
            }

            if (handleLockers == null || handleLockers.Count == 0)
            {
                Status = "Unlocked";
            }
            else
            {
                Status = "Locked";
            }

            return handleLockers;
        }

        /// <summary>
        /// Get a list of processes that are locking a file
        /// </summary>
        /// <returns>A list of processes that are blocking a file</returns>
        private async Task<List<HandleLockerDetail>> GetFileLockers()
        {
            List<HandleLockerDetail> lockers = new List<HandleLockerDetail>();
            await Task.Run(() =>
            {
                try
                {
                    if (IsCancelled) throw new OperationCanceledException();
                    lockers.AddRange(NativeMethods.FindLockingProcesses(ActualPath).Select(p => new HandleLockerDetail(p)));
                }
                catch (OperationCanceledException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            });
            return lockers;
        }

        /// <summary>
        /// Get a list of processes that are locking a directory
        /// </summary>
        /// <returns>A list of processes that are locking a directory</returns>
        private async Task<List<HandleLockerDetail>> GetDirectoryLockers()
        {
            List<HandleLockerDetail> lockers = new List<HandleLockerDetail>();
            await Task.Run(() =>
            {
                try
                {
                    foreach (string path in GetDirectoryFiles(ActualPath, "*.*", SearchOption.AllDirectories))
                    {
                        foreach (Process p in NativeMethods.FindLockingProcesses(path))
                        {
                            bool add = true;
                            foreach (HandleLockerDetail l in lockers)
                            {
                                if (IsCancelled) throw new OperationCanceledException();
                                try
                                {
                                    if (l.ProcessId == p.Id)
                                    {
                                        add = false;
                                    }
                                }
                                catch (Win32Exception)
                                {
                                    add = false;
                                }
                            }
                            if (add)
                            {
                                lockers.Add(new HandleLockerDetail(p));
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            });
            return lockers;
        }

        /// <summary>
        /// Call this method when a property has changed
        /// </summary>
        /// <param name="propertyName">The property that has changed</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
