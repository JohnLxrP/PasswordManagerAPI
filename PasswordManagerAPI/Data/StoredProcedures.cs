using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PasswordManagerAPI.Data;
using System.Threading.Tasks;

namespace PasswordManagerAPI
{
    public static class StoredProcedures
    {
        public static async Task ExecuteInsertPasswordmngrStoredProcedure(apiDBContext dbContext, int userId, string account, string password, string description)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                @"EXEC [dbo].[InsertPasswordmngr] @UserId, @Accountr, @Passwordr, @Description",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Accountr", account),
                new SqlParameter("@Passwordr", password),
                new SqlParameter("@Description", description)
            );
        }

        // Define more stored procedure methods as needed
    }
}