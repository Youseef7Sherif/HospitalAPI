using HospitalAPI.Data;
using HospitalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HospitalContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(HospitalContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }



    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] Patients patient)
    {
        if (patient == null)
            return BadRequest(new { message = "Patient data is required." });

        // Validate email contains "@"
        if (string.IsNullOrEmpty(patient.email) || !patient.email.Contains("@"))
            return BadRequest(new { message = "Invalid email. Email must contain '@'." });

        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.email == patient.email);

        if (existingPatient != null)
            return Conflict(new { message = "Email is already registered." });

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
            return BadRequest(new { message = "Email and password are required." });

        if (!loginRequest.Email.Contains("@"))
            return BadRequest(new { message = "Invalid email. Email must contain '@'." });

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.email == loginRequest.Email);

        if (patient == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, patient.password))
            return Unauthorized(new { message = "Invalid email or password." });


        // توليد JWT
        var token = GenerateJwtToken(patient);

        return Ok(new
        {
            Message = "Login successful",
            Token = token,  // هنا التوكن
            PatientId = patient.patient_id,
            FirstName = patient.first_name,
            LastName = patient.last_name,
            Email = patient.email
        });
    }

    // دالة توليد JWT
    private string GenerateJwtToken(Patients patient)
    {
        var claims = new[]
        {
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, patient.email),
        new System.Security.Claims.Claim("PatientId", patient.patient_id.ToString()),
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }



}
