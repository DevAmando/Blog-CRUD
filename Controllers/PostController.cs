namespace Blog.Controllers
{
    using Blog.Models.ViewModels;
    using Blog.Data;
    using Blog.Models;
    using Blog.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;


    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly BlogDataContext _dbcontext;

        public PostController(BlogDataContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetPosts([FromQuery] int skip = 0, [FromQuery] int take = 25)
        {
            if (take > 100) take = 100;
            var posts = await _dbcontext
                .Posts
                .AsNoTracking()
                .Skip(skip) 
                .Take(take)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Category = x.Category != null ? x.Category.Name : "Sem Categoria",
                    LastUpdateDate = x.LastUpdateDate,
                    Author = x.Author != null ? x.Author.Name : "Anônimo"

                })
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> DetailAsync([FromRoute] int id)
        {
            try
            {
                var post = await _dbcontext
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .ThenInclude(x => x.Roles)
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Conteudo nao encontrado"));

                return Ok(new ResultViewModel<Post>(post));

            }
            catch (System.Exception ex)
            {
                return BadRequest(new ResultViewModel<Post>(ex.Message));
            }

        }

        [HttpGet("v1/posts/category/{category}")]

        public async Task<IActionResult> GetByCategoryAsync([FromRoute] string category)
        {
            var posts = await _dbcontext
                .Posts
                .AsNoTracking()
                .Where(x => x.Category.Slug == category)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Category = x.Category.Name,
                    LastUpdateDate = x.LastUpdateDate,
                    Author = x.Author.Name
                })
                .ToListAsync();
            return Ok(posts);
        }

    }
}
