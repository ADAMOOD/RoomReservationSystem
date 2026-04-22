using RoomReservatingSystem.Shared;
using RoomReservationSystem.Desktop.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RoomReservationSystem.Desktop.UserControls.Dialogs
{
    public partial class RoomDialog : Window
    {
        private List<Room> _rooms;
        private ApiService _apiService;

        private bool _isEditMode;
        private List<Room> _originalRoomsToEdit;

        // PAMĚŤ NA ID: Uchovává ID místnosti, která je momentálně načtená v textových polích
        private int _currentEditingRoomId = 0;

        // Constructor for adding
        public RoomDialog(ApiService apiService)
        {
            InitializeComponent();
            _rooms = new List<Room>();
            _apiService = apiService;
            _isEditMode = false;
            this.Title = "Add New Room";
        }

        // Constructor for editing
        public RoomDialog(ApiService apiService, List<Room> roomsToEdit)
        {
            InitializeComponent();
            _apiService = apiService;
            _rooms = new List<Room>();
            _originalRoomsToEdit = roomsToEdit;
            _isEditMode = true;

            this.Title = "Edit Rooms";

            AddNextRoomBtn.Visibility = Visibility.Collapsed;
            ConfirmBtn.Content = "Save Changes";

            SetupEditMode();
        }

        private void SetupEditMode()
        {
            _rooms.Clear();
            OverviewPanel.Children.Clear();

            foreach (var originalRoom in _originalRoomsToEdit)
            {
                Room clonedRoom = CloneRoom(originalRoom);
                _rooms.Add(clonedRoom);

                Button overviewBtn = new Button();
                overviewBtn.Content = $"Room - {clonedRoom.Name}";
                overviewBtn.Tag = clonedRoom;
                overviewBtn.Click += EditOverviewRoom_Click;
                OverviewPanel.Children.Add(overviewBtn);
            }

            if (OverviewPanel.Children.Count > 0)
            {
                Button firstBtn = (Button)OverviewPanel.Children[0];
                EditOverviewRoom_Click(firstBtn, null);
            }
        }

        private Room CloneRoom(Room original)
        {
            return new Room
            {
                Id = original.Id,
                Name = original.Name,
                Capacity = original.Capacity,
                Equipment = original.Equipment,
                MaxReservationMinutes = original.MaxReservationMinutes
            };
        }

        private void EquipmentTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ManageEquipmentTextBoxes();
        }

        private void ManageEquipmentTextBoxes()
        {
            int emptyBoxesCount = 0;
            foreach (var child in EquipmentPanel.Children)
            {
                if (child is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                {
                    emptyBoxesCount++;
                }
            }
            if (emptyBoxesCount == 0)
            {
                TextBox newTb = new TextBox
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                newTb.TextChanged += EquipmentTB_TextChanged;
                newTb.PreviewTextInput += NoCommas_PreviewTextInput;

                EquipmentPanel.Children.Add(newTb);
            }
            else if (emptyBoxesCount > 1)
            {
                for (int i = EquipmentPanel.Children.Count - 1; i >= 0 && emptyBoxesCount > 1; i--)
                {
                    if (EquipmentPanel.Children[i] is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                    {
                        EquipmentPanel.Children.RemoveAt(i);
                        emptyBoxesCount--;
                    }
                }
            }
        }

        private async void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            // Zachráníme rozepsanou místnost z formuláře
            if (!string.IsNullOrWhiteSpace(RoomNameTB.Text) ||
                !string.IsNullOrWhiteSpace(CapacityTB.Text) ||
                !string.IsNullOrWhiteSpace(MaxTimeTB.Text))
            {
                var lastWorkedRoom = getRoomFromIputs();

                if (lastWorkedRoom is not null)
                {
                    _rooms.Add(lastWorkedRoom);
                }
                else
                {
                    return; // Zastavíme, pokud byla rozepsaná špatně
                }
            }

            if (_rooms.Count != 0)
            {
                // Zablokujeme tlačítko proti dvojkliku během ukládání
                ConfirmBtn.IsEnabled = false;

                foreach (var room in _rooms)
                {
                    if (_isEditMode)
                    {
                        var original = _originalRoomsToEdit.FirstOrDefault(r => r.Id == room.Id);

                        if (original != null)
                        {
                            bool isChanged = original.Name != room.Name ||
                                             original.Capacity != room.Capacity ||
                                             original.Equipment != room.Equipment ||
                                             original.MaxReservationMinutes != room.MaxReservationMinutes;

                            if (isChanged)
                            {
                                await _apiService.Client.PutAsJsonAsync($"Api/Rooms/{room.Id}", room);
                            }
                        }
                    }
                    else
                    {
                        await _apiService.Client.PostAsJsonAsync("Api/Rooms", room);
                    }
                }

                string message = _isEditMode ? "Rooms successfully updated!" : "Rooms successfully created!";
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close(); // Ujistíme se, že se okno po potvrzení zavře
            }
            else
            {
                MessageBox.Show("Please create a Room first");
            }
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (IsTextAllowed(e.Text))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void NoCommas_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void AddNextRoomBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Room newRoom = getRoomFromIputs();
            if (newRoom is not null)
            {
                _rooms.Add(newRoom);
                Button overviewBtn = new Button();
                overviewBtn.Content = $"Room - {newRoom.Name}";
                overviewBtn.Tag = newRoom;
                overviewBtn.Click += EditOverviewRoom_Click;
                OverviewPanel.Children.Add(overviewBtn);
                ClearInputs();
            }
        }

        private Room? getRoomFromIputs()
        {
            if (string.IsNullOrWhiteSpace(RoomNameTB.Text))
            {
                MessageBox.Show("Room name cannot be empty!");
                return null;
            }
            List<string> equipmentList = new List<string>();
            foreach (var child in EquipmentPanel.Children)
            {
                if (child is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
                {
                    equipmentList.Add(tb.Text.Trim());
                }
            }
            string equipmentString = string.Join(", ", equipmentList);

            if (!int.TryParse(CapacityTB.Text, out int capacity))
            {
                MessageBox.Show("Capacity must be a number!");
                return null;
            }
            if (!int.TryParse(MaxTimeTB.Text, out int minutes))
            {
                MessageBox.Show("Max minutes must be a number!");
                return null;
            }

            return new Room
            {
                // ZDE SE VYUŽIJE PAMĚŤ: Pokud tvoříme novou, bude to 0. Pokud editujeme, bude to správné ID.
                Id = _currentEditingRoomId,
                Name = RoomNameTB.Text,
                Capacity = capacity,
                Equipment = equipmentString,
                MaxReservationMinutes = minutes
            };
        }

        private void ClearInputs()
        {
            // VYČIŠTĚNÍ PAMĚTI: Po sbalení místnosti vymažeme ID pro tu další
            _currentEditingRoomId = 0;

            RoomNameTB.Clear();
            CapacityTB.Clear();
            MaxTimeTB.Clear();
            EquipmentPanel.Children.Clear();
            ManageEquipmentTextBoxes();
        }

        private void FillInputs(Room room)
        {
            RoomNameTB.Text = room.Name;
            CapacityTB.Text = room.Capacity.ToString();
            MaxTimeTB.Text = room.MaxReservationMinutes.ToString();

            EquipmentPanel.Children.Clear();

            if (!string.IsNullOrWhiteSpace(room.Equipment))
            {
                var equipments = room.Equipment.Split(',');
                foreach (var equipment in equipments)
                {
                    TextBox newTb = new TextBox
                    {
                        Padding = new Thickness(5),
                        Margin = new Thickness(0, 0, 0, 5),
                        Text = equipment.Trim()
                    };
                    newTb.TextChanged += EquipmentTB_TextChanged;
                    newTb.PreviewTextInput += NoCommas_PreviewTextInput; // Přidáno blokování čárek i sem!
                    EquipmentPanel.Children.Add(newTb);
                }
            }

            ManageEquipmentTextBoxes();
        }

        private void EditOverviewRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(RoomNameTB.Text) ||
                !string.IsNullOrWhiteSpace(CapacityTB.Text) ||
                !string.IsNullOrWhiteSpace(MaxTimeTB.Text))
            {
                Room draftRoom = getRoomFromIputs();

                if (draftRoom != null)
                {
                    _rooms.Add(draftRoom);
                    Button draftBtn = new Button();
                    draftBtn.Content = $"Room - {draftRoom.Name} (Draft)";
                    draftBtn.Tag = draftRoom;
                    draftBtn.Click += EditOverviewRoom_Click;
                    OverviewPanel.Children.Add(draftBtn);
                }
                else
                {
                    MessageBox.Show("Please finish or clear the current room details before editing another.", "Unfinished Room", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            Button clickedButton = (Button)sender;
            Room roomToEdit = (Room)clickedButton.Tag;

            _currentEditingRoomId = roomToEdit.Id;

            FillInputs(roomToEdit);
            _rooms.Remove(roomToEdit);
            OverviewPanel.Children.Remove(clickedButton);
        }
    }
}