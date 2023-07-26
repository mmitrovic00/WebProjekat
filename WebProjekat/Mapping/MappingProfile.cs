using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.DTO;
using WebProjekat.DTO.User;
using WebProjekat.Models;

namespace WebProjekat.Mapping
{
	public class MappingProfile : Profile
	{
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap(); //Kazemo mu da mapira Subject na SubjectDto i obrnuto
            CreateMap<Item, ItemDto>().ReverseMap();
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Customer, UserDto>().ReverseMap();
            CreateMap<Customer, RegistrationUserDto>().ReverseMap();
            CreateMap<Seller, UserDto>().ReverseMap();
            CreateMap<Seller, RegistrationUserDto>().ReverseMap();
            CreateMap<Admin, UserDto>().ReverseMap();
            CreateMap<Admin, RegistrationUserDto>().ReverseMap();
        }
    }
}
