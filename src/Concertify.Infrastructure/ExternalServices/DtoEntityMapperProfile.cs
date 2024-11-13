using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Models;
using AutoMapper;

namespace Concertify.Infrastructure.ExternalServices;

public class DtoEntityMapperProfile : Profile
{
    public DtoEntityMapperProfile()
    {
        CreateMap<UserRegisterDto, ApplicationUser>()
           .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());

        CreateMap<ApplicationUser, UserInfoDto>();
        CreateMap<UserUpdateDto, ApplicationUser>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
