using RoomReservatingSystem.Shared;
using RoomReservationSystem.Desktop.Services;
using RoomReservationSystem.Desktop.UserControls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RoomReservationSystem.Desktop.UserControls
{
    public partial class UsersControl : UserControl
    {
        private ApiService? _apiService;
        private List<User> _users = new List<User>();

        public UsersControl()
        {
            InitializeComponent();
        }

        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            _users = await _apiService.Client.GetFromJsonAsync<List<User>>("Api/Users") ?? new List<User>();
            ApplyFilters(); // Aplikujeme filtry rovnou po načtení
        }

        // Společná událost pro změnu textu i kliknutí na CheckBox
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_users == null) return;

            var filteredList = _users.AsEnumerable();

            // 1. Filtr na smazané uživatele
            bool showDeleted = ShowDeletedCB.IsChecked ?? false;
            if (!showDeleted)
            {
                filteredList = filteredList.Where(r => !r.IsDeleted);
            }

            // 2. Filtr na text (ID nebo Username)
            string userText = SearchTB.Text.Trim();
            if (!string.IsNullOrWhiteSpace(userText))
            {
                if (int.TryParse(userText, out int userId))
                {
                    filteredList = filteredList.Where(r => r.Id == userId ||
                                                           (r.Username != null && r.Username.Contains(userText, StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    filteredList = filteredList.Where(r => r.Username != null && r.Username.Contains(userText, StringComparison.OrdinalIgnoreCase));
                }
            }

            UsersDG.ItemsSource = filteredList.ToList();
        }

        private void UserDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var selected = UsersDG.SelectedItems.Cast<User>().ToList();

            if (selected.Count == 0)
            {
                DisableActionButtons();
            }
            else if (selected.Count == 1)
            {
                User selectedUser = selected.FirstOrDefault();

                // Nelze upravovat smazané uživatele nebo adminy
                if (selectedUser.IsDeleted)
                {
                    DisableActionButtons();
                    editBTN.Content = "Deleted (Read-only)";
                    deleteBTN.Content = "Already deleted";
                }
                else if (selectedUser.IsAdmin)
                {
                    DisableActionButtons();
                }
                else
                {
                    deleteBTN.IsEnabled = true;
                    editBTN.IsEnabled = true;
                    editBTN.Content = $"Edit {selectedUser.Id} - {selectedUser.Username}";
                    deleteBTN.Content = $"Delete {selectedUser.Id} - {selectedUser.Username}";
                }
            }
            else
            {
                // Pokud vybere více záznamů a je mezi nimi aspoň jeden smazaný, zablokujeme hromadnou akci
                if (selected.Any(u => u.IsDeleted))
                {
                    DisableActionButtons();
                    editBTN.Content = "Selection contains deleted users";
                    deleteBTN.Content = "Selection contains deleted users";
                }
                else
                {
                    deleteBTN.IsEnabled = true;
                    editBTN.IsEnabled = true;
                    editBTN.Content = "Edit selected users";
                    deleteBTN.Content = "Delete selected users";
                }
            }
        }

        private void DisableActionButtons()
        {
            deleteBTN.IsEnabled = false;
            editBTN.IsEnabled = false;
            editBTN.Content = "Edit";
            deleteBTN.Content = "Delete";
        }

        private async void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            UserDialog dialog = new UserDialog(_apiService);
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                _ = LoadUsers();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersDG.SelectedItems.Cast<User>().ToList();

            // Zabráníme editaci adminů i smazaných uživatelů
            var validToEdit = selected.Where(u => !u.IsAdmin && !u.IsDeleted).ToList();

            if (!validToEdit.Any())
            {
                Helper.ShowWarning("No valid user selected for editing (Admins and deleted users cannot be edited).");
                return;
            }

            bool refresh = false;
            foreach (var user in validToEdit)
            {
                UserDialog dialog = new UserDialog(_apiService, user);
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    refresh = true;
                }
            }

            if (refresh)
            {
                LoadUsers();
            }
        }

        private async Task<List<User>> GetNonAdminsAsync(List<User> users)
        {
            var admins = users.Where(r => r.IsAdmin).ToList();
            var nonAdmins = users.Where(r => !r.IsAdmin).ToList();

            if (admins.Any())
            {
                string adminsStr = string.Join("\n", admins.Select(r => $"• ID {r.Id}: {r.Username}"));
                string nonAdminsStr = string.Join("\n", nonAdmins.Select(r => $"• ID {r.Id}: {r.Username}"));

                MessageBoxResult dialogResult = MessageBox.Show(
                    $"Following users are admins and cannot be deleted:\n\n{adminsStr}\n\nDo you want to continue the action ONLY with valid users?\n\n{nonAdminsStr}",
                    "Found admins",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    return await Task.FromResult(nonAdmins);
                }
                else
                {
                    return await Task.FromResult(new List<User>());
                }
            }
            return await Task.FromResult(nonAdmins);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersDG.SelectedItems.Cast<User>().ToList();
            var validToProcess = await GetNonAdminsAsync(selected);

            if (!validToProcess.Any()) return;

            if (MessageBox.Show("This step is irreversible.", "Delete", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;

            bool needsReload = false;

            foreach (var user in validToProcess)
            {
                try
                {
                    var reply = await _apiService.Client.DeleteAsync($"api/Users/{user.Id}");
                    if (!reply.IsSuccessStatusCode)
                    {
                        Helper.ShowWarning($"User {user.Id} - {user.Username} cannot be deleted.");
                    }
                    else
                    {
                        needsReload = true;
                    }
                }
                catch (Exception)
                {
                    Helper.ShowWarning($"Network error while trying to delete user: {user.Id} - {user.Username}.");
                }
            }

            if (needsReload)
            {
                await LoadUsers();
            }
        }
    }
}