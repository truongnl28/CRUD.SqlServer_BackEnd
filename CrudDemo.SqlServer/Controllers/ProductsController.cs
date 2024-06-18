using CrudDemo.SqlServer.Models;
using CrudDemo.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrudDemo.SqlServer.Controllers
{
	public class ProductsController : Controller
	{
		private readonly ApplicationDbContext context;
		private readonly IWebHostEnvironment environment;

		public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
		{
			this.context = context;
			this.environment = environment;
		}

		public IActionResult Index(string searchCategory, int page = 1, int pageSize = 10)
		{
			var products = context.Products
				.Where(p => !p.IsDeleted)
				.Include(p => p.Category)
				.OrderByDescending(p => p.Id)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchCategory))
			{
				searchCategory = searchCategory.ToLower();
				products = products.Where(p => p.Category != null && p.Category.CategoryName.ToLower().Contains(searchCategory));
			}

			int totalItems = products.Count();
			var paginatedProducts = products
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
			ViewBag.SearchCategory = searchCategory;

			return View(paginatedProducts);
		}

		public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(context.Categories, "CategoryId", "CategoryName");
			return View();
		}

		[HttpPost]
		public IActionResult Create(ProductDto productDto)
		{
			if (productDto.ImageFile == null)
			{
				ModelState.AddModelError("ImageFile", "The image file is required");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Categories = new SelectList(context.Categories, "CategoryId", "CategoryName", productDto.CategoryId);
				return View(productDto);
			}

			// Save the image file
			string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

			string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);
			using (var stream = new FileStream(imageFullPath, FileMode.Create))
			{
				productDto.ImageFile.CopyTo(stream);
			}

			// Save the new product in the database
			Product product = new Product()
			{
				Name = productDto.Name,
				Brand = productDto.Brand,
				CategoryId = productDto.CategoryId,
				Price = productDto.Price,
				Description = productDto.Description,
				ImageFileName = newFileName,
				CreatedAt = DateTime.Now,
			};

			context.Products.Add(product);
			context.SaveChanges();

			return RedirectToAction("Index");
		}

		public IActionResult Edit(int id)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index");
			}

			var productDto = new ProductDto()
			{
				Name = product.Name,
				Brand = product.Brand,
				CategoryId = product.CategoryId ?? 0,
				Price = product.Price,
				Description = product.Description,
			};

			ViewBag.Categories = new SelectList(context.Categories, "CategoryId", "CategoryName", product.CategoryId);
			ViewData["ProductId"] = product.Id;
			ViewData["ImageFileName"] = product.ImageFileName;
			ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

			return View(productDto);
		}

		[HttpPost]
		public IActionResult Edit(int id, ProductDto productDto)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Categories = new SelectList(context.Categories, "CategoryId", "CategoryName", productDto.CategoryId);
				ViewData["ProductId"] = product.Id;
				ViewData["ImageFileName"] = product.ImageFileName;
				ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
				return View(productDto);
			}

			string newFileName = product.ImageFileName;
			if (productDto.ImageFile != null)
			{
				newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile.FileName);
				string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);
				using (var stream = new FileStream(imageFullPath, FileMode.Create))
				{
					productDto.ImageFile.CopyTo(stream);
				}
				string oldImageFullPath = Path.Combine(environment.WebRootPath, "products", product.ImageFileName);
				System.IO.File.Delete(oldImageFullPath);
			}

			product.Name = productDto.Name;
			product.Brand = productDto.Brand;
			product.CategoryId = productDto.CategoryId;
			product.Price = productDto.Price;
			product.Description = productDto.Description;
			product.ImageFileName = newFileName;

			context.SaveChanges();

			return RedirectToAction("Index");
		}

		public IActionResult Delete(int id)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index", "Products");
			}

			product.IsDeleted = true;
			context.SaveChanges(true);

			return RedirectToAction("Index", "Products");
		}

	}
}
