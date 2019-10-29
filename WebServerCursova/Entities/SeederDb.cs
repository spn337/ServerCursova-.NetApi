using WebServerCursova.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace WebServerCursova.Entities
{
    public class SeederDb
    {
        public static void SeedUsers(LoginVM model, UserManager<DbUser> userManager, RoleManager<DbRole> roleManager)
        {
            string roleName = "Admin";
            var role = roleManager.FindByNameAsync(roleName).Result;
            if (role == null)
            {
                role = new DbRole
                {
                    Name = roleName
                };

                var addRoleResult = roleManager.CreateAsync(role).Result;
            }

            var user = userManager.FindByNameAsync(model.Email).Result;
            if (user == null)
            {
                user = new DbUser
                {
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = userManager.CreateAsync(user, model.Password).Result;
                if (result.Succeeded)
                {
                    result = userManager.AddToRoleAsync(user, roleName).Result;
                }
            }
        }
        public static void SeedData(IServiceProvider services, IHostingEnvironment env, IConfiguration config)
        {
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EFDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DbUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<DbRole>>();

                SeedUsers(new LoginVM { Email = "bomba@gmail.com", Password = "Qwerty1-" }, userManager, roleManager);

                SeedProduct(context, new DbProduct
                {
                    Name = "Пістони",
                    Price = 4.75M
                });
                SeedProduct(context, new DbProduct
                {
                    Name = "ДАРТС F701",
                    Price = 30.48M
                });
                SeedProduct(context, new DbProduct
                {
                    Name = "МЯЧ ФУТБОЛ. KEPAI MALADUONA ЛАКОВАНИЙ PU FH402",
                    Price = 331.27M
                });
                SeedProduct(context, new DbProduct
                {
                    Name = "РОЛИКИ HAPPY №1 L, ЧЕРНЫЙ",
                    Price = 507.87M
                });
            }
        }

        public static void SeedProduct (EFDbContext context, DbProduct model)
        {
            var product = context.Products.SingleOrDefault(p => p.Name == model.Name);
            if(product == null)
            {
                product = new DbProduct
                {
                    Name = model.Name,
                    Price = model.Price,
                    DateCreate = DateTime.Now,
                    PhotoName = model.PhotoName
                };
                context.Products.Add(product);
                context.SaveChanges();
            }
        }
    }
}
