using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        //.forMember explicitly defines what to bind to a property,
        //here it maps the photo tagged (photo.IsMain = true)'s .Url to the user's .PhotoUrl property.
        //it also maps age.
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url)
            )
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
    }
}
