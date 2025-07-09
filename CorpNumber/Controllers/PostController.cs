using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CorpNumber.Controllers
{
    public class PostController : Controller
    {
        private readonly CorpNumberDbContext _context;

        public PostController(CorpNumberDbContext context)
        {
            _context = context;
        }

        public IActionResult PostIndex()
        {
            var categories = _context.PostCategories.OrderBy(c => c.Category).ToList();

            var model = _context.Posts
                .Include(p => p.PostCategoryNavigation)
                .Select(p => new PostViewModel
                {
                    CodePost = p.CodePost,
                    Post = p.Postt,
                    PostCh = p.PostCh,
                    Category = p.Category,
                    PostCategoryName = p.PostCategoryNavigation != null 
                        ? p.PostCategoryNavigation.Category + " " + p.PostCategoryNavigation.CategoryCh : "-",
                    EmployeeCount = _context.Employees.Count(e => e.Post == p.CodePost && (e.Fired == false || e.Fired == null))
                })

                .ToList();

            ViewBag.PostCategories = categories;
            return View(model);
        }

        //Метод фильтрации таблицы постов с поиском и выбором категории
        [HttpGet]
        public IActionResult FilterPosts(string searchText, int? categoryId)
        {
            var query = _context.Posts.Include(p => p.PostCategoryNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p => p.Postt != null && p.Postt.Contains(searchText));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Category == categoryId.Value);
            }

            var model = query.Select(p => new PostViewModel
            {
                CodePost = p.CodePost,
                Post = p.Postt,
                PostCh = p.PostCh,
                Category = p.Category,
                PostCategoryName = p.PostCategoryNavigation != null
                    ? p.PostCategoryNavigation.Category + " " + p.PostCategoryNavigation.CategoryCh
                    : "-",
                EmployeeCount = _context.Employees.Count(e => e.Post == p.CodePost && (e.Fired == false || e.Fired == null))
            }).ToList();

            return PartialView("_PostTable", model);
        }



        [HttpGet]
        public IActionResult GetPostById(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.CodePost == id);
            if (post == null) return NotFound();

            return Json(new
            {
                codePost = post.CodePost,
                post = post.Postt,
                postCh = post.PostCh,
                category = post.Category
            });
        }

        [HttpPost]
        public IActionResult SavePost(Post model)
        {
            if (model.CodePost == 0)
            {
                _context.Posts.Add(model);
            }
            else
            {
                var existing = _context.Posts.Find(model.CodePost);
                if (existing == null) return NotFound();

                existing.Postt = model.Postt;
                existing.PostCh = model.PostCh;
                existing.Category = model.Category;
            }

            _context.SaveChanges();
            return Ok();
        }
    }
}
