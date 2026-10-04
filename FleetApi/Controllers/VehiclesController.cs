using FleetApi.Models;
using FleetApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(_vehicleService.GetAll());
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var vehicle = _vehicleService.GetById(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Vehicle vehicle)
        {
            if (vehicle == null) return BadRequest();
            var created = _vehicleService.Create(vehicle);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] Vehicle vehicle)
        {
            var updated = _vehicleService.Update(id, vehicle);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _vehicleService.Delete(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("status/{status}")]
        public IActionResult GetByStatus(string status)
        {
            return Ok(_vehicleService.GetByStatus(status));
        }

        [HttpGet("analytics/average-mileage")]
        public IActionResult GetAverageMileage()
        {
            return Ok(_vehicleService.GetAverageFleetMileage());
        }
    }
}