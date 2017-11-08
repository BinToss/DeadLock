using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DeadLock.Classes.Locker;
using DeadLock.Classes.Native;

namespace DeadLock.Classes.Utils
{
    internal static class HashCalculator
    {
        /// <summary>
        /// Calculate the SHA-256 value of a file
        /// </summary>
        /// <param name="path">The path to a file</param>
        /// <returns>The SHA-256 value of the given file</returns>
        internal static async Task<string> GetSha256FromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            StringBuilder sb = new StringBuilder();
            await Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    using (SHA256 sha = new SHA256Managed())
                    {

                        foreach (byte t in sha.ComputeHash(fs))
                        {
                            sb.Append(t.ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            });
            return sb.ToString();
        }
    }
}
