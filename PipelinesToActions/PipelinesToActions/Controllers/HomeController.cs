using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelinesToActionsWeb.Models;
using AzurePipelinesToGitHubActionsConverter.Core;

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
            ConversionResult gitHubResult = new ConversionResult();
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpPost]
        public IActionResult Index(string txtAzurePipelinesYAML)
        {
            ConversionResult gitHubResult;
            try
            {
                Conversion conversion = new Conversion();
                gitHubResult = conversion.ConvertAzurePipelineToGitHubAction(txtAzurePipelinesYAML);
                gitHubResult.pipelinesYaml = txtAzurePipelinesYAML; //TODO: Move this into the Conversion module
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                gitHubResult = new ConversionResult
                {
                    actionsYaml = "Error processing YAML, it's likely the original YAML is not valid" + Environment.NewLine +
                    "Original error message: " + ex.Message,
                    pipelinesYaml = txtAzurePipelinesYAML
                };
            }
            catch (Exception ex)
            {
                gitHubResult = new ConversionResult
                {
                    actionsYaml = "Unexpected error: " + ex.ToString(),
                    pipelinesYaml = txtAzurePipelinesYAML
                };
            }

            //Return the result
            if (gitHubResult != null)
            {
                return View(model: gitHubResult);
            }
            else
            {
                gitHubResult = new ConversionResult
                {
                    actionsYaml = "Unknown error",
                    pipelinesYaml = txtAzurePipelinesYAML
                };
                return View(model: gitHubResult);
            }
        }

        [HttpPost]
        public IActionResult CIExample()
        {
            string yaml = @"
name: ""Feature Flags CI""

on: [push]

jobs:
  build:

    runs-on: windows-latest
      
    steps:
    # checkout the repo
    - uses: actions/checkout@v1
     
    # install dependencies, build, and test
    - name: Setup Dotnet for use with actions
      uses: actions/setup-dotnet@v1.0.0
    
    #Build and test service  
    - name: Run automated unit and integration tests
      run: dotnet build MyProject/MyProject.Tests/MyProject.Service.csproj --configuration Release
    - name: Run automated unit and integration tests
      run: dotnet test MyProject/MyProject.Tests/MyProject.Tests.csproj --configuration Release

    #Publish dotnet objects
    - name: DotNET Publish Web Service
      run: dotnet publish MyProject/MyProject.Service/MyProject.Service.csproj --configuration Release 
    
    #Publish build artifacts to GitHub
    - name: Upload web service build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: serviceapp
        path: MyProject/MyProject.Service/bin/Release/netcoreapp3.0/publish
";

            ConversionResult gitHubResult = new ConversionResult
            {
                actionsYaml = yaml
            };
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpPost]
        public IActionResult CDExample()
        {
            string yaml = @"
name: ""Feature Flags CD""

on: [push]

jobs:
  #Deploy the artifacts to Azure
  deploy:
    runs-on: windows-latest

    needs: build
        
    steps:        
    # Login with the secret SP details
    - name: Log into Azure
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}  
    
    #Download the artifacts from GitHub
    - name: Download serviceapp artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: serviceapp
    
    #Deploy service to Azure staging slots
    - name: Deploy web service to Azure WebApp
      uses: Azure/webapps-deploy@v1
      with:
        app-name: MyProject-data-eu-service
        package: serviceapp  
  
