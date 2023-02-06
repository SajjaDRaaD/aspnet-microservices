using Dapper;
using Discount.API.Entities;
using Npgsql;
using System.Data;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IDbConnection _db;

        public DiscountRepository(IConfiguration configuration)
        {
            _db = new NpgsqlConnection(configuration.GetConnectionString("Default"));
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            var affected =
                await _db.ExecuteAsync
                    ("INSERT INTO Coupon (ProductName,Amount,Description) VALUES (@ProductName,@Amount,@Description)",
                        new { ProductName = coupon.ProductName, Amount = coupon.Amount, Description = coupon.Description });

            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            var affected =
                await _db.ExecuteAsync
                    ("DELETE FROM Coupon WHERE ProductName = @ProductName",
                        new { ProductName = productName });

            if (affected == 0)
                return false;

            return true;
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            var coupon = await _db.QueryFirstOrDefaultAsync<Coupon>("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });

            if (coupon == null)
                return new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Description" };

            return coupon;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            var affected =
                await _db.ExecuteAsync
                    ("UPDATE Coupon SET ProductName=@ProductName, Amount=@Amount, Description=@Description WHERE Id=@Id",
                        new { ProductName = coupon.ProductName, Amount = coupon.Amount, Description = coupon.Description, Id = coupon.Id });

            if (affected == 0)
                return false;


            return true;
        }
    }
}
