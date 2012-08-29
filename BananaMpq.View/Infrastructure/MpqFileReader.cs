using System.IO;
using System.Linq;
using BananaMpq.Geometry;
using CrystalMpq.WoW;

namespace BananaMpq.View.Infrastructure
{
    public class MpqFileReader : IFileReader
    {
        private static readonly WoWInstallation Installation = WoWInstallation.Find();
        private readonly WoWMpqFileSystem _fileSystem;

        public MpqFileReader()
        {
            var english = Installation.LanguagePacks.First(l => l.Culture.Name == "en-GB");
            _fileSystem = Installation.CreateFileSystem(english, false);
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