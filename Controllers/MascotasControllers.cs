using Microsoft.AspNetCore.Mvc;
using VetClinicAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VetClinicAPI.Data;

namespace VetClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MascotasController : ControllerBase
    {
        private readonly VetClinicContext _context;

        public MascotasController(VetClinicContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Mascotas.ToList());
        }

        [HttpPost]
        public IActionResult Post(Mascota mascota)
        {
            _context.Mascotas.Add(mascota);
            _context.SaveChanges();
            return Ok("Mascota guardada en SQL Server");
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, Mascota mascotaActualizada)
        {
            var mascota = _context.Mascotas.Find(id);

            if (mascota == null)
                return NotFound();

            mascota.Nombre = mascotaActualizada.Nombre;
            mascota.Especie = mascotaActualizada.Especie;
            mascota.Raza = mascotaActualizada.Raza;
            mascota.Edad = mascotaActualizada.Edad;

            _context.SaveChanges();

            return Ok("Mascota actualizada");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var mascota = _context.Mascotas.Find(id);

            if (mascota == null)
                return NotFound();

            _context.Mascotas.Remove(mascota);
            _context.SaveChanges();

            return Ok("Mascota eliminada");
        }

    }
}