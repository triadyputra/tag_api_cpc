using ClosedXML.Excel;
using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Helper;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.MasterData.MasterKaset;
using cpcApi.Model.MasterData;
using cpcApi.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.MasterData
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterKasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterKasetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListMasterKaset")]
        public async Task<ActionResult<PaginatedResponse<ViewMasterKasetDto>>> GetListMasterKaset(
            [FromQuery] string? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                IQueryable<MasterKaset> query = _context.MasterKaset
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var like = $"%{filter}%";

                    query = query.Where(x =>
                        EF.Functions.Like(x.NoSerial, like) ||
                        EF.Functions.Like(x.KdBank, like) ||
                        EF.Functions.Like(x.NmBank, like)
                    );
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.NoSerial)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewMasterKasetDto
                    {
                        KdKaset = x.KdKaset,
                        NoSerial = x.NoSerial,

                        KdBank = x.KdBank,
                        NmBank = x.NmBank,

                        KdMerek = x.KdMerek,
                        NmMerek = x.Merek != null ? x.Merek.NmMerek : "-",

                        Tipe = x.Tipe,
                        Jenis = x.Jenis,
                        StatusFisik = x.StatusFisik,
                        KdCabang = x.KdCabang
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewMasterKasetDto>
                {
                    Data = items,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FormMasterKasetDto>>> GetById(string id)
        {
            var kaset = await _context.MasterKaset
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.KdKaset == id);

            if (kaset == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            return Ok(ApiResponse<FormMasterKasetDto>.Success(new FormMasterKasetDto
            {
                KdKaset = kaset.KdKaset,
                KdBank = kaset.KdBank,
                NmBank = kaset.NmBank,
                KdMerek = kaset.KdMerek,
                Tipe = kaset.Tipe,
                Jenis = kaset.Jenis,
                NoSerial = kaset.NoSerial,
                StatusFisik = kaset.StatusFisik,
                KdCabang = kaset.KdCabang
            }));
        }

        // =====================================================
        // CREATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostMasterKaset([FromBody] FormMasterKasetDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NoSerial))
                    return Ok(ApiResponse<object>.Error("NoSerial wajib diisi", "400"));

                var exists = await _context.MasterKaset
                    .AnyAsync(x => x.NoSerial == dto.NoSerial);

                if (exists)
                    return Ok(ApiResponse<object>.Error("NoSerial sudah terdaftar", "400"));

               
                var kaset = new MasterKaset
                {
                    KdKaset = await HelperApp.GenerateKdKasetBankAsync(_context, dto.KdBank), ////dto.KdKasetBank,
                    KdBank = dto.KdBank,
                    NmBank = dto.NmBank,
                    KdMerek = dto.KdMerek,
                    Tipe = dto.Tipe,
                    Jenis = dto.Jenis,
                    NoSerial = dto.NoSerial,
                    StatusFisik = dto.StatusFisik,
                    KdCabang = dto.KdCabang
                };

                _context.MasterKaset.Add(kaset);
                await _context.SaveChangesAsync();


                // 2️⃣ AUTO CREATE KASET STOCK
                var kasetStock = new KasetStock
                {
                    KdKaset = kaset.KdKaset,     // ✅ BENAR
                    Status = "EMPTY",
                    LocationType = "VAULT",
                    LocationId = dto.KdCabang,   // ✅ BENAR
                    UpdatedAt = DateTime.UtcNow
                };

                _context.KasetStock.Add(kasetStock);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData());
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutMasterKaset(string id, [FromBody] FormMasterKasetDto dto)
        {
            try
            {
                var kaset = await _context.MasterKaset
                    .FirstOrDefaultAsync(x => x.KdKaset == id);

                if (kaset == null)
                    return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

                //kaset.KdKasetBank = dto.KdKasetBank;
                kaset.KdMerek = dto.KdMerek;
                kaset.Tipe = dto.Tipe;
                kaset.Jenis = dto.Jenis;
                kaset.StatusFisik = dto.StatusFisik;
                kaset.KdCabang = dto.KdCabang;
                kaset.NoSerial = dto.NoSerial;
                kaset.KdBank = dto.KdBank;
                kaset.NmBank = dto.NmBank;
               
                // ⚠️ BANK TIDAK BOLEH DIUBAH SEMBARANGAN
                // kaset.KdBank & NmBank TIDAK DIUPDATE


                // 2️⃣ AUTO CREATE KASET STOCK
                //var kasetStock = new KasetStock
                //{
                //    IdKaset = kaset.IdKaset,     // ✅ BENAR
                //    Status = "EMPTY",
                //    LocationType = "VAULT",
                //    LocationId = dto.KdCabang,   // ✅ BENAR
                //    UpdatedAt = DateTime.UtcNow
                //};

                //_context.KasetStock.Add(kasetStock);

                await _context.SaveChangesAsync();


                return Ok(ApiResponse<object>.SuccessNoData());
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // DELETE (HARD DELETE – DEV ONLY / ADMIN)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMasterKaset(string id)
        {
            var kaset = await _context.MasterKaset
                .FirstOrDefaultAsync(x => x.KdKaset == id);

            if (kaset == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            _context.MasterKaset.Remove(kaset);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData("Data berhasil dihapus"));
        }

        //[ApiKeyAuthorize]
        [HttpPost("PrintBarcodeKaset")]
        public async Task<IActionResult> PrintBarcodeKaset(
        [FromBody] PrintKasetRequest req)
        {
            try
            {


                if (req.Ids == null || !req.Ids.Any())
                return BadRequest("Data kaset belum dipilih");

                var data = await _context.MasterKaset
                    .AsNoTracking()
                    .Where(x => req.Ids.Contains(x.KdKaset))
                    .OrderBy(x => x.KdKaset)
                    .Select(x => new
                    {
                        x.KdKaset,
                        x.NoSerial,
                        x.KdBank,
                        x.NmBank,
                        x.KdMerek,
                        x.Tipe,
                        x.Jenis
                    })
                    .ToListAsync();

                if (!data.Any())
                    return BadRequest("Data kaset tidak ditemukan");

                // ===============================
                // VALIDASI 1 BANK SAJA
                // ===============================
                var distinctBank = data
                    .Select(x => x.KdBank)
                    .Distinct()
                    .ToList();

                if (distinctBank.Count > 1)
                    return BadRequest("Print hanya boleh untuk satu bank");

                var kdBank = distinctBank.First();

                // ===============================
                // GENERATE REPORT
                // ===============================
                var report = new RepBarcodeKaset();
                report.DataSource = data;

                using var ms = new MemoryStream();
                report.ExportToPdf(ms);

                var base64Output = Convert.ToBase64String(ms.ToArray());
                //return File(
                //    ms.ToArray(),
                //    "application/pdf",
                //    $"Label_Kaset_{kdBank}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                //);

                return Ok(new
                {
                    response = base64Output,
                    metadata = new
                    {
                        message = "Berhasil",
                        code = "200",
                        format = "PDF"
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

        #region upload-data
        [ApiKeyAuthorize]
        [HttpGet("DownloadTemplate")]
        public IActionResult DownloadTemplate()
        {
            using var wb = new XLWorkbook();

            // =====================================================
            // SHEET UTAMA
            // =====================================================
            var ws = wb.Worksheets.Add("MasterKaset");

            // HEADER
            ws.Cell("A1").Value = "KdBank";
            ws.Cell("B1").Value = "NmBank";
            ws.Cell("C1").Value = "KdMerek";
            ws.Cell("D1").Value = "Tipe";
            ws.Cell("E1").Value = "Jenis";
            ws.Cell("F1").Value = "NoSerial";
            ws.Cell("G1").Value = "StatusFisik";
            ws.Cell("H1").Value = "KdCabang";

            ws.Range("A1:H1").Style.Font.Bold = true;
            ws.Range("A1:H1").Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.SheetView.FreezeRows(1);

            // =====================================================
            // SAMPLE DATA (ROW 2)
            // =====================================================
            ws.Cell("A2").Value = "BCA";
            ws.Cell("B2").Value = "BANK CENTRAL ASIA";
            ws.Cell("C2").Value = "NCR";
            ws.Cell("D2").Value = "S2";
            ws.Cell("E2").Value = "DISPENSE";
            ws.Cell("F2").Value = "SN-ATM-001";
            ws.Cell("G2").Value = "GOOD";
            ws.Cell("H2").Value = "JKT";

            // =====================================================
            // SHEET REFERENSI (HIDDEN)
            // =====================================================
            var refSheet = wb.Worksheets.Add("REF");

            // REF JENIS
            refSheet.Cell("A1").Value = "DISPENSE";
            refSheet.Cell("A2").Value = "REJECT";
            refSheet.Cell("A3").Value = "RETRACT";

            // REF STATUS FISIK
            refSheet.Cell("B1").Value = "GOOD";
            refSheet.Cell("B2").Value = "DAMAGED";
            refSheet.Cell("B3").Value = "REPAIR";

            // VERY HIDDEN (tidak bisa di-unhide manual)
            refSheet.Visibility = XLWorksheetVisibility.VeryHidden;

            // =====================================================
            // DROPDOWN VALIDATION
            // =====================================================

            // 🔽 JENIS (Column E)
            var jenisRange = ws.Range("E2:E1000");
            var jenisValidation = jenisRange.CreateDataValidation();
            jenisValidation.IgnoreBlanks = true;
            jenisValidation.InCellDropdown = true;
            jenisValidation.AllowedValues = XLAllowedValues.List;
            jenisValidation.List(refSheet.Range("A1:A3"));

            // 🔽 STATUS FISIK (Column G)
            var statusRange = ws.Range("G2:G1000");
            var statusValidation = statusRange.CreateDataValidation();
            statusValidation.IgnoreBlanks = true;
            statusValidation.InCellDropdown = true;
            statusValidation.AllowedValues = XLAllowedValues.List;
            statusValidation.List(refSheet.Range("B1:B3"));

            // =====================================================
            // AUTO WIDTH
            // =====================================================
            ws.Columns().AdjustToContents();

            // =====================================================
            // RETURN FILE
            // =====================================================
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Template_MasterKaset.xlsx"
            );
        }

        private byte[] GenerateErrorExcel(List<UploadKasetResultDto> errors)
        {
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Upload_Error");

            ws.Cell(1, 1).Value = "Row Excel";
            ws.Cell(1, 2).Value = "NoSerial";
            ws.Cell(1, 3).Value = "Error Message";

            ws.Range("A1:C1").Style.Font.Bold = true;
            ws.Range("A1:C1").Style.Fill.BackgroundColor =
                ClosedXML.Excel.XLColor.LightPink;

            int row = 2;
            foreach (var err in errors)
            {
                ws.Cell(row, 1).Value = err.Row;
                ws.Cell(row, 2).Value = err.NoSerial;
                ws.Cell(row, 3).Value = err.Message;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        [ApiKeyAuthorize]
        [HttpPost("UploadKaset")]
        public async Task<IActionResult> UploadKaset(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Ok(ApiResponse<object>.Error("File tidak ditemukan", "400"));

            var results = new List<UploadKasetResultDto>();

            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet("MasterKaset");

            var rows = ws.RangeUsed()
                .RowsUsed()
                .Skip(1)
                .ToList();

            var groupedByBank = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Cell(1).GetString()))
                .GroupBy(r => r.Cell(1).GetString());

            var strategy = _context.Database.CreateExecutionStrategy();

            foreach (var bankGroup in groupedByBank)
            {
                var kdBank = bankGroup.Key;
                var bankRows = bankGroup.ToList();

                var generatedCodes = await HelperApp
                    .GenerateBulkKdKasetBankAsync(_context, kdBank, bankRows.Count);

                int index = 0;

                foreach (var row in bankRows)
                {
                    var rowNo = row.RowNumber();
                    var noSerial = row.Cell(6).GetString();

                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var tx = await _context.Database.BeginTransactionAsync();

                        try
                        {
                            if (string.IsNullOrWhiteSpace(noSerial))
                                throw new Exception("NoSerial kosong");

                            if (await _context.MasterKaset.AnyAsync(x => x.NoSerial == noSerial))
                                throw new Exception("NoSerial sudah terdaftar");

                            var kaset = new MasterKaset
                            {
                                KdKaset = generatedCodes[index++],
                                KdBank = kdBank,
                                NmBank = row.Cell(2).GetString(),
                                KdMerek = row.Cell(3).GetString(),
                                Tipe = row.Cell(4).GetString(),
                                Jenis = row.Cell(5).GetString(),
                                NoSerial = noSerial,
                                StatusFisik = row.Cell(7).GetString(),
                                KdCabang = row.Cell(8).GetString()
                            };

                            _context.MasterKaset.Add(kaset);
                            await _context.SaveChangesAsync();

                            _context.KasetStock.Add(new KasetStock
                            {
                                KdKaset = kaset.KdKaset,
                                Status = "EMPTY",
                                LocationType = "VAULT",
                                LocationId = kaset.KdCabang,
                                UpdatedAt = DateTime.UtcNow
                            });

                            await _context.SaveChangesAsync();
                            await tx.CommitAsync();

                            results.Add(new UploadKasetResultDto
                            {
                                Row = rowNo,
                                NoSerial = noSerial,
                                Success = true,
                                Message = "OK"
                            });
                        }
                        catch (Exception ex)
                        {
                            await tx.RollbackAsync();

                            results.Add(new UploadKasetResultDto
                            {
                                Row = rowNo,
                                NoSerial = noSerial,
                                Success = false,
                                Message = ex.InnerException?.Message ?? ex.Message
                            });
                        }
                    });
                }
            }

            // ===============================
            // JIKA ADA ERROR → DOWNLOAD EXCEL
            // ===============================
            var errors = results.Where(x => !x.Success).ToList();

            if (errors.Any())
            {
                using var errWb = new XLWorkbook();
                var errWs = errWb.Worksheets.Add("Upload_Error");

                errWs.Cell(1, 1).Value = "Row Excel";
                errWs.Cell(1, 2).Value = "NoSerial";
                errWs.Cell(1, 3).Value = "Error Message";

                errWs.Range("A1:C1").Style.Font.Bold = true;
                errWs.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightPink;

                int r = 2;
                foreach (var err in errors)
                {
                    errWs.Cell(r, 1).Value = err.Row;
                    errWs.Cell(r, 2).Value = err.NoSerial;
                    errWs.Cell(r, 3).Value = err.Message;
                    r++;
                }

                errWs.Columns().AdjustToContents();

                using var errStream = new MemoryStream();
                errWb.SaveAs(errStream);

                return File(
                    errStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Upload_MasterKaset_Error_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                );
            }

            return Ok(ApiResponse<object>.SuccessNoData("Upload berhasil"));
        }



        #endregion



    }
}
