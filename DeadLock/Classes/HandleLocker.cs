using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DeadLock.Classes
{
    /// <summary>
    /// This class represents a file which may or may not be unlocked or taken ownership of
    /// </summary>
    internal sealed class HandleLocker
    {
        /// <summary>
        /// The full path of the file
        /// </summary>
        public string ActualPath { get; set; }
        /// <summary>
        /// The current status of the file
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// An indicator whether ownership of the file is valid or not
        /// </summary>
        public string Ownership { get; set; }
        /// <summary>
        /// A list of details concerning the file in question
        /// </summary>
        public List<HandleLockerDetails> Details { get; set; }

        internal HandleLocker(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File does not exist!", path);
            }
            ActualPath = path;
            Status = "Unknown";
            Ownership = HasOwnership().ToString();
        }

        /// <summary>
        /// Check whether or not the operator has ownership rights to the file or folder that is associated with the ListViewLocker.
        /// </summary>
        /// <returns>A boolean that represents whether or not the operator has ownership rights to the file or folder that is associated with the ListViewLocker.</returns>
        internal bool HasOwnership()
        {
            bool isWriteAccess = false;
            try
            {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(ActualPath).GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in collection)
                {
                    if (rule.AccessControlType != AccessControlType.Allow) continue;
                    isWriteAccess = true;
                    break;
                }
            }
            catch (Exception)
            {
                isWriteAccess = false;
            }
            return isWriteAccess;
        }
    }
}