";

            ConversionResult gitHubResult = new ConversionResult
            {
                actionsYaml = yaml
            };
            return View(viewName: "Index", model: gitHubResult);
        }

        [HttpPost]
        public IActionResult CICDExample()
        {
            string yaml = @"
name: ""Feature Flags CI/CD""

on: [push]

jobs:
  build:

    runs-on: windows-latest
   
    steps:
    # checkout the repo
    - uses: actions/checkout@v1
     
    # install dependencies, build, and test
    - name: Setup Dotnet for use with actions
      uses: actions/setup-dotnet@v1.0.0
     
    #Build and test service   
    - name: Run automated unit and integration tests
      run: dotnet test MyProject/MyProject.Tests/MyProject.Tests.csproj --configuration Release 

    #Publish dotnet objects
    - name: DotNET Publish Web Service
      run: dotnet publish MyProject/MyProject.Service/MyProject.Service.csproj --configuration Release 
    - name: DotNET Publish Web Site
      run: dotnet publish MyProject/MyProject.Web/MyProject.Web.csproj --configuration Release 
    - name: DotNET Publish functional tests
      run: dotnet publish MyProject/MyProject.FunctionalTests/MyProject.FunctionalTests.csproj --configuration Release
    - name: Copy chromedriver for functional test
      run: copy ""MyProject/MyProject.FunctionalTests/bin/Release/netcoreapp3.0/chromedriver.exe"" ""MyProject/MyProject.FunctionalTests/bin/Release/netcoreapp3.0/publish""
      shell: powershell
    
    #Publish build artifacts to GitHub
    - name: Upload web service build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: serviceapp
        path: MyProject/MyProject.Service/bin/Release/netcoreapp3.0/publish
    - name: Upload website build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: webapp
        path: MyProject/MyProject.Web/bin/Release/netcoreapp3.0/publish
    - name: Upload function test build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: functionaltests
        path: MyProject/MyProject.FunctionalTests/bin/Release/netcoreapp3.0/publish

  #Deploy the artifacts to Azure
  preDeploy:
    runs-on: windows-latest

    needs: build
        
    steps:        
    # Login with the secret SP details
    - name: Log into Azure
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}  
    
    #Download the artifacts from GitHub
    - name: Download serviceapp artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: serviceapp
    - name: Download webapp artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: webapp
    - name: Download functionaltests artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: functionaltests
    
    #Deploy service and website to Azure staging slots
    - name: Deploy web service to Azure WebApp
      uses: Azure/webapps-deploy@v1
      with:
        app-name: MyProject-data-eu-service
        package: serviceapp
        slot-name: staging     
    - name: Deploy website to Azure WebApp
      uses: Azure/webapps-deploy@v1
      with:
        app-name: MyProject-data-eu-web
        package: webapp
        slot-name: staging 

    # Run functional tests on staging slots     
    - name: Functional Tests
      run: |
        $vsTestConsoleExe = ""C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\Extensions\\TestPlatform\\vstest.console.exe""
        $targetTestDll = ""functionaltests\MyProject.FunctionalTests.dll""
        $testRunSettings = ""/Settings:`""functionaltests\test.runsettings`"" ""
        $parameters = "" -- TestEnvironment=""""Dev""""  ServiceUrl=""""https://MyProject-data-eu-service-staging.azurewebsites.net/"""" WebsiteUrl=""""https://MyProject-data-eu-web-staging.azurewebsites.net/"""" ""
        #Note that the `"" is an escape character to quote strings, and the `& is needed to start the command
        $command = ""`& `""$vsTestConsoleExe`"" `""$targetTestDll`"" $testRunSettings $parameters "" 
        Write-Host ""$command""
        Invoke-Expression $command
      shell: powershell
  
  #Deploy the artifacts to Azure
  deploySwapSlots:
    runs-on: ubuntu-latest # Note, Azure CLI requires a Linux runner...
    
    needs: [build, preDeploy]
    #Only deploy if running off the master branch - we don't want to deploy off feature branches
    if: github.ref == 'refs/heads/master'
        
    steps:
    # Login with the secret SP details
    - name: Log into Azure
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}     
      #Swap staging slots with prod
    - name: Swap web service staging slot to production
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group MyProjectRG --name MyProject-data-eu-service --slot staging --target-slot production
    - name: Swap web site staging slot to production
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group MyProjectRG --name MyProject-data-eu-web --slot staging --target-slot production
";
            ConversionResult gitHubResult = new ConversionResult
            {
                actionsYaml = yaml
            };
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
