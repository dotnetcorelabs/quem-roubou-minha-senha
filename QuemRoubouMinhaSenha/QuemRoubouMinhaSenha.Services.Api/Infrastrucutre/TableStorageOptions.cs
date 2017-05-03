using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Infrastrucutre
{
    public class TableStorageOptions
    {
        public string ConnectionString { get; set; }

        public TableStorageOptions()
            : this(connectionString: string.Empty)
        {

        }

        public TableStorageOptions(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
