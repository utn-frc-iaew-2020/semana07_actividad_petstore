using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

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

        private readonly DynamoDBContext _context;


        public PetsController(ILogger<PetsController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);

            _context = new DynamoDBContext(client);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pets>>> GetAll(string name)
        {
            _logger.LogInformation("Iniciando GetAll");

            var conditions = new List<ScanCondition>();
            if (!string.IsNullOrEmpty(name))
            {
                conditions.Add(new ScanCondition("Name", ScanOperator.Equal, name));

            }

            var listPets = await _context.ScanAsync<Pets>(conditions).GetRemainingAsync();
            return Ok(listPets);

        }

        [HttpPost]
        public async Task<IActionResult> Post(Pets bodyParam)
        {
            _logger.LogInformation("Iniciando Post");

            await _context.SaveAsync<Pets>(bodyParam);

            return Created(string.Concat("/pets/", bodyParam.Id), bodyParam);
        }

        [HttpGet("{petId}")]
        public async Task<IActionResult> GetById(string petId)
        {
            _logger.LogInformation("Iniciando GetById");
            Pets item = await _context.LoadAsync<Pets>(petId);
            return Ok(item);
        }

        [HttpPut("{petId}")]
        public async Task<IActionResult> Put(string petId, [FromBody] Pets body)
        {
            _logger.LogInformation("Iniciando Put");
            Pets itemChanged = await _context.LoadAsync<Pets>(petId);

            if (itemChanged == null)
            {
                return NotFound();
            }

            itemChanged.Name = body.Name;
            itemChanged.Tag = body.Tag;

            await _context.SaveAsync<Pets>(itemChanged);
            return Ok(body);
        }

        [HttpPatch("{petId}")]
        public async Task<IActionResult> Patch(string petId, [FromBody] JsonPatchDocument<Pets> body)
        {
            _logger.LogInformation("Iniciando Patch");

            Pets item = await _context.LoadAsync<Pets>(petId);

            if (item == null)
            {
                return NotFound();
            }

            body.ApplyTo(item);
            await _context.SaveAsync<Pets>(item);

            return Ok(item);
        }

        [HttpDelete("{petId}")]
        public async Task<IActionResult> Delete(string petId)
        {
            _logger.LogInformation("Iniciando Delete");

            Pets item = await _context.LoadAsync<Pets>(petId);

            if (item == null)
            {
                return NotFound();
            }
            await _context.DeleteAsync<Pets>(petId);
            return NoContent();
        }
    }
}
