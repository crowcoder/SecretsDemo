using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SecretsDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IConfiguration _config;
        public ValuesController(IConfiguration config)
        {
            _config = config;
        }
        
        // GET api/values demo various providers
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            string theKeyVaultSecret = _config.GetValue<string>("secret1");
            string theKeyVaultSecret2 = _config.GetValue<string>("secret2");

            return new string[] {theKeyVaultSecret, theKeyVaultSecret2 };
        }
    }
}
