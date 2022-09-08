using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc
{
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;

        public GrpcPlatformService(IPlatformRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override async Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
        {
            var responce = new PlatformResponse();
            var platforms = _repository.GetAllPlatforms();
            foreach (var platfrom in platforms)
            {
                responce.Platform.Add(_mapper.Map<GrpcPlatformModel>(platfrom));
            }

            return await Task.FromResult<PlatformResponse>(responce);
        }
    }
}