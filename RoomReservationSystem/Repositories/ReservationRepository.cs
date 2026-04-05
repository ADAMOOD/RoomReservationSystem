using Dapper;
using Microsoft.Data.SqlClient;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.ViewModels;
using System.Data;
using System.Runtime.CompilerServices;

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
        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetAsync<Reservation>(id);
            }
        }

        public async Task<int?> CreateReservationAsync(Reservation reservation)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var newReservation = await db.InsertAsync<Reservation>(reservation);
                return (int)newReservation;
            }
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsByUserAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetListAsync<Reservation>(new { OrganizerId = id });
            }
        }

        public async Task<IEnumerable<UserProfileReservationViewModel>> GetReservationsWithRoomNameByUserAsync(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString)) 
            {
                string sql = @"
            SELECT 
                re.Id, re.RoomId, re.StartTime, re.EndTime, re.Purpose, re.PersonCount, re.Status,
                rm.Name AS RoomName
            FROM Reservation re
            INNER JOIN Room rm ON re.RoomId = rm.Id
            WHERE re.OrganizerId = @UserId
            ORDER BY re.StartTime DESC";
                return await db.QueryAsync<UserProfileReservationViewModel>(sql, new { UserId = userId });
            }
        }
        public async Task UpdateReservationAsync(Reservation reservation)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.UpdateAsync(reservation);
            }
        }
    }
}
