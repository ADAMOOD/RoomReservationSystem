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
                string sql = "SELECT * FROM [User] WHERE Username = @Username;";
                return await db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
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
                return await db.GetListAsync<User>();
            }
        }
    }
}