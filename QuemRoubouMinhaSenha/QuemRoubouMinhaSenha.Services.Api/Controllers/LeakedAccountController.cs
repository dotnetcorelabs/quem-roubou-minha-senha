using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuemRoubouMinhaSenha.Services.Api.Repository;
using QuemRoubouMinhaSenha.Services.Api.Models;
using Microsoft.Extensions.Primitives;

namespace QuemRoubouMinhaSenha.Services.Api.Controllers
{
    [Route("api/[controller]")]
    public class LeakedAccountController : Controller
    {
        private readonly ILeakedAccountRepository _repository;
        public LeakedAccountController(ILeakedAccountRepository repository)
        {
            _repository = repository;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/leakedaccount/name/domain
        [HttpGet("{domain}", Name = "GetByDomain")]
        public async Task<Account[]> Get(string domain)
        {
            var accountColl = await _repository.GetAccountByDomainAsync(domain);

            return accountColl;
        }

        // GET api/leakedaccount/name/domain
        [HttpGet("{name}/{domain}", Name = "GetByAccount")]
        public async Task<Account> Get(string name, string domain)
        {
            string sasKey = string.Empty;
            StringValues valColl = default(StringValues);
            Request.Headers.TryGetValue("sas", out valColl);

            sasKey = valColl[0];

            var account = await _repository.GetAccountAsync(name, domain);
            return account;
        }
    }
}
