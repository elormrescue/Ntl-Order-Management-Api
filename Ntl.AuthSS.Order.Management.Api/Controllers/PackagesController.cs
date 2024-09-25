using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : BaseController
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService, IOrderMetaService orderMetaService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "OrderFulfillment")]
        public IActionResult GetCoilDetailsForFulfillment(string reelCode, Guid orderItemId)
        {
            var result = _packageService.GetReelDetailsFromCodeForFullfillment(reelCode, orderItemId);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            if (result.reel.IsUsed == true)
            {
                return BadRequest("Reel is already used");
            }
            else
            {
                return Ok(result.reel);
            }
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "OrderFulfillment")]
        public IActionResult GetCartonsDetailsForFulfillment(string cartonCode, Guid orderItemId)
        {
            var result = _packageService.GetCartonDetailsFromCodeForFulfillment(cartonCode, orderItemId);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            if (result.carton.Reels.Where(a => a.IsUsed == false).Count() == 0)
            {
                return BadRequest("This Carton is already used");
            }
            else
            {
                return Ok(result.carton);
            }
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ReturnOrderFulfillment")]
        public IActionResult GetReelDetailsForOrderReturn(string reelCode)
        {
            var mfId = User.Claims.Single(x => x.Type == "OrgId").Value;
            var result = _packageService.GetReelDetailsFromCodeForReturnOrder(reelCode, Convert.ToInt32(mfId));
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            return Ok(result.reel);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ReturnOrderFulfillment")]
        public IActionResult GetCartonDetailsForOrderReturn(string cartonCode)
        {
            var mfId = User.Claims.Single(x => x.Type == "OrgId").Value;
            var result = _packageService.GetCartonDetailsFromCodeForReturnOrder(cartonCode, Convert.ToInt32(mfId));
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            return Ok(result.carton);
        }


        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Policy = "ViewPackageTypes")]
        public IActionResult GetPackageTypes()
        {
            var result = Enum.GetValues(typeof(PackageType)).Cast<PackageType>().ToDictionary(t => ((int)t).ToString(), t => t.ToString());
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewPackageTypes")]
        public IActionResult TraceReel(string reelCode)
        {
            var result = _packageService.TraceReel(reelCode);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            return Ok(result.reel);
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewPackageTypes")]
        public IActionResult TraceCarton(string cartonCode)
        {
            var result = _packageService.TraceCarton(cartonCode);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            return Ok(result.carton);
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ViewPackageTypes")]
        public IActionResult TracePallet(string palletCode)
        {
            var result = _packageService.TracePallet(palletCode);
            if (result.errorMessage != null)
                return BadRequest(result.errorMessage);
            return Ok(result.pallet);
        }


    }
}
