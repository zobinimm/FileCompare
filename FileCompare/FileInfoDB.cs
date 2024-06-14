using FileCompare.Models;
using Microsoft.EntityFrameworkCore;

namespace FileCompare
{
    public class FileInfoDB : DbContext
    {
        public DbSet<FileContext> FileContext { get; set; }

        public string DbPath { get; }
        public FileInfoDB()
        {

            var path = Directory.GetCurrentDirectory();
            DbPath = Path.Combine(path, "FileContext.db");
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=" + DbPath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileContext>()
                  .HasKey(m => new { m.FolderPath, m.FilePath });
        }
    }
}
