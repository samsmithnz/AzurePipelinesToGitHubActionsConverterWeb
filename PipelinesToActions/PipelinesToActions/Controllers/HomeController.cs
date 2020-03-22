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
        public IActionResult CIExample()
        {
            string yaml = Samples.CISample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CDExample()
        {
            string yaml = Samples.CDSample();
            ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CICDExample()
        {

            string yaml = Samples.CICDSample(); ConversionResponse gitHubResult = ProcessConversion(yaml);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult ContainerExample()
        {
            string yaml = Samples.ContainerSample();
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
