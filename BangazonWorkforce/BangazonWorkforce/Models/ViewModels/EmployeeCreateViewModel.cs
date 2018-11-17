using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeCreateViewModel
    {
        private readonly IConfiguration _config;

        public Employee Employee { get; set; }

        [Display(Name="Current Department")]
        public List<SelectListItem> Departments { get; private set; }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public EmployeeCreateViewModel() {}

        public EmployeeCreateViewModel(IConfiguration config)
        {
            _config = config;

            using (IDbConnection conn = Connection) {
                Departments = conn.Query<Department> (
                    @"SELECT
                        d.Id,
                        d.Name
                    FROM Department d"
                )
                .OrderBy(l => l.Name)
                .Select(li => new SelectListItem {
                    Text = li.Name,
                    Value = li.Id.ToString()
                }).ToList();

                // Add a prompt so that the <select> element isn't blank for a new employee
                Departments.Insert(0, new SelectListItem {
                    Text = "Choose department...",
                    Value = "0"
                });
            }
        }
    }
}
