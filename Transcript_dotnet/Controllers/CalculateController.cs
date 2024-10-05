using Calculate.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Calculate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculateController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public CalculateController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            configuration = config;
        }

        [HttpPost(Name = "Post")]
        public IActionResult Post([FromBody] JSON data)
        {
            if (data == null)
            {
                return BadRequest();
            }
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            Initial initial = new Initial();
            return Ok(initial.Init(data, connectionString));
        }
    }
}