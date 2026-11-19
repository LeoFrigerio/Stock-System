using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Stock_System.Models.ViewModels.Stock
{
    public class StockDetalleItemViewModel
    {
        public int id { get; set; }
        public int idProducto { get; set; }
        public int idDeposito { get; set; }

        [Display(Name = "Código")]
        public string? CodigoProducto { get; set; }

        [Display(Name = "Producto")]
        public string ProductoNombre { get; set; } = string.Empty;

        [Display(Name = "Categoría")]
        public string CategoriaNombre { get; set; } = string.Empty;

        [Display(Name = "Depósito")]
        public string DepositoNombre { get; set; } = string.Empty;

        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        [Display(Name = "Stock Mínimo")]
        public int StockMinimo { get; set; }

        public bool BajoStockMinimo => Cantidad <= StockMinimo;
    }

    public class StockIndexViewModel
    {
        public string? Buscar { get; set; }
        public int? idProducto { get; set; }
        public int? idDeposito { get; set; }
        public bool SoloConStock { get; set; } = true;

        public List<SelectListItem> Productos { get; set; } = new();
        public List<SelectListItem> Depositos { get; set; } = new();
        public List<StockDetalleItemViewModel> Items { get; set; } = new();

        public decimal StockTotalFiltrado => Items.Sum(i => i.Cantidad);
    }
}