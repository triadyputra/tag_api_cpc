using cpcApi.Data;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using Dapper;
using System.Data;

namespace cpcApi.Services.Mesin
{
    public class RepoMesin : IRepoMesin
    {
        private readonly DapperSistagContext _context;
        public RepoMesin(DapperSistagContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<MasterMesin>> GetMesin(
        string? cabang,
        string? wsid,
        string? merek,
        string? tipe,
        string? kdAreaCr,
        int page,
        int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            const string procedureName = "Web_Asp_MasterWSID";

            using var connection = _context.CreateConnection();

            // ===============================
            // 1️⃣ DATA (sdata = 1)
            // ===============================
            var dataParams = new DynamicParameters();
            dataParams.Add("@ckdcabang", cabang);
            dataParams.Add("@cwsid", wsid);
            dataParams.Add("@cmerek", merek);
            dataParams.Add("@ctipe", tipe);
            dataParams.Add("@ckdareacr", kdAreaCr);
            dataParams.Add("@sdata", 1);
            dataParams.Add("@StartRowIndex", page);     // PAGE (bukan skip)
            dataParams.Add("@PageSize", pageSize);

            var data = await connection.QueryAsync<MasterMesin>(
                procedureName,
                dataParams,
                commandType: CommandType.StoredProcedure
            );

            // ===============================
            // 2️⃣ TOTAL COUNT (sdata = 2)
            // ===============================
            var countParams = new DynamicParameters();
            countParams.Add("@ckdcabang", cabang);
            countParams.Add("@cwsid", wsid);
            countParams.Add("@cmerek", merek);
            countParams.Add("@ctipe", tipe);
            countParams.Add("@ckdareacr", kdAreaCr);
            countParams.Add("@sdata", 2);
            countParams.Add("@StartRowIndex", 1); // dummy
            countParams.Add("@PageSize", 1);       // dummy

            var total = await connection.ExecuteScalarAsync<int>(
                procedureName,
                countParams,
                commandType: CommandType.StoredProcedure
            );

            return new PagedResult<MasterMesin>
            {
                Data = data,
                Total = total
            };
        }
    }
}
