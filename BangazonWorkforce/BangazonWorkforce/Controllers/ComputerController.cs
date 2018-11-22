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
    public class ComputerController : Controller {
        private readonly IConfiguration _config;

        public ComputerController (IConfiguration config) {
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
                Dictionary<int, Computer> computers = new Dictionary<int, Computer>();

                await conn.QueryAsync<Computer, ComputerEmployee, Employee, Computer> (
                    @"SELECT
                        c.Id,
                        c.Make,
                        c.Manufacturer,
                        c.PurchaseDate,
                        c.DecomissionDate,
                        ce.Id,
                        ce.ComputerId,
                        ce.EmployeeId,
                        ce.AssignDate,
                        ce.UnassignDate,
                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.DepartmentId,
                        e.IsSupervisor
                    FROM Computer c
                    LEFT JOIN ComputerEmployee ce ON c.Id = ce.ComputerId
                    LEFT JOIN Employee e ON ce.EmployeeId = e.Id",
                    (c, ce, e) => {
                        if (!computers.ContainsKey(c.Id)) {
                            computers[c.Id] = c;
                        }

                        if (e != null) {
                            computers[c.Id].Employees.Add(e);
                        }

                        if (ce != null && ce.UnassignDate == null && ce.AssignDate != null) {
                            computers[c.Id].CurrentOwner = e;
                        }
                        return c;
                    }
                );
                var response = computers.Values.AsEnumerable();
                return View (response);
            }
        }

        // GET: Computer/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                SELECT
                    c.Id,
                    c.Make,
                    c.manufacturer,
                    c.PurchaseDate,
                    c.DecomissionDate
                FROM Computer c
                WHERE c.Id = {id}
            ";


            using (IDbConnection conn = Connection) {
                Computer computer = await conn.QuerySingleAsync<Computer> (sql);

                if (computer == null) {
                    return NotFound ();
                }

                return View (computer);
            }
        }

        // GET: Computer/Create
        public IActionResult Create () {
            return View ();
        }

        // POST: Computer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Computer computer) {
            if (ModelState.IsValid) {
                string sql = $@"
                    INSERT INTO Computer
                    ( Make, Manufacturer, PurchaseDate, DecomissionDate )
                    VALUES
                    ( @Make, @Manufacturer,  @PurchaseDate, null )";

                object mapper = new {
                    Make = computer.Make,
                    Manufacturer = computer.Manufacturer,
                    PurchaseDate = DateTime.Today
                };

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql, mapper);

                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                }
            }

            return View (computer);
        }

        // GET: Computer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"SELECT
                c.Id,
                c.Make,
                c.Manufacturer,
                c.PurchaseDate,
                c.DecomissionDate
            FROM Computer c
            WHERE c.Id = {id}";

            using (IDbConnection conn = Connection) {
                var computer = await conn.QuerySingleAsync<Computer> (sql);
                return View (computer);
            }
        }

        // POST: Computer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, Computer computer) {
            if (id != computer.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"UPDATE Computer SET
                    Make='{computer.Make}',
                    Manufacturer='{computer.Manufacturer}',
                    PurchaseDate='{computer.PurchaseDate}',
                    DecomissionDate='{computer.DecomissionDate}'
                WHERE Id={id}";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);
                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                    throw new Exception ("No rows affected");
                }
            } else {
                return View(computer);
            }
        }

        // GET: Computer/Delete/5
        public async Task<IActionResult> Delete (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"SELECT
                    c.Id, c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate
                FROM Computer c WHERE c.Id = {id}";

            using (IDbConnection conn = Connection) {
                Computer computer = (await conn.QuerySingleAsync<Computer> (sql));

                if (computer == null) {
                    return NotFound ();
                }

                return View (computer);
            }
        }

        // POST: Computer/Delete/5
        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {
            string sql = $@"DELETE FROM Computer WHERE Id = {id}";

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
