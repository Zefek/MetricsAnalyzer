using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MetricsAnalyzers.Analyzers;

namespace MetricsAnalyzers
{
    public abstract class Node
    {
        public IEnumerable<Metrics.Metric> Metrics { get; protected set; }
        protected MetricsContext Context { get; private set; }
        protected Node(MetricsContext context)
        {
            Context = context;
            Metrics = new List<Metrics.Metric>();
        }
    }
    public class MemberNode:Node
    {

        List<IAnalyzer> analyzers = new List<IAnalyzer>();
        List<MemberPart> memberParts = new List<MemberPart>();
        public string Name { get; private set; }
        internal Location Location { get; private set; }

        public MemberNode(MetricsContext context):base (context)
        {
            Initialize();
        }
        private void Initialize()
        {
            analyzers.Add(new Analyzers.LinesOfCodeAnalyzer());
            analyzers.Add(new Analyzers.CyclomaticComplexityAnalyzer());
            analyzers.Add(new Analyzers.MaintainabilityIndexAnalyzer(analyzers[0] as LinesOfCodeAnalyzer, analyzers[1] as CyclomaticComplexityAnalyzer));
            analyzers.Add(new Analyzers.NumberOfLocalVariablesAnalyzer());
            analyzers.Add(new Analyzers.NumberOfParametersAnalyzer());
            analyzers.Add(new Analyzers.LinesOfCommentAnalyzer());

            memberParts.Add(new ContructorMember());
            memberParts.Add(new DestructorMember());
            memberParts.Add(new MethodMember());
            memberParts.Add(new PropertyGetterMember());
            memberParts.Add(new PropertySetterMember());
            memberParts.Add(new EventAddMember());
            memberParts.Add(new EventRemoveMember());
            memberParts.Add(new IndexerGetterMember());
            memberParts.Add(new IndexerSetterMember());
        }
        internal void Calculate(SyntaxNode syntaxNode)
        {
            foreach (MemberPart m in memberParts)
            {
                if(m.IsMatch(syntaxNode))
                {
                    BlockSyntax blockSyntax = m.GetBody(syntaxNode);
                    Name = m.GetName(syntaxNode);
                    Location = m.GetLocation(syntaxNode);

                    Task[] tasks = GetTasks(blockSyntax);
                    Task.WaitAll(tasks);
                    List<MetricsAnalyzers.Metrics.Metric> metrics = new List<MetricsAnalyzers.Metrics.Metric>();
                    for(int i =0; i<tasks.Length; i++)
                    {
                        metrics.Add(analyzers[i].Metric);
                    }
                    Metrics = metrics;
                    Context.Invoke(this);
                    break;
                }
            }
        }
        private Task[] GetTasks(BlockSyntax blockSyntax)
        {
            Task[] result = new Task[analyzers.Count];
            int i = 0;
            foreach(IAnalyzer analyzer in analyzers)
            {
                result[i++] = analyzer.Calculate(blockSyntax);
            }
            return result;
        }
    }
    #region Members
    internal abstract class MemberPart
    {
        public abstract bool IsMatch(SyntaxNode syntaxNode);
        public virtual BlockSyntax GetBody(SyntaxNode syntaxNode) { return null; }
        public virtual string GetName(SyntaxNode syntaxNode) { return string.Empty; }
        public virtual Location GetLocation(SyntaxNode syntaxNode) { return syntaxNode.GetLocation(); }
    }
    internal abstract class PropertyMember:MemberPart
    {
        private SyntaxKind _syntaxKind;
        protected PropertyMember(SyntaxKind syntaxKind)
        {
            _syntaxKind = syntaxKind;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as AccessorDeclarationSyntax).Body;
        }
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent != null && syntaxNode.Parent.Parent != null)
                return syntaxNode.Parent.Parent is PropertyDeclarationSyntax && (syntaxNode as AccessorDeclarationSyntax).Kind() == _syntaxKind;
            return false;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return (syntaxNode.Parent.Parent as PropertyDeclarationSyntax).Identifier.Text;
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return syntaxNode.GetLocation();
        }
    }
    internal abstract class IndexerMember : MemberPart
    {
        private SyntaxKind _syntaxKind;
        protected IndexerMember(SyntaxKind syntaxKind)
        {
            _syntaxKind = syntaxKind;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as AccessorDeclarationSyntax).Body;
        }
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent != null && syntaxNode.Parent.Parent != null)
                return syntaxNode.Parent.Parent is IndexerDeclarationSyntax && (syntaxNode as AccessorDeclarationSyntax).Kind() == _syntaxKind;
            return false;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return "this";
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return syntaxNode.GetLocation();
        }
    }
    internal abstract class EventMember : MemberPart
    {
        private SyntaxKind _syntaxKind;
        protected EventMember(SyntaxKind syntaxKind)
        {
            _syntaxKind = syntaxKind;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as AccessorDeclarationSyntax).Body;
        }
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is AccessorDeclarationSyntax && (syntaxNode as AccessorDeclarationSyntax).Kind() == _syntaxKind;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return ((syntaxNode as AccessorDeclarationSyntax).Parent.Parent as PropertyDeclarationSyntax).Identifier.Text;
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return (syntaxNode as AccessorDeclarationSyntax).GetLocation();
        }
    }
    internal class ContructorMember : MemberPart
    {
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is ConstructorDeclarationSyntax;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as ConstructorDeclarationSyntax).Body;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return (syntaxNode as ConstructorDeclarationSyntax).Identifier.Text;
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return (syntaxNode as ConstructorDeclarationSyntax).Identifier.GetLocation();
        }
    }
    internal class DestructorMember : MemberPart
    {
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is DestructorDeclarationSyntax;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as DestructorDeclarationSyntax).Body;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return (syntaxNode as DestructorDeclarationSyntax).Identifier.Text;
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return (syntaxNode as DestructorDeclarationSyntax).Identifier.GetLocation();
        }
    }
    internal class MethodMember : MemberPart
    {
        public override bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is MethodDeclarationSyntax;
        }
        public override BlockSyntax GetBody(SyntaxNode syntaxNode)
        {
            return (syntaxNode as MethodDeclarationSyntax).Body;
        }
        public override string GetName(SyntaxNode syntaxNode)
        {
            return (syntaxNode as MethodDeclarationSyntax).Identifier.Text;
        }
        public override Location GetLocation(SyntaxNode syntaxNode)
        {
            return (syntaxNode as MethodDeclarationSyntax).Identifier.GetLocation();
        }
    }
    internal class PropertyGetterMember : PropertyMember
    {
        public PropertyGetterMember() : base(SyntaxKind.GetAccessorDeclaration) { }
    }
    internal class PropertySetterMember : PropertyMember
    {
        public PropertySetterMember() : base(SyntaxKind.SetAccessorDeclaration) { }
    }
    internal class EventAddMember : EventMember
    {
        public EventAddMember() : base(SyntaxKind.AddAccessorDeclaration) { }
    }
    internal class EventRemoveMember : EventMember
    {
        public EventRemoveMember() : base(SyntaxKind.RemoveAccessorDeclaration) { }
    }
    internal class IndexerGetterMember : IndexerMember
    {
        public IndexerGetterMember() : base(SyntaxKind.GetAccessorDeclaration) { }
    }
    internal class IndexerSetterMember : IndexerMember
    {
        public IndexerSetterMember() : base(SyntaxKind.SetAccessorDeclaration) { }
    }
    #endregion
}
