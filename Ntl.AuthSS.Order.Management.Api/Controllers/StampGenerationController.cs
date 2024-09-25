using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StampGenerationController : BaseController
    {
        private readonly IStampGenerationService _stampGenerationService;
        private readonly IConfiguration _configuration;
        public StampGenerationController(IStampGenerationService stampGenerationService, IConfiguration configuration)
        {
            _stampGenerationService = stampGenerationService;
            _configuration = configuration;
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetStampGenerate(StampGenerationFilterDto filter)
        {
            return Ok(_stampGenerationService.GetStampsGenerate(filter));
        }
        [HttpPost]
        [Route("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DownloadFilteredStampsGeneration(StampGenerationFilterDto filter)
        {
            var Data = _stampGenerationService.GetFilteredStampGenerationDownloadList(filter);
            var result = Data.Select(x => new { PrintOrderNumber = x.PrintOrderNumber, RequestedDate = x.RequestedDate, Status = x.Status }).ToList();

            if (result == null)
            {
                return BadRequest("not found");
            }
            else
            {
                using (var excelFile = new ExcelPackage())
                {
                    var worksheet = excelFile.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].LoadFromCollection(Collection: result, PrintHeaders: true);
                    worksheet.Column(2).Style.Numberformat.Format = "dd-mmm-yyyy";
                    var streamedData = new MemoryStream(excelFile.GetAsByteArray());
                    return File(streamedData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StampGenerationList_" + DateTime.Now.Ticks.ToString());
                }
            }
        }
    }
}
