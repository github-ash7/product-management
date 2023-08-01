using AutoMapper;
using Entities.DTOs;
using Entities.Models;
using Microsoft.Data.SqlClient;

namespace Services
{
    /// <summary>
    /// The ProductProfile class holds all the mappings 
    /// </summary>
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductResponseDto>();

            CreateMap<SqlDataReader, Product>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => (Guid)src["id"]))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => (string)src["product_name"]))
            .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => (Guid)src["category_id"]))
            .ForMember(dest => dest.SupplierID, opt => opt.MapFrom(src => (Guid)src["supplier_id"]))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => (decimal)src["unit_price"]))
            .ForMember(dest => dest.UnitsInStock, opt => opt.MapFrom(src => (int)src["units_in_stock"]))
            .ForMember(dest => dest.Discontinued, opt => opt.MapFrom(src => (bool)src["discontinued"]));
        }
    }
}