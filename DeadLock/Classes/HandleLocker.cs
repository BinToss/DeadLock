using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DeadLock.Classes
{
    internal sealed class HandleLocker
    {
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


        public string ActualPath { get; set; }
        public string Status { get; set; }
        public string Ownership { get; set; }
        public List<HandleLockerDetails> Details{get; set; }
    }
}
