using apbd_lab10.Data;
using apbd_lab10.Models;
using apbd_lab10.Models.Dto;
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

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AddClientTripDto addClientTripDto)
        {
            // Check if a client with the specified PESEL already exists
            var existingClient = await _context.Clients.SingleOrDefaultAsync(c => c.Pesel == addClientTripDto.Pesel);
            if (existingClient != null)
            {
                return BadRequest("Client with this PESEL already exists.");
            }

            // Check if the client is already assigned to the trip
            var existingClientTrip = await _context.ClientTrips
                .AnyAsync(ct => ct.IdClientNavigation.Pesel == addClientTripDto.Pesel && ct.IdTrip == idTrip);
            if (existingClientTrip)
            {
                return BadRequest("Client is already assigned to this trip.");
            }

            // Check if the trip exists and if DateFrom is in the future
            var trip = await _context.Trips.FindAsync(idTrip);
            if (trip == null || trip.DateFrom <= DateTime.Now)
            {
                return BadRequest("Trip does not exist or has already started.");
            }

            // Create a new client
            var client = new Client
            {
                FirstName = addClientTripDto.FirstName,
                LastName = addClientTripDto.LastName,
                Email = addClientTripDto.Email,
                Telephone = addClientTripDto.Telephone,
                Pesel = addClientTripDto.Pesel
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Assign the client to the trip
            var clientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = addClientTripDto.IdTrip,
                PaymentDate = addClientTripDto.PaymentDate,
                RegisteredAt = DateTime.Now
            };

            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return Ok();
        }
}