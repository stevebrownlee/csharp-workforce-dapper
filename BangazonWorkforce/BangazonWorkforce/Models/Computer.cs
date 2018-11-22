using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonWorkforce.Models
{
    public class Computer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        [Display(Name="Purchased On")]
        public DateTime PurchaseDate { get; set; }

        [Display(Name="Decomissioned On")]
        public DateTime? DecomissionDate { get; set; }

        [Display(Name="Owners")]
        public List<Employee> Employees { get; set; } = new List<Employee>();


        [Display(Name="Current Owner")]
        public Employee CurrentOwner { get; set; }

        public string Designation {
            get {
                return $"{Manufacturer} {Make}";
            }
        }
    }
}