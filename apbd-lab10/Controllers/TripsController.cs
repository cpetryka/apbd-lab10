using apbd_lab10.Data;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetTrips()
    {
        return Ok();
    }
}