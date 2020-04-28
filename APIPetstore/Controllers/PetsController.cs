using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;

namespace Petstore.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("v{v:apiVersion}/[controller]")]
    public class PetsController : ControllerBase
    {
        private static List<Pets> listPets;
        private readonly ILogger<PetsController> _logger;

        private readonly IConfiguration _config;

        public PetsController(ILogger<PetsController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            if (listPets == null)
            {
                listPets = new List<Pets>();
                listPets.Add(new Pets()
                {
                    Id = "123",
                    Name = "collar perro",
                    Tag = "perro",
                });
                listPets.Add(new Pets()
                {
                    Id = "124",
                    Name = "collar gato",
                    Tag = "gato",
                });
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<Pets>> GetAll(string name)
        {
            _logger.LogInformation("Iniciando GetAll");
            if (!string.IsNullOrEmpty(name))
            {
                var result = listPets.FindAll(p => p.Name.Contains(name));
                if (result.Count == 0)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            else
            {
                return Ok(listPets);
            }

        }

        [HttpPost]
        public IActionResult Post(Pets bodyParam)
        {
            _logger.LogInformation("Iniciando Post");
            listPets.Add(bodyParam);

            return Created(string.Concat("/pets/", bodyParam.Id), bodyParam);
        }

        [HttpGet("{petId}")]
        public IActionResult GetById(string petId)
        {
            _logger.LogInformation("Iniciando GetById");
            if (!listPets.Exists(p => p.Id == petId))
            {
                return NotFound();
            }
            return Ok(listPets.FirstOrDefault(p => p.Id == petId));
        }

        [HttpPut("{petId}")]
        public IActionResult Put(string petId, [FromBody] Pets body)
        {
            _logger.LogInformation("Iniciando Put");

            if (!listPets.Exists(p => p.Id == petId))
            {
                return NotFound();
            }

            Pets item = listPets.FirstOrDefault(p => p.Id == petId);
            item.Name = body.Name;
            item.Tag = body.Tag;

            return Ok(body);
        }

        [HttpPatch("{petId}")]
        public IActionResult Patch(string petId, [FromBody] JsonPatchDocument<Pets> body)
        {
            _logger.LogInformation("Iniciando Patch");

            if (!listPets.Exists(p => p.Id == petId))
            {
                return NotFound();
            }
            var item = listPets.FirstOrDefault(p => p.Id == petId);
            body.ApplyTo(item);

            return Ok(item);
        }

        [HttpDelete("{petId}")]
        public IActionResult Delete(string petId)
        {
            _logger.LogInformation("Iniciando Delete");

            if (!listPets.Exists(p => p.Id == petId))
            {
                return NotFound();
            }
            listPets.RemoveAll(p => p.Id == petId);
            return NoContent();
        }
    }
}
