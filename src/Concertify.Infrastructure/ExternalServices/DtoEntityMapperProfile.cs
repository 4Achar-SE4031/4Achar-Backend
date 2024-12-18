﻿using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Models;
using AutoMapper;
using Concertify.Domain.Dtos.Concert;

namespace Concertify.Infrastructure.ExternalServices;

public class DtoEntityMapperProfile : Profile
{
    public DtoEntityMapperProfile()
    {
        CreateMap<UserRegisterDto, ApplicationUser>()
           .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());

        CreateMap<ApplicationUser, UserInfoDto>();
        CreateMap<UserUpdateDto, ApplicationUser>()
            .ForSourceMember(src => src.Email, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.UserName, opt => opt.DoNotValidate())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Concert, ConcertDetailsDto>();
        CreateMap<Concert, ConcertSummaryDto>();
    }
}
