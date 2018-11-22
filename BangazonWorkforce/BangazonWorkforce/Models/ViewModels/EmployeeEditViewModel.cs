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

        private int _employeeId;

        public Employee Employee { get; set; }

        // Property to hold all training sessions for selection on edit form
        [Display(Name="Training Sessions")]
        public MultiSelectList Sessions { get; private set; }

        // This will accept the selected training sessions on form POST
        public List<int> SelectedSessions { get; set; }

        [Display(Name="Current Department")]
        public List<SelectListItem> Departments { get; private set; }

        [Display(Name="Current Computer")]
        public List<SelectListItem> Computers { get; private set; }

        public int SelectedComputer {get;set;}

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public EmployeeEditViewModel() {}

        private void GetEmployee () {
            using (IDbConnection conn = Connection) {
                // Create instance of employee
                Employee = conn.QuerySingle<Employee> ($@"
                    SELECT
                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.DepartmentId
                    FROM Employee e
                    WHERE e.Id = {_employeeId}
                ");
            }
        }

        private void GetCurrentComputer () {
            using (IDbConnection conn = Connection) {
                // Get employee's current computer
                SelectedComputer = conn.QueryFirstOrDefault<int> (
                    $@"
                    SELECT
                        CASE WHEN UnassignDate IS NULL THEN
                            ComputerId
                        ELSE
                            0
                        END AS 'CurrentComputer'
                    FROM
                        ComputerEmployee
                    WHERE
                        EmployeeId = {_employeeId}
                        AND UnassignDate IS NULL;
                    "
                );
            }
        }

        private void CreateComputerSelectList () {
            using (IDbConnection conn = Connection) {
                // List of all computers
                Computers = conn.Query<Computer> (
                    $@"
                    SELECT
                        c.Id,
                        c.Manufacturer,
                        c.Make
                    FROM
                        Computer c
                    WHERE
                        c.Id NOT IN (
                            SELECT
                                c.Id
                            FROM
                                Computer c
                            LEFT JOIN ComputerEmployee ce ON ce.Computerid = c.Id
                        LEFT JOIN Employee e ON e.Id = ce.EmployeeId
                    WHERE
                        ce.AssignDate IS NOT NULL
                        AND ce.UnassignDate IS NULL
                        AND EmployeeId != {_employeeId})
                    "
                )
                .OrderBy(c => c.PurchaseDate)
                .Select(li => new SelectListItem {
                    Text = $"{li.Manufacturer} {li.Make}",
                    Value = li.Id.ToString()
                }).ToList();

                // Add a prompt so that the <select> element isn't blank for a new employee
                Computers.Insert(0, new SelectListItem {
                    Text = "Choose computer...",
                    Value = "0",
                    Selected = false
                });
            }
        }

        private void CreateDepartmentSelectList () {
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
            }
        }

        public void CreateTrainingMultiSelect () {
            using (IDbConnection conn = Connection) {
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
                    AND et.EmployeeId = {_employeeId}"
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

        public EmployeeEditViewModel(IConfiguration config, int employeeId)
        {
            _config = config;
            _employeeId = employeeId;

            GetEmployee();
            GetCurrentComputer();
            CreateComputerSelectList();
            CreateDepartmentSelectList();
            CreateTrainingMultiSelect();
        }
    }
}
