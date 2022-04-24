using System;
using System.Windows.Forms;

namespace DeadLock.WinForms.Helpers
{
    /// <summary>
    ///     Helpers for wrapping DeadLock.Core.LanguageManager
    /// </summary>
    internal static class LanguageManager
    {
        public static void LoadLanguage(this Core.LanguageManager manager, string path)
        {
            try
            {
                manager.LoadLanguage(path);
            }
            catch (Exception ex)
            {
                // TODO: use modern-style MessageBox
                // MessageBoxAdv.Show(ex.Message, "DeadLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(ex.Message, "DeadLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                manager.LoadLanguage(1);
            }
        }
    }
}
