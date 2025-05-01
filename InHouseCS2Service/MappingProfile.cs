using AutoMapper;
using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2.Core.Managers.Models;
using InHouseCS2Service.Controllers.Models;

namespace InHouseCS2Service;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        this.CreateMap<MatchDataWrapper, CoreMatchDataWrapperRecord>();
        this.CreateMap<MatchDataObject, CoreMatchDataRecord>();
        this.CreateMap<MatchKillData, CoreMatchKillDataRecord>();
        this.CreateMap<MatchMetadata, CoreMatchMetadataRecord>();
        this.CreateMap<MatchStatPerPlayer, CoreMatchStatPerPlayerRecord>();
    }
}
