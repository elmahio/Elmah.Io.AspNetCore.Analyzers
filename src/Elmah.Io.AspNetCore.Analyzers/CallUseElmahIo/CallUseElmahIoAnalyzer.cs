using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Elmah.Io.AspNetCore.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CallUseElmahIoAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EIO1001";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Configure must call UseElmahIo", "Configure must call UseElmahIo", "Elmah.Io.CSharp.AspNetCoreRules", DiagnosticSeverity.Warning, isEnabledByDefault: true);

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

                bool useElmahIoInvocationFound = false;

                cb.RegisterSyntaxNodeAction(ctx =>
                {
                    var node = ctx.Node as InvocationExpressionSyntax;
                    if (node == null) return;
                    var expression = node.Expression as MemberAccessExpressionSyntax;
                    if (expression == null) return;
                    var methodName = expression.Name?.Identifier.ValueText;
                    if (methodName == "UseElmahIo") useElmahIoInvocationFound = true;
                }, SyntaxKind.InvocationExpression);

                cb.RegisterCodeBlockEndAction(ctx =>
                {
                    if (!useElmahIoInvocationFound)
                    {
                        var diag = Diagnostic.Create(Rule, method.Locations[0]);
                        ctx.ReportDiagnostic(diag);
                    }
                });
            });
        }
    }
}
