using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Elmah.Io.AspNetCore.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseElmahIoOrderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EIO1002";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "UseElmahIo must be called in the right order", "UseElmahIo must be called {0} {1}", "Elmah.Io.CSharp.AspNetCoreRules", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCodeBlockStartAction<SyntaxKind>(cb =>
            {
                // We only care about method bodies.
                if (cb.OwningSymbol.Kind != SymbolKind.Method) return;
                var method = (IMethodSymbol)cb.OwningSymbol;
                // We only care about methods named ConfigureServices
                if (method.Name != "Configure") return;
                if (method.ContainingType.Name != "Startup") return;
                Location useElmahIoLocation = null;

                List<string> calls = new List<string>();

                cb.RegisterSyntaxNodeAction(ctx =>
                {
                    var node = ctx.Node as InvocationExpressionSyntax;
                    if (node == null) return;
                    var expression = node.Expression as MemberAccessExpressionSyntax;
                    if (expression == null) return;
                    var methodName = expression.Name?.Identifier.ValueText;
                    if (string.IsNullOrWhiteSpace(methodName) || !methodName.StartsWith("Use")) return;
                    calls.Add(methodName);
                    if (methodName.Equals("UseElmahIo")) useElmahIoLocation = expression.GetLocation();
                }, SyntaxKind.InvocationExpression);

                cb.RegisterCodeBlockEndAction(ctx =>
                {
                    // EIO1001 will catch this
                    if (!calls.Contains("UseElmahIo")) return;

                    var index = calls.IndexOf("UseElmahIo");

                    var useDeveloperExceptionPageIndex = calls.IndexOf("UseDeveloperExceptionPage");
                    var useExceptionHandlerIndex = calls.IndexOf("UseExceptionHandler");
                    var useAuthorizationIndex = calls.IndexOf("UseAuthorization");
                    var useAuthenticationIndex = calls.IndexOf("UseAuthentication");
                    var useEndpointsIndex = calls.IndexOf("UseEndpoints");
                    var useMvcIndex = calls.IndexOf("UseMvc");

                    if (useDeveloperExceptionPageIndex != -1 && index < useDeveloperExceptionPageIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "after", "UseDeveloperExceptionPage");
                    else if (useExceptionHandlerIndex != -1 && index < useExceptionHandlerIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "after", "UseExceptionHandler");
                    else if (useAuthorizationIndex != -1 && index < useAuthorizationIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "after", "UseAuthorization");
                    else if (useAuthenticationIndex != -1 && index < useAuthenticationIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "after", "UseAuthentication");
                    else if (useEndpointsIndex != -1 && index > useEndpointsIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "before", "UseEndpoints");
                    else if (useMvcIndex != -1 && index > useMvcIndex)
                        Report(ctx, useElmahIoLocation ?? method.Locations[0], "before", "UseMvc");
                });
            });
        }

        private void Report(CodeBlockAnalysisContext context, Location location, string beforeAfter, string use)
        {
            var diag = Diagnostic.Create(Rule, location, beforeAfter, use);
            context.ReportDiagnostic(diag);
        }
    }
}
