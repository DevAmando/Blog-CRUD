using Bogus;
using Blog.Data;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
    public static class DataSeeder
    {
        public static void Seed(BlogDataContext context)
        {
            if (context.Users.Any() || context.Posts.Any())
                return;

            var faker = new Faker("pt_BR");

            // Categorias
            var categories = new List<Category>
            {
                new Category { Name = "Tecnologia", Slug = "tecnologia" },
                new Category { Name = "Desenvolvimento", Slug = "desenvolvimento" },
                new Category { Name = "Notícias", Slug = "noticias" },
                new Category { Name = "Tutoriais", Slug = "tutoriais" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Tags
            var tags = new List<Tag>
            {
                new Tag { Name = "CSharp", Slug = "csharp" },
                new Tag { Name = "AspNetCore", Slug = "aspnetcore" },
                new Tag { Name = "EFCore", Slug = "efcore" }
            };
            context.Tags.AddRange(tags);
            context.SaveChanges();

            // Usuários
            var users = new Faker<User>("pt_BR")
                .RuleFor(u => u.Name, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PasswordHash, f => "123456")
                .RuleFor(u => u.Slug, f => f.Internet.UserName().ToLower() + f.Random.Int(1, 1000))
                .Generate(5);

            context.Users.AddRange(users);
            context.SaveChanges();

            // Posts (5000 Registros)
            var posts = new List<Post>();
            context.ChangeTracker.AutoDetectChangesEnabled = false; // Aumenta velocidade

            foreach (var user in users)
            {
                for (int i = 0; i < 1000; i++) // 1000 por usuário
                {
                    var category = faker.PickRandom(categories);
                    var post = new Post
                    {
                        Title = faker.Lorem.Sentence(4),
                        Summary = faker.Lorem.Sentence(10),
                        Body = faker.Lorem.Paragraphs(2),
                        Slug = faker.Lorem.Slug() + "-" + Guid.NewGuid().ToString().Substring(0, 8),
                        CreateDate = DateTime.Now.AddDays(-faker.Random.Int(1, 60)),
                        LastUpdateDate = DateTime.Now,
                        AuthorId = user.Id,
                        CategoryId = category.Id
                    };
                    posts.Add(post);
                }
            }

            context.Posts.AddRange(posts);
            context.SaveChanges();
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }
}