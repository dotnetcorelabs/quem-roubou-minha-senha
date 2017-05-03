using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Models
{
    public class Account
    {
        public string Title { get; set; }

        public DateTime LeakedDate { get; set; }

        public string ServiceProvider { get; set; }
    }
}
