using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Elmah.Io.AspNetCore.Analyzers.Test
{
    [TestClass]
    public class CallAddElmahIoAnalyzerTest : CodeFixVerifier
    {

        [TestMethod]
        public void NoDiagnosticsExpectedToShowUp()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DiagnosticTriggeredAndCheckedFor()
        {
            var test = @"
namespace ConsoleApplication1
{
    class Startup
    {   
        public void ConfigureServices()
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "EIO1000",
                Message = "ConfigureServices must call AddElmahIo",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 6, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CallAddElmahIoAnalyzer();
        }
    }
}
