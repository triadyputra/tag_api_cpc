using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc;
using cpcApi.Model.DTO.Cpc.Report;
using cpcApi.Model.MasterData;

namespace cpcApi.Services.Order
{
    public interface IRepoOrder
    {
        Task<PagedResult<OrderCpcDto>> GetOrderCpc(
        string? nowo,
        string? cabang,
        string? bank,
        string? tanggalawal,
        string? tanggalakhir,
        int page,
        int pageSize);

        Task<List<OrderPengisianAtmDto>> PrintOrderCpc(
        string? nowo,
        string? cabang,
        string? bank,
        string? tanggalawal,
        string? tanggalakhir);
    }
}
