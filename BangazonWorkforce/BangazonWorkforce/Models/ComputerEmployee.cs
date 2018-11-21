using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkforce.Models
{
    public class ComputerEmployee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComputerId { get; set; }

        public Computer Computer {get;set;}

        [Required]
        public int EmployeeId { get; set; }

        public Employee Employee {get;set;}

        [Required]
        [Display(Name="Assigned")]
        public DateTime AssignDate { get; set; }

        [Display(Name="Unassigned")]
        public DateTime UnassignDate { get; set; }
    }
}