using Dapper;
using Microsoft.Data.SqlClient;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.ViewModels;
using System.Data;
using System.Data.Common;
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

        public async Task<IEnumerable<Reservation>> GetReservationsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.GetListAsync<Reservation>();
            }
        }


        public async Task<IEnumerable<UserProfileReservationViewModel>> GetFilteredReservationsAsync(int roomId, string? purpose, bool hideCancelled, int? loggedUserId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string sql = @"
            SELECT 
                r.Id, r.RoomId, r.StartTime, r.EndTime, r.Purpose, r.PersonCount, r.Status,
                rm.Name AS RoomName
            FROM Reservation r
            INNER JOIN Room rm ON r.RoomId = rm.Id
            WHERE 1=1 ";

                var parameters = new DynamicParameters();


                // room is selected (greater than 0)
                if (roomId > 0)
                {
                    sql += " AND r.RoomId = @RoomId ";
                    parameters.Add("@RoomId", roomId);
                }

                // purpose is not empty
                if (!string.IsNullOrWhiteSpace(purpose))
                {
                    sql += " AND r.Purpose LIKE @Purpose ";
                    parameters.Add("@Purpose", "%" + purpose + "%");
                }

                if (hideCancelled)
                {
                    sql += " AND r.Status = @ActiveStatus ";
                    parameters.Add("@ActiveStatus", ReservationStatus.Active);
                }

                if (loggedUserId.HasValue)
                {
                    sql += " AND r.OrganizerId = @OrganizerId ";
                    parameters.Add("@OrganizerId", loggedUserId.Value);
                }
                return await db.QueryAsync<UserProfileReservationViewModel>(sql, parameters);
            }
        }
    }
}
