using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentTaskManagement.Data;
using StudentTaskManagement.Models;

namespace StudentTaskManagement.Controllers;

public class SubjectsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var subjects = await _context.Subjects
            .Include(s => s.Assignments)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(subjects);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subject subject)
    {
        if (await _context.Subjects.AnyAsync(
            s => s.SubjectCode == subject.SubjectCode))
        {
            ModelState.AddModelError(
                nameof(subject.SubjectCode),
                "This subject code already exists.");
        }

        if (!ModelState.IsValid)
            return View(subject);

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Subject created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var subject = await _context.Subjects.FindAsync(id);

        if (subject == null)
            return NotFound();

        return View(subject);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Subject subject)
    {
        if (id != subject.Id)
            return NotFound();

        if (await _context.Subjects.AnyAsync(
            s => s.SubjectCode == subject.SubjectCode && s.Id != id))
        {
            ModelState.AddModelError(
                nameof(subject.SubjectCode),
                "This subject code already exists.");
        }

        if (!ModelState.IsValid)
            return View(subject);

        _context.Update(subject);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Subject updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var subject = await _context.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
            return NotFound();

        return View(subject);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
            return NotFound();

        if (subject.Assignments.Any())
        {
            TempData["Error"] =
                "Cannot delete a subject that has assignments.";
            return RedirectToAction(nameof(Index));
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Subject deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}