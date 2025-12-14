
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DicomDir.Models
{
    public class ImageTable
    {
        [Key]
        public string InstanceUID { get; set; }
        [ForeignKey("StudyUID")]
        public virtual StudyTable StudyUID_Fkey { get; set; }
        [ForeignKey("SeriesUID")]
        public virtual SeriesTable SeriesUID_Fkey { get; set; }
        public string StudyUID { get; set; }
        public string SeriesUID { get; set; }
        public string SopClassUID { get; set; }
        public string PathFile { get; set; }
    }
}
