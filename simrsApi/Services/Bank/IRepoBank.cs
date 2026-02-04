using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;

namespace cpcApi.Services.Bank
{
    public interface IRepoBank
    {
        Task<PagedResult<MasterBank>> GetBank(
        string? filter,
        int page,
        int pageSize);
    }
}
