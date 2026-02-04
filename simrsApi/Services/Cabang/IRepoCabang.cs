using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;

namespace cpcApi.Services.Cabang
{
    public interface IRepoCabang
    {
        Task<PagedResult<MasterCabang>> GetCabang(
        string? filter,
        int page,
        int pageSize);

        //public Task<IEnumerable<MasterCabang>> GetCabang(string filter, int skip, int take);
    }
}
