    using Microsoft.AspNetCore.Mvc;
    using CorpNumber.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;

    namespace CorpNumber.ViewComponents
    {
        public class IncompleteOperationsViewComponent : ViewComponent
        {
            private readonly CorpNumberDbContext _context;

            public IncompleteOperationsViewComponent(CorpNumberDbContext context)
            {
                _context = context;
            }

            public IViewComponentResult Invoke()
            {
                var model = _context.Operations
                    .Where(o => o.Complete == false || o.Complete == null)
                    .Include(o => o.Phone)
                    .Include(o => o.OperationTypes)
                    .Select(o => new OperationViewModel
                    {
                        CodeOperation = o.CodeOperation,
                        RequestDate = o.RequestDate,
                        OperDate = o.OperDate,
                        Comments = o.Comments,
                        Information = o.Information,
                        CodeOperType = o.CodeOperType,
                        Type = o.OperationTypes.Type,
                        PhoneNumber = o.Phone.Number.HasValue ? o.Phone.Number.Value.ToString() : "",
                        Complete = o.Complete
                    })
                    .ToList();

                return View("Default", model);
            }
        }
    }
