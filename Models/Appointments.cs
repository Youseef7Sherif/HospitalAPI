using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAPI.Models
{
    public class Appointments
    {
        [Key]
        [Column("appointment_id")] // اسم العمود في SQL
        public int AppointmentId { get; set; }

        [Required]
        [Column("appointment_date")] // اسم العمود في SQL
        public DateTime AppointmentDate { get; set; }

        [Column("queue_number")] // اسم العمود في SQL
        public int QueueNumber { get; set; }

        [Required]
        [Column("patient_id")] // اسم العمود في SQL
        public int PatientId { get; set; }

        [ForeignKey("PatientId")] // يربط الـPatientId مع جدول Patients
        public Patients Patient { get; set; }
    }
}
