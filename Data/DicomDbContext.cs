using DicomDir.Models;
using Microsoft.EntityFrameworkCore;

namespace DicomDir.Data
{
    public class DicomDbContext : DbContext
    {
        public DicomDbContext(){}
        public DicomDbContext(DbContextOptions<DicomDbContext> options) : base(options)
        {
        }

        public DbSet<PatientTable> PatientTable { get; set; }
        public DbSet<StudyTable> StudyTable { get; set; }
        public DbSet<SeriesTable> SeriesTable{ get; set; }
        public DbSet<ImageTable> ImageTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=:memory:");
            }
        }
    }
}
