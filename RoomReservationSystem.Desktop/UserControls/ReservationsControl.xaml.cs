using RoomReservationSystem.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared.DTOs;

namespace RoomReservationSystem.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for ReservationsControl.xaml
    /// </summary>
    public partial class ReservationsControl : UserControl
    {
        private ApiService? _apiService;
        private List<ReservationDTO> _reservations;
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadReservations();
        }
        public ReservationsControl( )
        {
            InitializeComponent();
        }
            
        private async Task LoadReservations()
        {
            _reservations = await _apiService.Client.GetFromJsonAsync<List<ReservationDTO>>("Api/Reservations");
            ReservationsDG.ItemsSource = _reservations;
        }

        private void ReservationsDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag != null)
            {
                string headerName = cb.Tag.ToString();
                var column = ReservationsDG.Columns.FirstOrDefault(c => c.Header.ToString() == headerName);

                if (column != null)
                {
                    column.Visibility = (cb.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }


        private void ReservationsDG_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "RoomName")
            {
                e.Column.Visibility = (ShowRoomNameCB.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (e.Column.Header.ToString() == "UserName")
            {
                e.Column.Visibility = (ShowUserNameCB.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_reservations == null) return;
            var filteredList = _reservations.AsEnumerable();

            string roomText = FilterRoomTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(roomText))
            {
                if (int.TryParse(roomText, out int roomId))
                {
                    filteredList = filteredList.Where(r => r.RoomId == roomId ||
                                                          (r.RoomName != null && r.RoomName.Contains(roomText, StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    filteredList = filteredList.Where(r => r.RoomName != null && r.RoomName.Contains(roomText, StringComparison.OrdinalIgnoreCase));
                }
            }

            string userText = FilterUserTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(userText))
            {
                if (int.TryParse(userText, out int userId))
                {
                    filteredList = filteredList.Where(r => r.OrganizerId == userId ||
                                                          (r.UserName != null && r.UserName.Contains(userText, StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    filteredList = filteredList.Where(r => r.UserName != null && r.UserName.Contains(userText, StringComparison.OrdinalIgnoreCase));
                }
            }

            string purposeText = FilterPurposeTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(purposeText))
            {
                filteredList = filteredList.Where(r => r.Purpose != null && r.Purpose.Contains(purposeText, StringComparison.OrdinalIgnoreCase));
            }
            ReservationsDG.ItemsSource = filteredList.ToList();
        }
    }
}
