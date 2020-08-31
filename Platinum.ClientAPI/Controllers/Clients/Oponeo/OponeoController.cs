using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Platinum.ClientAPI.Auth;

namespace Platinum.ClientAPI.Controllers.Clients.Oponeo
{
    [ApiController]
    [Route("[controller]")]
    [BasicAuth("Oponeo")]
    public class OponeoController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] {"kekw", "XD"};
        }
    }
}