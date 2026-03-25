using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurenindenAPI.DTOs;
using SurenindenAPI.Models;
using SurenindenAPI.Repositories;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IGenericRepository<Category> _repository;

    public CategoryController(IGenericRepository<Category> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _repository.GetAllAsync();
        return Ok(categories);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Add(CategoryDTO model)
    {
        var category = new Category { Name = model.Name };
        await _repository.AddAsync(category);
        await _repository.SaveAsync();
        return Ok("Kategori başarıyla eklendi.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryDTO model)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return NotFound("Kategori bulunamadı.");

        category.Name = model.Name;
        _repository.Update(category);
        await _repository.SaveAsync();
        return Ok("Kategori güncellendi.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromServices] IGenericRepository<Car> carRepo)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return NotFound("Kategori bulunamadı.");

        var hasCars = carRepo.Where(x => x.CategoryId == id).Any();
        if (hasCars)
        {
            return BadRequest("Bu kategoriye bağlı araçlar olduğu için silemezsiniz. Önce araçları silmeli veya taşımalısınız.");
        }

        _repository.Delete(category);
        await _repository.SaveAsync();
        return Ok("Kategori başarıyla silindi.");
    }
}