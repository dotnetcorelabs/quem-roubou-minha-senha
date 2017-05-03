using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Repository
{
    public interface ITokenRepository
    {
        Task<string> RequestStorageTokenAsync(string account, string domain);
    }
}
