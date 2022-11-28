﻿using Com.Efrata.Service.Inventory.Lib.Models.InventoryWeavingModel;
using Com.Efrata.Service.Inventory.Lib.Services;
using Com.Efrata.Service.Inventory.Lib.Services.InventoryWeaving;
using Com.Efrata.Service.Inventory.Lib.ViewModels.InventoryWeavingViewModel;
using Com.Efrata.Service.Inventory.WebApi.Controllers.v1.WeavingInventory;
using Com.Efrata.Service.Inventory.Test.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Efrata.Service.Inventory.Test.Controllers.InventoryWeavingReport.Reports.WeavingInventoryRecap
{
    public class WeavingInventoryRecapControllerTest
    {
        protected Lib.Models.InventoryWeavingModel.InventoryWeavingMovement Model
        {
            get { return new Lib.Models.InventoryWeavingModel.InventoryWeavingMovement(); }
        }

        protected InventoryWeavingInOutViewModel ViewModel
        {
            get { return new InventoryWeavingInOutViewModel(); }
        }

        protected ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        protected (Mock<IIdentityService> IdentityService, Mock<IValidateService> ValidateService, Mock<IInventoryWeavingMovementService> service) GetMocks()
        {
            return (IdentityService: new Mock<IIdentityService>(), ValidateService: new Mock<IValidateService>(), service: new Mock<IInventoryWeavingMovementService>());
        }

        protected WeavingInventoryRecapController GetController((Mock<IIdentityService> IdentityService, Mock<IValidateService> ValidateService, Mock<IInventoryWeavingMovementService> service) mocks)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            WeavingInventoryRecapController controller = (WeavingInventoryRecapController)Activator.CreateInstance(typeof(WeavingInventoryRecapController), mocks.IdentityService.Object, mocks.ValidateService.Object, mocks.service.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Get_Report_Success_GetReportRecap()
        {
            var mocks = GetMocks();
            mocks.service.Setup(f => f.ReadReportRecap(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new Tuple<List<InventoryWeavingInOutViewModel>, int>(new List<InventoryWeavingInOutViewModel>(), 1));
            var controller = GetController(mocks);
            var response = controller.GetReport("{}", DateTime.MinValue, DateTime.Now, 1, 25, "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Get_Report_Fail_GetReportRecap()
        {
            var mocks = GetMocks();
            mocks.service.Setup(f => f.ReadReportRecap(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception());
            var controller = GetController(mocks);
            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/json";
            var response = controller.GetReport("()", DateTime.MinValue, DateTime.Now, 1, 25, "{}");
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void GetXls_Report_Success_GetReportRecap()
        {
            var mocks = GetMocks();
            mocks.service.Setup(f => f.GenerateExcelRecap(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MemoryStream());
            var controller = GetController(mocks);
            var response = controller.GetXlsAll("{}", DateTime.MinValue, DateTime.Now);
            Assert.NotNull(response);
        }

        [Fact]
        public void GetXls_Report_Fail_GetReportRecap()
        {
            var mocks = GetMocks();
            mocks.service.Setup(f => f.GenerateExcelRecap(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Throws(new Exception());
            var controller = GetController(mocks);
            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/json";
            var response = controller.GetXlsAll("{}", DateTime.MinValue, DateTime.Now);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}