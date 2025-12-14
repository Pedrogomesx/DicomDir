using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DicomDir.Models
{
    public class StudyTable
    {
        [Key]
        public string StudyInstanceUID { get; set; } 

        public string Accessionnumber { get; set; }
        public string PatientName { get; set; }
        public DateTime? StudyDate { get; set; }
        public string Modality { get; set; }
        public string PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual PatientTable Patient { get; set; }

        public string StudyDescription { get; set; }
    }
}