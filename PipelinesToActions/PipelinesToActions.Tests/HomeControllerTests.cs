using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PipelinesToActionsWeb.Controllers;
using System.ComponentModel.DataAnnotations;

namespace PipelinesToActions.Tests
{
    [TestClass]
    public class HomeControllerTests
    {
        private HomeController _controller = null!;
        private ILogger<HomeController> _logger = null!;
        private TelemetryClient _telemetryClient = null!;

        [TestInitialize]
        public void Setup()
        {
            _logger = new NullLogger<HomeController>();
            _telemetryClient = new TelemetryClient();
            _controller = new HomeController(_logger, _telemetryClient);
        }

        [TestMethod]
        public void Index_Get_ReturnsViewWithEmptyModel()
        {
            //Arrange

            //Act
            var result = _controller.Index();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
        }

        [TestMethod]
        public void Index_Post_WithValidModel_ProcessesConversion()
        {
            //Arrange
            string testYaml = "trigger:\n- main\n\npool:\n  vmImage: 'ubuntu-latest'\n\nsteps:\n- script: echo Hello world";
            bool addWorkflowDispatch = false;

            //Act
            var result = _controller.Index(testYaml, addWorkflowDispatch);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
        }

        [TestMethod]
        public void Index_Post_WithInvalidModel_ReturnsEmptyResult()
        {
            //Arrange
            string testYaml = "valid yaml";
            bool addWorkflowDispatch = false;
            
            // Simulate invalid model state
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.Index(testYaml, addWorkflowDispatch);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            // The model should be an empty ConversionResponse when ModelState is invalid
        }

        [TestMethod]
        public void CIExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.CIExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void CIExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.CIExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void DotNetFrameworkDesktopExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.DotNetFrameworkDesktopExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void DotNetFrameworkDesktopExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.DotNetFrameworkDesktopExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void NodeExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.NodeExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void PythonExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.PythonExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }
    }
}