using apieventsr.Application.Dtos.Requests;
using apieventsr.Application.Dtos.Responses;
using apieventsr.Domain.Entities;
using AutoMapper;

namespace apieventsr.Application.Mappers
{
    public class EntityDtoMapper : Profile
    {
        public EntityDtoMapper()
        {
            CreateMap<EntityRequest, DomainEntity>();
            CreateMap<DomainEntity, EntityResponse>();
        }
    }
}
