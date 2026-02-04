using cpcApi.Data;
using cpcApi.Model.DTO;
using Dapper;

namespace cpcApi.Services.Combo
{
    public class RepoCombo : IRepoCombo
    {
        private readonly DapperSistagContext _context;
        public RepoCombo(DapperSistagContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ComboViewModel>> ComboCabang()
        {
            var query = "SELECT KDCABANG Id, NMCABANG Name FROM TBL_CABANG order by NMCABANG";


            using (var connection = _context.CreateConnection())
            {
                var companies = await connection.QueryAsync<ComboViewModel>(query);
                return companies.ToList();
            }
        }

        public async Task<IEnumerable<ComboViewModel>> ComboBank()
        {
            var query = "SELECT KDBANK Id, NMBANK Name FROM TBL_BANK order by NMBANK";


            using (var connection = _context.CreateConnection())
            {
                var companies = await connection.QueryAsync<ComboViewModel>(query);
                return companies.ToList();
            }
        }
    }
}
