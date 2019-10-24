using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using WebServerCursova.Entities;
using WebServerCursova.ViewModels;

namespace WebServerCursova.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly EFDbContext _context;
        //доступ до файла app.setting
        private readonly IConfiguration _configuration;
        //отримати доступ до сервера
        private readonly IHostingEnvironment _env;

        public ProductController(IHostingEnvironment env, IConfiguration configuration, EFDbContext context)
        {
            _configuration = configuration;
            _env = env;
            _context = context;
        }

        #region HttpGET
        [HttpGet]
        public IActionResult GetProducts()
        {
            var model = _context.Products.Select(
                p => new ProductGetVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price
                }).ToList();

            return Ok(model);
        }
        #endregion

        #region HttpPOST
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody]ProductPostVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            // передаємо модель в БД
            DbProduct p = new DbProduct
            {
                Name = model.Name,
                Price = model.Price,
                DateCreate = DateTime.Now
            };
            _context.Products.Add(p);
            _context.SaveChanges();

            return Ok(p.Id);
        }
        #endregion

        #region HttpDELETE
        [HttpDelete]
        public IActionResult Delete([FromBody]ProductDeleteVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var prod = _context.Products.SingleOrDefault(p => p.Id == model.Id);
            if (prod != null)
            {
                _context.Products.Remove(prod);
                _context.SaveChanges();
            }
            return Ok(prod.Id);
        }
        #endregion
    }
}
