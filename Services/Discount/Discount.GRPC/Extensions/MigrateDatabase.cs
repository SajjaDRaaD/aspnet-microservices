using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;

namespace Discount.GRPC.Extensions
{
    public static class MigrateDatabase
    {
        public static WebApplication MigratePostgreDatabase(this WebApplication app, int? retry = 0)
        {
            var retryForAvailability = retry.Value;

            var services = app.Services;
            var configuration = services.GetService<IConfiguration>();
            var logger = services.GetService<ILogger<WebApplication>>();
            var connectionString = configuration.GetConnectionString("Default");

            try
            {
                logger.LogInformation("Migrate postgresql database.");

                using var db = new NpgsqlConnection(connectionString);
                db.Open();

                using var command = new NpgsqlCommand()
                {
                    Connection = db
                };

                command.CommandText = "DROP TABLE IF EXISTS Coupon";
                command.ExecuteNonQuery();

                command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                command.ExecuteNonQuery();

                logger.LogInformation("Migrated postgresql database.");
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the postresql database");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    System.Threading.Thread.Sleep(2000);
                    MigratePostgreDatabase(app, retryForAvailability);
                }
            }

            return app;

        }
    }
}
