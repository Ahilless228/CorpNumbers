using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CorpNumber.Controllers
{
    public class PostCategoryController : Controller
    {
        private readonly CorpNumberDbContext _context;
        public PostCategoryController(CorpNumberDbContext context)
        {
            _context = context;
        }
        public IActionResult PostCategoryIndex() 
        {
            var model= _context.PostCategories
                .Select(c => new PostCategory
                {
                    Code = c.Code,
                    Category = c.Category,
                    CategoryCh = c.CategoryCh
                })
                .OrderBy(c => c.Category)
                .ToList();
            return View(model);
        }
        //метод поиска по названию категории
        [HttpGet]
        public IActionResult FilterPostCategories(string searchText)
        {
            var query = _context.PostCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => c.Category!=null && c.Category.Contains(searchText));
            }

            var model = query.OrderBy(c => c.Category).ToList();
            return PartialView("_PostCategoryTable", model);
        }

        [HttpGet]
        public IActionResult GetPostCategoryById(int id)
        {
            var category = _context.PostCategories.FirstOrDefault(c => c.Code == id);
            if (category == null) return NotFound();

            return Json(new { code = category.Code, category = category.Category, categoryCh = category.CategoryCh });
        }
        //метод сохранения и обновления категории поста
        [HttpPost]
        public IActionResult SavePostCategory(PostCategory model)
        {
            if (model.Code == 0)
            {
                _context.PostCategories.Add(model);
            }
            else
            {
                var existing = _context.PostCategories.Find(model.Code);
                if (existing == null) return NotFound();

                existing.Category = model.Category;
                existing.CategoryCh = model.CategoryCh;
            }

            _context.SaveChanges();
            return Ok();
        }
    }
}
