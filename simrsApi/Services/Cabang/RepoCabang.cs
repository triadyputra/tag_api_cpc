using cpcApi.Data;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using Dapper;

namespace cpcApi.Services.Cabang
{
    public class RepoCabang : IRepoCabang
    {
        private readonly DapperSistagContext _context;
        public RepoCabang(DapperSistagContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<MasterCabang>> GetCabang(
        string? filter,
        int page,
        int pageSize)
        {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var sql = @"
                            -- DATA
                            SELECT *
                            FROM TBL_CABANG
                            WHERE (@filter IS NULL OR NMCABANG LIKE '%' + @filter + '%')
                            ORDER BY NMCABANG
                            OFFSET (@page - 1) * @pageSize ROWS
                            FETCH NEXT @pageSize ROWS ONLY;

                            -- TOTAL
                            SELECT COUNT(1)
                            FROM TBL_CABANG
                            WHERE (@filter IS NULL OR NMCABANG LIKE '%' + @filter + '%');
                        ";

            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                filter,
                page,
                pageSize
            });

            var data = await multi.ReadAsync<MasterCabang>();
            var total = await multi.ReadFirstAsync<int>();

            return new PagedResult<MasterCabang>
            {
                Data = data,
                Total = total
            };
        }

        //public async Task<IEnumerable<MasterCabang>> GetCabang(string filter, int skip, int take)
        //{
        //    var query = "SELECT * FROM TBL_CABANG WHERE (@filter IS NULL OR NMCABANG LIKE '%'+@filter+'%') "
        //              + "ORDER BY NMCABANG "
        //              + "OFFSET(@StartRowIndex - 1) * @PageSize ROWS "
        //              + "FETCH NEXT @PageSize ROWS ONLY; ";

        //    using (var connection = _context.CreateConnection())
        //    {
        //        var companies = await connection.QueryAsync<MasterCabang>(query, new
        //        {
        //            filter = filter,
        //            StartRowIndex = skip,
        //            PageSize = take
        //        });
        //        return companies.ToList();
        //    }
        //}
    }
}
