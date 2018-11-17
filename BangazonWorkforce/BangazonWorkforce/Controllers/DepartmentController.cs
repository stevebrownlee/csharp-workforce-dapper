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

namespace BangazonWorkforce.Controllers {
    public class DepartmentController : Controller {
        private readonly IConfiguration _config;

        public DepartmentController (IConfiguration config) {
            _config = config;
        }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        // GET: Department
        public async Task<IActionResult> Index () {
            using (IDbConnection conn = Connection) {
                IEnumerable<Department> departments = await conn.QueryAsync<Department> (
                    @"SELECT
                        d.Id,
                        d.Name,
                        d.Budget,
                        count(e.Id) EmployeeCount
                    FROM Department d
                    LEFT JOIN Employee e on e.DepartmentId = d.Id
                    GROUP BY d.Id, d.Name, d.Budget"
                );
                return View (departments);
            }
        }

        // GET: Department/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            WHERE d.Id = {id}";

            using (IDbConnection conn = Connection) {
                Department department = await conn.QuerySingleAsync<Department> (sql);

                if (department == null) {
                    return NotFound ();
                }

                return View (department);
            }
        }

        // GET: Department/Create
        public IActionResult Create () {
            return View ();
        }

        // POST: Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Department department) {
            if (ModelState.IsValid) {
                string sql = $@"
                    INSERT INTO Department
                        ( Name, Budget )
                        VALUES
                        ( '{department.Name}', {department.Budget} )
                    ";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);

                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                }
            }

            return View (department);
        }

        // GET: Department/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                SELECT
                    d.Id,
                    d.Name,
                    d.Budget
                FROM Department d
                WHERE d.Id = {id}";

            using (IDbConnection conn = Connection) {
                Department dept = await conn.QuerySingleAsync<Department> (sql);
                return View (dept);
            }
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, Department department) {
            if (id != department.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"
                    UPDATE Department
                    SET
                        Name='{department.Name}',
                        Budget={department.Budget}
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

        // GET: Department/Delete/5
        public async Task<IActionResult> Delete (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                SELECT d.Id, d.Name
                FROM Department d
                WHERE d.Id = {id}";

            using (IDbConnection conn = Connection) {
                Department dept = (await conn.QueryAsync<Department> (sql)).ToList ().Single ();

                if (dept == null) {
                    return NotFound ();
                }

                return View (dept);
            }
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {
            string sql = $@"DELETE FROM Department WHERE Id = {id}";

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
