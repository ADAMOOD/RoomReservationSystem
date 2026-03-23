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
        public IEnumerable<Room> GetAllRooms()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.GetList<Room>();
            }
        }
        public int AddRoom(Room room)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (int)db.Insert(room);
            }
        }
    }
}
