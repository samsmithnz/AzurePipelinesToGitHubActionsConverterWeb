using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.ApplicationInsights;
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
        private readonly TelemetryClient _telemetry;

        public HomeController(ILogger<HomeController> logger, TelemetryClient telemetry)
        {
            _logger = logger;
            _telemetry = telemetry;
        }

        [HttpGet]
        [HttpHead]
        public IActionResult Index()
        {
            ConversionResponse gitHubResult = new ConversionResponse();
            return View(viewName: "Index", model: (gitHubResult, false));
        }

        [HttpPost]
        public IActionResult Index(string txtAzurePipelinesYAML, bool chkAddWorkflowDispatch)
        {
            if (!ModelState.IsValid)
            {
                // If model state is invalid, return to the form with an empty result
                ConversionResponse emptyResult = new ConversionResponse();
                return View(model: (emptyResult, chkAddWorkflowDispatch));
            }

            (ConversionResponse, bool) gitHubResult = ProcessConversion(txtAzurePipelinesYAML, chkAddWorkflowDispatch);

            return View(model: gitHubResult);
        }

        private (ConversionResponse, bool) ProcessConversion(string input, bool chkAddWorkflowDispatch = false)
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
                gitHubResult = conversion.ConvertAzurePipelineToGitHubAction(input, chkAddWorkflowDispatch);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                //if the YAML conversion failed - highlight that.
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Error processing YAML, it's likely the original YAML is not valid" + Environment.NewLine +
                    "Original error message: " + ex.ToString(),
                    pipelinesYaml = input
                };
            }
            catch (Exception ex)
            {
                //Otherwise something else unexpected and bad happened
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Unexpected error: " + ex.ToString(),
                    pipelinesYaml = input
                };
            }

            //Return the result
            if (gitHubResult != null)
            {
                if (gitHubResult.comments != null)
                {
                    //Log conversion task errors to application insights to track tasks that can't convert
                    //We are only capturing the task name and frequency to help with prioritization - no YAML is to be captured!
                    foreach (string comment in gitHubResult.comments)
                    {
                        if (comment.IndexOf("' does not have a conversion path yet") >= 0)
                        {
                            //Log as exception to Application Insights
                            string task = comment.Replace("#Error: the step '", "").Replace("' does not have a conversion path yet", "");
                            _telemetry.TrackException(new Exception("Unknown Task: " + task));
                        }
                    }
                }
                return (gitHubResult, chkAddWorkflowDispatch);
            }
            else
            {
                gitHubResult = new ConversionResponse
                {
                    actionsYaml = "Unknown error",
                    pipelinesYaml = input
                };
                return (gitHubResult, chkAddWorkflowDispatch);
            }
        }


        //[HttpGet]
        //[HttpPost]
        //public IActionResult ASPDotNetCoreSimpleExample(bool chkAddWorkflowDispatch = false)
        //{
        //    string yaml = Examples.ASPDotNetCoreSimpleExample();
        //    (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
        //    return View(viewName: "Index", model: gitHubResult);
        //}

        [HttpGet]
        [HttpPost]
        public IActionResult DotNetFrameworkDesktopExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.DotNetFrameworkDesktopExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult ASPDotNetFrameworkExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.ASPDotNetFrameworkExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult NodeExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.NodeExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CIExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.CIExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CDExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.CDExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult CICDExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.CICDExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult DockerExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.DockerExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult AntExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.AntExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GradleExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.GradleExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult MavenExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.MavenExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult PythonExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.PythonExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpGet]
        [HttpPost]
        public IActionResult RubyExample(bool chkAddWorkflowDispatch = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            string yaml = Examples.RubyExample();
            (ConversionResponse, bool) gitHubResult = ProcessConversion(yaml, chkAddWorkflowDispatch);
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
