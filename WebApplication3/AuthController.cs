using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;

/// <summary>
/// Api that register users and login
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _authService = new AuthService(dbContext, userManager);
    }
    /// <summary>
    /// Register. Saves new user and password
    /// </summary>
    /// <param name="model">Contains user and password</param>
    /// <returns>Returns 200 for success operation. 400 for failed operation</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        bool isRegistered = await _authService.Register(model.Username, model.Password);
        return isRegistered ? Ok("Usuario registrado correctamente.") : BadRequest("Registración fallida.");
    }

    /// <summary>
    /// Login. Check if the user and password are ok
    /// </summary>
    /// <param name="model">Contains user and password</param>
    /// <returns>Returns 200 for success operation. 400 for failed operation</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var isValidLogin = await _authService.ValidateLogin(model.Username, model.Password);

        if (!isValidLogin)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        return Ok(new { message = "Login exitoso" });
    }
}