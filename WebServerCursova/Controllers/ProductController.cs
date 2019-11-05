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

        private readonly string kNamePhotoDefault = "Empty.jpg";


        public ProductController(IHostingEnvironment env, IConfiguration configuration, EFDbContext context)
        {
            _configuration = configuration;
            _env = env;
            _context = context;
            dirPathSave = ImageHelper.GetImageFolder(_env, _configuration);
        }

        #region HttpGetId
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var model = _context.Products
                .Select(p => new ProductPutVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    PhotoName = p.PhotoName
                })
                .SingleOrDefault(p => p.Id == id);
            if (model == null)
            {
                return NotFound(new { invalid = "Not fount by id" });
            }
            return Ok(model);
        }
        #endregion

        #region HttpGet
        [HttpGet]
        public IActionResult GetProducts()
        {
            var model = _context.Products.Select(
                p => new ProductGetVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    PhotoName = p.PhotoName
                }).ToList();

            return Ok(model);
        }
        #endregion

        #region HttpPost
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
            var bmp = model.PhotoBase64.FromBase64StringToImage();
            if (bmp != null)
            {
                model.PhotoName = Path.GetRandomFileName() + ".jpg";

                string imageNamePath = Path.Combine(dirPathSave, model.PhotoName);
                var image = ImageHelper.CreateImage(bmp, 200, 200);
                image.Save(imageNamePath, ImageFormat.Jpeg);
            }
            else
            {
                model.PhotoName = kNamePhotoDefault;
            }

            // передаємо модель в БД
            DbProduct prod = new DbProduct
            {
                Name = model.Name,
                Price = model.Price,
                DateCreate = DateTime.Now,
                PhotoName = model.PhotoName
            };
            _context.Products.Add(prod);
            _context.SaveChanges();

            return Ok(prod.Id);
        }
        #endregion

        #region HttpDelete
        [HttpDelete]
        public IActionResult Delete([FromBody]ProductDeleteVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var fullProduct = _context.Products.SingleOrDefault(p => p.Id == model.Id);
            if (fullProduct != null)
            {
                //видаляємо фото(якщо не за замовчуванням)
                if (fullProduct.PhotoName != kNamePhotoDefault && fullProduct.PhotoName != null)
                {
                    string imageNamePath = Path.Combine(dirPathSave, fullProduct.PhotoName);
                    System.IO.File.Delete(imageNamePath);
                }
                //видаляємо продукт
                _context.Products.Remove(fullProduct);
                _context.SaveChanges();
            }

            return Ok(fullProduct.Id);
        }
        #endregion

        #region HttpPut
        [HttpPut]
        public IActionResult EditSave([FromBody]ProductPutVM newModel)
        {
            List<string> err = new List<string>();

            // перевіряємо нову модель на валідність
            if (!ModelState.IsValid)
            {
                var errors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errors);
            }

            //// дістаємо стару модель
            var product = _context.Products
                .SingleOrDefault(p => p.Id == newModel.Id);

            if (product != null)
            {      
                // якщо вибрали нове фото
                if (newModel.PhotoBase64 != "")
                {
                    product.PhotoName = Path.GetRandomFileName() + ".jpg";

                    var bmp = newModel.PhotoBase64.FromBase64StringToImage();

                    var imageNamePath = Path.Combine(dirPathSave, product.PhotoName);
                    var image = ImageHelper.CreateImage(bmp, 200, 200);
                    image.Save(imageNamePath, ImageFormat.Jpeg);
                }

                product.Name = newModel.Name;
                product.Price = newModel.Price;
                product.DateCreate = DateTime.Now;

                _context.SaveChanges();
            }
            else
            {
                return NotFound();
            }

            return Ok(newModel.Id);
        }
        #endregion
    }
}
