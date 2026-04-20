using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RoomReservationSystem.Desktop.Services;

namespace RoomReservationSystem.Desktop
{

    public partial class AdminPanel : Window
    {
        private readonly ApiService _apiService;
        public AdminPanel(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            UsersTab.Setup(_apiService);
            RoomsTab.Setup(_apiService);
            ReservationsTab.Setup(_apiService);
        }
    }
}
