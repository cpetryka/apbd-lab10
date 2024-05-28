using apbd_lab10.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_lab10.Controllers;

[Controller]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        // Find a client with the specified ID
        var client = await _context.Clients.FindAsync(idClient);

        // If the client does not exist, return 404 Not Found
        if (client == null)
        {
            return NotFound("Client not found.");
        }

        // Check if the client has assigned any trips
        var clientTrips = await _context.ClientTrips.AnyAsync(ct => ct.IdClient == idClient);

        // If the client has assigned trips, return 400 Bad Request
        if (clientTrips)
        {
            return BadRequest("Client has assigned trips.");
        }

        // Remove the client from the database
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}