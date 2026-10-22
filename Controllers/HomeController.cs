using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentTaskManagement.Data;

namespace StudentTaskManagement.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        ViewBag.SubjectCount = await _context.Subjects.CountAsync();
        ViewBag.AssignmentCount = await _context.Assignments.CountAsync();
        ViewBag.PendingCount = await _context.Assignments
            .CountAsync(a => a.Status == "Pending");
        ViewBag.InProgressCount = await _context.Assignments
            .CountAsync(a => a.Status == "In Progress");
        ViewBag.CompletedCount = await _context.Assignments
            .CountAsync(a => a.Status == "Completed");
        ViewBag.OverdueCount = await _context.Assignments
            .CountAsync(a => a.Status != "Completed" &&
                             a.DueDate < today);

        var upcoming = await _context.Assignments
            .Include(a => a.Subject)
            .Where(a => a.Status != "Completed")
            .OrderBy(a => a.DueDate)
            .Take(5)
            .ToListAsync();

        return View(upcoming);
    }
}
