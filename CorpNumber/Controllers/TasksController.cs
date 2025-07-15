using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
using System.Linq;

namespace CorpNumber.Controllers
{
    public class TasksController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public TasksController(CorpNumberDbContext context)
        {
            _context = context;
        }

        // GET: /Tasks
        public IActionResult TasksIndex()
        {
            var tasks = _context.Tasks.ToList();
            return View("TasksIndex", tasks);
        }

        // Заглушки для будущей реализации
        // GET: /Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Tasks/Edit/{id}
        public IActionResult Edit(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.CodeTask == id);
            if (task == null) return NotFound();
            return View(task);
        }

        // GET: /Tasks/Delete/{id}
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.CodeTask == id);
            if (task == null) return NotFound();
            return View(task);
        }
    }
}
