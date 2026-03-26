using Microsoft.Data.SqlClient;
using RoomReservatingSystem.Shared;
using System.Data;
using System.Runtime.CompilerServices;
using Dapper;

namespace RoomReservationSystem.Repositories
{
    public class ReservationRepository
    {
        private readonly string _connectionString;
        public ReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<Reservation>> GetAllReservationsForRoomAsync(int roomId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetListAsync<Reservation>(new { RoomId = roomId, Status = ReservationStatus.Active });
            }
        }

        public async Task<int?> CreateReservationAsync(Reservation reservation)
        {
            using (IDbConnection db = new SqlConnection(_connectionString) )
            {
                var newReservation = await db.InsertAsync<Reservation>(reservation);
                return (int)newReservation;
            }
        }
    }
}
