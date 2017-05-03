using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QuemRoubouMinhaSenha.Services.Api.Infrastrucutre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Services.Api.Repository.AzureTableStorage
{
    public class SasKeyStorageRepository : ITokenRepository
    {
        private readonly TableStorageOptions _options;
        private readonly double AccessPolicyDurationInMinutes = 60;
        private CloudTable _table;
        private bool _initialized;

        public SasKeyStorageRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private async Task EnsureInitConnectionAsync()
        {
            if (!_initialized)
            {
                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                CloudTable table = tableClient.GetTableReference("people");

                // Create the table if it doesn't exist.
                await table.CreateIfNotExistsAsync();
                _table = table;
                _initialized = true;
            }
        }

        public async Task<string> RequestStorageTokenAsync(string account, string domain)
        {
            await EnsureInitConnectionAsync();
            // Omitting any authentication code since this is beyond the scope of
            // this sample code

            // creating a shared access policy that expires in 30 minutes.
            // No start time is specified, which means that the token is valid immediately.
            // The policy specifies full permissions.
            SharedAccessTablePolicy policy = new SharedAccessTablePolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(AccessPolicyDurationInMinutes),
                Permissions = SharedAccessTablePermissions.Query
            };

            // Generate the SAS token. No access policy identifier is used which
            // makes it a non-revocable token
            // limiting the table SAS access to only the request customer's id
            string sasToken = this._table.GetSharedAccessSignature(
                policy   /* access policy */,
                null     /* access policy identifier */,
                domain /* start partition key */,
                account     /* start row key */,
                domain /* end partition key */,
                account     /* end row key */);

            return sasToken;
        }
    }
}
