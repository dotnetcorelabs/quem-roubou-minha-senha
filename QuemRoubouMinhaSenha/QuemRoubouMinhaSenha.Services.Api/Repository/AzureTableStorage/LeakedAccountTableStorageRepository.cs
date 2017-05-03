using QuemRoubouMinhaSenha.Services.Api.Infrastrucutre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuemRoubouMinhaSenha.Services.Api.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QuemRoubouMinhaSenha.Services.Api.Repository.AzureTableStorage.Models;
using System.Threading;
using Microsoft.Extensions.Options;

namespace QuemRoubouMinhaSenha.Services.Api.Repository.AzureTableStorage
{
    public class LeakedAccountTableStorageRepository : ILeakedAccountRepository
    {
        private readonly TableStorageOptions _options;
        private CloudTable _table;
        private bool _initialized;

        public LeakedAccountTableStorageRepository(IOptions<TableStorageOptions> options)
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

        public async Task<Account> GetAccountAsync(string account, string domain, CancellationToken cancelationToken = default(CancellationToken))
        {
            await EnsureInitConnectionAsync();
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<LeakedAccountEntity>(domain, account);

            // Execute the retrieve operation.
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            Account accountResult = default(Account);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
            {
                var entity = (LeakedAccountEntity)retrievedResult.Result;
                accountResult = new Account
                {
                    LeakedDate = entity.LeakedDate,
                    ServiceProvider = entity.Source,
                    Title = entity.RowKey + entity.PartitionKey
                };
            }
            return accountResult;
        }

        public async Task<Account[]> GetAccountByDomainAsync(string domain, CancellationToken cancelationToken = default(CancellationToken))
        {
            await EnsureInitConnectionAsync();
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<LeakedAccountEntity> query = new TableQuery<LeakedAccountEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, domain));

            List<LeakedAccountEntity> leakedAcCollection = new List<LeakedAccountEntity>();
            TableContinuationToken token = default(TableContinuationToken);

            do
            {
                TableQuerySegment<LeakedAccountEntity> segmentedResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segmentedResult.ContinuationToken;
                leakedAcCollection.AddRange(segmentedResult);
            } while (token != null && cancelationToken.IsCancellationRequested);

            Account[] accountColl = new Account[leakedAcCollection.Count];
            for (int i = 0; i < leakedAcCollection.Count; i++)
            {
                accountColl[i] = new Account
                {
                    LeakedDate = leakedAcCollection[i].LeakedDate,
                    ServiceProvider = leakedAcCollection[i].Source,
                    Title = leakedAcCollection[i].RowKey + leakedAcCollection[i].PartitionKey
                };
            }
            return accountColl;
        }
    }
}
