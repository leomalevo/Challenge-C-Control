using Microsoft.AspNetCore.Identity;

/// <summary>
/// Service that manage the transaction in the Db context
/// </summary>
public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> Register(string username, string password)
    {
        var user = new ApplicationUser { UserName = username };
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded;
    }

    public async Task<bool> ValidateLogin(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user != null && await _userManager.CheckPasswordAsync(user, password);
    }
}