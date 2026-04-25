using RoomReservatingSystem.Shared;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace RoomReservationSystem.Repositories
{
    public class UserRepository
    {
        private readonly string _connection;

        public UserRepository(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                string sql = "SELECT * FROM [User] WHERE Username = @Username AND IsDeleted = 0;";
                return await db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
            }
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                string sql = "SELECT * FROM [User] WHERE Id = @Id AND IsDeleted = 0;";
                return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
            }
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string deleteUserSql = "UPDATE [User] SET IsDeleted = 1 WHERE Id = @Id";
                        await db.ExecuteAsync(deleteUserSql, new { Id = userId }, transaction);

                        // 2. Cancel future active reservations for this user
                        // We use the integer value '1' for Active status based on the typical Enum mapping, 
                        // and '2' for Cancelled. You may need to adjust these values if your enum mapping differs.
                        string cancelReservationsSql = @"
                            UPDATE Reservation 
                            SET Status = @CancelledStatus 
                            WHERE OrganizerId = @UserId 
                              AND Status = @ActiveStatus 
                              AND StartTime > GETDATE()";

                        await db.ExecuteAsync(cancelReservationsSql, new
                        {
                            UserId = userId,
                            CancelledStatus = ReservationStatus.Cancelled,
                            ActiveStatus = ReservationStatus.Active
                        }, transaction);

                        // If both succeed, commit the transaction
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        // If anything fails, rollback the transaction
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<int?> CreateUserAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.PasswordHash);
            using (IDbConnection db = new SqlConnection(_connection))
            {
                return await db.InsertAsync<User>(user);
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                return await db.GetListAsync<User>("WHERE IsDeleted = 0");
            }
        }
    }
}