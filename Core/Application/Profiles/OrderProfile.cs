using Application.DTOs.Requests;
using Application.DTOs.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderItemRequest, OrderItem>();
        CreateMap<Order, OrderVm>().ForMember(x => x.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<OrderItem, OrderItemVm>().ForMember(x => x.Product, opt => opt.MapFrom(src => src.Product));
    }
}