using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Elmah.Io.AspNetCore.Analyzers.Test
{
    [TestClass]
    public class UseElmahIoOrderAnalyzerTest : CodeFixVerifier
    {
        [TestMethod]
        public void NoDiagnosticsExpectedToShowUp()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NoDiagnosticsOnCorrectOrder()
        {
            var test = @"
namespace ConsoleApplication1
{
    class Startup
    {   
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(""/Home/Error"");
            }

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseElmahIo();

            app.UseEndpoints();
            app.UseMvc();
        }
    }
}";

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseElmahIo();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(""/Home/Error"");
            }

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints();
            app.UseMvc();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "EIO1002",
                Message = "UseElmahIo must be called after UseDeveloperExceptionPage",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 8, 13)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseElmahIoOrderAnalyzer();
        }
    }
}
