using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Controllers
{
    [Route("api/[controller]")]
    public class KeyController
    {
        // POST api/values
        [HttpPost]
        public string Post([FromBody]string value)
        {
            return value;
        }
    }
}
