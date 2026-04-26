using Microsoft.Data.SqlClient;
using RoomReservatingSystem.Shared;
using System.Data;
using Dapper;

namespace RoomReservationSystem.Repositories
{
    public class RoomRepository
    {
        private readonly string _connectionString;

        public RoomRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync(int? minCapacity = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                if (minCapacity.HasValue && minCapacity.Value > 0)
                {
                    string sql = "SELECT * FROM Room WHERE Capacity >= @MinCapacity";
                    return await db.QueryAsync<Room>(sql, new { MinCapacity = minCapacity.Value });
                }
                return await db.GetListAsync<Room>();
            }
        }

        public async Task<int?> AddRoomAsync(Room room)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var newId = await db.InsertAsync<Room>(room);
                return (int) newId;
            }
        }

        public async Task<bool> DeleteAsync(int roomId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.DeleteAsync<Room>(roomId);
                    return rowsAffected > 0;
                }
                catch (SqlException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<Room> GetRoomByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetAsync<Room>(id);
            }
        }

        public async Task<bool> UpdateRoomAsync(Room roomToUpdate)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                int rowsAffected = await db.UpdateAsync<Room>(roomToUpdate);
                return rowsAffected > 0;
            }
        }
    }
}
