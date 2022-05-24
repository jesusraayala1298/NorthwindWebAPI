using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindWebAPI.Data;
using NorthwindWebAPI.Models;

namespace NorthwindWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly NorthwindContext _context;

        public EmployeesController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Products
        [HttpGet]
        [Route("Products")]
        public async Task<ActionResult<IEnumerable<Product>>> AllProducts()
        {
            return await _context.Products.OrderBy(e=>e.ProductName).ToListAsync();
        }

        // GET: api/
        [HttpGet]
        [Route("ByCompany")]
        public IEnumerable<Object> GetEmployeesByCompany()
        {
            return _context.Employees
                .GroupBy(e => e.CompanyId)
                .Select(e => new {
                    Company = e.Key,
                    Empleados = e.Count()
                });
            
        }

        // GET: api/
        [HttpGet]
        [Route("top5")]
        public IEnumerable<Object> GetTop5()
        {
            return _context.Employees
                .Where(e =>  e.CompanyId == 1)
                .Join(_context.Movements,
                e => e.EmployeeId,
                m => m.EmployeeId,
                (e, m) => new
                {
                    Empleado = e.FirstName + " " + e.LastName,
                    IdMovimiento = m.MovementId,
                    Anio = m.Date.Year
                })
                .Where(em=>em.Anio==1996)
                .Join(_context.Movementdetails,
                em => em.IdMovimiento,
                md => md.MovementId,
                (em, md) => new
                {
                    Empleado = em.Empleado,
                    Cantidad = md.Quantity
                })
                .GroupBy(e=> e.Empleado)
                .Select(e=> new { 
                    Empleado = e.Key,
                    Ventas = e.Sum(g=>g.Cantidad)
                })
                .OrderByDescending(e => e.Ventas)
                .Take(5);

        }

        // GET: api/
        [HttpGet]
        [Route("Productos_mes")]
        public IEnumerable<Object> GetProducts(int anio, string idProducto)
        {
            return _context.Products
                .Where(e => e.CompanyId == 1)
                .Join(_context.Movementdetails,
                e => e.ProductId,
                m => m.ProductId,
                (e, m) => new
                {
                    Producto = e.ProductId,
                    Nombre = e.ProductName,
                    IdMovimiento = m.MovementId,
                    Cantidad = m.Quantity
                })
                .Join(_context.Movements,
                em => em.IdMovimiento,
                md => md.MovementId,
                (em, md) => new
                {
                    Producto = em.Producto,
                    Nombre = em.Nombre,
                    Cantidad = em.Cantidad,
                    Anio = md.Date.Year,
                    Mes = md.Date.Month,
                    Fecha=md.Date
                })
                .Where(e => e.Anio==anio && e.Nombre.Equals(idProducto))
                .GroupBy(e => e.Mes)
                .Select(e => new{
                    Mes = e.Key,
                    Ventas = e.Sum(g=>g.Cantidad)
                })
                .OrderBy(e => e.Mes);
        }


        //GET: api/  TOP 5 DE PRODUCTOS VENDIDOS EN UN AÑO POR TRIMESTRE

        [HttpGet]
        [Route("productos_trim")]
        public IEnumerable<Object> GetTop5Productos(int ano, int trimestre)
        {

            return _context.Products.Where(p => p.CompanyId == 1)
                .Join(_context.Movementdetails,
                p => p.ProductId,
                md => md.ProductId,
                (p, md) => new
                {
                    Producto = p.ProductName,
                    IdMovimiento = md.MovementId,
                    cantidad = md.Quantity
                })
                .Join(_context.Movements,
                md => md.IdMovimiento,
                mo => mo.MovementId,
                (md, mo) => new
                {
                    Producto = md.Producto,
                    Anio = mo.Date.Year,
                    Trim = Math.Ceiling(mo.Date.Month / 3.0),
                    Tipo = mo.Type,
                    quantity = md.cantidad
                }).Where(mo => mo.Anio == ano && mo.Trim == trimestre && mo.Tipo == "VENTA")
                .GroupBy(p => p.Producto)
                .Select(p => new
                {
                    Producto = p.Key,
                    Cantidad = p.Sum(c => c.quantity)
                })
                .OrderByDescending(p => p.Cantidad)
                .Take(5);
        }

        // GET: api/
        [HttpGet]
        [Route("Ventas_almacen")]
        public IEnumerable<Object> GetVentas_Almacen()
        {
            return _context.Products
                .Where(p => p.CompanyId == 1)
                .Join(_context.Movementdetails,
                    p => p.ProductId,
                    md => md.ProductId,
                    (p, md) => new
                    {
                        Nombre = p.ProductName,
                        cantidad = md.Quantity,
                        Movimiento = md.MovementId
                    })
                .Join(_context.Movements,
                    md => md.Movimiento,
                    m => m.MovementId,
                    (md, m) => new
                    {
                        Nombre = md.Nombre,
                        Almacen = m.OriginWarehouseId,
                        cantidad = md.cantidad,
                    })
                .Join(_context.Warehouses,
                    m => m.Almacen,
                    w => w.WarehouseId,
                    (m,w) => new
                    {
                        Nombre = m.Nombre,
                        NombreAlmacen = w.Description,
                        cantidad = m.cantidad
                    })
                .GroupBy(g => new { g.NombreAlmacen, g.Nombre  })
                .Select(p => new {
                    Almacen = p.Key.NombreAlmacen,
                    Nombre = p.Key.Nombre,
                    cantidad = p.Sum(g => g.cantidad)
                }).OrderBy(e => e.Nombre);
        }



        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // GET: api/Employees/5
        [HttpGet("GetProduct")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployee", new { id = employee.EmployeeId }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
