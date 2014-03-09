using System.Globalization;
using System.IO;
using System.Linq;
using BananaMpq.Geometry;
using CrystalMpq.WoW;

namespace BananaMpq.View.Infrastructure
{
    public class MpqFileReader : IFileReader
    {
        private readonly WoWMpqFileSystem _fileSystem;

        public MpqFileReader() : this(WoWInstallation.Find())
        {
        }

        public MpqFileReader(WoWInstallation installation)
        {
            var languagePack = installation.LanguagePacks.FirstOrDefault(l => l.Culture.Equals(CultureInfo.CurrentUICulture))
                ?? installation.LanguagePacks.FirstOrDefault(l => l.Culture.Name == "en-GB")
                ?? installation.LanguagePacks.First();
            _fileSystem = installation.CreateFileSystem(languagePack, false);
        }

        #region Implementation of IFileReader

        public byte[] Read(string fileName)
        {
            using (var stream = Open(fileName))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        #endregion

        public Stream Open(string fileName)
        {
            return _fileSystem.FindFile(fileName).Open();
        }
    }
}