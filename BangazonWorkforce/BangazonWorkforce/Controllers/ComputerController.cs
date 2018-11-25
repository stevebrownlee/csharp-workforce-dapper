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
                        IFNULL(c.Id, 0) as Id,
                        c.Make,
                        c.Manufacturer,
                        c.PurchaseDate,
                        c.DecomissionDate,
                        IFNULL(ce.Id, 0) as Id,
                        IFNULL(ce.ComputerId, 0) as ComputerId,
                        IFNULL(ce.EmployeeId, 0) as EmployeeId,
                        ce.AssignDate,
                        ce.UnassignDate,
                        IFNULL(e.Id, 0) as Id,
                        e.FirstName,
                        e.LastName,
                        IFNULL(e.DepartmentId, 0) as DepartmentId,
                        e.IsSupervisor
                    FROM Computer c
                    LEFT JOIN ComputerEmployee ce ON c.Id = ce.ComputerId
                    LEFT JOIN Employee e ON ce.EmployeeId = e.Id
                    WHERE c.DecomissionDate IS NULL
                    ORDER BY c.PurchaseDate DESC
                    ",
                    (c, ce, e) => {
                        if (!computers.ContainsKey(c.Id) && c.Id != 0) {
                            computers[c.Id] = c;
                        }

                        if (e.Id != 0) {
                            computers[c.Id].Employees.Add(e);
                        }

                        if (ce.Id != 0 && ce.UnassignDate == null && ce.AssignDate != null) {
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
                    PurchaseDate='{computer.PurchaseDate}'
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
            string sql = $@"
                UPDATE Computer
                SET DecomissionDate = '{DateTime.Today}'
                WHERE Id = {id}
            ";

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
