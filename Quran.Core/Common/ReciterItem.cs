using System;
using System.IO;
using System.Threading.Tasks;
using Quran.Core.Utils;
using SQLite.Net.Attributes;
using Windows.Storage;

namespace Quran.Core.Common
{
    public class ReciterItem
    {
        [Column("id"), PrimaryKey]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("url")]
        public string ServerUrl { get; set; }
        [Column("local")]
        public string LocalFolderName { get; set; }
        public async Task<StorageFolder> GetStorageFolder()
        {
            StorageFolder localFolder = await FileUtils.AudioFolder.TryGetItemAsync(LocalFolderName) as StorageFolder;
            if (localFolder == null)
            {
                localFolder = await FileUtils.AudioFolder.CreateFolderAsync(LocalFolderName);
            }

            if (localFolder == null)
            {
                throw new InvalidDataException("Reciter directory cannot be null.");
            }
            return localFolder;
        }
        [Column("dbname")]
        public string GaplessDatabasePath { get; set; }
        [Column("gapless")]
        public bool IsGapless { get; set; }
    }    
}
