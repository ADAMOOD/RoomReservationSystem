using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace RoomReservationSystem.Desktop.UserControls
{
    public static class Helper
    {
        public  enum SaveResult
        {
            Success,
            NoChanges,
            Failed
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public static void NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }
        public static bool ShowWarning(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Validation Error", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            return result == MessageBoxResult.OK;
        }

        public static bool ValidateRequiredFields(params TextBox[] fields)
        {
            foreach (var tb in fields)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    string fieldName = tb.Tag?.ToString() ?? "This field";
                    ShowWarning($"{fieldName} is required!");

                    return false;
                }
            }
            return true;
        }
    }
}
