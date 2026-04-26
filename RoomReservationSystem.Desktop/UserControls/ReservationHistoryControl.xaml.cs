using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared.DTOs;
using RoomReservationSystem.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RoomReservationSystem.Desktop.UserControls
{
    public partial class ReservationHistoryControl : UserControl
    {
        private ApiService? _apiService;
        private List<ReservationHistoryDTO> _historyRecords = new List<ReservationHistoryDTO>();

        public ReservationHistoryControl()
        {
            InitializeComponent();
        }

        // Tuto metodu zavoláš z AdminPanelWindow, stejně jako jsi to udělal u Users/Rooms
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                _historyRecords = await _apiService.Client.GetFromJsonAsync<List<ReservationHistoryDTO>>("Api/Reservations/history")
                                  ?? new List<ReservationHistoryDTO>();
                ApplyFilters();
            }
            catch (Exception)
            {
                Helper.ShowWarning("Failed to load reservation history from the server.");
            }
        }

        private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _ = LoadHistoryAsync();
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_historyRecords == null) return;

            var filteredList = _historyRecords.AsEnumerable();

            // Filtr na Místnost
            string roomText = FilterRoomTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(roomText))
            {
                filteredList = filteredList.Where(r => r.RoomName != null && r.RoomName.Contains(roomText, StringComparison.OrdinalIgnoreCase));
            }

            // Filtr na Uživatele (toho, kdo to změnil)
            string userText = FilterUserTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(userText))
            {
                filteredList = filteredList.Where(r => r.ChangedByUserName != null && r.ChangedByUserName.Contains(userText, StringComparison.OrdinalIgnoreCase));
            }

            // Filtr na Účel
            string purposeText = FilterPurposeTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(purposeText))
            {
                filteredList = filteredList.Where(r => r.Purpose != null && r.Purpose.Contains(purposeText, StringComparison.OrdinalIgnoreCase));
            }

            HistoryDG.ItemsSource = filteredList.ToList();
        }

        // Tady schováme IDčka a uděláme hezké názvy sloupců
        private void HistoryDG_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id" || e.PropertyName == "ReservationId")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            else if (e.PropertyName == "ChangedAt")
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss";
                e.Column.Header = "Changed At";
            }
            else if (e.PropertyName == "OldStatus")
            {
                e.Column.Header = "Old Status";
            }
            else if (e.PropertyName == "NewStatus")
            {
                e.Column.Header = "New Status";
            }
            else if (e.PropertyName == "ChangedByUserName")
            {
                e.Column.Header = "Changed By User";
            }
            else if (e.PropertyName == "RoomName")
            {
                e.Column.Header = "Room";
            }
            else if (e.PropertyName == "Purpose")
            {
                e.Column.Header = "Purpose";
            }
        }
    }
}