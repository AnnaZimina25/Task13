using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Binder;
using Task13.DbWork.Model;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using System.Threading;
using System.Net.Http.Headers;
using Task13.WebServer;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Task13.DbWork
{
    class DatabaseServer
    {
        
        
        public static async Task<ArrayList> TableDataOutput()
        {
            AppConfig configObj = DatabaseServer.GetConfigObj();
            
            string connectString = DatabaseServer.GetConnectString(configObj.DbHost, configObj.User.Login, configObj.User.Password, configObj.User.DatabaseName);

            await using (var conn = new NpgsqlConnection(connectString))
            {
               await conn.OpenAsync();
                await using (var cmd = new NpgsqlCommand("SELECT * FROM form", conn))
                {
                    var contentList = new ArrayList();


                    await using (NpgsqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (await dr.ReadAsync())
                        {
                            contentList.Add(dr.GetString(0));
                            contentList.Add(dr.GetString(1));
                        }


                        return contentList;
                    }
                }

            }
        }

        public static async Task TableDataInput(ArrayList dataValues)
        {
            AppConfig configObj = DatabaseServer.GetConfigObj();
            
            string connectString = DatabaseServer.GetConnectString(configObj.DbHost, configObj.User.Login, configObj.User.Password, configObj.User.DatabaseName);


            await using (var conn = new NpgsqlConnection(connectString))
            {
                await conn.OpenAsync();

                if (dataValues.Count > 1)
                {

                    await using (var cmd = new NpgsqlCommand("INSERT INTO form VALUES(@firstName, @lastName);", conn))
                    {

                        cmd.Parameters.AddWithValue("@firstName", NpgsqlDbType.Text, dataValues[0]);
                        cmd.Parameters.AddWithValue("@lastName", NpgsqlDbType.Text, dataValues[1]);
                        var t = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

        }


        public static async Task<bool> IsCurrentTableExist(NpgsqlConnection conn, string tableName)
        {
            await using (var cmd = new NpgsqlCommand("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@tableName;", conn))
            {
                cmd.Parameters.AddWithValue("@tableName", NpgsqlDbType.Text, tableName);

                await using (NpgsqlDataReader dr = cmd.ExecuteReader())
                {
                    return await dr.ReadAsync(CancellationToken.None);
                }
            }
        }

        public static async Task TableCreation(NpgsqlConnection conn, string tableName)
        {
            string colomn1 = "FirstName";
            string colomn2 = "LastName";

            await using (var cmd = new NpgsqlCommand($"CREATE TABLE {tableName} ( {colomn1} text, {colomn2} text );", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }

        }

        public static async Task DatabaseCreation(NpgsqlConnection conn, string user, string dbn)
        {
            await using (var cmd = new NpgsqlCommand($"CREATE DATABASE {dbn} WITH OWNER = {user}", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON DATABASE {dbn} TO {user};", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task UserCreation(NpgsqlConnection conn, string user, string pw)
        {
            await using (var cmd = new NpgsqlCommand($"CREATE USER {user} WITH PASSWORD '{pw}' CREATEDB", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> IsUserExist(NpgsqlConnection conn, string user)
        {
            await using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_roles WHERE rolname=@name", conn))
            {
                cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user);

                await using (NpgsqlDataReader dr = cmd.ExecuteReader())
                {
                    return await dr.ReadAsync(CancellationToken.None);
                }
            }
        }

        public static async Task<bool> IsDatabaseExist(NpgsqlConnection conn, string dbn)
        {
            await using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower(@dbn);", conn))
            {
                cmd.Parameters.AddWithValue("@dbn", NpgsqlDbType.Text, dbn);

                await using (NpgsqlDataReader dr = cmd.ExecuteReader())
                {
                    return await dr.ReadAsync(CancellationToken.None);
                }
            }
        }

        public static AppConfig GetConfigObj()
        {
             var builder = new ConfigurationBuilder()
                .SetBasePath(Directory
                .GetCurrentDirectory() + $"/DbWork")
                .AddJsonFile("config.json")
                .Build();

            var configObj = builder.Get<AppConfig>();

            Console.WriteLine($"Host: {configObj.DbHost}\r\nLogin: {configObj.SuperUser.Login}\r\nPassword: {configObj.SuperUser.Password}\r\nDatabaseName: {configObj.SuperUser.DatabaseName}");

            return configObj;

        }

        public static string GetConnectString(string host, string login, string password, string dataBaseName)
        {
            return $"Host={host};Username={login};Password={password};Database={dataBaseName}";
        }
    }
}
