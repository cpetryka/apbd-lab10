using apbd_lab10.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_lab10.Controllers;

[Controller]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Fetch data about trips from the database and order them by date
        var tripsQuery = _context.Trips
            .OrderByDescending(t => t.DateFrom);

        // Calculate the total number of trips
        var totalTrips = await tripsQuery.CountAsync();

        // Fetch the trips for the current page
        var trips = await tripsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(trip => new
            {
                Name = trip.Name,
                Description = trip.Description,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                MaxPeople = trip.MaxPeople,
                Countries = trip.IdCountries.Select(country => new
                {
                    Name = country.Name
                }),
                Clients = trip.ClientTrips.Select(tripClient => new
                {
                    FirstName = tripClient.IdClientNavigation.FirstName,
                    LastName = tripClient.IdClientNavigation.LastName
                })
            })
            .ToListAsync();

        // Return the trips along with some additional information
        return Ok(new
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            Trips = trips
        });
    }
}