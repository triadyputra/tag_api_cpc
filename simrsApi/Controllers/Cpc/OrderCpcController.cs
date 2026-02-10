using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc;
using cpcApi.Model.DTO.Cpc.orderkaset;
using cpcApi.Model.DTO.Cpc.Report;
using cpcApi.Model.MasterData;
using cpcApi.Report;
using cpcApi.Services.Mesin;
using cpcApi.Services.Order;
using DevExpress.XtraPrinting.Native;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace cpcApi.Controllers.Cpc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderCpcController : ControllerBase
    {
        private readonly DapperSistagContext _daperContext;
        private readonly IRepoOrder _order;
        private readonly ApplicationDbContext _context;
        public OrderCpcController(ApplicationDbContext context, DapperSistagContext daperContext, IRepoOrder order)
        {
            _daperContext = daperContext;
            _order = order;
            _context = context;
        }

        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListOrderCpc")]
        public async Task<ActionResult<PaginatedResponse<OrderCpcDto>>> GetListOrderCpc(
        [FromQuery] string? nowo = null,
        [FromQuery] string? cabang = null,
        [FromQuery] string? bank = null,
        [FromQuery] string? tanggalawal = null,
        [FromQuery] string? tanggalakhir = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 🔹 Panggil repository (SESUAI INTERFACE)
                var result = await _order.GetOrderCpc(
                    nowo,
                    cabang,
                    bank,
                    tanggalawal,
                    tanggalakhir,
                    page,
                    pageSize);

                return Ok(new PaginatedResponse<OrderCpcDto>
                {
                    Data = result.Data,
                    TotalCount = result.Total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(result.Total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        //[ApiKeyAuthorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetailOrderCpc(string id)
        {
            try
            {
                var order = await _context.OrderPengisianKaset
                    .Include(o => o.Details)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound("Order tidak ditemukan");

                var response = new OrderPengisianKasetDetailResponse
                {
                    Id = order.Id,
                    NomorMesin = order.NomorMesin,
                    TanggalOrder = order.TanggalOrder,
                    Lokasi = order.Lokasi,
                    MerekMesin = order.MerekMesin,
                    KDBANK = order.KDBANK,
                    KDCABANG = order.KDCABANG,
                    Jumlah = order.Jumlah,
                    Details = order.Details
                        .OrderBy(d => d.Kaset)
                        .Select(d => new OrderPengisianKasetDetailDto
                        {
                            Kaset = d.Kaset,
                            KodeKaset = d.KodeKaset,
                            NoSeal = d.NoSeal,
                            Denom = d.Denom,
                            Lembar = d.Lembar
                        })
                        .ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [ApiKeyAuthorize]
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostOrderCpc(
            [FromBody] OrderPengisianKasetRequest payload
        )
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            if (payload.Details == null || payload.Details.Count == 0)
            {
                return Ok(ApiResponse<object>.Error(
                    "Detail kaset wajib diisi.",
                    "400"
                ));
            }

            if (payload.Details.Select(x => x.KodeKaset).Distinct().Count()
                != payload.Details.Count)
            {
                return Ok(ApiResponse<object>.Error(
                    "Kaset tidak boleh duplikat.",
                    "400"
                ));
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var trx = await _context.Database.BeginTransactionAsync();

                try
                {
                    var timeNow = DateTime.UtcNow;

                    var order = await _context.OrderPengisianKaset
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x => x.Id == payload.Id);

                    var isEdit = order != null;

                    // ===============================
                    // KASET BARU DARI PAYLOAD
                    // ===============================
                    var newKasetList = payload.Details
                        .Select(d => d.KodeKaset)
                        .Distinct()
                        .ToList();

                    // ===============================
                    // KASET LAMA (KHUSUS EDIT)
                    // ===============================
                    var oldKasetList = isEdit
                        ? order!.Details.Select(d => d.KodeKaset).Distinct().ToList()
                        : new List<string>();

                    var kasetToRelease = oldKasetList.Except(newKasetList).ToList();
                    var kasetToApply = newKasetList.Except(oldKasetList).ToList();

                    // ===============================
                    // CREATE HEADER
                    // ===============================
                    if (!isEdit)
                    {
                        order = new OrderPengisianKaset
                        {
                            Id = payload.Id,
                            created = username,
                            createdat = timeNow
                        };

                        _context.OrderPengisianKaset.Add(order);
                        kasetToApply = newKasetList; // CREATE → semua apply
                    }

                    // ===============================
                    // RELEASE KASET LAMA (EDIT)
                    // ===============================
                    if (isEdit && kasetToRelease.Any())
                    {
                        var oldStocks = await _context.KasetStock
                            .Where(x => kasetToRelease.Contains(x.KdKaset))
                            .ToListAsync();

                        foreach (var ks in oldStocks)
                        {
                            ks.Status = "LOADED";
                            ks.LocationType = null;
                            ks.LocationId = null;
                            ks.UpdatedAt = timeNow;
                        }

                        var oldKotak = await _context.ProsesKotakUangCpc
                            .Where(x =>
                                kasetToRelease.Contains(x.NomorKotakUang!) &&
                                x.Status == StatusKotakUangCpc.Used
                            )
                            .ToListAsync();

                        foreach (var k in oldKotak)
                        {
                            k.Status = StatusKotakUangCpc.Ready;
                            k.UpdatedAt = timeNow;
                        }
                    }

                    // ===============================
                    // APPLY KASET BARU (CREATE / EDIT)
                    // ===============================
                    if (kasetToApply.Any())
                    {
                        var newStocks = await _context.KasetStock
                            .Where(x => kasetToApply.Contains(x.KdKaset))
                            .ToListAsync();

                        var invalid = newStocks.FirstOrDefault(x =>
                            !isEdit && x.Status != "LOADED" ||
                            isEdit && x.Status == "USED"
                        );

                        if (invalid != null)
                        {
                            await trx.RollbackAsync();
                            return Ok(ApiResponse<object>.Error(
                                $"Kaset {invalid.KdKaset} tidak bisa digunakan.",
                                "409"
                            ));
                        }

                        foreach (var ks in newStocks)
                        {
                            ks.Status = "ON_TRIP";
                            ks.LocationType = "WO";
                            ks.LocationId = order!.Id;
                            ks.UpdatedAt = timeNow;
                        }

                        var newKotak = await _context.ProsesKotakUangCpc
                            .Where(x =>
                                kasetToApply.Contains(x.NomorKotakUang!) &&
                                x.Status != StatusKotakUangCpc.Used
                            )
                            .ToListAsync();

                        foreach (var k in newKotak)
                        {
                            k.Status = StatusKotakUangCpc.Used;
                            k.UpdatedAt = timeNow;
                        }
                    }

                    // ===============================
                    // UPDATE HEADER
                    // ===============================
                    order!.NomorMesin = payload.NomorMesin;
                    order.TanggalOrder = payload.TanggalOrder;
                    order.Lokasi = payload.Lokasi;
                    order.MerekMesin = payload.MerekMesin;
                    order.KDBANK = payload.KDBANK;
                    order.KDCABANG = payload.KDCABANG;
                    order.updated = username;
                    order.updatedat = timeNow;

                    // ===============================
                    // REPLACE DETAIL
                    // ===============================
                    if (isEdit)
                    {
                        _context.OrderPengisianKasetDetail.RemoveRange(order.Details);
                        order.Details.Clear();
                    }

                    foreach (var d in payload.Details)
                    {
                        order.Details.Add(new OrderPengisianKasetDetail
                        {
                            OrderId = order.Id,
                            Kaset = d.Kaset,
                            KodeKaset = d.KodeKaset,
                            NoSeal = d.NoSeal,
                            Denom = d.Denom,
                            Lembar = d.Lembar
                        });
                    }

                    // ===============================
                    // HITUNG TOTAL
                    // ===============================
                    order.Jumlah = order.Details.Sum(x => x.Denom * x.Lembar);

                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();

                    return Ok(ApiResponse<object>.Success(
                        new { orderId = order.Id },
                        isEdit
                            ? "Order pengisian kaset berhasil diperbarui"
                            : "Order pengisian kaset berhasil disimpan"
                    ));
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync();

                    var fullError =
                        ex.InnerException?.Message != null
                            ? $"{ex.Message} | {ex.InnerException.Message}"
                            : ex.Message;

                    return Ok(ApiResponse<object>.Error(
                        fullError,
                        "500"
                    ));
                }
            });
        }

        //public async Task<ActionResult<ApiResponse<object>>> PostOrderCpc(
        //    [FromBody] OrderPengisianKasetRequest payload
        //)
        //{
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

        //    if (payload.Details == null || payload.Details.Count == 0)
        //    {
        //        return Ok(ApiResponse<object>.Error(
        //            "Detail kaset wajib diisi.",
        //            "400"
        //        ));
        //    }

        //    if (payload.Details.Select(x => x.KodeKaset).Distinct().Count()
        //        != payload.Details.Count)
        //    {
        //        return Ok(ApiResponse<object>.Error(
        //            "Kaset tidak boleh duplikat.",
        //            "400"
        //        ));
        //    }

        //    var strategy = _context.Database.CreateExecutionStrategy();

        //    return await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var trx = await _context.Database.BeginTransactionAsync();

        //        try
        //        {
        //            var order = await _context.OrderPengisianKaset
        //                .Include(x => x.Details)
        //                .FirstOrDefaultAsync(x => x.Id == payload.Id);

        //            var isEdit = order != null;
        //            var timeNow = DateTime.UtcNow;

        //            // ===============================
        //            // CREATE
        //            // ===============================
        //            if (!isEdit)
        //            {
        //                order = new OrderPengisianKaset
        //                {
        //                    Id = payload.Id,
        //                    created = username,
        //                    createdat = timeNow
        //                };

        //                _context.OrderPengisianKaset.Add(order);

        //                // 1️⃣ Ambil KdKasetBank dari payload
        //                var kdKasetBankList = payload.Details
        //                    .Select(d => d.KodeKaset) // ⬅️ ini KdKasetBank
        //                    .Distinct()
        //                    .ToList();

        //                // 2️⃣ Mapping ke MasterKaset → IdKaset
        //                var masterKasets = await _context.MasterKaset
        //                    .Where(x => kdKasetBankList.Contains(x.KdKaset))
        //                    .Select(x => new
        //                    {
        //                        x.KdKaset,
        //                        //x.KdKasetBank
        //                    })
        //                    .ToListAsync();

        //                if (masterKasets.Count != kdKasetBankList.Count)
        //                {
        //                    await trx.RollbackAsync();
        //                    return Ok(ApiResponse<object>.Error(
        //                        "Ada kode kaset bank yang tidak terdaftar.",
        //                        "400"
        //                    ));
        //                }

        //                var idKasetList = masterKasets
        //                    .Select(x => x.KdKaset)
        //                    .ToList();

        //                // 3️⃣ Validasi KasetStock pakai IdKaset
        //                var kasetStocks = await _context.KasetStock
        //                    .Where(x => idKasetList.Contains(x.KdKaset))
        //                    .ToListAsync();

        //                if (kasetStocks.Count != idKasetList.Count)
        //                {
        //                    await trx.RollbackAsync();
        //                    return Ok(ApiResponse<object>.Error(
        //                        "Ada kaset yang tidak ditemukan di stok.",
        //                        "400"
        //                    ));
        //                }

        //                // 4️⃣ Validasi status
        //                var notEmpty = kasetStocks.FirstOrDefault(x => x.Status != "LOADED");
        //                if (notEmpty != null)
        //                {
        //                    var kdKasetBank = masterKasets
        //                        .First(x => x.KdKaset == notEmpty.KdKaset)
        //                        .KdKaset;

        //                    await trx.RollbackAsync();
        //                    return Ok(ApiResponse<object>.Error(
        //                        $"Kaset {kdKasetBank} tidak berstatus LOADED.",
        //                        "409"
        //                    ));
        //                }

        //                // 5️⃣ Update stok
        //                foreach (var ks in kasetStocks)
        //                {
        //                    ks.Status = "ON_TRIP";
        //                    ks.LocationType = "WO";
        //                    ks.LocationId = order.Id;
        //                    ks.UpdatedAt = timeNow;
        //                }

        //                // Update status kotak proses
        //                var kotakUangList = await _context.ProsesKotakUangCpc
        //                    .Where(x =>
        //                        idKasetList.Contains(x.NomorKotakUang!) &&
        //                        x.Status != StatusKotakUangCpc.Used
        //                    )
        //                    .ToListAsync();
        //                foreach (var kotak in kotakUangList)
        //                {
        //                    kotak.Status = StatusKotakUangCpc.Used;
        //                    kotak.UpdatedAt = DateTime.UtcNow;
        //                }

        //            }

        //            // ===============================
        //            // UPDATE HEADER
        //            // ===============================
        //            order.NomorMesin = payload.NomorMesin;
        //            order.TanggalOrder = payload.TanggalOrder;
        //            order.Lokasi = payload.Lokasi;
        //            order.MerekMesin = payload.MerekMesin;
        //            order.KDBANK = payload.KDBANK;
        //            order.KDCABANG = payload.KDCABANG;
        //            order.updated = username;
        //            order.updatedat = timeNow;

        //            // ===============================
        //            // REPLACE DETAIL
        //            // ===============================
        //            if (isEdit)
        //            {
        //                _context.OrderPengisianKasetDetail.RemoveRange(order.Details);
        //                order.Details.Clear();
        //            }

        //            foreach (var d in payload.Details)
        //            {
        //                order.Details.Add(new OrderPengisianKasetDetail
        //                {
        //                    OrderId = order.Id,
        //                    Kaset = d.Kaset,
        //                    KodeKaset = d.KodeKaset,
        //                    NoSeal = d.NoSeal,
        //                    Denom = d.Denom,
        //                    Lembar = d.Lembar
        //                });
        //            }


        //            // ===============================
        //            // HITUNG TOTAL
        //            // ===============================
        //            order.Jumlah = order.Details.Sum(x => x.Denom * x.Lembar);

        //            await _context.SaveChangesAsync();
        //            await trx.CommitAsync();

        //            return Ok(ApiResponse<object>.Success(
        //                new { orderId = order.Id },
        //                isEdit
        //                    ? "Order pengisian kaset berhasil diperbarui"
        //                    : "Order pengisian kaset berhasil disimpan"
        //            ));
        //        }
        //        catch (Exception ex)
        //        {
        //            await trx.RollbackAsync();

        //            var fullError =
        //            ex.InnerException?.Message != null
        //            ? $"{ex.Message} | {ex.InnerException.Message}"
        //            : ex.Message;

        //            return Ok(ApiResponse<object>.Error(
        //                fullError,
        //                "500"
        //            ));
        //        }
        //    });
        //}




        [HttpPost("PrintLaporanRpl")]
        public async Task<IActionResult> PrintLaporanRpl([FromBody] FormPrintDto request)
        {
            try
            {
                // ===============================
                // VALIDASI DASAR
                // ===============================
                if (request.tanggalawal == null || request.tanggalakhir == null)
                {
                    return Ok(new
                    {
                        response = "",
                        metadata = new { message = "Tanggal awal & akhir wajib diisi", code = "201" }
                    });
                }


                // ===============================
                // AMBIL DATA
                // ===============================
                var data = await _order.PrintOrderCpc(
                    request.nowo,
                    request.cabang,
                    request.bank,
                    request.tanggalawal,
                    request.tanggalakhir
                );

                if (data == null || !data.Any())
                {
                    return Ok(new
                    {
                        response = "",
                        metadata = new { message = "Data tidak ditemukan", code = "201" }
                    });
                }

                // ===============================
                // PARAMETER REPORT
                // ===============================
                string periode =
                    $"Periode : {request.tanggalawal:dd-MM-yyyy} s/d {request.tanggalakhir:dd-MM-yyyy}";

                RepOrderPengisianAtm report = new RepOrderPengisianAtm();
                //report.Parameters["periode"].Value = periode;
                report.DataSource = data;
                report.CreateDocument();

                // ===============================
                // EXPORT
                // ===============================
                using MemoryStream ms = new MemoryStream();

                var format = request.format?.ToLower();

                if (format == "excel" || format == "xlsx")
                {
                    report.ExportToXlsx(ms);
                }
                else
                {
                    report.ExportToPdf(ms);
                }

                var base64Output = Convert.ToBase64String(ms.ToArray());

                return Ok(new
                {
                    response = base64Output,
                    metadata = new
                    {
                        message = "Berhasil",
                        code = "200",
                        format = format?.ToUpper() ?? "PDF"
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    response = "",
                    metadata = new
                    {
                        message = ex.InnerException?.Message ?? ex.Message,
                        code = "201"
                    }
                });
            }
        }
        
        /// <summary>
        /// Simpan Order Pengisian Kaset + lock kaset (EMPTY → ON_TRIP)
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> PostOrderCpc([FromBody] OrderPengisianKasetRequest payload)
        //{
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

        //    if (payload.Details == null || payload.Details.Count == 0)
        //        return BadRequest("Detail kaset wajib diisi.");

        //    // 🔐 TRANSACTION
        //    await using var trx = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // 1️⃣ Validasi kaset (semua harus EMPTY)
        //        var kasetIds = payload.Details.Select(d => d.KodeKaset).ToList();

        //        var kasetStocks = await _context.KasetStock
        //            .Where(k => kasetIds.Contains(k.IdKaset))
        //            .ToListAsync();

        //        if (kasetStocks.Count != kasetIds.Count)
        //            return BadRequest("Ada kaset yang tidak ditemukan.");

        //        var notEmpty = kasetStocks.FirstOrDefault(k => k.Status != "EMPTY");
        //        if (notEmpty != null)
        //            return Conflict($"Kaset {notEmpty.IdKaset} tidak berstatus EMPTY.");

        //        // 2️⃣ Simpan HEADER
        //        var order = new OrderPengisianKaset
        //        {
        //            Id = payload.Id,
        //            NomorMesin = payload.NomorMesin,
        //            Lokasi = payload.Lokasi,
        //            MerekMesin = payload.MerekMesin,
        //            KDBANK = payload.KDBANK,
        //            KDCABANG = payload.KDCABANG,
        //            Jumlah = payload.Jumlah,
        //            created = username,
        //            createdat = DateTime.UtcNow,
        //        };

        //        _context.OrderPengisianKaset.Add(order);

        //        // 3️⃣ Simpan DETAIL
        //        foreach (var d in payload.Details)
        //        {
        //            var detail = new OrderPengisianKasetDetail
        //            {
        //                OrderId = order.Id,
        //                Kaset = d.Kaset,
        //                KodeKaset = d.KodeKaset,
        //                NoSeal = d.NoSeal,
        //                Denom = d.Denom,
        //                Lembar = d.Lembar
        //            };

        //            _context.OrderPengisianKasetDetail.Add(detail);
        //        }

        //        // 4️⃣ LOCK KASET → ON_TRIP
        //        foreach (var ks in kasetStocks)
        //        {
        //            ks.Status = "ON_TRIP";
        //            ks.LocationType = "WO";
        //            ks.LocationId = order.Id;
        //            ks.UpdatedAt = DateTime.UtcNow;
        //        }

        //        await _context.SaveChangesAsync();
        //        await trx.CommitAsync();

        //        return Ok(new
        //        {
        //            message = "Order pengisian kaset berhasil disimpan",
        //            orderId = order.Id
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await trx.RollbackAsync();
        //        return StatusCode(500, ex.Message);
        //    }
        //}

    }
}
