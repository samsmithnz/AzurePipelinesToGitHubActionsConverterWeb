using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelinesToActionsWeb.Models;
using System;
using System.Diagnostics;

namespace PipelinesToActionsWeb.Controllers
{
    /// <summary>
    /// This controller follows the KISS principal. It's not always pretty, but HTTP server side posts was the quickest way to implement this functionality
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ConversionResponse gitHubResult = new ConversionResponse();
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpPost]
        public IActionResult Index(string txtAzurePipelinesYAML)
        {
            ConversionResponse gitHubResult = ProcessConversion(txtAzurePipelinesYAML);

            return View(model: gitHubResult);
        }

        private ConversionResponse ProcessConversion(string input)
        {
            if (string.IsNullOrEmpty(input) == false)
            {
                input = input.TrimStart().TrimEnd();
            }

            //process the yaml
            ConversionResponse gitHubResult;
            try
            {
                Conversion conversion = new Conversion();
                gitHubResult = conversion.ConvertAzurePipelineToGitHubAction(input);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Error processing YAML, it's likely the original YAML is not valid" + Environment.NewLine +
                    "Original error message: " + ex.ToString(),
                    pipelinesYaml = input
                };
            }
            catch (Exception ex)
            {
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Unexpected error: " + ex.ToString(),
                    pipelinesYaml = input
                };
            }

            //Return the result
            if (gitHubResult != null)
            {
                return gitHubResult;
            }
            else
            {
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Unknown error",
                    pipelinesYaml = input
                };
                return gitHubResult;
            }
        }



        [HttpGet]
        [HttpPost]
        public IActionResult ASPDotNetCoreSimpleExample()
        {
            string yaml = Examples.ASPDotNetCoreSimpleExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult DotNetFrameworkDesktopExample()
        {
            string yaml = Examples.DotNetFrameworkDesktopExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult ASPDotNetFrameworkExample()
        {
            string yaml = Examples.ASPDotNetFrameworkExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult NodeExample()
        {
            string yaml = Examples.NodeExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }


        [HttpGet]
        [HttpPost]
        public IActionResult CIExample()
        {
            string yaml = Examples.CIExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CDExample()
        {
            string yaml = Examples.CDExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CICDExample()
        {
            string yaml = Examples.CICDExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult ContainerExample()
        {
            string yaml = Examples.ContainerExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult AntExample()
        {
            string yaml = Examples.AntExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GradleExample()
        {
            string yaml = Examples.GradleExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult MavenExample()
        {
            string yaml = Examples.MavenExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult PythonExample()
        {
            string yaml = Examples.PythonExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult RubyExample()
        {
            string yaml = Examples.RubyExample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
