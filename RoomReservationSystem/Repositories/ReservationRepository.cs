using Dapper;
using Microsoft.Data.SqlClient;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.ViewModels;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using RoomReservatingSystem.Shared.DTOs;

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

        public async Task<IEnumerable<ReservationDTO>> GetReservationDTOsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                SELECT 
                    r.Id, r.RoomId, r.OrganizerId, r.StartTime, r.EndTime, r.Purpose, r.PersonCount, r.Status,
                    rm.Name AS RoomName, u.UserName AS UserName
                FROM Reservation r
                INNER JOIN Room rm ON r.RoomId = rm.Id
                INNER JOIN [User] u ON r.OrganizerId = u.Id";

                return (await db.QueryAsync<ReservationDTO>(sql)).ToList();
            }
        }

        public async Task AddHistoryRecordAsync(int reservationId, ReservationStatus? oldStatus, ReservationStatus newStatus, int changedByUserId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
            INSERT INTO ReservationHistory (ReservationId, OldStatus, NewStatus, ChangedByUserId)
            VALUES (@ReservationId, @OldStatus, @NewStatus, @ChangedByUserId)";

                await db.ExecuteAsync(sql, new
                {
                    ReservationId = reservationId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedAt = DateTime.Now,
                    ChangedByUserId = changedByUserId
                });
            }
        }

        
        public async Task<bool> DeleteReservationAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // delete history records first to maintain referential integrity
                        string deleteHistorySql = "DELETE FROM ReservationHistory WHERE ReservationId = @Id";
                        await db.ExecuteAsync(deleteHistorySql, new { Id = id }, transaction);

                        // now we can delete the actual reservation
                        string deleteReservationSql = "DELETE FROM Reservation WHERE Id = @Id";
                        int deleted = await db.ExecuteAsync(deleteReservationSql, new { Id = id }, transaction);

                        transaction.Commit();
                        return deleted > 0;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        public async Task<IEnumerable<UserProfileReservationViewModel>> GetFilteredReservationsAsync(int roomId, string? purpose, bool hideCancelled, int? loggedUserId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {

                string sql = @"
            SELECT 
                r.Id,r.OrganizerId, r.RoomId, r.StartTime, r.EndTime, r.Purpose, r.PersonCount, r.Status,
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
        public async Task<bool> CheckCollisionAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeReservationId = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT COUNT(1) 
            FROM Reservation 
            WHERE RoomId = @RoomId 
              AND Status = @ActiveStatus
              AND StartTime < @EndTime 
              AND EndTime > @StartTime";
                var parameters = new DynamicParameters();
                parameters.Add("@RoomId", roomId);
                parameters.Add("@StartTime", startTime);
                parameters.Add("@EndTime", endTime);
                parameters.Add("@ActiveStatus", ReservationStatus.Active);
                if (excludeReservationId.HasValue)
                {
                    sql += " AND Id != @ExcludeReservationId ";
                    parameters.Add("@ExcludeReservationId", excludeReservationId.Value);
                }
                int count = await db.ExecuteScalarAsync<int>(sql, parameters);
                return count > 0;
            }
        }
        public async Task<bool> UpdateStatusAsync(int reservationId, ReservationStatus newStatus)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Reservation SET Status = @Status WHERE Id = @Id";
                var parameters = new { Status = newStatus, Id = reservationId };
                int rowsAffected = await db.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }
        internal async Task<bool> UpdateRoomAsync(Reservation updatedReservation)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    int rowsAffected = await db.UpdateAsync<Reservation>(updatedReservation);
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

        public async Task<IEnumerable<ReservationHistoryDTO>> GetReservationHistoryAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT 
                rh.Id, 
                rh.ReservationId, 
                rh.OldStatus, 
                rh.NewStatus, 
                rh.ChangedAt,
                u.Username AS ChangedByUserName,
                rm.Name AS RoomName,
                re.Purpose AS Purpose
            FROM ReservationHistory rh
            LEFT JOIN [User] u ON rh.ChangedByUserId = u.Id
            LEFT JOIN Reservation re ON rh.ReservationId = re.Id
            LEFT JOIN Room rm ON re.RoomId = rm.Id
            ORDER BY rh.ChangedAt DESC";

                return await db.QueryAsync<ReservationHistoryDTO>(sql);
            }
        }
    }
}
