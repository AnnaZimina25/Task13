using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Binder;
using Task13.DbWork;
using Task13.DbWork.Model;
using Task13.WebServer;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using System.Threading;
using System.Net.Http.Headers;

namespace Task13
{
    class Program
    {


        static async Task Main()
        {

            // Делаем миграцию
            await DbMigration();

            // Запускаем веб сервер
            WebServerWork.HttpServer server = new WebServerWork.HttpServer(80);
            server.Start();

        }

        private static async Task DbMigration()
        {
            AppConfig configObj = DatabaseServer.GetConfigObj();
            string user = configObj.User.Login;
            string pw = configObj.User.Password;
            string dbn = configObj.User.DatabaseName;

            // подключаемся через супер юзера

            string connString1 = DatabaseServer.GetConnectString(configObj.DbHost, configObj.SuperUser.Login, configObj.SuperUser.Password, configObj.SuperUser.DatabaseName);

            await using (var conn1 = new NpgsqlConnection(connString1))
            {
                await conn1.OpenAsync();

                // Define a query
                var isUserExist = await DatabaseServer.IsUserExist(conn1, user);
                var isDatabaseExists = await DatabaseServer.IsDatabaseExist(conn1, dbn);

                if (isUserExist)
                {
                    Console.WriteLine($"{isUserExist}, User exists.");

                }
                else
                {
                    Console.WriteLine($"{isUserExist}, User doesn't exists. Creating new user...");

                    await DatabaseServer.UserCreation(conn1, user, pw);
                }

                if (isDatabaseExists)
                {
                    Console.WriteLine($"{isDatabaseExists}, Database exists.");
                }
                else
                {
                    Console.WriteLine($"{isDatabaseExists}, Database doesn't exist. Creating new database...");
                    await DatabaseServer.DatabaseCreation(conn1, user, dbn);
                }

                // подключаемся через юзера
                string connString2 = DatabaseServer.GetConnectString(configObj.DbHost, configObj.User.Login, configObj.User.Password, configObj.User.DatabaseName);

                await using (var conn2 = new NpgsqlConnection(connString2))
                {
                    await conn2.OpenAsync();
                    string tableName = "form";
                    var isTableExists = await DatabaseServer.IsCurrentTableExist(conn2, tableName);

                    if (isTableExists)
                    {
                        Console.WriteLine($"Table {tableName} exists");
                    }
                    else
                    {
                        await DatabaseServer.TableCreation(conn2, tableName);
                        Console.WriteLine($"Table {tableName} is created");
                    }
                }

            }
        }
    }
}


