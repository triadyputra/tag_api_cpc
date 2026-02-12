using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc.Vault;
using cpcApi.Services.Cpc.Vault;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.Cpc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StokVaultController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IVaultService _vaultService;

        public StokVaultController(ApplicationDbContext context, IVaultService vaultService)
        {
            _context = context;
            _vaultService = vaultService;
        }

        // =====================================================
        // GET LIST STOK VAULT (SALDO TERAKHIR)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListStokVault")]
        public async Task<ActionResult<PaginatedResponse<StokVaultDto>>> GetListStokVault(
        [FromQuery] string? kdCabang,
        [FromQuery] string? kdBank,
        [FromQuery] int? nominal,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.StokVaultCabang
                .AsNoTracking()
                .AsQueryable();

            // ================= FILTER =================
            if (!string.IsNullOrWhiteSpace(kdCabang))
                query = query.Where(x => x.KdCabang == kdCabang);

            if (!string.IsNullOrWhiteSpace(kdBank))
                query = query.Where(x => x.KdBank == kdBank);

            if (nominal.HasValue)
                query = query.Where(x => x.Nominal == nominal.Value);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.KdCabang)
                .ThenBy(x => x.Nominal)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new StokVaultDto
                {
                    KdCabang = x.KdCabang,
                    KdBank = x.KdBank,   // 🔥 TAMBAHAN DI DTO
                    Nominal = x.Nominal,
                    Saldo = x.SaldoLembar,
                    LastUpdate = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<StokVaultDto>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // ================================================
        // POST MUTASI (MANUAL / SALDO AWAL / ADJUSTMENT)
        // ================================================
        [ApiKeyAuthorize]
        [HttpPost("PostMutasi")]
        public async Task<ActionResult<ApiResponse<object>>> PostMutasi(
        [FromBody] MutasiRequest request)
        {
            var result = await _vaultService.MutasiAsync(
                request.KdCabang,
                request.KdBank,
                request.Nominal,
                request.QtyLembar,
                request.TipeMutasi,
                request.ReferenceNo
            );

            if (!result.Success)
                return Ok(ApiResponse<object>.Error(result.Message, "400"));

            return Ok(ApiResponse<object>.SuccessNoData("Mutasi berhasil"));
        }

        // ================================================
        // TRANSFER ANTAR CABANG
        // ================================================
        [ApiKeyAuthorize]
        [HttpPost("PostTransfer")]
        public async Task<ActionResult<ApiResponse<object>>> PostTransfer(
            [FromBody] TransferVaultRequest request)
        {
            var result = await _vaultService.TransferAsync(
                request.KdCabangAsal,
                request.KdCabangTujuan,
                request.KdBank,
                request.Nominal,
                request.QtyLembar,
                request.ReferenceNo
            );

            if (!result.Success)
                return Ok(ApiResponse<object>.Error(result.Message, "400"));

            return Ok(ApiResponse<object>.SuccessNoData("Transfer berhasil"));
        }


        // ================================================
        // POST OPNAME
        // ================================================
        [ApiKeyAuthorize]
        [HttpPost("PostOpname")]
        public async Task<ActionResult<ApiResponse<object>>> PostOpname(
            [FromBody] OpnameVaultRequest request)
        {
            var result = await _vaultService.OpnameAsync(
                request.KdCabang,
                request.Nominal,
                request.SaldoFisik
            );

            if (!result.Success)
                return Ok(ApiResponse<object>.Error(result.Message, "400"));

            return Ok(ApiResponse<object>.SuccessNoData("Opname berhasil"));
        }
    }
}
