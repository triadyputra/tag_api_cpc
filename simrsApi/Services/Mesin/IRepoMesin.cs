using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;

namespace cpcApi.Services.Mesin
{
    public interface IRepoMesin
    {
        Task<PagedResult<MasterMesin>> GetMesin(
        string? cabang,
        string? wsid,
        string? merek,
        string? tipe,
        string? kdAreaCr,
        int page,
        int pageSize);
    }
}
