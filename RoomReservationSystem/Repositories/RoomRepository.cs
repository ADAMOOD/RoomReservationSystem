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
        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetListAsync<Room>();
            }
        }
        public async Task<int> AddRoomAsync(Room room)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var newId = await db.InsertAsync<Room>(room);
                return (int)newId;
            }
        }
    }
}
