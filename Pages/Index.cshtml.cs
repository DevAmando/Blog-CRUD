
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models.ViewModels;

namespace Blog.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BlogDataContext _context;

        public IndexModel(BlogDataContext context)
        {
            _context = context;
        }

        public List<ListPostsViewModel> Posts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Posts = await _context
                .Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .OrderByDescending(x => x.LastUpdateDate)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title ?? "Sem título",
                    Summary = x.Summary ?? "",
                    Slug = x.Slug ?? "",
                    Category = x.Category != null ? x.Category.Name : "Sem categoria",
                    LastUpdateDate = x.LastUpdateDate,
                    Author = x.Author != null ? x.Author.Name ?? "Autor desconhecido" : "Autor desconhecido"
                })
                .ToListAsync();
        }
    }
}

