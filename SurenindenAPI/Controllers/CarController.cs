using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurenindenAPI.DTOs;
using SurenindenAPI.Models;
using SurenindenAPI.Repositories;
using System.IO;

namespace SurenindenAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly IGenericRepository<Car> _repository;

        public CarController(IGenericRepository<Car> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cars = await _repository.GetAllAsync();
            return Ok(cars);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var cars = await _repository.GetAllAsync();
            var availableCars = cars.Where(c => c.IsAvailable).ToList();
            return Ok(availableCars);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] string? fuelType,
    [FromQuery] string? transmission,
    [FromQuery] int? categoryId)
        {
            var cars = await _repository.GetAllAsync();
            var filtered = cars.Where(c => c.IsAvailable).AsQueryable();

            if (minPrice.HasValue)
                filtered = filtered.Where(c => c.DailyPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                filtered = filtered.Where(c => c.DailyPrice <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(fuelType) && !fuelType.Equals("null", StringComparison.OrdinalIgnoreCase))
                filtered = filtered.Where(c => c.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(transmission) && !transmission.Equals("null", StringComparison.OrdinalIgnoreCase))
                filtered = filtered.Where(c => c.Transmission.Equals(transmission, StringComparison.OrdinalIgnoreCase));

            if (categoryId.HasValue && categoryId.Value > 0)
                filtered = filtered.Where(c => c.CategoryId == categoryId.Value);

            return Ok(filtered.ToList());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await _repository.GetByIdAsync(id);
            if (car == null) return NotFound("Araç bulunamadı.");
            return Ok(car);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(CarDTO model)
        {
            var car = new Car
            {
                CategoryId = model.CategoryId,
                Brand = model.Brand,
                Model = model.Model,
                Year = model.Year,
                Plate = model.Plate,
                DailyPrice = model.DailyPrice,
                FuelType = model.FuelType,
                Transmission = model.Transmission,
                ImagePath = model.ImagePath ?? "/images/car-default.jpg",
                IsAvailable = model.IsAvailable
            };

            await _repository.AddAsync(car);
            await _repository.SaveAsync();
            return Ok("Araç başarıyla eklendi.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Dosya bulunamadı.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/images/cars/{uniqueFileName}";
            return Ok(new { path = relativePath });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CarDTO model)
        {
            if (id != model.Id) return BadRequest("ID uyuşmazlığı.");

            var existingCar = await _repository.GetByIdAsync(id);
            if (existingCar == null) return NotFound("Güncellenecek araç bulunamadı.");

            existingCar.Brand = model.Brand;
            existingCar.Model = model.Model;
            existingCar.Year = model.Year;
            existingCar.Plate = model.Plate;
            existingCar.DailyPrice = model.DailyPrice;
            existingCar.FuelType = model.FuelType;
            existingCar.Transmission = model.Transmission;
            existingCar.IsAvailable = model.IsAvailable;
            existingCar.CategoryId = model.CategoryId;

            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                existingCar.ImagePath = model.ImagePath;
            }

            _repository.Update(existingCar);
            await _repository.SaveAsync();

            return Ok("Araç bilgileri başarıyla güncellendi.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _repository.GetByIdAsync(id);
            if (car == null) return NotFound();
            _repository.Delete(car);
            await _repository.SaveAsync();
            return Ok("Araç silindi.");
        }
    }
}
