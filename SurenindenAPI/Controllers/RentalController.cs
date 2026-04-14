using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _context;

    public RentalController(IGenericRepository<Rental> rentalRepo,
                            IGenericRepository<Car> carRepo,
                            AppDbContext context)
    {
        _rentalRepo = rentalRepo;
        _carRepo = carRepo;
        _context = context; 
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
            ReturnDate = null,
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
    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var rentals = await _rentalRepo.GetAllAsync();
        var cars = await _carRepo.GetAllAsync();

        var users = await _context.Users.ToListAsync(); 

        var result = rentals.Select(r => {
            var car = cars.FirstOrDefault(c => c.Id == r.CarId);
            var user = users.FirstOrDefault(u => u.Id == r.AppUserId);

            return new RentalDetailDTO
            {
                Id = r.Id,
                CarId = r.CarId,
                CarInfo = car != null ? $"{car.Brand} {car.Model}" : "Bilinmeyen Araç",
                Plate = car?.Plate ?? "-",
                AppUserId = r.AppUserId,
                UserName = user?.UserName ?? "Bilinmeyen Kullanıcı",
                UserEmail = user?.Email ?? "-",
                RentDate = r.RentDate,
                ReturnDate = r.ReturnDate,
                TotalPrice = r.TotalPrice
            };
        }).OrderByDescending(x => x.RentDate).ToList();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("return/{rentalId}")]
    public async Task<IActionResult> ReturnCar(int rentalId, [FromQuery] decimal finalPrice, [FromQuery] DateTime returnDate)
    {
        try
        {
            var rental = await _rentalRepo.GetByIdAsync(rentalId);
            if (rental == null) return NotFound(new { message = "Kiralama kaydı bulunamadı." });

            var car = await _carRepo.GetByIdAsync(rental.CarId);
            if (car == null) return NotFound(new { message = "İlgili araç bulunamadı." });

            car.IsAvailable = true;
            _carRepo.Update(car);

            rental.ReturnDate = returnDate;
            rental.TotalPrice = finalPrice;

            _rentalRepo.Update(rental);

            await _carRepo.SaveAsync();
            await _rentalRepo.SaveAsync();

            return Ok(new { message = "Araç başarıyla teslim alındı." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}