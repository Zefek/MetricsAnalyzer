using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsAnalyzers.Analyzers
{
    class HalsteadAnalyzer : Analyzer<Metrics.HalsteadMetrics>
    {
        SyntaxKind[] _operators = new SyntaxKind[] { SyntaxKind.DotToken, SyntaxKind.EqualsToken, SyntaxKind.SemicolonToken, SyntaxKind.PlusPlusToken, SyntaxKind.PlusToken, SyntaxKind.PlusEqualsToken, SyntaxKind.MinusMinusToken, SyntaxKind.MinusToken, SyntaxKind.MinusEqualsToken, SyntaxKind.AsteriskToken, SyntaxKind.AsteriskEqualsToken, SyntaxKind.SlashToken, SyntaxKind.SlashEqualsToken, SyntaxKind.PercentToken, SyntaxKind.PercentEqualsToken, SyntaxKind.AmpersandToken, SyntaxKind.BarToken, SyntaxKind.CaretToken, SyntaxKind.TildeToken, SyntaxKind.ExclamationToken, SyntaxKind.ExclamationEqualsToken, SyntaxKind.GreaterThanToken, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.LessThanToken, SyntaxKind.LessThanEqualsToken };
        SyntaxKind[] _operands = new SyntaxKind[] { SyntaxKind.IdentifierToken, SyntaxKind.StringLiteralToken, SyntaxKind.NumericLiteralToken, SyntaxKind.AddKeyword, SyntaxKind.AliasKeyword, SyntaxKind.AscendingKeyword, SyntaxKind.AsKeyword, SyntaxKind.AsyncKeyword, SyntaxKind.AwaitKeyword, SyntaxKind.BaseKeyword, SyntaxKind.BoolKeyword, SyntaxKind.BreakKeyword, SyntaxKind.ByKeyword, SyntaxKind.ByteKeyword, SyntaxKind.CaseKeyword, SyntaxKind.CatchKeyword, SyntaxKind.CharKeyword, SyntaxKind.CheckedKeyword, SyntaxKind.ChecksumKeyword, SyntaxKind.ClassKeyword, SyntaxKind.ConstKeyword, SyntaxKind.ContinueKeyword, SyntaxKind.DecimalKeyword, SyntaxKind.DefaultKeyword, SyntaxKind.DefineKeyword, SyntaxKind.DelegateKeyword, SyntaxKind.DescendingKeyword, SyntaxKind.DisableKeyword, SyntaxKind.DoKeyword, SyntaxKind.DoubleKeyword, SyntaxKind.ElifKeyword, SyntaxKind.ElseKeyword, SyntaxKind.EndIfKeyword, SyntaxKind.EndRegionKeyword, SyntaxKind.EnumKeyword, SyntaxKind.EqualsKeyword, SyntaxKind.ErrorKeyword, SyntaxKind.EventKeyword, SyntaxKind.ExplicitKeyword, SyntaxKind.ExternKeyword, SyntaxKind.FalseKeyword, SyntaxKind.FieldKeyword, SyntaxKind.FinallyKeyword, SyntaxKind.FixedKeyword, SyntaxKind.FloatKeyword, SyntaxKind.ForEachKeyword, SyntaxKind.ForKeyword, SyntaxKind.FromKeyword, SyntaxKind.GetKeyword, SyntaxKind.GlobalKeyword, SyntaxKind.GotoKeyword, SyntaxKind.GroupKeyword, SyntaxKind.HiddenKeyword, SyntaxKind.IfKeyword, SyntaxKind.ImplicitKeyword, SyntaxKind.InKeyword, SyntaxKind.InterfaceKeyword, SyntaxKind.InternalKeyword, SyntaxKind.IntKeyword, SyntaxKind.IntoKeyword, SyntaxKind.IsKeyword, SyntaxKind.JoinKeyword, SyntaxKind.LetKeyword, SyntaxKind.LineKeyword, SyntaxKind.LockKeyword, SyntaxKind.LongKeyword, SyntaxKind.MakeRefKeyword, SyntaxKind.MethodKeyword, SyntaxKind.ModuleKeyword, SyntaxKind.NamespaceKeyword, SyntaxKind.NullKeyword, SyntaxKind.ObjectKeyword, SyntaxKind.OnKeyword, SyntaxKind.OperatorKeyword, SyntaxKind.OrderByKeyword, SyntaxKind.OutKeyword, SyntaxKind.OverrideKeyword, SyntaxKind.ParamKeyword, SyntaxKind.ParamsKeyword, SyntaxKind.PartialKeyword, SyntaxKind.PragmaKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.PropertyKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword, SyntaxKind.ReferenceKeyword, SyntaxKind.RefKeyword, SyntaxKind.RefTypeKeyword, SyntaxKind.RefValueKeyword, SyntaxKind.RegionKeyword, SyntaxKind.RemoveKeyword, SyntaxKind.RestoreKeyword, SyntaxKind.ReturnKeyword, SyntaxKind.SByteKeyword, SyntaxKind.SealedKeyword, SyntaxKind.SelectKeyword, SyntaxKind.SetKeyword, SyntaxKind.ShortKeyword, SyntaxKind.SizeOfKeyword, SyntaxKind.StackAllocKeyword, SyntaxKind.StaticKeyword, SyntaxKind.StringKeyword, SyntaxKind.StructKeyword, SyntaxKind.SwitchKeyword, SyntaxKind.ThisKeyword, SyntaxKind.TrueKeyword, SyntaxKind.TryKeyword, SyntaxKind.TypeKeyword, SyntaxKind.TypeOfKeyword, SyntaxKind.TypeVarKeyword, SyntaxKind.UIntKeyword, SyntaxKind.ULongKeyword, SyntaxKind.UncheckedKeyword, SyntaxKind.UndefKeyword, SyntaxKind.UnsafeKeyword, SyntaxKind.UShortKeyword, SyntaxKind.UsingKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.VoidKeyword, SyntaxKind.VolatileKeyword, SyntaxKind.WarningKeyword, SyntaxKind.WhereKeyword, SyntaxKind.WhileKeyword, SyntaxKind.YieldKeyword };

        public override Task Calculate (BlockSyntax blockSyntax)
        {
            return Task.Run(() =>
            {
                this.Visit(blockSyntax);
                return this.Metric;
            });
        }
        private IDictionary<SyntaxKind, IList<string>> ParseTokens(IEnumerable<SyntaxToken> tokens, IEnumerable<SyntaxKind> filter)
        {
            IList<string> item;
            IDictionary<SyntaxKind, IList<string>> syntaxKinds = new Dictionary<SyntaxKind, IList<string>>();
            foreach (SyntaxToken token in tokens)
            {
                SyntaxKind syntaxKind = token.Kind();
                if (!filter.Any<SyntaxKind>((SyntaxKind x) => x == syntaxKind))
                {
                    continue;
                }
                string valueText = token.ValueText;
                if (!syntaxKinds.TryGetValue(syntaxKind, out item))
                {
                    syntaxKinds[syntaxKind] = new List<string>();
                    item = syntaxKinds[syntaxKind];
                }
                item.Add(valueText);
            }
            return syntaxKinds;
        }

        public override void VisitBlock(BlockSyntax node)
        {
            base.VisitBlock(node);
            IEnumerable<SyntaxToken> list = node.DescendantTokens(null, false).ToList<SyntaxToken>();
            IDictionary<SyntaxKind, IList<string>> operands = ParseTokens(list, _operands);
            IDictionary<SyntaxKind, IList<string>> operators = ParseTokens(list, _operators);
            Metrics.HalsteadMetrics halsteadMetric = new Metrics.HalsteadMetrics()
            {
                NumOperands = operands.Values.SelectMany<IList<string>, string>((IList<string> x) => x).Count<string>(),
                NumUniqueOperands = operands.Values.SelectMany<IList<string>, string>((IList<string> x) => x).Distinct<string>().Count<string>(),
                NumOperators = operators.Values.SelectMany<IList<string>, string>((IList<string> x) => x).Count<string>(),
                NumUniqueOperators = operators.Values.SelectMany<IList<string>, string>((IList<string> x) => x).Distinct<string>().Count<string>()
            };
            this.Metric = halsteadMetric;
        }
    }
}
