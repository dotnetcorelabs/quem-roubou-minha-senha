using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuemRoubouMinhaSenha.Services.Api.Models;
using System.Threading;

namespace QuemRoubouMinhaSenha.Services.Api.Repository
{
    public interface ILeakedAccountRepository
    {
        Task<Account> GetAccountAsync(string account, string domain, CancellationToken cancelationToken = default(CancellationToken));

        Task<Account[]> GetAccountByDomainAsync(string domain, CancellationToken cancelationToken = default(CancellationToken));
    }
}
