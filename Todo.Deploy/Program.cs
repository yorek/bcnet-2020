using System;
using DbUp;
using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace Todo.Deploy
{
    class Program
    {
        static int Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var backEndUserPassword = Environment.GetEnvironmentVariable("BackEndUserPassword");
            
            var csb = new SqlConnectionStringBuilder(connectionString);
            Console.WriteLine($"Deploying database: {csb.InitialCatalog}");

            Console.WriteLine("Testing connection...");
            var conn = new SqlConnection(csb.ToString());
            conn.Open();
            conn.Close();

            Console.WriteLine("Starting deployment...");
            var dbup = DeployChanges.To
                .SqlDatabase(csb.ConnectionString)
                .WithScriptsFromFileSystem("./sql") 
                .JournalToSqlTable("dbo", "$__dbup_journal")                                               
                .WithVariable("BackEndUserPassword", backEndUserPassword)
                .LogToConsole()
                .Build();
         
            var result = dbup.PerformUpgrade();

            if (!result.Successful)
            {
                Console.WriteLine(result.Error);
                return -1;
            }

            Console.WriteLine("Success!");
            return 0;
        }
    }
}
