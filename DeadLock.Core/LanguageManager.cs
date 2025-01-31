﻿using System.IO;
using System.Xml.Serialization;

namespace DeadLock.Core
{
    /// <summary>
    /// The LanguageManager can be used to load or return a Language object.
    /// </summary>
    public class LanguageManager
    {
        #region Variables

        private Language _currentLanguage;

        #endregion

        /// <summary>
        /// Generate a new LanguageManager.
        /// </summary>
        public LanguageManager()
        {
            _currentLanguage = new Language();
        }

        public LanguageManager(string path)
        {
            _currentLanguage = new Language();
            LoadLanguage(path);
        }

        /// <summary>
        /// Load a custom language.
        /// </summary>
        /// <param name="path">Path to the XML language file.</param>
        /// TODO: exception documentation
        public void LoadLanguage(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(_currentLanguage.GetType());
                using (StreamReader reader = new StreamReader(path))
                {
                    _currentLanguage = (Language)serializer.Deserialize(reader);
                }
            }
            catch
            {
                LoadLanguage(1);
                throw;
            }
        }

        /// <summary>
        /// Load a language using the Resources, depending on the index.
        /// </summary>
        /// <param name="index">The index of the language that should be loaded.</param>
        public void LoadLanguage(int index)
        {
            typeof(LanguageManager).Assembly.GetManifestResourceInfo(string.Empty);
            XmlSerializer serializer = new XmlSerializer(_currentLanguage.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                string res;
                switch (index)
                {
                    case 0:
                        res = Properties.Resources.nl;
                        break;
                    case 2:
                        res = Properties.Resources.fr;
                        break;
                    case 3:
                        res = Properties.Resources.ger;
                        break;
                    case 4:
                        res = Properties.Resources.ita;
                        break;
                    case 5:
                        res = Properties.Resources.kor;
                        break;
                    case 6:
                        res = Properties.Resources.pl;
                        break;
                    case 7:
                        res = Properties.Resources.rus;
                        break;
                    case 8:
                        res = Properties.Resources.sr;
                        break;
                    case 9:
                        res = Properties.Resources.esp;
                        break;
                    case 10:
                        res = Properties.Resources.swe;
                        break;
                    case 11:
                        res = Properties.Resources.tr;
                        break;
                    default:
                        res = Properties.Resources.eng;
                        break;
                }
                writer.Write(res);
                writer.Flush();
                stream.Position = 0;
                _currentLanguage = (Language)serializer.Deserialize(stream);
                writer.Dispose();
            }
        }

        /// <summary>
        /// Get the current language.
        /// </summary>
        /// <returns>The current language.</returns>
        public Language GetLanguage()
        {
            return _currentLanguage;
        }
    }
}
