using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;

namespace BangazonWorkforce.Controllers {
    public class TrainingController : Controller {
        private readonly IConfiguration _config;

        public TrainingController (IConfiguration config) {
            _config = config;
        }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        // GET: Training
        public async Task<IActionResult> Index () {
            using (IDbConnection conn = Connection) {
                Dictionary<int, TrainingProgram> trainingPrograms = new Dictionary<int, TrainingProgram>();

                await conn.QueryAsync<TrainingProgram, Employee, TrainingProgram> (
                    @"SELECT
                        tp.Id,
                        tp.Title,
                        tp.MaxAttendees,
                        tp.StartDate,
                        IFNULL(e.Id, 0) as Id,
                        e.FirstName,
                        e.LastName
                    FROM TrainingProgram tp
                    LEFT JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id
                    LEFT JOIN Employee e ON et.EmployeeId = e.Id",
                    (training, employee) => {
                        if (!trainingPrograms.ContainsKey(training.Id)) {
                            trainingPrograms[training.Id] = training;
                        }

                        /*
                            This is awful and bad

                            It is a way to handle a Dapper bug:
                                https://github.com/StackExchange/Dapper/issues/642

                            Note that above in the SQL that the employee Id column
                            is wrapped in in an IFNULL() function to ensure that the
                            Id column is never NULL
                        */
                        if (employee.Id != 0) {
                            trainingPrograms[training.Id].Attendees.Add(employee);
                        }
                        return training;
                    }
                );
                var programs = trainingPrograms.Values.AsEnumerable();
                return View (programs);
            }
        }

        // GET: Training/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
            SELECT
                tp.Id,
                tp.Title,
                tp.MaxAttendees,
                tp.StartDate,
                tp.EndDate
            FROM TrainingProgram tp
            WHERE tp.Id = {id}";

            using (IDbConnection conn = Connection) {
                TrainingProgram program = await conn.QuerySingleAsync<TrainingProgram> (sql);

                if (program == null) {
                    return NotFound ();
                }

                return View (program);
            }
        }

        // GET: Training/Create
        public IActionResult Create () {
            return View ();
        }

        // POST: Training/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (TrainingProgram program) {
            if (ModelState.IsValid) {
                string sql = $@"
                    INSERT INTO TrainingProgram
                    ( Title, MaxAttendees, StartDate, EndDate )
                    VALUES
                    ( @Title, @MaxAttendees,  @StartDate, @EndDate )";

                object mapper = new {
                    Title = program.Title,
                    MaxAttendees = program.MaxAttendees,
                    StartDate = program.StartDate,
                    EndDate = program.EndDate
                };

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql, mapper);

                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                }
            }

            return View (program);
        }

        // GET: Training/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"SELECT
                tp.Id,
                tp.Title,
                tp.MaxAttendees,
                tp.StartDate,
                tp.EndDate
            FROM TrainingProgram tp
            WHERE tp.Id = {id}";

            using (IDbConnection conn = Connection) {
                var program = await conn.QuerySingleAsync<TrainingProgram> (sql);
                return View (program);
            }
        }

        // POST: Training/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, TrainingProgram program) {
            if (id != program.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"UPDATE TrainingProgram SET
                    Title='{program.Title}',
                    MaxAttendees={program.MaxAttendees},
                    StartDate='{program.StartDate}',
                    EndDate='{program.EndDate}'
                WHERE Id={id}";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);
                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                    throw new Exception ("No rows affected");
                }
            } else {
                return View(program);
            }
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                SELECT e.Id, e.FirstName, e.LastName
                FROM Employee e
                WHERE e.Id = {id}";

            using (IDbConnection conn = Connection) {
                Employee employee = (await conn.QuerySingleAsync<Employee> (sql));

                if (employee == null) {
                    return NotFound ();
                }

                return View (employee);
            }
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {
            string sql = $@"DELETE FROM Employee WHERE Id = {id}";

            using (IDbConnection conn = Connection) {
                int rowsAffected = await conn.ExecuteAsync (sql);
                if (rowsAffected > 0) {
                    return RedirectToAction (nameof (Index));
                }
                throw new Exception ("No rows affected");
            }
        }
    }
}
