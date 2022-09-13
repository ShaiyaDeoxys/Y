using Imgeneus.Core.Structures.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Imgeneus.Login.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly LoginConfiguration _configuration;

        public VersionController(IOptions<LoginConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult(_configuration.ClientVersion);
        }
    }
}
