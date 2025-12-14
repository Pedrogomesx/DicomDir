using System.ComponentModel.DataAnnotations;

namespace DicomDir.Models
{
    public class PatientTable
    {
        [Key]
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public DateTime PatientBirthDay { get; set; }
    }
}