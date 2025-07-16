using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
using System;
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

        // Главная страница задач
        public IActionResult TasksIndex()
        {
            var tasks = _context.Tasks.ToList();
            return View(tasks);
        }

        // Частичное обновление таблицы
        public IActionResult TasksIndexPartial()
        {
            var tasks = _context.Tasks.ToList();
            return PartialView("_TasksTablePartial", tasks);
        }

        // Получение одной задачи
        public IActionResult GetTask(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.CodeTask == id);
            if (task == null) return NotFound();

            return Json(new
            {
                codeTask = task.CodeTask,
                createDate = task.CreateDate?.ToString("yyyy-MM-dd"),
                taskDate = task.TaskDate?.ToString("yyyy-MM-dd"),
                taskText = task.TaskText,
                complete = task.Complete
            });
        }

        // Создание новой задачи
        [HttpPost]
        public IActionResult Create(TaskItem task)
        {
            if (string.IsNullOrWhiteSpace(task.TaskText))
                return BadRequest("Текст задачи обязателен");

            task.Complete = false;
            _context.Tasks.Add(task);
            _context.SaveChanges();
            return Ok();
        }

        // Обновление существующей задачи
        [HttpPost]
        public IActionResult Update(TaskItem task)
        {
            var existing = _context.Tasks.FirstOrDefault(t => t.CodeTask == task.CodeTask);
            if (existing == null) return NotFound();

            existing.CreateDate = task.CreateDate;
            existing.TaskDate = task.TaskDate;
            existing.TaskText = task.TaskText;
            _context.SaveChanges();
            return Ok();
        }

        // Удаление задачи
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.CodeTask == id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return Ok();
        }

        // Переключение выполнено/не выполнено
        [HttpPost]
        public IActionResult ToggleComplete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.CodeTask == id);
            if (task == null) return NotFound();

            task.Complete = !(task.Complete ?? false);
            _context.SaveChanges();
            return Ok();
        }
    }
}
