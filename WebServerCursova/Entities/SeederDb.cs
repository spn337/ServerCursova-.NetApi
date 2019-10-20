using WebServerCursova.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            }
        }
    }
}
