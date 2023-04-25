using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace UdemyRabbitMQ.WaterMark.UI.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageName { get; set; }
        public byte[]? QrCode { get; set; }
        public string? Barcode { get; set; }
    }
}
