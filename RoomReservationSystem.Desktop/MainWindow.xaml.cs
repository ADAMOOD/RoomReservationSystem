using RoomReservationSystem.Desktop.Services;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoomReservationSystem.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;

        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }
        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
        }
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            var loginData = new
            {
                Username = username,
                Password = password
            };
            try
            {
                var response = await _apiService.Client.PostAsJsonAsync("api/Auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {

                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        if (!result.IsAdmin)
                        {
                            MessageBox.Show("Unouthorised access", "Access error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        _apiService.SetToken(result.Token);

                        AdminPanel panel = new AdminPanel(_apiService);
                        panel.Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect Name or Password", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to the Api", "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}