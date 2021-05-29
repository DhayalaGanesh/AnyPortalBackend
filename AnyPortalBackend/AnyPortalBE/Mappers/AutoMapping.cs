using AnyPortalBE.Models;
using AnyPortalBE.Models.View;
using AutoMapper;

namespace AnyPortalBE.Mappers
{
    public class AutoMapping: Profile
    {
        public AutoMapping()
        {
            CreateMap<RegistrationViewModel, AppUser>().AfterMap((src,dest)=> 
            {
                dest.UserName = src.FirstName;
            });
        }
    }
}
