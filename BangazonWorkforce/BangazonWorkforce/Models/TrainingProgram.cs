using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkforce.Models
{
    public class TrainingProgram
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage="Please provide a title for training program")]
        [DataType(DataType.Text)]
        [Display(Name="Training Program Title")]
        public string Title { get; set; }

        [Required(ErrorMessage="Please provide the maximum attendees")]
        [DataType(DataType.Text)]
        [Display(Name="Maximum Attendees")]
        public int MaxAttendees { get; set; }

        [Required(ErrorMessage="You must specify when the training program starts")]
        [DataType(DataType.Date)]
        [Display(Name="Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage="You must specify when the training program ends")]
        [DataType(DataType.Date)]
        [Display(Name="End Date")]
        public DateTime EndDate { get; set; }

        public virtual ICollection<EmployeeTraining> Attendees { get; set; }
    }
}