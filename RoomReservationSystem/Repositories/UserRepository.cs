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

                        ///first we insert records about future reservations of this user to the history,
                        /// then we cancel these reservations (we need to do it in this order, because after cancellation the reservation
                        /// will not be visible for the user and we won't be able to get the data for history)
                        string insertHistorySql = @"
                    INSERT INTO ReservationHistory (ReservationId, OldStatus, NewStatus, ChangedAt, ChangedByUserId)
                    SELECT Id, @ActiveStatus, @CancelledStatus, GETDATE(), @UserId 
                    FROM Reservation 
                    WHERE OrganizerId = @UserId 
                      AND Status = @ActiveStatus 
                      AND StartTime > GETDATE()";

                        await db.ExecuteAsync(insertHistorySql, new
                        {
                            UserId = userId,
                            CancelledStatus = ReservationStatus.Cancelled,
                            ActiveStatus = ReservationStatus.Active
                        }, transaction);

                        // after we have recorded the history of future reservations, we can cancel them,
                        // so they won't be visible for the user in the profile and won't cause any confusion
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

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
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
                try
                {
                    var newId = await db.InsertAsync<User>(user);
                    return (int)newId;
                }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                {
                    // Error for duplicate key (unique constraint violation)
                    return null;
                }
                catch (Exception)
                {
                    // Other database errors
                    return null;
                }
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                return await db.GetListAsync<User>();
            }
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            using (IDbConnection db = new SqlConnection(_connection))
            {
                string sql;
                var parameters = new DynamicParameters();
                parameters.Add("@Id", user.Id);
                parameters.Add("@Username", user.Username);
                parameters.Add("@IsAdmin", user.IsAdmin);

                // Kontrola, zda bylo zasláno nové heslo (není null ani prázdné)
                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    // Hashing probíhá zde na backendu těsně před uložením
                    string newHash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.PasswordHash);
                    parameters.Add("@PasswordHash", newHash);

                    sql = @"UPDATE [User] 
                    SET Username = @Username, 
                        PasswordHash = @PasswordHash, 
                        IsAdmin = @IsAdmin 
                    WHERE Id = @Id";
                }
                else
                {
                    // Pokud heslo nepřišlo, sloupec PasswordHash v SQL vynecháme
                    sql = @"UPDATE [User] 
                    SET Username = @Username, 
                        IsAdmin = @IsAdmin 
                    WHERE Id = @Id";
                }

                int rowsAffected = await db.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }
    }
}