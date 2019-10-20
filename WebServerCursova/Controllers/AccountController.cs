using WebServerCursova.Entities;
using WebServerCursova.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCursova.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        public readonly string secretePhrase = "this is the secrete phrase";

        private readonly EFDbContext _context;
        // створює юзера
        private readonly UserManager<DbUser> _userManager;
        // логінить юзера
        private readonly SignInManager<DbUser> _signInManager;

        private readonly RoleManager<DbRole> _roleManager;

        public AccountController(UserManager<DbUser> userManager, SignInManager<DbUser> signInManager, RoleManager<DbRole> roleManager, EFDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            // логіним юзера, який прийшов параметром як модель
            var result = await _signInManager
                .PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return BadRequest(new { invalid = "Юзера не знайдено" });
            }
            // якщо все ок - шукаємо юзера по емейлу в базі
            var user = await _userManager.FindByEmailAsync(model.Email);
            // і логінимо його
            await _signInManager.SignInAsync(user, isPersistent: false);

            // видаємо юзеру токен авторизації
            return Ok(
                new
                {
                    token = CreateTokenJWT(user)
                });
        }

        // метод, який буде видавати JWT-токен
        private string CreateTokenJWT(DbUser user)
        {
            // отримуємо всі ролі про юзера
            var roles = _userManager.GetRolesAsync(user).Result;
            var claims = new List<Claim>()
            {
                new Claim ("id", user.Id.ToString()),
                new Claim ("Name", user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            }

            // шифруємо токен 
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretePhrase));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                expires: DateTime.Now.AddHours(1));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] RegistrationVM model)
        {
            List<string> err = new List<string>();

            // перевіряємо модель на валідність
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                foreach (var item in errors)
                {
                    string message = "";

                    foreach (var i in item)
                    {
                        message += i.ErrorMessage + " ";
                    }

                    err.Add(message);
                }
            }
            else
            {
                // створюємо роль адмін
                string roleAdmin = "Admin";

                // шукаємо роль в базі. Якщо немає - додаємо
                var role = _roleManager.FindByNameAsync(roleAdmin).Result;
                if (role == null)
                {
                    role = new DbRole
                    {
                        Name = roleAdmin
                    };

                    var addRoleResult = _roleManager.CreateAsync(role).Result;
                }

                // шукаємо юзера в базі по імейлу. якщо немає - додаємо
                var user = _userManager.FindByNameAsync(model.Email).Result;
                if (user == null)
                {
                    user = new DbUser
                    {
                        UserName = model.Email,
                        Email = model.Email
                    };

                    var result = _userManager.CreateAsync(user, model.Password).Result;
                    // якщо додало - додаємо роль
                    if (result.Succeeded)
                    {
                        result = _userManager.AddToRoleAsync(user, roleAdmin).Result;
                        // логінимо юзера
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        // передаємо модель в БД
                        UserProfile up = new UserProfile
                        {
                            DbUserId = user.Id,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Phone = model.Phone
                        };
                        _context.UserProfiles.Add(up);
                        _context.SaveChanges();
                        return Ok(
                       new
                       {
                           token = CreateTokenJWT(user)
                       });
                    }
                }
                else
                {
                    err.Add("Така пошта вже зареєстрована!");
                }
            }

            return BadRequest(err);
        }
    }
}
