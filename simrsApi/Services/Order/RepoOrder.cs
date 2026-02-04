using cpcApi.Data;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc;
using cpcApi.Model.DTO.Cpc.Report;
using cpcApi.Model.MasterData;
using Dapper;
using System.Data;

namespace cpcApi.Services.Order
{
    public class RepoOrder : IRepoOrder
    {
        private readonly DapperSistagContext _context;
        public RepoOrder(DapperSistagContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<OrderCpcDto>> GetOrderCpc(
        string? nowo,
        string? cabang,
        string? bank,
        string? tanggalawal,
        string? tanggalakhir,
        int page,
        int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            const string procedureName = "Web_Asp_OrderCpc";

            using var connection = _context.CreateConnection();

            // ===============================
            // 1️⃣ DATA (sdata = 1)
            // ===============================
            var dataParams = new DynamicParameters();
            dataParams.Add("@nowo", nowo);
            dataParams.Add("@ckdcabang", cabang);
            dataParams.Add("@ckdbank", bank);
            dataParams.Add("@tanggalawal", tanggalawal);
            dataParams.Add("@tanggalakhir", tanggalakhir);
            dataParams.Add("@sdata", 1);
            dataParams.Add("@StartRowIndex", page);     // PAGE (bukan skip)
            dataParams.Add("@PageSize", pageSize);

            var data = await connection.QueryAsync<OrderCpcDto>(
                procedureName,
                dataParams,
                commandType: CommandType.StoredProcedure
            );

            // ===============================
            // 2️⃣ TOTAL COUNT (sdata = 2)
            // ===============================
            var countParams = new DynamicParameters();
            countParams.Add("@nowo", nowo);
            countParams.Add("@ckdcabang", cabang);
            countParams.Add("@ckdbank", bank);
            countParams.Add("@tanggalawal", tanggalawal);
            countParams.Add("@tanggalakhir", tanggalakhir);
            countParams.Add("@sdata", 2);
            countParams.Add("@StartRowIndex", 1); // dummy
            countParams.Add("@PageSize", 1);       // dummy

            var total = await connection.ExecuteScalarAsync<int>(
                procedureName,
                countParams,
                commandType: CommandType.StoredProcedure
            );

            return new PagedResult<OrderCpcDto>
            {
                Data = data,
                Total = total
            };
        }

        public async Task<List<OrderPengisianAtmDto>> PrintOrderCpc(
        string? nowo,
        string? cabang,
        string? bank,
        string? tanggalawal,
        string? tanggalakhir)
        {
            const string procedureName = "Web_Asp_PrintOrderCpc";

            using var connection = _context.CreateConnection();

            var param = new DynamicParameters();
            param.Add("@nowo", nowo);
            param.Add("@ckdcabang", cabang);
            param.Add("@ckdbank", bank);
            param.Add("@tanggalawal", tanggalawal);
            param.Add("@tanggalakhir", tanggalakhir);

            var data = await connection.QueryAsync<OrderPengisianAtmDto>(
                procedureName,
                param,
                commandType: CommandType.StoredProcedure
            );

            return data.ToList();
        }
    }
}
