using System.ComponentModel.DataAnnotations;

namespace WebServerCursova.ViewModels
{
    public class ProductGetVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PhotoName { get; set; }
    }

    public class ProductPostVM
    {
        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public decimal Price { get; set; }
        public string Photo { get; set; }
        public string PhotoName { get; set; }
    }

    public class ProductDeleteVM
    {
        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public int Id { get; set; }
    }

    public class ProductPutVM
    {
        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле не може бути порожнім")]
        public decimal Price { get; set; }
    }
}
