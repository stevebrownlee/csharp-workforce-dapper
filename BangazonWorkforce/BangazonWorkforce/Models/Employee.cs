using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkforce.Models
{
    public class Employee
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage="You must provide a first name for this employee")]
        [DataType(DataType.Text)]
        [Display(Name="First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="You must provide a last name for this employee")]
        [DataType(DataType.Text)]
        [Display(Name="Last Name")]
        public string LastName { get; set; }

        [Display(Name="Full Name")]
        public string FullName {
            get {
                return $"{FirstName} {LastName}";
            }
        }

        [Required(ErrorMessage="Please select which department this employee is assigned to")]
        [Display(Name="Department")]
        public int DepartmentId { get; set; }

        public Department Department { get; set; }
        public Computer Computer { get; set; }

        [Display(Name="Training Programs Attending")]
        public List<TrainingProgram> SessionsAttending { get; set; }

    }
}