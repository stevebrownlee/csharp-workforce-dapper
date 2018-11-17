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
    public class EmployeeController : Controller {
        private readonly IConfiguration _config;

        public EmployeeController (IConfiguration config) {
            _config = config;
        }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        // GET: Employee
        public async Task<IActionResult> Index () {
            using (IDbConnection conn = Connection) {
                var employees = await conn.QueryAsync<Employee, Department, Employee> (
                    @"SELECT
                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.DepartmentId,
                        d.Id,
                        d.Name
                    FROM Employee e
                    JOIN Department d ON e.DepartmentId = d.Id",
                    (employee, department) => {
                        employee.Department = department;
                        return employee;
                    }
                );
                return View (employees);
            }
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
            SELECT
                e.Id,
                e.FirstName,
                e.LastName
            FROM Employee e
            WHERE e.Id = {id}";

            using (IDbConnection conn = Connection) {
                Employee employee = await conn.QuerySingleAsync<Employee> (sql);

                if (employee == null) {
                    return NotFound ();
                }

                return View (employee);
            }
        }

        // GET: Employee/Create
        public IActionResult Create () {
            var model = new EmployeeCreateViewModel(_config);
            return View (model);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (EmployeeCreateViewModel model) {
            if (ModelState.IsValid) {
                string sql = $@"
                    INSERT INTO Employee
                    ( FirstName, LastName, DepartmentId )
                    VALUES
                    ( @FirstName, @LastName,  @DepartmentId )";

                object mapper = new {
                    FirstName = model.Employee.FirstName,
                    LastName = model.Employee.LastName,
                    DepartmentId = model.Employee.DepartmentId
                };

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql, mapper);

                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                }
            }

            return View (model);
        }

        // GET: Employee/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"SELECT
                e.Id,
                e.FirstName,
                e.LastName,
                e.DepartmentId,
                d.Id,
                d.Name
            FROM Employee e
            JOIN Department d ON e.Id = d.Id
            WHERE e.EmployeeId = {id}";

            using (IDbConnection conn = Connection) {
                Employee dept = await conn.QuerySingleAsync<Employee> (sql);
                return View (dept);
            }
        }

        // POST: Employee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, EmployeeEditViewModel model) {
            if (id != model.Employee.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"UPDATE Employee SET
                    FirstName='{model.Employee.FirstName}',
                    LastName='{model.Employee.LastName}',
                    Id={model.Employee.Id}
                WHERE Id={id}";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);
                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                    throw new Exception ("No rows affected");
                }
            } else {
                return new StatusCodeResult (StatusCodes.Status406NotAcceptable);
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
