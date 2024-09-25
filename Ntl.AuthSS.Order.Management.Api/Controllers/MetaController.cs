using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize]
    public class MetaController : BaseController
    {
        private readonly IOrderMetaService _orderMetaService;

        public MetaController(IOrderMetaService orderMetaService)
        {
            _orderMetaService = orderMetaService;
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetSuppliers()
        {
            int? orgId = GetEntityOrgId();
            if (GetEntityOrgType() == OrgType.Ntl.ToString() || GetEntityOrgType() == OrgType.TaxAuthority.ToString())
                orgId = null;
            return Ok(_orderMetaService.GetSuppliers(orgId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetProducts()
        {
            var entityType = GetOrderEntityType();
            if (entityType == null)
                return BadRequest("Cannot get products for unknown order enity type");

            var entityId = GetEntityOrgId();
            return Ok(_orderMetaService.GetProducts(entityId, entityType.Value));
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "CanViewOrders")]
        public IActionResult GetOrgBrandProducts()
        {
            var entityType = GetOrderEntityType();
            if (entityType == null)
                return BadRequest("Cannot get BrandProducts for unknown order enity type");

            var entityId = GetEntityOrgId();
            return Ok(_orderMetaService.GetOrgBrandProducts(entityId));
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetProductOrigns()
        {
            var entityType = GetOrderEntityType();
            if (entityType == null)
                return BadRequest("Cannot get products for unknown order enity type");

            var entityId = GetEntityOrgId();

            return Ok(_orderMetaService.GetProductOrigns(entityId, entityType.Value));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetWarehouses()
        {
            var entityId = GetEntityOrgId();
            var entityType = GetOrderEntityType();
            if (entityType == null)
                return BadRequest("Cannot get warehouses for unknown order enity type");

            return Ok(_orderMetaService.GetWarehouses(entityId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetBrandProducts(int productId)
        {
            var entityType = GetOrderEntityType();
            if (entityType != OrderEntityType.Manufacturer)
                return BadRequest("Can obtain brand products only for Manufacturers");
            var entityId = Convert.ToInt32(User.Claims.Single(x => x.Type == "OrgId").Value);

            return Ok(_orderMetaService.GetBrandProducts(entityId, productId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetReelSizes(int productId)
        {
            var entityType = GetOrderEntityType();
            if (entityType == null)
                return BadRequest("Cannot obtain ReelSizes for unknown order entity type");
            return Ok(_orderMetaService.GetReelSizes(productId, entityType.Value));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetSkus(int productId)
        {
            return Ok(_orderMetaService.GetSkus(productId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetStampPrice(int productId)
        {
            return Ok(_orderMetaService.GetStampPrice(productId));
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetShippingCharges()
        {
            return Ok(_orderMetaService.GetOrderShippingCharges());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetTax()
        {
            return Ok(_orderMetaService.GetOrderTax());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetPrintPartners()
        {
            var entityType = GetOrderEntityType();
            if (entityType != OrderEntityType.Ntl)
                return BadRequest("Cannot obtain Print partners for unknown order entity type");

            return Ok(_orderMetaService.GetPrintPartners());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetShippers()
        {
            return Ok(_orderMetaService.GetShippers());
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetNtlWarehouses()
        {
            return Ok(_orderMetaService.GetNtlWarehouses());
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetOrgBalance(int orgId)
        {
            return Ok(_orderMetaService.orgBalance(orgId));
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetTaxSlabs(int status)
        {
            return Ok(_orderMetaService.TaxSlabs(status));
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateOrUpdateTaxslab(TaxSlabDto taxSlab)
        {
            return Ok(_orderMetaService.AddTaxSlab(taxSlab));
        }

    }
}
