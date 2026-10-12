
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestForge.Web.Data;
using TestForge.Web.Models;

namespace TestForge.Web.Controllers;

public class TasksController(AppDbContext context) : Controller
{
    private readonly AppDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var tasks = await _context.Tasks
            .OrderByDescending(task => task.CreatedAtUtc)
            .ToListAsync();

        return View(tasks);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
            return View(task);

        task.CreatedAtUtc = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,IsCompleted")] TaskItem updatedTask)
    {
        if (id != updatedTask.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(updatedTask);

        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.IsCompleted = updatedTask.IsCompleted;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
