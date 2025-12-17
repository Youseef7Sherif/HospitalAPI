using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalAPI.Models
{
    public class Patients
    {
        [Key]
        public int patient_id { get; set; }

        [Required]
        public string first_name { get; set; }

        [Required]
        public string last_name { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string address { get; set; }

        [Required]
        public DateTime date_of_birth { get; set; }

        [Required]
        public string occupation { get; set; }

        [Required]
        public string national_id { get; set; }

    }
}