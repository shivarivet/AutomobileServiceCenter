using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorageExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount storageAccount;
            CloudTableClient tableClient;

            storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable bookTable = tableClient.GetTableReference("Book");
            bookTable.CreateIfNotExistsAsync();

            Book book = new Book(1, "Apress") { Author = "Shiva", BookName = "Asp.Net Core with Azure", Publisher = "Apress" };
            book.CreatedDate = DateTime.UtcNow;
            book.UpdatedDate = DateTime.UtcNow;

            TableOperation tableOperation = TableOperation.Insert(book);
            bookTable.ExecuteAsync(tableOperation);

            Console.ReadLine();
        }
    }
}
