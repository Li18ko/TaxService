using TaxService.Data;
using TaxService.Models;

namespace TaxService.Services;

public class LogService
{
    private readonly ApplicationDbContext _context;

    public LogService(ApplicationDbContext context) {
        _context = context;
    }

    public async Task LogUserAction(int userId, string action) {
        var log = new Log
        {
            Userid = userId,
            Action = action
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }
}
