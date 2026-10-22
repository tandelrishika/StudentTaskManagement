using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentTaskManagement.Data;
using StudentTaskManagement.Models;

namespace StudentTaskManagement.Controllers;

public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssignmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? searchString, string? status)
    {
        var query = _context.Assignments
            .Include(a => a.Subject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(a =>
                a.Title.Contains(searchString) ||
                (a.Subject != null &&
                 a.Subject.Name.Contains(searchString)));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            new[] { "Pending", "In Progress", "Completed" }
                .Contains(status))
        {
            query = query.Where(a => a.Status == status);
        }

        ViewBag.SearchString = searchString;
        ViewBag.Status = status;

        return View(await query.OrderBy(a => a.DueDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadSubjects();

        return View(new Assignment
        {
            DueDate = DateTime.Today.AddDays(7),
            Status = "Pending"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Assignment assignment)
    {
        if (!await _context.Subjects
            .AnyAsync(s => s.Id == assignment.SubjectId))
        {
            ModelState.AddModelError(
                nameof(assignment.SubjectId),
                "Please select a valid subject.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSubjects(assignment.SubjectId);
            return View(assignment);
        }

        assignment.CreatedDate = DateTime.Now;
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Assignment created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var assignment = await _context.Assignments.FindAsync(id);

        if (assignment == null)
            return NotFound();

        await LoadSubjects(assignment.SubjectId);
        return View(assignment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Assignment assignment)
    {
        if (id != assignment.Id)
            return NotFound();

        if (!await _context.Subjects
            .AnyAsync(s => s.Id == assignment.SubjectId))
        {
            ModelState.AddModelError(
                nameof(assignment.SubjectId),
                "Please select a valid subject.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSubjects(assignment.SubjectId);
            return View(assignment);
        }

        var existing = await _context.Assignments.FindAsync(id);

        if (existing == null)
            return NotFound();

        existing.Title = assignment.Title;
        existing.Description = assignment.Description;
        existing.SubjectId = assignment.SubjectId;
        existing.DueDate = assignment.DueDate;
        existing.Status = assignment.Status;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Assignment updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var assignment = await _context.Assignments
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null)
            return NotFound();

        return View(assignment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);

        if (assignment == null)
            return NotFound();

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Assignment deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSubjects(int? selectedId = null)
    {
        ViewBag.SubjectId = new SelectList(
            await _context.Subjects.OrderBy(s => s.Name).ToListAsync(),
            "Id",
            "Name",
            selectedId);
    }
}