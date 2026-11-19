using AutoMapper;
using Stock_System.Models.Entities;
using Stock_System.Models.ViewModels.Movimiento;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SistemaStock.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movimiento, MovimientoViewModel>();
            CreateMap<MovimientoViewModel, Movimiento>();
        }
    }
}