using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Importer
{
    public class LeakedAccountEntity : TableEntity
    {
        public LeakedAccountEntity(string domain, string account)
        {
            this.PartitionKey = domain;
            this.RowKey = account;
        }

        public LeakedAccountEntity()
        {

        }

        /// <summary>
        /// Source of leak
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// When was leaked
        /// </summary>
        public DateTime LeakedDate { get; set; }
    }
}
