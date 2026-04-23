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
using RoomReservationSystem.Desktop.UserControls.Dialogs;

namespace RoomReservationSystem.Desktop.UserControls
{

    public partial class ReservationsControl : UserControl
    {
        private ApiService? _apiService;
        private List<ReservationDTO> _reservations;
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadReservationsAsync();
        }
        public ReservationsControl( )
        {
            InitializeComponent();
        }
            
        private async Task LoadReservationsAsync()
        {
            _reservations = await _apiService.Client.GetFromJsonAsync<List<ReservationDTO>>("Api/Reservations");
            Filter_TextChanged(null, null);
        }

        private void ReservationsDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (ReservationsDG.SelectedItems.Count == 0)
            {
                deleteBTN.IsEnabled = false;
                editBTN.IsEnabled = false;
                editBTN.Content = "Edit";
                deleteBTN.Content = "Delete";
            }
            else if (ReservationsDG.SelectedItems.Count == 1)
            {
                var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().FirstOrDefault();
                deleteBTN.IsEnabled = true;
                editBTN.IsEnabled = true;

                editBTN.Content = $"Edit {selected?.Id} - {selected?.RoomName} - {selected?.UserName}";
                deleteBTN.Content = $"Delete {selected?.Id} - {selected?.RoomName} - {selected?.UserName}";
            }
            else
            {
                deleteBTN.IsEnabled = true;
                editBTN.IsEnabled = true;
                editBTN.Content = "Edit selected reservations";
                deleteBTN.Content = "Delete selected reservations";
            }
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ReservationDialog rd = new ReservationDialog(_apiService);
            bool? result = rd.ShowDialog();

            if (result == true)
            {
                await LoadReservationsAsync();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select reservations to Edit", "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ReservationDialog rd = new ReservationDialog(_apiService, selected);
            rd.ShowDialog();
            LoadReservationsAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select reservations you want to delete", "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("This step is irreversible.", "Delete", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;

            bool needsReload = false;
            foreach (var reservation in selected)
            {
                try
                {
                    var reply = await _apiService.Client.DeleteAsync($"api/Reservations/{reservation.Id}");
                    if (!reply.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Reservation {reservation?.Id} - {reservation?.RoomName} - {reservation?.UserName} cannot be deleted.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        needsReload = true;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Network error while trying to delete room: {reservation?.Id} - {reservation?.RoomName} - {reservation?.UserName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (needsReload)
            {
                await LoadReservationsAsync();
                ReservationsDG_OnSelectedCellsChanged(null, null);
            }
        }


        private async void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadReservationsAsync();
        }
    }
}
