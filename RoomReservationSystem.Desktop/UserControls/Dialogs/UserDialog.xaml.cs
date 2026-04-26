using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared.DTOs;
using RoomReservationSystem.Desktop.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RoomReservationSystem.Desktop.UserControls.Dialogs
{
    public partial class UserDialog : Window
    {
        private bool _isEditMode;
        private ApiService _apiService;
        private User _originalUser;

        // Constructor for adding new user
        public UserDialog(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            _isEditMode = false;
            this.Title = "Add New User";
            this.Loaded += UserDialog_Loaded;
        }

        // Constructor for editing user
        public UserDialog(ApiService apiService, User user)
        {
            InitializeComponent();
            _apiService = apiService;
            _originalUser = user;
            _isEditMode = true;
            this.Title = $"Edit User: {user.Username}";
            this.Loaded += UserDialog_Loaded;
        }

        private void UserDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                SetupEditMode();
            }
        }

        private void SetupEditMode()
        {
            UserNameTB.Text = _originalUser.Username;
            isAdminCB.IsChecked = _originalUser.IsAdmin;
        }

        private User? GetValidUserFromInput()
        {
            if (!Helper.ValidateRequiredFields(UserNameTB)) return null;

            string pwd1 = PasswordBox1.Password;
            string pwd2 = PasswordBox2.Password;

            if (!_isEditMode && string.IsNullOrWhiteSpace(pwd1))
            {
                Helper.ShowWarning("Password is required for a new user.");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(pwd1) || !string.IsNullOrWhiteSpace(pwd2))
            {
                if (pwd1 != pwd2)
                {
                    Helper.ShowWarning("Passwords do not match.");
                    return null;
                }
            }

            bool isCurrentlyAdmin = isAdminCB.IsChecked ?? false;
            bool wasAlreadyAdmin = _isEditMode && _originalUser.IsAdmin;

            if (isCurrentlyAdmin && !wasAlreadyAdmin)
            {
                if (!Helper.ShowWarning($"Are you sure you want to make user: {UserNameTB.Text.Trim()} an admin?"))
                {
                    isAdminCB.IsChecked = false;
                    return null;
                }
            }

            return new User
            {
                Id = _isEditMode ? _originalUser.Id : 0,
                Username = UserNameTB.Text.Trim(),
                PasswordHash = pwd1,
                IsAdmin = isCurrentlyAdmin
            };
        }

        private bool IsUserChanged(User original, User current)
        {
            // If current.PasswordHash is NOT empty, the user wants to change the password.
            bool passwordIsChanging = !string.IsNullOrWhiteSpace(current.PasswordHash);

            return original.Username != current.Username ||
                   original.IsAdmin != current.IsAdmin ||
                   passwordIsChanging;
        }
        private async void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            User? userToSave = GetValidUserFromInput();
            if (userToSave == null) return;

            ConfirmBtn.IsEnabled = false;

            bool success = await SaveUserToApiAsync(userToSave);

            if (success)
            {
                string message = _isEditMode ? "User successfully updated!" : "User successfully created!";
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Pokud to selhalo, odemkneme tlačítko, aby to admin mohl zkusit znovu
                ConfirmBtn.IsEnabled = true;
            }
        }

        private async Task<bool> SaveUserToApiAsync(User userToSave)
        {
            try
            {
                HttpResponseMessage response;

                if (_isEditMode)
                {
                    if (_originalUser != null && IsUserChanged(_originalUser, userToSave))
                    {
                        response = await _apiService.Client.PutAsJsonAsync($"Api/Users/{userToSave.Id}", userToSave);
                        if (!response.IsSuccessStatusCode)
                        {
                            Helper.ShowWarning($"Failed to update user: {userToSave.Username}.");
                            return false;
                        }
                    }
                }
                else
                {
                    // Vytvoření nového uživatele (POST)
                    response = await _apiService.Client.PostAsJsonAsync("Api/Users", userToSave);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            Helper.ShowWarning($"Username '{userToSave.Username}' already exists.");
                        }
                        else
                        {
                            Helper.ShowWarning($"Failed to create user: {userToSave.Username}.");
                        }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Network error while communicating with the server.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}