using Microsoft.AspNetCore.Mvc;
using QuemRoubouMinhaSenha.Services.Api.Models;
using QuemRoubouMinhaSenha.Services.Api.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Controllers
{
    [Route("api/[controller]")]
    public class ClientTokenController
    {
        private readonly ITokenRepository _repository;
        public ClientTokenController(ITokenRepository repository)
        {
            _repository = repository;
        }

        // POST api/values
        [HttpPost]
        public async Task<ClientToken> Post([FromBody]ClientToken value)
        {
            var token = await _repository.RequestStorageTokenAsync(value.Account, value.Domain);
            value.Token = token;
            return value;
        }
    }
}
