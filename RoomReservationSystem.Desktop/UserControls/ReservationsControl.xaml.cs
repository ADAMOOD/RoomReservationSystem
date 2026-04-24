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
        private List<ReservationDTO> GetValidFutureReservations(List<ReservationDTO> selectedReservations)
        {
            var pastReservations = selectedReservations.Where(r => r.StartTime < DateTime.Now).ToList();
            var validReservations = selectedReservations.Where(r => r.StartTime >= DateTime.Now).ToList();

            if (pastReservations.Any())
            {
                string pastNames = string.Join("\n", pastReservations.Select(r => $"• ID {r.Id}: {r.RoomName} ({r.StartTime:g})"));

                string validNames = string.Join("\n", validReservations.Select(r => $"• ID {r.Id}: {r.RoomName} ({r.StartTime:g})"));

                MessageBoxResult dialogResult = MessageBox.Show(
                    $"Following reservations have already occurred:\n\n{pastNames}\n\nDo you want to continue the action ONLY with valid future reservations?\n\n{validNames}",
                    "Found past reservations",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    return validReservations;
                }
                else
                {
                    return new List<ReservationDTO>();
                }
            }
            return validReservations;
        }
        private void ReservationsDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();

            if (selected.Count == 0)
            {
                deleteBTN.IsEnabled = false;
                editBTN.IsEnabled = false;
                editBTN.Content = "Edit";
                deleteBTN.Content = "Delete";
            }
            else if (selected.Count == 1)
            {
                var res = selected.First();
                bool isPast = res.StartTime < DateTime.Now;
                deleteBTN.IsEnabled = !isPast;
                editBTN.IsEnabled = !isPast;

                if (isPast)
                {
                    editBTN.Content = "Cannot edit past res.";
                    deleteBTN.Content = "Cannot delete past res.";
                }
                else
                {
                    editBTN.Content = $"Edit {res.Id} - {res.RoomName}";
                    deleteBTN.Content = $"Delete {res.Id} - {res.RoomName}";
                }
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

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();

            var validToProcess = GetValidFutureReservations(selected);

            if (!validToProcess.Any()) return;
            Dialogs.ReservationDialog editResDialog = new Dialogs.ReservationDialog(_apiService, validToProcess);

            bool? result = editResDialog.ShowDialog();

            if (result == true)
            {
                await LoadReservationsAsync();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();
            var validToProcess = GetValidFutureReservations(selected);

            if (!validToProcess.Any()) return;

            if (MessageBox.Show("This step is irreversible.", "Delete", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;

            bool needsReload = false;

            // IMPORTANT - here have to be valid reservations
            foreach (var reservation in validToProcess)
            {
                try
                {
                    var reply = await _apiService.Client.DeleteAsync($"api/Reservations/{reservation.Id}");
                    if (!reply.IsSuccessStatusCode)
                    {
                        Helper.ShowWarning($"Reservation {reservation.Id} cannot be deleted.");
                    }
                    else
                    {
                        needsReload = true;
                    }
                }
                catch (Exception)
                {
                    Helper.ShowWarning($"Network error while trying to delete reservation: {reservation.Id}.");
                }
            }

            if (needsReload)
            {
                await LoadReservationsAsync();
            }
        }


        private async void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadReservationsAsync();

        }

        private async void activateBTN(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();

            var validToProcess = GetValidFutureReservations(selected)
                .Where(r => r.Status == ReservationStatus.Cancelled)
                .ToList();

            if (!validToProcess.Any()) return;

            bool needsReload = false;

            foreach (var reservation in validToProcess)
            {
                try
                {
                    var reply = await _apiService.Client.PutAsync($"Api/Reservations/{reservation.Id}/activate", null);

                    if (!reply.IsSuccessStatusCode)
                    {
                        string errorMsg = await reply.Content.ReadAsStringAsync();
                        Helper.ShowWarning($"Reservation {reservation.Id} cannot be activated.\nReason: {errorMsg}");
                    }
                    else
                    {
                        needsReload = true;
                    }
                }
                catch (Exception)
                {
                    Helper.ShowWarning($"Network error while trying to activate reservation: {reservation.Id}.");
                }
            }

            if (needsReload)
            {
                await LoadReservationsAsync();

                ReservationsDG_OnSelectedCellsChanged(null, null);
            }
        }

        private async void canbcelBTN(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDG.SelectedItems.Cast<ReservationDTO>().ToList();

            var validToProcess = GetValidFutureReservations(selected)
                .Where(r => r.Status == ReservationStatus.Active)
                .ToList();

            if (!validToProcess.Any()) return;

            bool needsReload = false;

            foreach (var reservation in validToProcess)
            {
                try
                {
                    var reply = await _apiService.Client.PutAsync($"Api/Reservations/{reservation.Id}/cancel", null);

                    if (!reply.IsSuccessStatusCode)
                    {
                        string errorMsg = await reply.Content.ReadAsStringAsync();
                        Helper.ShowWarning($"Reservation {reservation.Id} cannot be canceled.\nReason: {errorMsg}");
                    }
                    else
                    {
                        needsReload = true;
                    }
                }
                catch (Exception)
                {
                    Helper.ShowWarning($"Network error while trying to cancel reservation: {reservation.Id}.");
                }
            }

            if (needsReload)
            {
                await LoadReservationsAsync();
                ReservationsDG_OnSelectedCellsChanged(null, null);
            }
        }
    }
}
