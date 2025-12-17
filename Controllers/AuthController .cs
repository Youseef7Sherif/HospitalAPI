using HospitalAPI.Data;
using HospitalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HospitalContext _context;

    public AuthController(HospitalContext context)
    {
        _context = context;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] Patients patient)
    {
        if (patient == null)
            return BadRequest("Patient data is required.");

        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.email == patient.email);

        if (existingPatient != null)
            return Conflict("Email is already registered.");

        // Hash the password before saving
        patient.password = BCrypt.Net.BCrypt.HashPassword(patient.password);

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Patient registered successfully",
            PatientId = patient.patient_id
        });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            return BadRequest("Email and password are required.");

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.email == loginRequest.Email);

        if (patient == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, patient.password))
            return Unauthorized("Invalid email or password.");

        return Ok(new
        {
            Message = "Login successful",
            PatientId = patient.patient_id,
            FirstName = patient.first_name,
            LastName = patient.last_name,
            Email = patient.email
        });
    }
    

}
