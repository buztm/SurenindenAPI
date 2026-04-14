using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurenindenAPI.Models;
using SurenindenAPI.Repositories;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IGenericRepository<Rental> _rentalRepo;
    private readonly IGenericRepository<Car> _carRepo;
    private readonly IGenericRepository<AppUser> _userRepo;

    public DashboardController(
        IGenericRepository<Rental> rentalRepo,
        IGenericRepository<Car> carRepo,
        IGenericRepository<AppUser> userRepo)
    {
        _rentalRepo = rentalRepo;
        _carRepo = carRepo;
        _userRepo = userRepo;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var allCars = await _carRepo.GetAllAsync();
        var allRentals = await _rentalRepo.GetAllAsync();
        var allUsers = await _userRepo.GetAllAsync();

        var activeRentals = allRentals.Count(r => r.ReturnDate == null);
        var availableCars = allCars.Count(c => c.IsAvailable);
        var totalUsers = allUsers.Count();
        var totalRevenue = allRentals.Sum(r => r.TotalPrice);

        return Ok(new
        {
            activeRentals,
            availableCars,
            totalUsers,
            totalRevenue
        });
    }

    [HttpGet("monthly-revenue")]
    public async Task<IActionResult> GetMonthlyRevenue()
    {
        var allRentals = await _rentalRepo.GetAllAsync();
        var currentYear = DateTime.Now.Year;

        var monthlyRevenue = Enumerable.Range(1, 12)
            .Select(month => new
            {
                month = month,
                revenue = allRentals
                    .Where(r => r.RentDate.Year == currentYear && r.RentDate.Month == month)
                    .Sum(r => r.TotalPrice)
            })
            .ToList();

        return Ok(monthlyRevenue);
    }

    [HttpGet("recent-rentals")]
    public async Task<IActionResult> GetRecentRentals()
    {
        var allRentals = await _rentalRepo.GetAllAsync();
        var recentRentals = allRentals
            .OrderByDescending(r => r.RentDate)
            .Take(10)
            .ToList();

        return Ok(recentRentals);
    }
}
