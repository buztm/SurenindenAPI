using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurenindenAPI.DTOs;
using SurenindenAPI.Models;
using SurenindenAPI.Repositories;

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
                ImagePath = model.ImagePath ?? "default.jpg",
                IsAvailable = model.IsAvailable
            };

            await _repository.AddAsync(car);
            await _repository.SaveAsync();
            return Ok("Araç başarıyla eklendi.");
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
