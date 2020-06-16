using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using ASC.Web;
using ASC.Web.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using ASC.Utilities;
using Microsoft.AspNetCore.Http;
using ASC.Tests.TestUtilities;

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> mockOptions;
        private readonly Mock<ILogger<HomeController>> mockLogger;
        private readonly Mock<HttpContext> mockHttpContext;
        public HomeControllerTests()
        {
            mockOptions = new Mock<IOptions<ApplicationSettings>>();
            mockOptions.Setup(op => op.Value).Returns(new ApplicationSettings() { ApplicationTitle = "ASC Tets" });

            mockLogger = new Mock<ILogger<HomeController>>();

            mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(new FakeSession());
        }

        [Fact]
        public void HomeController_ViewResult_Test()
        {
            var controller = new HomeController(mockLogger.Object, mockOptions.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;
            Assert.IsType<ViewResult>((controller.Index() as ViewResult));
        }

        [Fact]
        public void HomeController_NoModel_Test()
        {
            var controller = new HomeController(mockLogger.Object, mockOptions.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;
            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Validation_Test()
        {
            var controller = new HomeController(mockLogger.Object, mockOptions.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;
            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        [Fact]
        public void HomeController_Index_Session_Test()
        {
            var controller = new HomeController(mockLogger.Object, mockOptions.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;
            controller.Index();
            // Session value with key "Test" should not be null.
            Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));
        }
    }
}
