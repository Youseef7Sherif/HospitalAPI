using HospitalAPI.Data;
using HospitalAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HospitalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly HospitalContext _context;

        public BookController(HospitalContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("BookAppointmentNow")]
        public async Task<IActionResult> BookAppointmentNow()
        {
            // استخراج PatientId من الـ JWT token
            var patientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PatientId");
            if (patientIdClaim == null)
                return Unauthorized(new { message = "PatientId not found in token." });

            int patientId = int.Parse(patientIdClaim.Value);

            // التأكد أن المريض موجود
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.patient_id == patientId);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            // تحديد ميعاد اليوم التالي
            DateTime nextDay = DateTime.Today.AddDays(1);

            // لو اليوم التالي جمعة، يحجز السبت
            if (nextDay.DayOfWeek == DayOfWeek.Friday)
                nextDay = nextDay.AddDays(1);

            // التأكد أن المريض مش محجز لنفس اليوم
            bool alreadyBooked = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId
                               && EF.Functions.DateDiffDay(a.AppointmentDate, nextDay) == 0);

            if (alreadyBooked)
                return BadRequest(new { message = "You already have an appointment for the next day." });

            // حساب رقم الدور لليوم ده
            int queueNumber = await _context.Appointments
                .CountAsync(a => EF.Functions.DateDiffDay(a.AppointmentDate, nextDay) == 0) + 1;

            // إنشاء الحجز
            var appointment = new Appointments
            {
                PatientId = patientId,
                AppointmentDate = nextDay,
                QueueNumber = queueNumber
            };

            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "You already have an appointment for this day." });
            }


            // الرد للمريض
            return Ok(new
            {
                PatientName = $"{patient.first_name} {patient.last_name}",
                AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                QueueNumber = appointment.QueueNumber
            });
        }
    }
}
