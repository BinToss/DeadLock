using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeadLock.Classes.Utils
{
    /// <summary>
    /// Helper class to open a file in VirusTotal
    /// </summary>
    internal static class VirusTotal
    {
        /// <summary>
        /// Calculate the SHA-256 value of a file
        /// </summary>
        /// <param name="path">The path to a file</param>
        /// <returns>The SHA-256 value of the given file</returns>
        private static async Task<string> GetSha256FromFile(string path)
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

        /// <summary>
        /// Open the results of a file in VirusTotal
        /// </summary>
        /// <param name="path">The path of the file</param>
        internal static async void OpenInVirusTotal(string path)
        {
            if (!File.Exists(path)) return;
            string sha256 = await GetSha256FromFile(path);
            if (!string.IsNullOrEmpty(sha256))
            {
                Process.Start("https://virustotal.com/#/file/" + sha256);
            }
        }
    }
}
