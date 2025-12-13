using ForumAPI.Models;
using ForumAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cats = await _service.GetAllAsync();
            return Ok(cats);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById(string id)
        {
            var cat = await _service.GetByIdAsync(id);
            if (cat == null) return NotFound();
            return Ok(cat);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var existing = await _service.GetByNameAsync(dto.Name);
            if (existing != null)
            {
                return Conflict("Category with the same name already exists.");
            }
            var category = new Category
            {
                Name = dto.Name
                // Id se generiše automatski u bazi ili u repozitorijumu
            };

            await _service.CreateAsync(category);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return Ok(id);
        }


    }
}
