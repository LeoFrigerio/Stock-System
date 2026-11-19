using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock_System.Data;
using Stock_System.Models.Enum;
using Stock_System.Models.ViewModels;
using Stock_System.Models.ViewModels.DashBoard;

namespace Stock_System.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DashboardController
        public async Task<IActionResult> Index()
        {
            // ===================== KPIs =====================
            var model = new DashboardViewModel();

            model.ValorTotalInventario = await _context.StockDepositos
                .Include(s => s.Producto)
                .Where(s => s.Activo && s.Producto.Activo)
                .SumAsync(s => s.Cantidad * s.Producto.PrecioCosto);

            model.TotalProductosActivos = await _context.Productos.CountAsync(p => p.Activo);
            model.TotalProveedoresActivos = await _context.Proveedores.CountAsync(p => p.Activo);
            model.TotalClientesActivos = await _context.Clientes.CountAsync(c => c.Activo);
            model.TotalDepositosActivos = await _context.Depositos.CountAsync(d => d.Activo);

            model.MovimientosHoy = await _context.Movimientos
                .CountAsync(m => m.Fecha >= DateTime.Today && m.Fecha < DateTime.Today.AddDays(1));

            model.ValorVentasHoy = await _context.Movimientos
                .Include(m => m.Producto)
                .Where(m => m.Fecha >= DateTime.Today && m.Fecha < DateTime.Today.AddDays(1) && m.Tipo == TipoMovimiento.Salida)
                .SumAsync(m => m.Cantidad * m.Producto.PrecioVenta);

            // ============ Tablas ============
            // Stock total por producto activo desde StockDepositos
            var stocksPorProducto = await _context.StockDepositos
                .AsNoTracking()
                .Where(s => s.Activo)
                .GroupBy(s => s.idProducto)
                .Select(g => new
                {
                    idProducto = g.Key,
                    StockTotal = g.Sum(s => s.Cantidad)
                })
                .ToListAsync();

            var productosActivos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .ToListAsync();

            var stockProductos = productosActivos.Select(p =>
            {
                var total = stocksPorProducto.FirstOrDefault(x => x.idProducto == p.id)?.StockTotal ?? 0;
                return new
                {
                    p.id,
                    p.Nombre,
                    CategoriaNombre = p.Categoria?.Nombre ?? "",
                    StockMinimo = (decimal)p.StockMinimo,
                    StockTotal = (decimal)total
                };
            }).ToList();

            // Alertas: BajoStockMinimo = StockTotal <= StockMinimo
            var alertas = stockProductos
                .Where(p => p.StockTotal <= p.StockMinimo)
                .OrderBy(p => p.StockTotal)
                .Select(p => new AlertaReposicion
                {
                    idProducto = p.id,
                    Producto = p.Nombre,
                    Categoria = p.CategoriaNombre,
                    StockTotal = p.StockTotal,
                    StockMinimo = p.StockMinimo
                })
                .ToList();

            model.AlertasBajoStock = alertas.Count;
            model.AlertasReposicion = alertas.Take(15).ToList();

            // ================= Gráficos =================
            model.TopMayorStock = stockProductos
                .OrderByDescending(p => p.StockTotal)
                .Take(5)
                .Select(p => new StockProducto { Producto = p.Nombre, StockTotal = p.StockTotal })
                .ToList();

            model.TopMenorStock = stockProductos
                .OrderBy(p => p.StockTotal)
                .Take(5)
                .Select(p => new StockProducto { Producto = p.Nombre, StockTotal = p.StockTotal })
                .ToList();

            return View(model);
        }
    }
}