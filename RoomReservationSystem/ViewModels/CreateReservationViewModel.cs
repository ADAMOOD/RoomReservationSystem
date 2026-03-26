using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.ViewModels
{
    public class CreateReservationViewModel
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Fill in Start date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? StartTime { get; set; }
        [Required(ErrorMessage = "Please Fill in End date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EndTime { get; set; }

        public string Purpose { get; set; } = string.Empty;
        public int PersonCount { get; set; } = 1;
        //automaticly called when form is send
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime.HasValue && StartTime.Value < DateTime.Now)
            {
                yield return new ValidationResult("Start date of the reservation can not be set to past", new[] { nameof(StartTime) });
            }

            if (StartTime.HasValue && EndTime.HasValue && EndTime.Value <= StartTime.Value)
            {
                yield return new ValidationResult("End time of the reservation can not be set prior to the start date.", new[] { nameof(EndTime) });
            }
        }
    }
}
