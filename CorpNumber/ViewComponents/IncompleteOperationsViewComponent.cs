using Microsoft.AspNetCore.Mvc;
using CorpNumber.Models;
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
                .Where(o => o.Complete != true)
                .Select(o => new OperationViewModel
                {
                    CodeOperation = o.CodeOperation,
                    CodeOperType = o.CodeOperType,
                    RequestDate = o.RequestDate,
                    OperDate = o.OperDate,
                    Number = o.Number,
                    Complete = o.Complete,
                    Comments = o.Comments,
                    Information = o.Information,
                    OrderN = o.OrderN,
                    Type = o.OperationTypes != null ? o.OperationTypes.Type : null,
                    PhoneNumber = o.Phone != null ? o.Phone.Number.ToString() : null
                })
                .ToList();

            return View("Default", model);
        }
    }
}
