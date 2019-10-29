using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WebServerCursova.Entities;
using WebServerCursova.Helpers;
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

        private readonly string dirPathSave;

        public ProductController(IHostingEnvironment env, IConfiguration configuration, EFDbContext context)
        {
            _configuration = configuration;
            _env = env;
            _context = context;
            dirPathSave = ImageHelper.CreateImageFolder(_env, _configuration);
        }

        #region HttpGET
        [HttpGet]
        public IActionResult GetProducts()
        {
            List<string> photoNames = Directory.GetFiles(dirPathSave)
                .Select(f => Path.GetFileName(f))
                .ToList();

            var model = _context.Products.Select(
                p => new ProductGetVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    PhotoPath = GetPhotoPath(dirPathSave, photoNames, p.PhotoName),
                }).ToList();

            return Ok(model);
        }

        private string GetPhotoPath(string dirPath, List<string> fileNames, string photoName)
        {
            var searchName = fileNames
                .Find(n => n == photoName);
            return (searchName != null) ? Path.Combine(dirPath, searchName) : null;
        }
        #endregion

        #region HttpPOST
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody]ProductPostVM model)
        {
            List<string> err = new List<string>();

            // перевіряємо модель на валідність
            if (!ModelState.IsValid)
            {
                var errors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errors);
            }

            // зберігаємо фото
            var bmp = model.Photo.FromBase64StringToImage();
            if (bmp != null)
            {
                model.PhotoName = Path.GetRandomFileName() + ".jpg";

                string imageNamePath = Path.Combine(dirPathSave, model.PhotoName);
                var image = ImageHelper.CreateImage(bmp, 200, 200);
                image.Save(imageNamePath, ImageFormat.Jpeg);
            }
            else
            {
                model.PhotoName = "Empty.jpg";
            }

            // передаємо модель в БД
            DbProduct p = new DbProduct
            {
                Name = model.Name,
                Price = model.Price,
                DateCreate = DateTime.Now,
                PhotoName = model.PhotoName
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
