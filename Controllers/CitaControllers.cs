using Microsoft.AspNetCore.Mvc;
using VetClinicAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VetClinicAPI.Data;

namespace VetClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CitasController : ControllerBase
    {
        private readonly VetClinicContext _context;

        public CitasController(VetClinicContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Citas.ToList());
        }

        [HttpPost]
        public IActionResult Post(Cita cita)
        {
            _context.Citas.Add(cita);
            _context.SaveChanges();
            return Ok("Cita guardada en SQL Server");
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, Cita citaActualizada)
        {
            var cita = _context.Citas.Find(id);

            if (cita == null)
                return NotFound();

            cita.Fecha = citaActualizada.Fecha;
            cita.Hora = citaActualizada.Hora;
            cita.Motivo = citaActualizada.Motivo;

            _context.SaveChanges();

            return Ok("Cita actualizada");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var cita = _context.Citas.Find(id);

            if (cita == null)
                return NotFound();

            _context.Citas.Remove(cita);
            _context.SaveChanges();

            return Ok("Cita eliminada");
        }   
    }
}
