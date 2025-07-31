using PipelinesToActionsWeb.Models;

namespace PipelinesToActions.Tests
{
    [TestClass]
    public class ModelsTests
    {
        [TestMethod]
        public void SamplesExistTest()
        {
            //Arrange

            //Act

            //Assert
            Assert.IsNotNull(Examples.AntExample());
            Assert.IsNotNull(Examples.ASPDotNetCoreSimpleExample());
            Assert.IsNotNull(Examples.ASPDotNetFrameworkExample());
            Assert.IsNotNull(Examples.CDExample());
            Assert.IsNotNull(Examples.CICDExample());
            Assert.IsNotNull(Examples.CIExample());
            Assert.IsNotNull(Examples.DockerExample());
            Assert.IsNotNull(Examples.DotNetFrameworkDesktopExample());
            Assert.IsNotNull(Examples.GradleExample());
            Assert.IsNotNull(Examples.MavenExample());
            Assert.IsNotNull(Examples.NodeExample());
            Assert.IsNotNull(Examples.PythonExample());
            Assert.IsNotNull(Examples.RubyExample());
        }


        [TestMethod]
        public void ErrorModelTest()
        {
            //Arrange
            ErrorViewModel errorViewModel = new();
            errorViewModel.RequestId = "abc";

            //Act

            //Assert
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual("abc", errorViewModel.RequestId);
            Assert.IsTrue(errorViewModel.ShowRequestId);
        }

        [TestMethod]
        public void ErrorModel_WithNullRequestId_ShowRequestIdReturnsFalse()
        {
            //Arrange
            ErrorViewModel errorViewModel = new();
            errorViewModel.RequestId = null;

            //Act

            //Assert
            Assert.IsNotNull(errorViewModel);
            Assert.IsNull(errorViewModel.RequestId);
            Assert.IsFalse(errorViewModel.ShowRequestId);
        }

        [TestMethod]
        public void ErrorModel_WithEmptyRequestId_ShowRequestIdReturnsFalse()
        {
            //Arrange
            ErrorViewModel errorViewModel = new();
            errorViewModel.RequestId = "";

            //Act

            //Assert
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual("", errorViewModel.RequestId);
            Assert.IsFalse(errorViewModel.ShowRequestId);
        }

        [TestMethod]
        public void ErrorModel_WithWhitespaceRequestId_ShowRequestIdReturnsTrue()
        {
            //Arrange
            ErrorViewModel errorViewModel = new();
            errorViewModel.RequestId = "   ";

            //Act

            //Assert
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual("   ", errorViewModel.RequestId);
            Assert.IsTrue(errorViewModel.ShowRequestId); // IsNullOrEmpty returns false for whitespace
        }
    }
}