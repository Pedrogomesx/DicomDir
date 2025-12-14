using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DicomDir.Models
{
    public class SeriesTable
    {
        [Key]
        public string SeriesUID { get; set; }

        public string Modality { get; set; }

        public string? SeriesNumber { get; set; }

        public string StudyUID { get; set; }

        [ForeignKey("StudyUID")]
        public virtual StudyTable Study { get; set; }
    }
}