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
    public class EmployeeEditViewModel
    {
        private readonly IConfiguration _config;

        public Employee Employee { get; set; }

        // Property to hold all training sessions for selection on edit form
        [Display(Name="Training Sessions")]
        public MultiSelectList Sessions { get; private set; }

        [Display(Name="Current Department")]
        public List<SelectListItem> Departments { get; private set; }

        // This will accept the selected training sessions on form POST
        public List<int> SelectedSessions { get; set; }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public EmployeeEditViewModel() {}

        public EmployeeEditViewModel(IConfiguration config, int employeeId)
        {
            _config = config;

            using (IDbConnection conn = Connection) {
                Departments = conn.Query<Department> (
                    @"SELECT d.Id, d.Name FROM Department d"
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

                // Build a list of training sessions that begin in the future
                List<TrainingProgram> availableSessions = conn.Query<TrainingProgram> (
                    $@"SELECT
                        t.Id,
                        t.Title,
                        t.MaxAttendees,
                        t.StartDate,
                        t.EndDate
                    FROM TrainingProgram t
                    WHERE t.StartDate > '{DateTime.Now}'"
                )
                .OrderBy(tp => tp.StartDate)
                .ToList();


                // Build a list of training program ids to pre-select in the multiselect element
                var goingToList = conn.Query<TrainingProgram> (
                    $@"SELECT
                        t.Id,
                        t.Title,
                        t.MaxAttendees,
                        t.StartDate,
                        t.EndDate
                    FROM TrainingProgram t
                    JOIN EmployeeTraining et ON et.TrainingProgramId = t.Id
                    WHERE t.StartDate > '{DateTime.Now}'
                    AND et.EmployeeId = {employeeId}"
                )
                .OrderBy(tp => tp.Id)
                .Select(tp => tp.Id)
                .ToList();

                /*
                    This MultiSelectList constructor takes 4 arguments. Here's what they all mean.
                        1. The collection that store all items I want in the <select> element
                        2. The column to use for the `value` attribute
                        3. The column to use for display text
                        4. A list of integers for ones to be pre-selected
                */
                Sessions = new MultiSelectList(availableSessions, "Id", "Title", goingToList);
            }
        }

        private void CreateDepartmentSelect ()
        {
            string sql = $@"SELECT Id, Name FROM Department";

            using (IDbConnection conn = Connection) {
                IEnumerable<Department> depts = conn.Query<Department> (sql);

                this.Departments = depts
                    .Select(li => new SelectListItem {
                        Text = li.Name,
                        Value = li.Id.ToString()
                    }).ToList();
            }


            // Add a prompt so that the <select> element isn't blank
            this.Departments.Insert(0, new SelectListItem {
                Text = "Choose cohort...",
                Value = "0"
            });
        }
    }
}
