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
    }
}
