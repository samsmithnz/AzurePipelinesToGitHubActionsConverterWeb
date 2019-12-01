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
            return View();
        }


        [HttpPost]
        public IActionResult Index(string txtAzurePipelinesYAML)
        {
            GitHubConversion gitHubResult;
            try
            {
                Conversion conversion = new Conversion();
                gitHubResult = conversion.ConvertAzurePipelineToGitHubAction(txtAzurePipelinesYAML);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                gitHubResult = new GitHubConversion();
                gitHubResult.yaml = "Error processing YAML, it's likely the original YAML is not valid" + Environment.NewLine +
                    "Original error message: " + ex.Message;
            }
            catch (Exception ex)
            {
                gitHubResult = new GitHubConversion();
                gitHubResult.yaml = "Unknown error: " + ex.ToString();
            }

            //Return the result
            if (gitHubResult != null)
            {
                return View(model: gitHubResult.yaml);
            }
            else
            {
                return View(model: "error");
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
    
    env:
      buildVersion: 1.0.0.0 #The initial build version, but this is updated below
   
    steps:
    # checkout the repo
    - uses: actions/checkout@v1
     
    # install dependencies, build, and test
    - name: Setup Dotnet for use with actions
      uses: actions/setup-dotnet@v1.0.0
    
    #Build and test service   
    - name: Run automated unit and integration tests
      run: dotnet test FeatureFlags/FeatureFlags.Tests/FeatureFlags.Tests.csproj --configuration Release --logger trx --collect ""Code coverage"" --settings:./FeatureFlags/FeatureFlags.Tests/CodeCoverage.runsettings

    #Publish dotnet objects
    - name: DotNET Publish Web Service
      run: dotnet publish FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csproj --configuration Release 
    - name: DotNET Publish Web Site
      run: dotnet publish FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj --configuration Release 
    - name: DotNET Publish functional tests
      run: dotnet publish FeatureFlags/FeatureFlags.FunctionalTests/FeatureFlags.FunctionalTests.csproj --configuration Release
    - name: Copy chromedriver for functional test
      run: copy ""FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/chromedriver.exe"" ""FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/publish""
      shell: powershell
    
    #Publish build artifacts to GitHub
    - name: Upload web service build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: serviceapp
        path: FeatureFlags/FeatureFlags.Service/bin/Release/netcoreapp3.0/publish
    - name: Upload website build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: webapp
        path: FeatureFlags/FeatureFlags.Web/bin/Release/netcoreapp3.0/publish
    - name: Upload function test build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: functionaltests
        path: FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/publish
";

            return View(viewName: "Index", model: yaml);
        }
        [HttpPost]
        public IActionResult CDExample()
        {
            string yaml = @"
name: ""Feature Flags CD""

on: [push]

jobs:
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
        app-name: featureflags-data-eu-service
        package: serviceapp
        slot-name: staging     
    - name: Deploy website to Azure WebApp
      uses: Azure/webapps-deploy@v1
      with:
        app-name: featureflags-data-eu-web
        package: webapp
        slot-name: staging 

    # Run functional tests on staging slots     
    - name: Functional Tests
      run: |
        $vsTestConsoleExe = ""C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\Extensions\\TestPlatform\\vstest.console.exe""
        $targetTestDll = ""functionaltests\FeatureFlags.FunctionalTests.dll""
        $testRunSettings = ""/Settings:`""functionaltests\test.runsettings`"" ""
        $parameters = "" -- TestEnvironment=""""Beta123""""  ServiceUrl=""""https://featureflags-data-eu-service-staging.azurewebsites.net/"""" WebsiteUrl=""""https://featureflags-data-eu-web-staging.azurewebsites.net/"""" ""
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
        inlineScript: az webapp deployment slot swap --resource-group SamLearnsAzureFeatureFlags --name featureflags-data-eu-service --slot staging --target-slot production
    - name: Swap web site staging slot to production
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group SamLearnsAzureFeatureFlags --name featureflags-data-eu-web --slot staging --target-slot production
";

            return View(viewName: "Index", model: yaml);
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
    
    env:
      buildVersion: 1.0.0.0 #The initial build version, but this is updated below
   
    steps:
    # checkout the repo
    - uses: actions/checkout@v1
     
    # install dependencies, build, and test
    - name: Setup Dotnet for use with actions
      uses: actions/setup-dotnet@v1.0.0
     
    #Build and test service   
    - name: Run automated unit and integration tests
      run: dotnet test FeatureFlags/FeatureFlags.Tests/FeatureFlags.Tests.csproj --configuration Release --logger trx --collect ""Code coverage"" --settings:./FeatureFlags/FeatureFlags.Tests/CodeCoverage.runsettings

    #Publish dotnet objects
    - name: DotNET Publish Web Service
      run: dotnet publish FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csproj --configuration Release 
    - name: DotNET Publish Web Site
      run: dotnet publish FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj --configuration Release 
    - name: DotNET Publish functional tests
      run: dotnet publish FeatureFlags/FeatureFlags.FunctionalTests/FeatureFlags.FunctionalTests.csproj --configuration Release
    - name: Copy chromedriver for functional test
      run: copy ""FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/chromedriver.exe"" ""FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/publish""
      shell: powershell
    
    #Publish build artifacts to GitHub
    - name: Upload web service build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: serviceapp
        path: FeatureFlags/FeatureFlags.Service/bin/Release/netcoreapp3.0/publish
    - name: Upload website build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: webapp
        path: FeatureFlags/FeatureFlags.Web/bin/Release/netcoreapp3.0/publish
    - name: Upload function test build artifacts back to GitHub
      uses: actions/upload-artifact@master
      with:
        name: functionaltests
        path: FeatureFlags/FeatureFlags.FunctionalTests/bin/Release/netcoreapp3.0/publish

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
        app-name: featureflags-data-eu-service
        package: serviceapp
        slot-name: staging     
    - name: Deploy website to Azure WebApp
      uses: Azure/webapps-deploy@v1
      with:
        app-name: featureflags-data-eu-web
        package: webapp
        slot-name: staging 

    # Run functional tests on staging slots     
    - name: Functional Tests
      run: |
        $vsTestConsoleExe = ""C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\Extensions\\TestPlatform\\vstest.console.exe""
        $targetTestDll = ""functionaltests\FeatureFlags.FunctionalTests.dll""
        $testRunSettings = ""/Settings:`""functionaltests\test.runsettings`"" ""
        $parameters = "" -- TestEnvironment=""""Beta123""""  ServiceUrl=""""https://featureflags-data-eu-service-staging.azurewebsites.net/"""" WebsiteUrl=""""https://featureflags-data-eu-web-staging.azurewebsites.net/"""" ""
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
        inlineScript: az webapp deployment slot swap --resource-group SamLearnsAzureFeatureFlags --name featureflags-data-eu-service --slot staging --target-slot production
    - name: Swap web site staging slot to production
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group SamLearnsAzureFeatureFlags --name featureflags-data-eu-web --slot staging --target-slot production
";

            return View(viewName: "Index", model: yaml);
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
