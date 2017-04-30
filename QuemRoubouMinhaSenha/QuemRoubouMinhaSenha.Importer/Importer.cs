﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.IO;
using System.Text.RegularExpressions;

namespace QuemRoubouMinhaSenha.Importer
{
    public class Importer
    {
        //TODO - GET A BETTER REGEX EMAIL VALIDATION
        private static Regex ValidateLine = new Regex(@"^.+?@.+$");
        public string Path { get; private set; }
        private CloudTable Table;
        private int tableItemsOnHold;
        private readonly int ITEM_ON_HOLD_LIMIT = 1;
        private readonly string SOURCE;
        private Dictionary<string, TableBatchOperation> OperationDirectonary;
        private bool initiated;

        public Importer(string path)
        {
            Path = path;
            SOURCE = "MY_SOURCE";
            OperationDirectonary = new Dictionary<string, TableBatchOperation>();
        }

        public void EnsureInit()
        {
            if (!initiated)
            {
                PrepareTableStorage();
                initiated = true;
            }
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

                operation.Insert(new LeakedAccountEntity(entity[1], entity[0])
                {
                    Source = SOURCE,
                    LeakedDate = DateTime.UtcNow
                });
                tableItemsOnHold++;

                if (tableItemsOnHold >= ITEM_ON_HOLD_LIMIT)
                {
                    EnsureInit();
                    Table.ExecuteBatch(operation);
                }
            }
        }

        public void ImportFile(string path)
        {
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
        }

        public void ImportFolder(string folderPath)
        {
            string[] filesInFolder = Directory.GetFiles(folderPath);
            foreach (var file in filesInFolder)
            {
                ImportFile(file);
            }
        }

        public void ImportFolder()
        {
            ImportFolder(Path);
        }
    }
}
