using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Blog.Models.ViewModels;
using Blog.Data;
using Blog.Extensions;
using SecureIdentity.Password;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{

    [ApiController]
    [Route("v1")]
    public class AccountController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly BlogDataContext _context;

        public AccountController(TokenService tokenService, BlogDataContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("accounts")]

        public async Task<IActionResult> Post([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<String>(ModelState.GetErrors()));

            var baseSlug = model.Email.Replace("@", "-").Replace(".", "-");
            var slug = await GenerateUniqueSlug(baseSlug);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = slug,

            };

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {

                    user = user.Email,
                    password

                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("Este Emial ja ta Cadastrado"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

        }

        // Gera um slug único com base no slug sugerido verificando a existência no banco
        private async Task<string> GenerateUniqueSlug(string baseSlug)
        {
            var slug = baseSlug;
            var i = 1;
            while (await _context.Users.AnyAsync(u => u.Slug == slug))
            {
                slug = $"{baseSlug}-{i++}";
            }
            return slug;
        }

        [HttpPost("/accounts/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<String>(ModelState.GetErrors()));

            var user = await _context.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalidos"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalidos"));

            try
            {
                var token = _tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

        }


    }
}
