using System.ComponentModel.DataAnnotations;

namespace CrudDemo.SqlServer.Models
{
    public class CategoryDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = "";
    }
}
