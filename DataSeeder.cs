using Bogus;
using Blog.Data;
using Blog.Models;

namespace Blog
{
    public static class DataSeeder
    {
        public static void Seed(BlogDataContext context)
        {
          
            if (context.Users.Any() || context.Posts.Any())
                return;

            var faker = new Faker("pt_BR");

          
            var categories = new List<Category>
            {
                new Category { Name = "Tecnologia", Slug = "tecnologia" },
                new Category { Name = "Desenvolvimento", Slug = "desenvolvimento" },
                new Category { Name = "Not�cias", Slug = "noticias" },
                new Category { Name = "Tutoriais", Slug = "tutoriais" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

          
            var tags = new List<Tag>
            {
                new Tag { Name = "CSharp", Slug = "csharp" },
                new Tag { Name = "AspNetCore", Slug = "aspnetcore" },
                new Tag { Name = "EntityFramework", Slug = "entityframework" },
                new Tag { Name = "JWT", Slug = "jwt" },
                new Tag { Name = "API", Slug = "api" }
            };
            context.Tags.AddRange(tags);
            context.SaveChanges();

         
            var users = new Faker<User>("pt_BR")
                .RuleFor(u => u.Name, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PasswordHash, f => "123456") // senha padr�o de teste
                .RuleFor(u => u.Image, f => f.Internet.Avatar())
                .RuleFor(u => u.Slug, f => f.Internet.UserName().ToLower())
                .RuleFor(u => u.Bio, f => f.Lorem.Sentence(8))
                .Generate(5);

            // garantir slugs únicos entre os usuários gerados
            var used = new HashSet<string>();
            foreach (var u in users)
            {
                var baseSlug = u.Slug ?? faker.Internet.UserName().ToLower();
                var slug = baseSlug;
                var i = 1;
                while (used.Contains(slug))
                {
                    slug = $"{baseSlug}-{i++}";
                }
                u.Slug = slug;
                used.Add(slug);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            var posts = new List<Post>();
            foreach (var user in users)
            {
                for (int i = 0; i < 100; i++)
                {
                    var category = faker.PickRandom(categories);
                    var post = new Post
                    {
                        Title = faker.Lorem.Sentence(4),
                        Summary = faker.Lorem.Sentence(10),
                        Body = faker.Lorem.Paragraphs(3),
                        Slug = faker.Lorem.Slug(),
                        CreateDate = DateTime.Now.AddDays(-faker.Random.Int(1, 60)),
                        LastUpdateDate = DateTime.Now,
                        AuthorId = user.Id,
                        CategoryId = category.Id
                    };

                    
                    post.Tags = faker.PickRandom(tags, 2).ToList();
                    posts.Add(post);
                }
            }

            context.Posts.AddRange(posts);
            context.SaveChanges();
        }
    }
}
