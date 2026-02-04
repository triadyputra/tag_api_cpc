using cpcApi.Data;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using Dapper;
using System.Data;

namespace cpcApi.Services.Bank
{
    public class RepoBank : IRepoBank
    {
        private readonly DapperSistagContext _context;
        public RepoBank(DapperSistagContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<MasterBank>> GetBank(
        string? filter,
        int page,
        int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            using var connection = _context.CreateConnection();

            var where = "";
            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                where = "WHERE KDBANK LIKE @filter OR NMBANK LIKE @filter";
                param.Add("@filter", $"%{filter}%");
            }

            // ===============================
            // DATA (PAGING)
            // ===============================
            var sqlData = $@"
                    SELECT ID, KDBANK, NMBANK
                    FROM dbo.TBL_BANK
                    {where}
                    ORDER BY NMBANK
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";

            param.Add("@Offset", (page - 1) * pageSize);
            param.Add("@PageSize", pageSize);

            var data = await connection.QueryAsync<MasterBank>(sqlData, param);

            // ===============================
            // TOTAL COUNT
            // ===============================
            var sqlCount = $@"
                        SELECT COUNT(1)
                        FROM dbo.TBL_BANK
                        {where}
                    ";

            var total = await connection.ExecuteScalarAsync<int>(sqlCount, param);

            return new PagedResult<MasterBank>
            {
                Data = data,
                Total = total
            };
        }
    }
}
