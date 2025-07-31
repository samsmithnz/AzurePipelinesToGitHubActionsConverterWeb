using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PipelinesToActionsWeb.Controllers;
using PipelinesToActionsWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

            // Setup MVC services for proper controller testing
            var services = new ServiceCollection();
            services.AddMvc();
            services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();
            var serviceProvider = services.BuildServiceProvider();

            // Setup HttpContext for Error method and MVC services
            var mockHttpContext = Substitute.For<HttpContext>();
            mockHttpContext.TraceIdentifier.Returns("test-trace-id");
            mockHttpContext.RequestServices.Returns(serviceProvider);

            // Setup RouteData for URL generation
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Index";

            // Create a proper ControllerActionDescriptor
            var controllerActionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = "Home",
                ActionName = "Index",
                ControllerTypeInfo = typeof(HomeController).GetTypeInfo()
            };

            var actionContext = new ActionContext(mockHttpContext, routeData, controllerActionDescriptor);

            _controller.ControllerContext = new ControllerContext(actionContext);

            // Initialize TempData with the factory
            var tempDataFactory = serviceProvider.GetService<ITempDataDictionaryFactory>();
            if (tempDataFactory != null)
            {
                _controller.TempData = tempDataFactory.GetTempData(mockHttpContext);
            }
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

        [TestMethod]
        public void ASPDotNetFrameworkExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.ASPDotNetFrameworkExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void ASPDotNetFrameworkExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.ASPDotNetFrameworkExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void NodeExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.NodeExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void CDExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.CDExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void CDExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.CDExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void CICDExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.CICDExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void CICDExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.CICDExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void DockerExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.DockerExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void DockerExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.DockerExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void AntExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.AntExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void AntExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.AntExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void GradleExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.GradleExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void GradleExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.GradleExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void MavenExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.MavenExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void MavenExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.MavenExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void PythonExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.PythonExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void RubyExample_WithValidModel_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.RubyExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void RubyExample_WithInvalidModel_RedirectsToIndex()
        {
            //Arrange
            _controller.ModelState.AddModelError("TestError", "Test validation error");

            //Act
            var result = _controller.RubyExample();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public void Privacy_ReturnsView()
        {
            //Arrange

            //Act
            var result = _controller.Privacy();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Error_ReturnsViewWithErrorModel()
        {
            //Arrange

            //Act
            var result = _controller.Error();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ErrorViewModel));
        }

        [TestMethod]
        public void Index_Post_WithNullInput_ProcessesCorrectly()
        {
            //Arrange
            string testYaml = null;
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
        public void Index_Post_WithEmptyInput_ProcessesCorrectly()
        {
            //Arrange
            string testYaml = "";
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
        public void Index_Post_WithInvalidYaml_HandlesYamlException()
        {
            //Arrange
            string testYaml = "invalid: yaml: [unclosed bracket";
            bool addWorkflowDispatch = false;

            //Act
            var result = _controller.Index(testYaml, addWorkflowDispatch);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
        }
    }
}