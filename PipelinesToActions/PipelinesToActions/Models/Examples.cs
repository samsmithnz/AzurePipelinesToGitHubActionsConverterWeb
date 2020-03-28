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
- master

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

    # Publish the artifacts
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
- master

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

stages:
- stage: Deploy
  displayName: 'Deploy Prod'
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))
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
- master
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
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))
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

        public static string ContainerExample()
        {
            string yaml = @"
pool:
  vmImage: 'ubuntu-16.04'

strategy:
  matrix:
    DotNetCore22:
      containerImage: mcr.microsoft.com/dotnet/core/sdk:2.2
    DotNetCore22Nightly:
      containerImage: mcr.microsoft.com/dotnet/core-nightly/sdk:2.2

container: $[ variables['containerImage'] ]

resources:
  containers:
  - container: redis
    image: redis

services:
  redis: redis

variables:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true

steps:
- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    command: build
    projects: '**/*.csproj'
    arguments: '--configuration release'

- task: DotNetCoreCLI@2
  displayName: Test
  inputs:
    command: test
    projects: '**/*Tests.csproj'
    arguments: '--configuration release'
  env:
    CONNECTIONSTRINGS_REDIS: redis:6379

- task: DotNetCoreCLI@2
  displayName: Publish
  inputs:
    command: publish
    projects: 'MyProject/MyProject.csproj'
    publishWebProjects: false
    zipAfterPublish: false
    arguments: '--configuration release'

- task: PublishPipelineArtifact@0
  displayName: Store artifact
  inputs:
    artifactName: 'MyProject'
    targetPath: 'MyProject/bin/release/netcoreapp2.2/publish/'
  condition: and(succeeded(), endsWith(variables['Agent.JobName'], 'DotNetCore22'))
";
            return yaml;
        }

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/.net-desktop.yml
        public static string DotNetDeskTopExample()
        {
            string yaml = @"
# .NET Desktop
# Build and run tests for .NET Desktop or Windows classic desktop solutions.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/apps/windows/dot-net

trigger:
- master

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
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

- task: VSTest@2
  inputs:
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
- master

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
- master

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
- master

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

        //https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/docker-build.yml
        public static string DockerBuildExample()
        {
            string yaml = @"
# Docker
# Build a Docker image 
# https://docs.microsoft.com/azure/devops/pipelines/languages/docker

trigger:
- master

resources:
- repo: self

variables:
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build image
  jobs:  
  - job: Build
    displayName: Build
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: Docker@2
      displayName: Build an image
      inputs:
        command: build
        dockerfile: '{{ dockerfilePath }}'
        tags: |
          $(tag)
";
            return yaml;
        }

        public static string DotNetDesktopExample()
        {
            string yaml = @"
DotNetDesktopExample Coming soon
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
NodeExample Coming soon
";
            return yaml;
        }

        public static string MavenExample()
        {
            string yaml = @"
trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Maven@3
  inputs:
    mavenPomFile: 'Maven/pom.xml'
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
- master

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
    scriptPath: 'Python/Hello.py'
";
            return yaml;
        }

        public static string RubyExample()
        {
            string yaml = @"
RubyExample Coming soon
";
            return yaml;
        }

    }
}
