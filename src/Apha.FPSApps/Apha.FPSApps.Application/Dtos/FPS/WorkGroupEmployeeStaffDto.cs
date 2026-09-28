using Apha.FPSApps.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class WorkGroupEmployeeStaffDto : IValidatableObject
    {
        public string? PactId { get; set; }
        public string SpNumber { get; set; } = null!;
        public string WorkGroupGrade { get; set; } = null!;
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Status is required")]
        [AllowedValues(StaffStatus.Active, StaffStatus.Inactive, ErrorMessage = "Status must be either A or I")]
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public int MakeAvailable { get; set; }

        public int TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }

        // "Available?" and "Time recorder?" are mutually exclusive - a staff member can be
        // one or the other (or neither), never both. Mirrors the client-side rule in
        // _AddEditWorkGroupStaff.cshtml so the constraint still holds for direct API calls.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MakeAvailable != 0 && TimeRecorder != 0)
            {
                yield return new ValidationResult(
                    "Available and Time recorder cannot both be selected. Please choose only one.",
                    new[] { nameof(MakeAvailable), nameof(TimeRecorder) });
            }
        }
    }
}