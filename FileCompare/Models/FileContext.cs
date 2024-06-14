using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileCompare.Models
{
    [Table("FileContext")]
    public class FileContext
    {
        [Key, Column(Order = 0)]
        public string FolderPath { get; set; }

        [Key, Column(Order = 1)]
        public string FilePath { get; set; }

        [Column("FileName")]
        public string FileName { get; set; } = default!;

        [Column("FileMd5")]
        public string FileMd5 { get; set; } = default!;

        [Column("FileSize")]
        public long FileSize { get; set; } = default!;
    }
}
