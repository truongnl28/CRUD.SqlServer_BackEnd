using CrudDemo.SqlServer.Models;
using CrudDemo.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrudDemo.SqlServer.Controllers
{
	public class CategoriesController : Controller
	{
		private readonly ApplicationDbContext context;

		public CategoriesController(ApplicationDbContext context)
		{
			this.context = context;
		}

		public IActionResult Index()
		{
			var category = context.Categories.OrderByDescending(c => c.CategoryId).ToList();
			return View(category);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(CategoryDto categoryDto)
		{
			if (!ModelState.IsValid)
			{
				return View(categoryDto);
			}

			// Save the new category in the database
			Category category = new Category()
			{
				CategoryName = categoryDto.CategoryName,
			};

			context.Categories.Add(category);
			context.SaveChanges();

			return RedirectToAction("Index");
		}

		public IActionResult Edit(int CategoryId)
		{
			var category = context.Categories.Find(CategoryId);

			if (category == null)
			{
				return RedirectToAction("Index");
			}

			var categoryDto = new CategoryDto()
			{
				CategoryName = category.CategoryName,
			};

			ViewData["CategoryId"] = category.CategoryId;

			return View(categoryDto);
		}

		[HttpPost]
		public IActionResult Edit(int? CategoryId, CategoryDto categoryDto)
		{
			var category = context.Categories.Find(CategoryId);

			if (category == null)
			{
				return RedirectToAction("Index");
			}

			if (!ModelState.IsValid)
			{
				ViewData["CategoryId"] = category.CategoryId;

				return View(categoryDto);
			}

			category.CategoryName = categoryDto.CategoryName;
			context.SaveChanges();
			return RedirectToAction("Index");
		}

		public IActionResult Delete(int CategoryId)
		{
			var category = context.Categories.Find(CategoryId);

			if (category == null)
			{
				return RedirectToAction("Index", "Categories");
			}

			var hasProducts = context.Products.Any(p => p.CategoryId == CategoryId);

			if (hasProducts)
			{
				TempData["ErrorMessage"] = "Cannot delete this category because it is being used by one or more products.";
				return RedirectToAction("Index");
			}

			context.Categories.Remove(category);
			context.SaveChanges();

			return RedirectToAction("Index", "Categories");
		}
	}
}
