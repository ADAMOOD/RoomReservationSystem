using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared.DTOs;
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

namespace RoomReservationSystem.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : UserControl
    {
        private ApiService? _apiService;
        private List<User> _users = new List<User>();
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadUsers();
        }
        public UsersControl()
        {
            InitializeComponent();
        }

        private async Task LoadUsers()
        {
            _users = await _apiService.Client.GetFromJsonAsync<List<User>>("Api/Users");
            UsersDG.ItemsSource = _users;
        }


        private void UserDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var selected = UsersDG.SelectedItems.Cast<User>().ToList();

            if (selected.Count == 0)
            {
                deleteBTN.IsEnabled = false;
                editBTN.IsEnabled = false;
                editBTN.Content = "Edit";
                deleteBTN.Content = "Delete";
            }
            else if (selected.Count == 1)
            {
                User selectedUser = selected.FirstOrDefault();
                if (!selectedUser.IsAdmin)
                {
                    deleteBTN.IsEnabled = true;
                    editBTN.IsEnabled = true;
                    editBTN.Content = $"Edit {selectedUser.Id} - {selectedUser.Username}";
                    deleteBTN.Content = $"Delete {selectedUser.Id} - {selectedUser.Username}";
                }
                else
                {
                    deleteBTN.IsEnabled = false;
                    editBTN.IsEnabled = false;
                    editBTN.Content = "Edit";
                    deleteBTN.Content = "Delete";
                }

            }
            else
            {
                deleteBTN.IsEnabled = true;
                editBTN.IsEnabled = true;
                editBTN.Content = "Edit selected users";
                deleteBTN.Content = "Delete selected users";
            }
        }

        private void SearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_users == null) return;

            var filteredList = _users.AsEnumerable();

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

        private async void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

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

            // IMPORTANT - here have to be valid reservations
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
