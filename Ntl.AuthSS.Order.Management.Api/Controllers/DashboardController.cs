using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ntl.AuthSS.Order_Management.Api.Models;
using Ntl.AuthSS.OrderManagement.Business;
using Ntl.AuthSS.OrderManagement.Business.Services;
using Ntl.AuthSS.OrderManagement.Data.Entities;

namespace Ntl.AuthSS.Order_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IOrderDashboardService _orderDashboardService;
        public DashboardController(IOrderDashboardService orderDashboardService)
        {
            _orderDashboardService = orderDashboardService;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetDashboardOrderDetailsCount()
        {
            var role = GetEntityRoleType();
            int? orgId=0, orgType=0 ;
            switch (role)
            {
                //case Roles.TsspAdmin:
                //case Roles.TsspIntermediate:
                //case Roles.TsspWarehouseIncharge:
                //case Roles.TaxAuthAdmin:
                //case Roles.TaxAuthRevenueOfficer:
                //    orgId = 0;
                //    break;
                case Roles.MfAccountManager:
                case Roles.MfAdmin:
                case Roles.MfWarehouseIncharge:
                    orgType = (int?)OrgType.Manufacturer;
                    orgId = GetEntityOrgId();
                    break;
                case Roles.TpsafAdmin:
                case Roles.TpsafFacilityAdmin:
                case Roles.TpsafFacilityIncharge:
                    orgType = (int?)OrgType.Tpsaf;
                    orgId = GetEntityOrgId();
                    break;
                default:
                    orgId = 0;
                    break;

            }
            return Ok(await _orderDashboardService.GetOrderDetails(orgId,orgType));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetMFSupplierbasedStampDetails()
        {
            var orgId = GetEntityOrgId();
            return Ok(await _orderDashboardService.GetMFStampDetailsForSuppliers(orgId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetMFBrandbasedStampDetails()
        {
            var orgId = GetEntityOrgId();
            return Ok(await _orderDashboardService.GetMFStampDetailsForBrands(orgId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetMFProductandSkubasedStampDetails()
        {
            var orgId = GetEntityOrgId();
            return Ok(await _orderDashboardService.GetMFStampDetailsBasedOnProductSku(orgId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetAdminDashboardPrintOrderDetailsCount()
        {
            return Ok(await _orderDashboardService.GetPrintOrderDetails());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Dashboard")]
        public async Task<IActionResult> GetAdminDashboardPrintOrderStampDetails()
        {
            return Ok(await _orderDashboardService.GetPrintOrderStampDetails());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Admin")]
        public IActionResult GetOrganizationCount()
        {
            return Ok(_orderDashboardService.GetOrganizationCountDetails());
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "Admin")]
        public IActionResult GetTotalPaymentDetails(DBPaymentPagination pagination)
        {
            return Ok(_orderDashboardService.GetTotalPaymentDetails(pagination));
        }

    }
}
