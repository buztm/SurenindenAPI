using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurenindenAPI.DTOs;
using SurenindenAPI.Models;
using SurenindenAPI.Repositories;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RentalController : ControllerBase
{
    private readonly IGenericRepository<Rental> _rentalRepo;
    private readonly IGenericRepository<Car> _carRepo;

    public RentalController(IGenericRepository<Rental> rentalRepo, IGenericRepository<Car> carRepo)
    {
        _rentalRepo = rentalRepo;
        _carRepo = carRepo;
    }

    [HttpPost("rent")]
    public async Task<IActionResult> Rent(RentalDTO model)
    {
        var car = await _carRepo.GetByIdAsync(model.CarId);
        if (car == null || !car.IsAvailable)
            return BadRequest("Araç şu an müsait değil.");

        var rental = new Rental
        {
            CarId = model.CarId,
            AppUserId = model.AppUserId,
            RentDate = model.RentDate,
            ReturnDate = model.ReturnDate,
            TotalPrice = model.TotalPrice
        };

        car.IsAvailable = false;
        _carRepo.Update(car);

        await _rentalRepo.AddAsync(rental);
        await _rentalRepo.SaveAsync();
        await _carRepo.SaveAsync();

        return Ok("Kiralama işlemi başarıyla tamamlandı.");
    }

    [HttpGet("my-rentals/{userId}")]
    public async Task<IActionResult> GetMyRentals(string userId)
    {
        var rentals = _rentalRepo.Where(x => x.AppUserId == userId).ToList();
        return Ok(rentals);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("return/{rentalId}")]
    public async Task<IActionResult> ReturnCar(int rentalId)
    {
        var rental = await _rentalRepo.GetByIdAsync(rentalId);
        if (rental == null) return NotFound("Kiralama kaydı bulunamadı.");

        var car = await _carRepo.GetByIdAsync(rental.CarId);
        if (car == null) return NotFound("İlgili araç bulunamadı.");

        car.IsAvailable = true;
        _carRepo.Update(car);

        rental.ReturnDate = DateTime.Now;
        _rentalRepo.Update(rental);

        // 5. Değişiklikleri kaydet
        await _carRepo.SaveAsync();
        await _rentalRepo.SaveAsync();

        return Ok("Araç başarıyla teslim alındı ve tekrar kiralanabilir duruma getirildi.");
    }
}