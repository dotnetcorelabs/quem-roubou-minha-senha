using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace QuemRoubouMinhaSenha.Importer
{
    public class Importer
    {
        //TODO - GET A BETTER REGEX EMAIL VALIDATION
        private static Regex ValidateLine = new Regex(@"^[\w\d.]+?@[\w\d.]+$");
        public string Path { get; private set; }
        private CloudTable Table;
        private int tableItemsOnHold;
        private readonly int ITEM_ON_HOLD_LIMIT = 100;
        private readonly string SOURCE;
        private Dictionary<string, TableBatchOperation> OperationDirectonary;
        private bool initiated;
        private DateTime leakedDate = new DateTime(2016, 5, 1);

        public Importer(string path)
        {
            Path = path;
            SOURCE = "LINKEDIN";
            OperationDirectonary = new Dictionary<string, TableBatchOperation>();
        }

        public void EnsureInit(bool force = false)
        {
            if (!initiated || force)
            {
                PrepareTableStorage();
                initiated = true;
            }
        }

        private void TraceOperationError(Exception ex, TableBatchOperation operation)
        {
            Trace.TraceError($"Bad error during execution of table operation... PartitionKey {operation[0].Entity.PartitionKey} Please see the exception below -->");
            StringBuilder sbEx = new StringBuilder();
            Exception e = ex;
            while (e != null)
            {
                sbEx.AppendLine(e.Message);
            }
            Trace.TraceError(sbEx.ToString());
            EnsureInit(force: true);
        }

        private void PrepareTableStorage()
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("people");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            Table = table;
        }

        internal void InsertItem(string leakedAccount)
        {
            if (ValidateLine.IsMatch(leakedAccount))
            {
                string[] entity = leakedAccount.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                string domain = entity[1];
                if (!OperationDirectonary.ContainsKey(domain))
                {
                    OperationDirectonary.Add(entity[1], new TableBatchOperation());
                }
                TableBatchOperation operation = OperationDirectonary[domain];

                operation.InsertOrReplace(new LeakedAccountEntity(entity[1], entity[0])
                {
                    Source = SOURCE,
                    LeakedDate = leakedDate
                });
                tableItemsOnHold++;

                if (operation.Count >= ITEM_ON_HOLD_LIMIT)
                {
                    try
                    {
                        EnsureInit();
                        Table.ExecuteBatch(operation);
                    }
                    catch (Exception ex)
                    {
                        TraceOperationError(ex, operation);
                    }

                    operation.Clear();
                }
            }
        }

        private void FlushPendingBatchOperations()
        {
            if (OperationDirectonary != null && OperationDirectonary.Count > 0)
            {
                Parallel.ForEach(OperationDirectonary, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async operationKeyValue =>
                {
                    if (operationKeyValue.Value.Count > 0)
                        try
                        {
                            await Table.ExecuteBatchAsync(operationKeyValue.Value);
                        }
                        catch (Exception ex)
                        {
                            TraceOperationError(ex, operationKeyValue.Value);
                        }

                });
                //foreach (var operationKeyValue in OperationDirectonary)
                //{
                //    if (operationKeyValue.Value.Count > 0)
                //    {
                //        try
                //        {
                //            Table.ExecuteBatch(operationKeyValue.Value);
                //        }
                //        catch (Exception ex)
                //        {
                //            TraceOperationError(ex, operationKeyValue.Value);
                //        }
                //    }
                //}
            }
        }

        public void ImportFile(params string[] pathColl)
        {
            for (int i = 0; i < pathColl.Length; i++)
            {
                string path = pathColl[i];
                Trace.WriteLine($"Starting file {path}");
                //load the file here
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();
                            InsertItem(line);
                        }
                    }
                }
                Trace.WriteLine($"Done file {path}");
            }


            FlushPendingBatchOperations();
        }

        public void ImportFolder(string folderPath)
        {
            string[] filesInFolder = Directory.GetFiles(folderPath);
            ImportFile(filesInFolder);
            Trace.WriteLine($"Done folder {folderPath}");
        }

        public void ImportFolder()
        {
            ImportFolder(Path);
            Trace.WriteLine("Done...");
        }
    }
}
