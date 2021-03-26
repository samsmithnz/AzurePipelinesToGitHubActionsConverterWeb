using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipelinesToActionsWeb.Models
{
    public class Examples
    {
        public static string CIExample()
        {
            string yaml = @"
trigger:
- main

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
- job: Build
  displayName: 'Build job'
  pool:
    vmImage: windows-latest
  steps:
  - task: DotNetCoreCLI@2
    displayName: 'Test dotnet code projects'
    inputs:
      command: test
      projects: |
       MyProject/MyProject.Tests/MyProject.Tests.csproj
      arguments: --configuration $(buildConfiguration) 
  - task: DotNetCoreCLI@2
    displayName: 'Publish dotnet core projects'
    inputs:
      command: publish
      publishWebProjects: false
      projects: |
        MyProject/MyProject.Web/MyProject.Web.csproj
      arguments: --configuration $(buildConfiguration) --output $(build.artifactstagingdirectory) 
      zipAfterPublish: true
  - task: PublishBuildArtifacts@1
    displayName: 'Publish Artifact'
    inputs:
      PathtoPublish: '$(build.artifactstagingdirectory)'
";
            return yaml;
        }

        public static string CDExample()
        {
            string yaml = @"
trigger:
- main

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
- job: Deploy
  displayName: ""Deploy job""
  pool:
    vmImage: ubuntu-latest
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  variables:
    AppSettings.Environment: 'data'
    ArmTemplateResourceGroupLocation: 'eu'
    ResourceGroupName: 'MyProjectRG'
    WebsiteName: 'myproject-web'
  steps:
  - task: DownloadBuildArtifacts@0
    displayName: 'Download the build artifacts'
    inputs:
      buildType: 'current'
      downloadType: 'single'
      artifactName: 'drop'
      downloadPath: '$(build.artifactstagingdirectory)'
  - task: AzureRmWebAppDeployment@3
    displayName: 'Azure App Service Deploy: web site'
    inputs:
      azureSubscription: 'connection to Azure Portal'
      WebAppName: $(WebsiteName)
      DeployToSlotFlag: true
      ResourceGroupName: $(ResourceGroupName)
      SlotName: 'staging'
      Package: '$(build.artifactstagingdirectory)/drop/MyProject.Web.zip'
      TakeAppOfflineFlag: true
      JSONFiles: '**/appsettings.json'        
  - task: AzureAppServiceManage@0
    displayName: 'Swap Slots: website'
    inputs:
      azureSubscription: 'connection to Azure Portal'
      WebAppName: $(WebsiteName)
      ResourceGroupName: $(ResourceGroupName)
      SourceSlot: 'staging'
";
            return yaml;
        }

        public static string CICDExample()
        {
            string yaml = @"
trigger:
- main
pr:
  branches:
    include:
    - '*'  # must quote since ""*"" is a YAML reserved character; we want a string

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

stages:
- stage: Build
  displayName: 'Build/Test Stage'
  jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: windows-latest
    steps:

    - task: CopyFiles@2
      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'
      inputs:
        SourceFolder: '$(system.defaultworkingdirectory)\MyProject\MyProject.ARMTemplates'
        Contents: '**\*' # **\* = Copy all files and all files in sub directories
        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'

    - task: DotNetCoreCLI@2
      displayName: 'Test dotnet code projects'
      inputs:
        command: test
        projects: |
         MyProject/MyProject.Tests/MyProject.Tests.csproj
        arguments: --configuration $(buildConfiguration) 

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core projects'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         MyProject/MyProject.Web/MyProject.Web.csproj
        arguments: --configuration $(buildConfiguration) --output $(build.artifactstagingdirectory) 
        zipAfterPublish: true

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core functional tests project'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         MyProject/MyProject.FunctionalTests/MyProject.FunctionalTests.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory)/FunctionalTests'
        zipAfterPublish: false

    - task: CopyFiles@2
      displayName: 'Copy Selenium Files to: $(build.artifactstagingdirectory)/FunctionalTests/MyProject.FunctionalTests'
      inputs:
        SourceFolder: 'MyProject/MyProject.FunctionalTests/bin/$(buildConfiguration)/netcoreapp3.0'
        Contents: '*chromedriver.exe*'
        TargetFolder: '$(build.artifactstagingdirectory)/FunctionalTests/MyProject.FunctionalTests'

    # Publish the artifacts
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifact'
      inputs:
        PathtoPublish: '$(build.artifactstagingdirectory)'

- stage: Deploy
  displayName: 'Deploy Prod'
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - job: Deploy
    displayName: ""Deploy job""
    pool:
      vmImage: ubuntu-latest  
    variables:
      AppSettings.Environment: 'data'
      ArmTemplateResourceGroupLocation: 'eu'
      ResourceGroupName: 'MyProjectRG'
      WebsiteName: 'myproject-web'
    steps:
    - task: DownloadBuildArtifacts@0
      displayName: 'Download the build artifacts'
      inputs:
        buildType: 'current'
        downloadType: 'single'
        artifactName: 'drop'
        downloadPath: '$(build.artifactstagingdirectory)'
    - task: AzureResourceGroupDeployment@2
      displayName: 'Deploy ARM Template to resource group'
      inputs:
        azureSubscription: 'connection to Azure Portal'
        resourceGroupName: $(ResourceGroupName)
        location: '[resourceGroup().location]'
        csmFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.json'
        csmParametersFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.parameters.json'
        overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)'
    - task: AzureRmWebAppDeployment@3
      displayName: 'Azure App Service Deploy: web site'
      inputs:
        azureSubscription: 'connection to Azure Portal'
        WebAppName: $(WebsiteName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/MyProject.Web.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'        
    - task: VSTest@2
      displayName: 'Run functional smoke tests on website'
      inputs:
        searchFolder: '$(build.artifactstagingdirectory)'
        testAssemblyVer2: |
          **\MyProject.FunctionalTests\MyProject.FunctionalTests.dll
        uiTests: true
        runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings'
    - task: AzureAppServiceManage@0
      displayName: 'Swap Slots: website'
      inputs:
        azureSubscription: 'connection to Azure Portal'
        WebAppName: $(WebsiteName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'
";
            return yaml;
        }

        public static string DockerExample()
        {
            string yaml = @"
trigger:
- main

resources:
- repo: self

variables:
  # Container registry service connection established during pipeline creation
  dockerRegistryServiceConnection: '{{ containerRegistryConnection.Id }}'
  imageRepository: '{{#toAlphaNumericString imageRepository 50}}{{/toAlphaNumericString}}'
  containerRegistry: '{{ containerRegistryConnection.Authorization.Parameters.loginServer }}'
  dockerfilePath: '{{ dockerfilePath }}'
  tag: '$(Build.BuildId)'

jobs:
- job: BuildJob
  displayName: Build and push
  pool:
    vmImage: ubuntu-latest
  steps:
  - task: Docker@2
    displayName: Build and push an image to container registry
    inputs:
      command: buildAndPush
      repository: $(imageRepository)
      dockerfile: $(dockerfilePath)
      containerRegistry: $(dockerRegistryServiceConnection)
      tags: |
        $(tag)
";
            return yaml;
        }

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/.net-desktop.yml
        public static string DotNetFrameworkDesktopExample()
        {
            string yaml = @"
# .NET Desktop
# Build and run tests for .NET Desktop or Windows classic desktop solutions.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/apps/windows/dot-net

trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/MyProject.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'
";
            return yaml;
        }

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/android.yml
        public static string GradleExample()
        {
            string yaml = @"
# Android
# Build your Android project with Gradle.
# Add steps that test, sign, and distribute the APK, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/android

trigger:
- main

pool:
  vmImage: 'macos-latest'

steps:
- task: Gradle@2
  inputs:
    workingDirectory: ''
    gradleWrapperFile: 'gradlew'
    gradleOptions: '-Xmx3072m'
    publishJUnitResults: false
    testResultsFiles: '**/TEST-*.xml'
    tasks: 'assembleDebug'
";
            return yaml;
        }

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/asp.net-core.yml
        public static string ASPDotNetCoreSimpleExample()
        {
            string yaml = @"
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- script: dotnet build --configuration $(buildConfiguration)
  displayName: 'dotnet build $(buildConfiguration)'
";
            return yaml;
        }

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/ant.yml
        public static string AntExample()
        {
            string yaml = @"
# Ant
# Build your Java projects and run tests with Apache Ant.
# Add steps that save build artifacts and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/java

trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Ant@1
  inputs:
    workingDirectory: ''
    buildFile: 'build.xml'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
";
            return yaml;
        }

        public static string ASPDotNetFrameworkExample()
        {
            string yaml = @"
ASPDotNetFrameworkExample Coming soon
";
            return yaml;
        }

        public static string NodeExample()
        {
            string yaml = @"
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: NodeTool@0
  inputs:
    versionSpec: '10.x'
  displayName: 'Install Node.js'

- script: |
    npm install
    npm start
  displayName: 'npm install and start'
";
            return yaml;
        }

        public static string MavenExample()
        {
            string yaml = @"
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Maven@3
  inputs:
    mavenPomFile: 'pom.xml'
    mavenOptions: '-Xmx3072m'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
    publishJUnitResults: true
    testResultsFiles: '**/surefire-reports/TEST-*.xml'
    goals: 'package'
";
            return yaml;
        }

        public static string PythonExample()
        {
            string yaml = @"
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'
strategy:
  matrix:
    Python35:
      PYTHON_VERSION: '3.5'
    Python36:
      PYTHON_VERSION: '3.6'
    Python37:
      PYTHON_VERSION: '3.7'
  maxParallel: 3

steps:
- task: UsePythonVersion@0
  inputs:
    versionSpec: '$(PYTHON_VERSION)'
    addToPath: true
    architecture: 'x64'
- task: PythonScript@0
  inputs:
    scriptSource: 'filePath'
    scriptPath: 'Hello.py'
";
            return yaml;
        }

        public static string RubyExample()
        {
            string yaml = @"
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseRubyVersion@0
  inputs:
    versionSpec: '>= 2.5'
- script: ruby HelloWorld.rb
";
            return yaml;
        }

    }
}
