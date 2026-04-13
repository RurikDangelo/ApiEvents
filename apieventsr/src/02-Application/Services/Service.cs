using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using apieventsr.Data.Interfaces;
using AutoMapper;

namespace apieventsr.Application.Services
{
    public class Service : IService
    {
        private readonly IEntityRepository _interfaceRepository;
        private IMapper _mapper;

        public Service(IEntityRepository interfaceRepository, IMapper mapper)
        {
            _interfaceRepository = interfaceRepository;
            _mapper = mapper;
        }

        public async Task<EntityResponse> GetEntity(int parameter)
        {
            var entity = _interfaceRepository.GetEntity(parameter);
            var response = _mapper.Map<EntityResponse>(entity);
            throw new NotImplementedException();
        }
    }
}
