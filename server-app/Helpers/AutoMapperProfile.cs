using AutoMapper;
using DocShareApp.Entities;
using DocShareApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocShareApp.Helpers
{

    public class AutoMapperProfile : Profile

    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();
        }
    }
}
