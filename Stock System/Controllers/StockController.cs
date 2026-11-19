using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stock_System.Data;
using Stock_System.Models.ViewModels;
using Stock_System.Models.ViewModels.Stock;

namespace Stock_System.Controllers
{
    public class StockController : Controller
    {
        private readonly AppDbContext _context;

        public StockController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Stock
        public async Task<IActionResult> Index(string buscar, int? buscarProducto, int? buscarDeposito)
        {
            var stocks = _context.StockDepositos
                .Include(s => s.Producto)
                    .ThenInclude(p => p.Categoria)
                .Include(s => s.Deposito)
                .Where(s => s.Activo && s.Producto.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                stocks = stocks.Where(s => s.Producto.Nombre.Contains(buscar) ||
                                           (s.Producto.Codigo != null && s.Producto.Codigo.Contains(buscar)));

            if (buscarProducto.HasValue)
                stocks = stocks.Where(s => s.idProducto == buscarProducto.Value);

            if (buscarDeposito.HasValue)
                stocks = stocks.Where(s => s.idDeposito == buscarDeposito.Value);

            ViewBag.Productos = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "id", "Nombre");
            ViewBag.Depositos = new SelectList(await _context.Depositos.Where(d => d.Activo).ToListAsync(), "id", "Nombre");

            var lista = await stocks
                .Where(s => s.Cantidad != 0)
                .OrderBy(s => s.Producto.Nombre)
                .Select(s => new StockDetalleItemViewModel
                {
                    id = s.id,
                    idProducto = s.idProducto,
                    idDeposito = s.idDeposito,
                    CodigoProducto = s.Producto.Codigo,
                    ProductoNombre = s.Producto.Nombre,
                    CategoriaNombre = s.Producto.Categoria != null ? s.Producto.Categoria.Nombre : "",
                    DepositoNombre = s.Deposito != null ? s.Deposito.Nombre : "",
                    Cantidad = s.Cantidad,
                    StockMinimo = s.Producto.StockMinimo
                })
                .ToListAsync();

            var model = new StockIndexViewModel
            {
                Buscar = buscar,
                idProducto = buscarProducto,
                idDeposito = buscarDeposito,
                Items = lista
            };

            return View(model);
        }
    }
}