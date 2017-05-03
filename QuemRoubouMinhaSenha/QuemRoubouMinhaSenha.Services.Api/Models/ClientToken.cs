using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Models
{
    public class ClientToken
    {
        [Required]
        [StringLength(20)]
        public string Account { get; set; }

        [Required]
        [StringLength(20)]
        public string Domain { get; set; }

        //TODO verify if it's correct
        [NotMapped]
        public string Token { get; set; }
    }
}
