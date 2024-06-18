using System.ComponentModel.DataAnnotations;

namespace CrudDemo.SqlServer.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string CategoryName { get; set; } = "";

        public ICollection<Product> Products { get; set; }
    }
}
