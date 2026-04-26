using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared.DTOs;
using RoomReservationSystem.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RoomReservationSystem.Desktop.UserControls.Dialogs
{
    public partial class ReservationDialog : Window
    {
        private ApiService _apiService;
        private List<ReservationDTO> _reservations;

        // ZDE SI BUDEME PAMATOVAT VŠECHNY EXISTUJÍCÍ REZERVACE Z DATABÁZE
        private List<ReservationDTO> _existingDbReservations;

        private bool _isEditMode;
        private List<ReservationDTO> _originalReservationsToEdit;

        private int _currentEditingReservationId = 0;

        public ReservationDialog(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            _reservations = new List<ReservationDTO>();
            _existingDbReservations = new List<ReservationDTO>();
            _isEditMode = false;

            this.Title = "Add New Reservation";
            this.Loaded += ReservationDialog_Loaded;
        }

        public ReservationDialog(ApiService apiService, List<ReservationDTO>? reservationsToEdit)
        {
            InitializeComponent();
            _apiService = apiService;
            _reservations = new List<ReservationDTO>();
            _existingDbReservations = new List<ReservationDTO>();
            _originalReservationsToEdit = reservationsToEdit;
            _isEditMode = true;

            this.Title = "Edit Reservations";
            AddNextReservationBtn.Visibility = Visibility.Collapsed;
            ConfirmReservationBtn.Content = "Save Changes";

            this.Loaded += ReservationDialog_Loaded;
        }

        private async void ReservationDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            await LoadDropdownDataAsync();
            this.IsEnabled = true;

            if (_isEditMode)
            {
                SetupEditMode();
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var rooms = await _apiService.Client.GetFromJsonAsync<List<Room>>("Api/Rooms");
                var users = await _apiService.Client.GetFromJsonAsync<List<User>>("Api/Users");

                // STÁHNEME SI VŠECHNY REZERVACE PRO KONTROLU KOLIZÍ
                _existingDbReservations = await _apiService.Client.GetFromJsonAsync<List<ReservationDTO>>("Api/Reservations") ?? new List<ReservationDTO>();

                RoomComboBox.ItemsSource = rooms;
                UserComboBox.ItemsSource = users;
            }
            catch (Exception)
            {
                MessageBox.Show("Network error while loading data. Please try again.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void SetupEditMode()
        {
            _reservations.Clear();
            ReservationsOverviewPanel.Children.Clear();

            foreach (var originalRes in _originalReservationsToEdit)
            {
                ReservationDTO clonedRes = CloneReservation(originalRes);
                _reservations.Add(clonedRes);
                AddReservationToOverview(clonedRes, isDraft: false);
            }

            if (ReservationsOverviewPanel.Children.Count > 0)
            {
                Button firstBtn = (Button)ReservationsOverviewPanel.Children[0];
                EditOverviewReservation_Click(firstBtn, null);
            }
        }

        private ReservationDTO CloneReservation(ReservationDTO original)
        {
            return new ReservationDTO
            {
                Id = original.Id,
                RoomId = original.RoomId,
                OrganizerId = original.OrganizerId,
                RoomName = original.RoomName,
                UserName = original.UserName,
                StartTime = original.StartTime,
                EndTime = original.EndTime,
                Purpose = original.Purpose,
                PersonCount = original.PersonCount,
                Status = original.Status
            };
        }

        private bool HasTimeCollision(int roomId, DateTime start, DateTime end, int currentResId)
        {

            // 1. Kontrola proti uloženým datům (Musíme přeskočit sami sebe při editaci a ignorovat zrušené)
            bool dbCollision = _existingDbReservations.Any(r =>
                r.RoomId == roomId &&
                r.Id != currentResId &&
                r.Status == ReservationStatus.Active &&
                start < r.EndTime && end > r.StartTime);

            if (dbCollision) return true;

            // 2. Kontrola proti draftům na levém panelu
            // Protože se zrovna upravovaný záznam fyzicky dočasně odstraní z _reservations při načtení (v EditOverviewReservation_Click),
            // kolekce _reservations obsahuje pouze *ostatní* drafty, což je naprosto dokonalé.
            bool draftCollision = _reservations.Any(r =>
                r.RoomId == roomId &&
                start < r.EndTime && end > r.StartTime);

            return draftCollision;
        }

        // --- VALIDACE A EXTRAKCE DAT ---

        private bool ValidateInputs(out DateTime startTime, out DateTime endTime, out Room selectedRoom, out User selectedUser, out string purpose, out int personCount)
        {
            startTime = DateTime.MinValue;
            endTime = DateTime.MinValue;
            selectedRoom = null;
            selectedUser = null;
            purpose = string.Empty;
            personCount = 0;

            if (!Helper.ValidateRequiredFields(PurposeTB, NumberOfPeopleTB)) return false;

            if (RoomComboBox.SelectedValue == null) { Helper.ShowWarning("Please select a room!"); return false; }
            if (UserComboBox.SelectedValue == null) { Helper.ShowWarning("Please select an organizer!"); return false; }

            selectedRoom = RoomComboBox.SelectedItem as Room;

            if (!int.TryParse(NumberOfPeopleTB.Text, out personCount) || personCount <= 0 || personCount > selectedRoom.Capacity)
            {
                Helper.ShowWarning($"Please enter a valid number of people (max {selectedRoom.Capacity})!");
                return false;
            }

            purpose = PurposeTB.Text.Trim();

            if (!StartTimePicker.Value.HasValue || !EndTimePicker.Value.HasValue) { Helper.ShowWarning("Please select both start and end times!"); return false; }

            DateTime rawStartTime = StartTimePicker.Value.Value;
            DateTime rawEndTime = EndTimePicker.Value.Value;

            startTime = new DateTime(rawStartTime.Year, rawStartTime.Month, rawStartTime.Day, rawStartTime.Hour, rawStartTime.Minute, 0);
            endTime = new DateTime(rawEndTime.Year, rawEndTime.Month, rawEndTime.Day, rawEndTime.Hour, rawEndTime.Minute, 0);

            bool isNewReservation = _currentEditingReservationId == 0;

            if (isNewReservation && startTime < DateTime.Now.AddMinutes(-5))
            {
                Helper.ShowWarning("Start date of a new reservation can not be set to the past.");
                return false;
            }

            if (endTime <= startTime) { Helper.ShowWarning("End time of the reservation can not be set prior to the start date."); return false; }

            if (selectedRoom != null && (endTime - startTime).TotalMinutes > selectedRoom.MaxReservationMinutes)
            {
                Helper.ShowWarning($"Reservation time cannot be greater than {selectedRoom.MaxReservationMinutes} minutes for this room.");
                return false;
            }

            // VOLÁNÍ KOLIZE - Zastaví proces a vyhodí upozornění, pokud je čas obsazený
            if (HasTimeCollision(selectedRoom.Id, startTime, endTime, _currentEditingReservationId))
            {
                Helper.ShowWarning("This room is already booked for the selected time slot. Please choose another room or time.");
                return false;
            }

            selectedUser = UserComboBox.SelectedItem as User;
            return true;
        }

        private ReservationDTO? GetReservationFromInputs()
        {
            if (!ValidateInputs(out DateTime startTime, out DateTime endTime, out Room selectedRoom, out User selectedUser, out string purpose, out int personCount))
            {
                return null;
            }
            return new ReservationDTO
            {
                Id = _currentEditingReservationId,
                RoomId = selectedRoom.Id,
                OrganizerId = selectedUser.Id,
                RoomName = selectedRoom.Name,
                UserName = selectedUser.Username,
                StartTime = startTime,
                EndTime = endTime,
                Purpose = purpose,
                PersonCount = personCount
            };
        }

        // --- SPRÁVA STAVU UI ---

        private void FillInputs(ReservationDTO res)
        {
            RoomComboBox.SelectedValue = res.RoomId;
            UserComboBox.SelectedValue = res.OrganizerId;
            StartTimePicker.Value = res.StartTime;
            EndTimePicker.Value = res.EndTime;
            PurposeTB.Text = res.Purpose;
            NumberOfPeopleTB.Text = res.PersonCount.ToString();
        }

        private void ClearInputs()
        {
            _currentEditingReservationId = 0;
            RoomComboBox.SelectedItem = null;
            UserComboBox.SelectedItem = null;
            StartTimePicker.Value = null;
            EndTimePicker.Value = null;
            NumberOfPeopleTB.Text = string.Empty;
            PurposeTB.Clear();
        }

        private void AddReservationToOverview(ReservationDTO res, bool isDraft)
        {
            Button btn = new Button();
            string prefix = isDraft ? "Draft" : "Res";
            btn.Content = $"{prefix} - {res.RoomName}";
            btn.Tag = res;
            btn.Click += EditOverviewReservation_Click;
            btn.Margin = new Thickness(0, 0, 0, 5);
            btn.Padding = new Thickness(5);

            ReservationsOverviewPanel.Children.Add(btn);
        }

        private bool HasUnsavedChanges()
        {
            return RoomComboBox.SelectedValue != null ||
                   UserComboBox.SelectedValue != null ||
                   !string.IsNullOrWhiteSpace(PurposeTB.Text) ||
                   !string.IsNullOrWhiteSpace(NumberOfPeopleTB.Text);
        }

        private bool TrySaveCurrentDraft()
        {
            if (!HasUnsavedChanges()) return true;

            ReservationDTO draftRes = GetReservationFromInputs();
            if (draftRes == null) return false;

            _reservations.Add(draftRes);
            AddReservationToOverview(draftRes, isDraft: true);
            return true;
        }

        // --- UDÁLOSTI TLAČÍTEK ---

        private void AddNextReservationBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ReservationDTO newRes = GetReservationFromInputs();
            if (newRes != null)
            {
                _reservations.Add(newRes);
                AddReservationToOverview(newRes, isDraft: true);
                ClearInputs();
            }
        }

        private void EditOverviewReservation_Click(object sender, RoutedEventArgs e)
        {
            if (!TrySaveCurrentDraft()) return;

            Button clickedButton = (Button)sender;
            ReservationDTO resToEdit = (ReservationDTO)clickedButton.Tag;

            _currentEditingReservationId = resToEdit.Id;
            FillInputs(resToEdit);

            _reservations.Remove(resToEdit);
            ReservationsOverviewPanel.Children.Remove(clickedButton);
        }

        private async void ConfirmReservationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!TrySaveCurrentDraft()) return;

            if (_reservations.Count == 0)
            {
                Helper.ShowWarning("Please create or edit a reservation first.");
                return;
            }

            ConfirmReservationBtn.IsEnabled = false;

            // Použití sdíleného Helper.SaveResult stavu
            Helper.SaveResult result = await SaveReservationsToApiAsync();

            if (result == Helper.SaveResult.Success)
            {
                string message = _isEditMode ? "Reservations successfully updated!" : "Reservations successfully created!";
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true; // Úspěch -> zavřeme s true
            }
            else if (result == Helper.SaveResult.NoChanges)
            {
                // Žádná z rezervací se reálně nezměnila -> zavřeme tiše
                this.DialogResult = false;
            }
            else
            {
                // Pokud to selhalo, odemkneme tlačítko
                ConfirmReservationBtn.IsEnabled = true;
            }
        }

        // --- KOMUNIKACE S API ---

        private async Task<Helper.SaveResult> SaveReservationsToApiAsync()
        {
            try
            {
                bool anyChangesMade = false;

                foreach (var res in _reservations)
                {
                    HttpResponseMessage response;

                    if (_isEditMode)
                    {
                        var original = _originalReservationsToEdit.FirstOrDefault(r => r.Id == res.Id);
                        if (original != null && IsReservationChanged(original, res))
                        {
                            response = await _apiService.Client.PutAsJsonAsync($"Api/Reservations/{res.Id}", res);
                            if (!response.IsSuccessStatusCode)
                            {
                                // Edge Case 1 ošetřen: Souběžná kolize na backendu
                                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                                {
                                    Helper.ShowWarning($"Reservation for room '{res.RoomName}' conflicts with another booking.");
                                }
                                else
                                {
                                    Helper.ShowWarning($"Failed to update reservation in {res.RoomName}.");
                                }
                                return Helper.SaveResult.Failed;
                            }
                            anyChangesMade = true;
                        }
                    }
                    else
                    {
                        response = await _apiService.Client.PostAsJsonAsync("Api/Reservations", res);
                        if (!response.IsSuccessStatusCode)
                        {
                            // Edge Case 1 ošetřen: Souběžná kolize na backendu
                            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                            {
                                Helper.ShowWarning($"Reservation for room '{res.RoomName}' conflicts with another booking.");
                            }
                            else
                            {
                                Helper.ShowWarning($"Failed to create reservation in {res.RoomName}.");
                            }
                            return Helper.SaveResult.Failed;
                        }
                        anyChangesMade = true;
                    }
                }

                // Vyhodnocení, zda se vůbec něco na server odeslalo
                if (_isEditMode && !anyChangesMade)
                {
                    return Helper.SaveResult.NoChanges;
                }

                return Helper.SaveResult.Success;
            }
            catch (Exception)
            {
                Helper.ShowWarning("Network error while communicating with the server.");
                return Helper.SaveResult.Failed;
            }
        }
        private bool IsReservationChanged(ReservationDTO original, ReservationDTO current)
        {
            return original.RoomId != current.RoomId ||
                   original.OrganizerId != current.OrganizerId ||
                   original.StartTime != current.StartTime ||
                   original.EndTime != current.EndTime ||
                   original.Purpose != current.Purpose ||
                   original.PersonCount != current.PersonCount;
        }

        private void RoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoomComboBox.SelectedItem is Room selectedRoom)
            {
                RoomHintTB.Text = $"Capacity: {selectedRoom.Capacity} people | Max duration: {selectedRoom.MaxReservationMinutes} mins";
                RoomHintTB.Visibility = Visibility.Visible;
            }
            else
            {
                RoomHintTB.Visibility = Visibility.Collapsed;
            }
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Helper.NumbersOnly(sender, e);
        }

    }
}