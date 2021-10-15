/* SCRIPT INSPECTOR 3
 * version 3.0.28, March 2021
 * Copyright © 2012-2020, Flipbook Games
 * 
 * Unity's legendary editor for C#, UnityScript, Boo, Shaders, and text,
 * now transformed into an advanced C# IDE!!!
 * 
 * Follow me on http://twitter.com/FlipbookGames
 * Like Flipbook Games on Facebook http://facebook.com/FlipbookGames
 * Join discussion in Unity forums http://forum.unity3d.com/threads/138329
 * Contact info@flipbookgames.com for feedback, bug reports, or suggestions.
 * Visit http://flipbookgames.com/ for more info.
 */


namespace ScriptInspector
{

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum SemanticFlags
{
	None = 0,

	SymbolDeclarationsMask = (1 << 8) - 1,
	ScopesMask = ~SymbolDeclarationsMask,

	SymbolDeclarationsBegin = 1,

	NamespaceDeclaration,
	UsingNamespace,
	UsingAlias,
	UsingStatic,
	ExternAlias,
	ClassDeclaration,
	TypeParameterDeclaration,
	BaseListDeclaration,
	ConstructorDeclarator,
	DestructorDeclarator,
	ConstantDeclarator,
	MethodDeclarator,
	LocalVariableDeclarator,
	ForEachVariableDeclaration,
	FromClauseVariableDeclaration,
	CaseVariableDeclaration,
	LabeledStatement,
	CatchExceptionParameterDeclaration,
	FixedParameterDeclaration,
	ParameterArrayDeclaration,
	ImplicitParameterDeclaration,
	ExplicitParameterDeclaration,
	PropertyDeclaration,
	IndexerDeclaration,
	GetAccessorDeclaration,
	SetAccessorDeclaration,
	EventDeclarator,
	EventWithAccessorsDeclaration,
	AddAccessorDeclaration,
	RemoveAccessorDeclaration,
	VariableDeclarator,
	OperatorDeclarator,
	ConversionOperatorDeclarator,
	StructDeclaration,
	InterfaceDeclaration,
	InterfacePropertyDeclaration,
	InterfaceMethodDeclaration,
	InterfaceEventDeclaration,
	InterfaceIndexerDeclaration,
	InterfaceGetAccessorDeclaration,
	InterfaceSetAccessorDeclaration,
	EnumDeclaration,
	EnumMemberDeclaration,
	DelegateDeclaration,
	AnonymousObjectCreation,
	MemberDeclarator,
	LambdaExpressionDeclaration,
	AnonymousMethodDeclaration,

	SymbolDeclarationsEnd,


	ScopesBegin                   = 1 << 8,

	CompilationUnitScope          = 1 << 8,
	NamespaceBodyScope            = 2 << 8,
	ClassBaseScope                = 3 << 8,
	TypeParameterConstraintsScope = 4 << 8,
	ClassBodyScope                = 5 << 8,
	StructInterfacesScope         = 6 << 8,
	StructBodyScope               = 7 << 8,
	InterfaceBaseScope            = 8 << 8,
	InterfaceBodyScope            = 9 << 8,
	FormalParameterListScope      = 10 << 8,
	EnumBodyScope                 = 11 << 8,
	MethodBodyScope               = 12 << 8,
	ConstructorInitializerScope   = 13 << 8,
	LambdaExpressionScope         = 14 << 8,
	LambdaExpressionBodyScope     = 15 << 8,
	AnonymousMethodScope          = 16 << 8,
	AnonymousMethodBodyScope      = 17 << 8,
	CodeBlockScope                = 18 << 8,
	SwitchBlockScope              = 19 << 8,
	SwitchSectionScope            = 20 << 8,
	ForStatementScope             = 21 << 8,
	EmbeddedStatementScope        = 22 << 8,
	UsingStatementScope           = 23 << 8,
	LocalVariableInitializerScope = 24 << 8,
	SpecificCatchScope            = 25 << 8,
	ArgumentListScope             = 26 << 8,
	AttributeArgumentsScope       = 27 << 8,
	MemberInitializerScope        = 28 << 8,

	TypeDeclarationScope          = 29 << 8,
	MethodDeclarationScope        = 30 << 8,
	AttributesScope               = 31 << 8,
	AccessorBodyScope             = 32 << 8,
	AccessorsListScope            = 33 << 8,
	QueryExpressionScope          = 34 << 8,
	QueryBodyScope                = 35 << 8,
	MemberDeclarationScope        = 36 << 8,

	ScopesEnd,
}

public interface IVisitableTreeNode<NonLeaf, Leaf>
{
	bool Accept(IHierarchicalVisitor<NonLeaf, Leaf> visitor);
}

public interface IHierarchicalVisitor<NonLeaf, Leaf>
{
	bool Visit(Leaf leafNode);
	bool VisitEnter(NonLeaf nonLeafNode);
	bool VisitLeave(NonLeaf nonLeafNode);
}

public class ParseTree
{
	public static uint resolverVersion = 2;
	
	public abstract class BaseNode
	{
		public Node parent;
		public BaseNode nextSibling;
		public short childIndex;
		
		public bool missing;
		public FGGrammar.Node grammarNode;
		public FGGrammar.ErrorMessageProvider syntaxError;
		public string semanticError;
		
		private uint _resolvedVersion = 1;
		private SymbolDefinition _resolvedSymbol;
		public SymbolDefinition resolvedSymbol {
			get {
				if (_resolvedSymbol != null && _resolvedVersion != 0 &&
					(_resolvedVersion != resolverVersion || !_resolvedSymbol.IsValid())
				)
					_resolvedSymbol = null;
				return _resolvedSymbol;
			}
			set {
				if (_resolvedVersion == 0)
				{
#if SI3_WARNINGS
					Debug.LogWarning("Whoops! " + _resolvedSymbol);
#endif
					return;
				}
				_resolvedVersion = resolverVersion;
				_resolvedSymbol = value;
			}
		}
		
		public SymbolDefinition GetDeclaredSymbol()
		{
			if (_resolvedVersion != 0)
				return null;
			return _resolvedSymbol;
		}
		
		public void SetDeclaredSymbol(SymbolDefinition symbol)
		{
			_resolvedSymbol = symbol;
			_resolvedVersion = 0;
		}

		public Leaf FindPreviousLeaf()
		{
			var result = this;
			while (result.childIndex == 0 && result.parent != null)
				result = result.parent;
			if (result.parent == null)
				return null;
			result = result.parent.ChildAt(result.childIndex - 1);
			Node node;
			while ((node = result as Node) != null)
			{
				if (node.numValidNodes == 0)
					return node.FindPreviousLeaf();
				result = node.LastValid;
			}
			return result as Leaf;
		}

		public Leaf FindNextLeaf()
		{
			var result = this;
			while (result.parent != null && result.childIndex == result.parent.numValidNodes - 1)
				result = result.parent;
			if (result.parent == null)
				return null;
			result = result.nextSibling;
			Node node;
			while ((node = result as Node) != null)
			{
				if (node.numValidNodes == 0)
					return node.FindNextLeaf();
				result = node.ChildAt(0);
			}
			return result as Leaf;
		}
		
		public BaseNode FindPreviousNode()
		{
			var result = this;
			while (result.childIndex == 0 && result.parent != null)
				result = result.parent;
			if (result.parent == null)
				return null;
			result = result.parent.ChildAt(result.childIndex - 1);
			return result;
		}

		public abstract void Dump(StringBuilder sb, int indent);

		public bool IsAncestorOf(BaseNode node)
		{
			while (node != null)
				if (node.parent == this)
					return true;
				else
					node = node.parent;
			return false;
		}
		
		public Node FindParentByName(string ruleName)
		{
			var result = parent;
			while (result != null && result.RuleName != ruleName)
				result = result.parent;
			return result;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			Dump(sb, 1);
			return sb.ToString();
		}

		public abstract string Print();

		public bool HasLeafs()
		{
			var it = this;
			do
			{
				var node = it as Node;
				if (node == null)
					return true;
				
				it = node.FirstChild;
				while (it != null && it.childIndex < node.numValidNodes)
				{
					node = it as Node;
					if (node == null)
						return true;
					it = node.FirstChild;
				}
				
				if (node == this)
					return false;
				
				it = node.nextSibling;
				while (it == null || it.childIndex >= it.parent.numValidNodes)
				{
					node = node.parent;
					if (node == this)
						return false;
					it = node.nextSibling;
				}
			} while (true);
		}
		
		public bool HasLeafs(bool validNodesOnly)
		{
			if (validNodesOnly)
				return HasLeafs();
				
			var it = this;
			do
			{
				var node = it as Node;
				if (node == null)
					return true;
				
				it = node.FirstChild;
				while (it != null)
				{
					node = it as Node;
					if (node == null)
						return true;
					it = node.FirstChild;
				}
				
				if (node == this)
					return false;
				
				it = node.nextSibling;
				while (it == null)
				{
					node = node.parent;
					if (node == this)
						return false;
					it = node.nextSibling;
				}
			} while (true);
		}

		public bool HasErrors()
		{
			var it = this;
			do
			{
				var node = it as Node;
				if (node == null)
				{
					if (it.syntaxError != null)
						return true;
					
					if (it == this)
						return false;

					var next = it.nextSibling;
					while (next == null || next.childIndex >= next.parent.numValidNodes)
					{
						it = it.parent;
						if (it == this)
							return false;
						next = it.nextSibling;
					}
					
					it = next;
					continue;
				}
				
				it = node.FirstChild;
				while (it != null && it.childIndex < node.numValidNodes)
				{
					var nextNode = it as Node;
					if (nextNode == null)
					{
						if (it.syntaxError != null)
							return true;
							
						it = it.nextSibling;
						continue;
					}
					node = nextNode;
					it = node.FirstChild;
				}
				
				if (node == this)
					return false;
				
				it = node.nextSibling;
				while (it == null || it.childIndex >= it.parent.numValidNodes)
				{
					node = node.parent;
					if (node == this)
						return false;
					it = node.nextSibling;
				}
			} while (true);
		}
		
		public abstract bool IsLit(string litText);
		
		public Leaf GetFirstLeaf() { return GetFirstLeaf(true); }
		public abstract Leaf GetFirstLeaf(bool validNodesOnly);
				
		public Leaf GetLastLeafInParent()
		{
			if (parent != null)
			{
				if (childIndex >= parent.numValidNodes)
					return null;

				if (nextSibling != null && nextSibling.childIndex < parent.numValidNodes)
				{
					var nextLeaf = nextSibling.GetLastLeafInParent();
					if (nextLeaf != null)
						return nextLeaf;
				}
			}

			var asLeaf = this as Leaf;
			if (asLeaf != null && asLeaf.token != null)
				return asLeaf;

			var asNode = this as Node;
			if (asNode.FirstChild != null)
				return asNode.FirstChild.GetLastLeafInParent();
			
			return null;
		}
	}

	public class Leaf : BaseNode
	{
		public int line {
			get {
				return token != null ? token.Line : 0;
			}
		}
		public int tokenIndex {
			get {
				return token != null && token.formatedLine != null ? token.formatedLine.tokens.IndexOf(token) : 0;
			}
		}
		public SyntaxToken token;

		public Leaf() {}

		public Leaf(FGGrammar.IScanner scanner)
		{
			//line = scanner.CurrentLine() - 1;
			//tokenIndex = scanner.CurrentTokenIndex();
			token = scanner.Current;
			token.parent = this;
		}

		public bool TryReuse(FGGrammar.IScanner scanner)
		{
			if (token == null)
				return false;
			var current = scanner.Current;
			if (current.parent == this)//token.text == current.text && token.tokenKind == current.tokenKind)
			{
				//	line = scanner.CurrentLine() - 1;
				//	tokenIndex = scanner.CurrentTokenIndex();
				token.parent = this;
				return true;
			}
			return false;
		}

		public override void Dump(StringBuilder sb, int indent)
		{
			sb.Append(' ', 2 * indent);
			sb.Append(childIndex);
			sb.Append(" ");
			if (syntaxError != null)
				sb.Append("? ");
			sb.Append(token);
			sb.Append(' ');
			sb.Append((line + 1));
			sb.Append(':');
			sb.Append(tokenIndex);
			if (syntaxError != null)
				sb.Append(' ').Append(syntaxError);
			sb.AppendLine();
		}

		public void ReparseToken()
		{
			if (token != null)
			{
				token.parent = null;
				token = null;
			}
			if (parent != null)
				parent.RemoveNodeAt(childIndex/*, false*/);
		}

		public override string Print()
		{
			var lit = grammarNode as FGGrammar.Lit;
			if (lit != null)
				return lit.pretty;
			return token != null ? token.text : "";
		}

		public override bool IsLit(string litText)
		{
			var lit = grammarNode as FGGrammar.Lit;
			return lit != null && lit.body == litText;
		}

		public override Leaf GetFirstLeaf(bool validNodesOnly)
		{
			return this;
		}
	}

	public class Node : BaseNode
	{
		protected BaseNode firstChild;
		public BaseNode FirstChild {
			get { return firstChild; }
		}
		private BaseNode lastValid;
		public BaseNode LastValid {
			get { return lastValid; }
		}
		public short numValidNodes;

		public Scope scope;
		public SymbolDeclaration declaration;

		public SemanticFlags semantics
		{
			get
			{
				var peer = ((FGGrammar.Id) grammarNode).peer;
				if (peer == null)
					Debug.Log("no peer for " + grammarNode);
				return peer != null ? ((FGGrammar.Rule) peer).semantics : SemanticFlags.None;
			}
		}

		public Node(FGGrammar.Id rule)
		{
			grammarNode = rule;
		}

		public BaseNode ChildAt(int index)
		{
			if (index < 0)
				index += numValidNodes;
			if (index < 0 || index >= numValidNodes)
				return null;
			
			var result = firstChild;
			while (result != null && index --> 0)
				result = result.nextSibling;
				
			return result;
		}

		public Leaf LeafAt(int index)
		{
			if (index < 0)
				index += numValidNodes;
			if (index < 0 || index >= numValidNodes)
				return null;
			
			var result = firstChild;
			while (result != null && index --> 0)
				result = result.nextSibling;
				
			return result as Leaf;
		}

		public Node NodeAt(int index)
		{
			if (index < 0)
				index += numValidNodes;
			if (index < 0 || index >= numValidNodes)
				return null;

			var result = firstChild;
			while (result != null && index --> 0)
				result = result.nextSibling;
				
			return result as Node;
		}

		public string RuleName
		{
			get { return ((FGGrammar.Id) grammarNode).GetName(); }
		}

		//static int createdTokensCounter;
		//static int reusedTokensCounter;
		
		public Leaf AddToken(FGGrammar.IScanner scanner)
		{
			var firstInvalid = lastValid != null ? lastValid.nextSibling : firstChild;
			
			var reused = firstInvalid as Leaf;
			if (reused != null && reused.TryReuse(scanner))
			{
				//	reused.missing = false;
				//	reused.errors = null;

				//reused.parent = this;
				//reused.childIndex = numValidNodes;
				++numValidNodes;
				lastValid = reused;

				//if (++reusedTokensCounter + createdTokensCounter == 1)
				//{
				//	UnityEditor.EditorApplication.delayCall += () => {
				//		Debug.Log("Tokens - Created: " + createdTokensCounter + " Reused: " + reusedTokensCounter);
				//		createdTokensCounter = 0;
				//		reusedTokensCounter = 0;
				//	};
				//}
				
				//Debug.Log("reused " + reused.token.text + " at line " + scanner.CurrentLine());
				return reused;
			}
			
			//if (reusedTokensCounter + ++createdTokensCounter == 1)
			//{
			//	UnityEditor.EditorApplication.delayCall += () => {
			//		Debug.Log("Tokens - Created: " + createdTokensCounter + " Reused: " + reusedTokensCounter);
			//		createdTokensCounter = 0;
			//		reusedTokensCounter = 0;
			//	};
			//}
			
			var leaf = new Leaf(scanner) { parent = this, childIndex = numValidNodes };
			if (lastValid != null)
			{
				leaf.nextSibling = lastValid.nextSibling;
				lastValid.nextSibling = leaf;
				++numValidNodes;
			}
			else
			{
				leaf.nextSibling = firstChild;
				firstChild = leaf;
				++numValidNodes;
			}
			lastValid = leaf;
			
			var next = leaf.nextSibling;
			while (next != null)
			{
				next.childIndex++;
				next = next.nextSibling;
			}

			return leaf;
		}

		public Leaf AddToken(SyntaxToken token)
		{
			var firstInvalid = lastValid != null ? lastValid.nextSibling : firstChild;
			
			if (!token.IsMissing())
			{
				var reused = firstInvalid as Leaf;
				if (reused != null && reused.token.text == token.text && reused.token.tokenKind == token.tokenKind)
				{
					reused.missing = false;
					reused.syntaxError = null;

					reused.token = token;
					reused.parent = this;
					reused.childIndex = numValidNodes;
					++numValidNodes;
					lastValid = reused;

					Debug.Log("reused " + reused.token + " from line " + (reused.line + 1));
					return reused;
				}
			}

			var leaf = new Leaf { token = token, parent = this, childIndex = numValidNodes };
			if (lastValid != null)
			{
				leaf.nextSibling = lastValid.nextSibling;
				lastValid.nextSibling = leaf;
				++numValidNodes;
			}
			else
			{
				leaf.nextSibling = firstChild;
				firstChild = leaf;
				++numValidNodes;
			}
			lastValid = leaf;
			
			var next = leaf.nextSibling;
			while (next != null)
			{
				next.childIndex++;
				next = next.nextSibling;
			}

			return leaf;
		}

		public int InvalidateFrom(int index)
		{
			var numInvalidated = index >= numValidNodes ? 0 : numValidNodes - index;
			if (numInvalidated == 0)
				return 0;
			numValidNodes -= (short) numInvalidated;
			if (numValidNodes == 0)
				lastValid = null;
			else
				lastValid = ChildAt(numValidNodes - 1);
			return numInvalidated;
		}

		public void RemoveNodeAt(int index/*, bool canReuse = true*/)
		{
			if (index == 0)
			{
				if (firstChild == null)
					return;

				//if (!canReuse)
					firstChild.parent = null;
					
				if (index < numValidNodes)
				{
					--numValidNodes;
					if (numValidNodes == 0)
						lastValid = null;
				}
				var node = firstChild as Node;
				firstChild = firstChild.nextSibling;
				if (node != null)
					node.Dispose();
				
				var next = firstChild;
				while (next != null)
				{
					if (next.childIndex == numValidNodes)
						lastValid = next;
					next.childIndex--;
					next = next.nextSibling;
				}
			}
			else
			{
				var prevChild = firstChild;
				for (var i = 1; prevChild != null && i < index; ++i)
					prevChild = prevChild.nextSibling;
				if (prevChild == null || prevChild.nextSibling == null)
					return;

				//if (!canReuse)
					prevChild.nextSibling.parent = null;

				if (index < numValidNodes)
				{
					--numValidNodes;
					lastValid = ChildAt(numValidNodes - 1);
				}
				//if (!canReuse || index <= numValidNodes)
				{
					var node = prevChild.nextSibling as Node;
					prevChild.nextSibling = prevChild.nextSibling.nextSibling;
					if (node != null)
						node.Dispose();
					
					var next = prevChild.nextSibling;
					while (next != null)
					{
						next.childIndex--;
						next = next.nextSibling;
					}
				}
			}
			
			if (parent != null && !HasLeafs(false))
				parent.RemoveNodeAt(childIndex/*, canReuse*/);
		}

		//static int reusedNodesCounter, createdNodesCounter;
		
		public Node AddNode(FGGrammar.Id rule, FGGrammar.IScanner scanner, out bool skipParsing)
		{
			skipParsing = false;

			bool removedReusable = false;
			
			var firstInvalid = lastValid != null ? lastValid.nextSibling : firstChild;
			
			var reusable = firstInvalid as Node;
			if (reusable != null)
			{
				var firstLeaf = reusable.GetFirstLeaf(false);
				if (reusable.grammarNode != rule)
				{
					//	Debug.Log("Cannot reuse (different rules) " + rule.GetName() + " at line " + scanner.CurrentLine() + ":"
					//		+ scanner.CurrentTokenIndex() + " vs. " + reusable.RuleName);
					if (firstLeaf == null || firstLeaf.token == null || firstLeaf.line <= scanner.CurrentLine() - 1)
					{
						reusable.Dispose();
						removedReusable = true;
					}
				}
				else
				{
					if (firstLeaf != null && firstLeaf.token != null && firstLeaf.line > scanner.CurrentLine() - 1)
					{
						// Ignore this node for now
					}
					else if (firstLeaf == null || firstLeaf.token != null && firstLeaf.syntaxError != null)
					{
					//	Debug.Log("Could reuse " + rule.GetName() + " at line " + scanner.CurrentLine() + ":"
					//		+ scanner.CurrentTokenIndex() + " (firstLeaf is null) ");
						reusable.Dispose();
						removedReusable = true;
					}
					else if (firstLeaf.token == scanner.Current)
					{
						var lastLeaf = reusable.GetLastLeaf();
						if (lastLeaf != null && !reusable.HasErrors())
						{
							if (lastLeaf.token != null)
							{
								//if (++reusedNodesCounter + createdNodesCounter == 1)
								//{
								//	UnityEditor.EditorApplication.delayCall += () => {
								//		Debug.Log("Nodes - Created: " + createdNodesCounter + " Reused: " + reusedNodesCounter);
								//		createdNodesCounter = 0;
								//		reusedNodesCounter = 0;
								//	};
								//}
								
								/*var moved =*/ ((CsGrammar.Scanner) scanner).MoveAfterLeaf(lastLeaf);
							//	Debug.Log(moved  + " skipping to " + scanner.CurrentGrammarNode + " at " + lastLeaf.line +":" + lastLeaf.tokenIndex);
								skipParsing = true;
							//}

							//if (lastLeaf == null || lastLeaf.token == null)
							//{
							////	Debug.LogWarning("lastLeaf has no token! " + lastLeaf);
							//}
							//else
							//{
								//	Debug.Log("Reused full " + rule.GetName() + " from line " + (firstLeaf.line + 1) + " up to line " + scanner.CurrentLine() + ":"
								//		+ scanner.CurrentTokenIndex() + " (" + scanner.Current.text + "...) ");
								++numValidNodes;
								lastValid = reusable;
								return scanner.CurrentParseTreeNode;
							}
						}
						else
						{
							//Debug.Log(firstLeaf.line);
							reusable.Dispose();
							removedReusable = true;
						}
					}
					else if (reusable.numValidNodes == 0)
					{
						//if (++reusedNodesCounter + createdNodesCounter == 1)
						//{
						//	UnityEditor.EditorApplication.delayCall += () => {
						//		Debug.Log("Nodes - Created: " + createdNodesCounter + " Reused: " + reusedNodesCounter);
						//		createdNodesCounter = 0;
						//		reusedNodesCounter = 0;
						//	};
						//}
						
						//Debug.Log("Reusing " + rule.GetName() + " at line " + scanner.CurrentLine() + ":"
						//	+ scanner.CurrentTokenIndex() + " (" + scanner.Current.text + "...) reusable.numValidNodes is 0");
						++numValidNodes;
						lastValid = reusable;
						reusable.syntaxError = null;
						reusable.missing = false;
						return reusable;
					}
					else if (scanner.Current != null && (firstLeaf.token == null || firstLeaf.line <= scanner.CurrentLine() - 1))
					{
						//	Debug.Log("Cannot reuse " + rule.GetName() + " at line " + scanner.CurrentLine() + ":"
						//		+ scanner.CurrentTokenIndex() + " (" + scanner.Current.text + "...) ");
						reusable.Dispose();
						if (firstLeaf.token == null || firstLeaf.line == scanner.CurrentLine() - 1)
						{
							removedReusable = true;
						}
						else
						{
							if (lastValid != null)
								lastValid.nextSibling = firstInvalid.nextSibling;
							else
								firstChild = firstInvalid.nextSibling;
							
							var next = firstInvalid.nextSibling;
							while (next != null)
							{
								next.childIndex--;
								next = next.nextSibling;
							}

							return AddNode(rule, scanner, out skipParsing);
						}
					}
					else
					{
					//	Debug.Log("Not reusing anything (scanner.Current is null). reusable is " + reusable.RuleName);
					}
				}
			}

			//if (reusedNodesCounter + ++createdNodesCounter == 1)
			//{
			//	UnityEditor.EditorApplication.delayCall += () => {
			//		Debug.Log("Nodes - Created: " + createdNodesCounter + " Reused: " + reusedNodesCounter);
			//		createdNodesCounter = 0;
			//		reusedNodesCounter = 0;
			//	};
			//}
			
			var node = new Node(rule) { parent = this, childIndex = numValidNodes };
			if (firstInvalid == null)
			{
				if (lastValid != null)
					lastValid.nextSibling = node;
				else
					firstChild = node;
				++numValidNodes;
				lastValid = node;
			}
			else
			{
				if (removedReusable)
				{
					if (lastValid != null)
					{
						lastValid.nextSibling = node;
						node.nextSibling = firstInvalid.nextSibling;
					}
					else
					{
						firstChild = node;
						node.nextSibling = firstInvalid.nextSibling;
					}
				}
				else if (lastValid != null)
				{
					node.nextSibling = lastValid.nextSibling;
					lastValid.nextSibling = node;
				}
				else
				{
					node.nextSibling = firstChild;
					firstChild = node;
				}
				++numValidNodes;
				lastValid = node;
			}
			
			var nextNotValid = node.nextSibling;
			if (nextNotValid != null && nextNotValid.childIndex != numValidNodes)
			{
				for (var i = numValidNodes; nextNotValid != null; ++i, nextNotValid = nextNotValid.nextSibling)
					nextNotValid.childIndex = i;
			}
			
			return node;
		}

		public BaseNode FindChildByName(string name)
		{
			for (var child = firstChild; child != null && child.childIndex < numValidNodes; child = child.nextSibling)
			{
				if (child.grammarNode != null && child.grammarNode.ToString() == name)
					return child;
			}
			return null;
		}

		public BaseNode FindChildByName(string name, string name1)
		{
			for (var child = firstChild; child != null && child.childIndex < numValidNodes; child = child.nextSibling)
			{
				if (child.grammarNode != null && child.grammarNode.ToString() == name)
				{
					var node = child as Node;
					if (node == null)
						return null;

					return node.FindChildByName(name1);
				}
			}
			return null;
		}

		public BaseNode FindChildByName(string name, string name1, string name2)
		{
			for (var child = firstChild; child != null && child.childIndex < numValidNodes; child = child.nextSibling)
			{
				if (child.grammarNode != null && child.grammarNode.ToString() == name)
				{
					var node = child as Node;
					if (node == null)
						return null;

					return node.FindChildByName(name1, name2);
				}
			}
			return null;
		}

		public BaseNode FindChildByName(params string[] name)
		{
			BaseNode result = this;
			foreach (var n in name)
			{
				var node = result as Node;
				if (node == null)
					return null;

				result = null;
				for (var child = node.firstChild; child != null && child.childIndex < node.numValidNodes; child = child.nextSibling)
				{
					if (child.grammarNode != null && child.grammarNode.ToString() == n)
					{
						result = child;
						break;
					}
				}
				if (result == null)
					return null;
			}
			return result;
		}

		public override void Dump(StringBuilder sb, int indent)
		{
			sb.Append(' ', 2 * indent);
			sb.Append(childIndex);
			sb.Append(' ');
			var id = grammarNode as FGGrammar.Id;
			if (id != null && id.Rule != null)
			{
				if (syntaxError != null)
					sb.Append("? ");
				sb.AppendLine(id.Rule.GetNt());
				if (syntaxError != null)
					sb.Append(' ').AppendLine(syntaxError.GetErrorMessage());
			}

			++indent;
			for (var child = firstChild; child != null && child.childIndex < numValidNodes; child = child.nextSibling)
				child.Dump(sb, indent);
		}

		public override string Print()
		{
			var result = string.Empty;
			for (var child = firstChild; child != null && child.childIndex < numValidNodes; child = child.nextSibling)
				result += child.Print();
			return result;
		}

		public override bool IsLit(string litText)
		{
			return false;
		}

		public override Leaf GetFirstLeaf(bool validNodesOnly)
		{
			for (var child = firstChild; child != null && (!validNodesOnly || child.childIndex < numValidNodes); child = child.nextSibling)
			{
				var leaf = child as Leaf;
				if (leaf != null)
					return leaf;
				leaf = ((Node) child).GetFirstLeaf(validNodesOnly);
				if (leaf != null)
					return leaf;
			}
			return null;
		}

		public Leaf GetLastLeaf()
		{
			if (firstChild == null)
				return null;
			
			return firstChild.GetLastLeafInParent();
		}

		public void Exclude()
		{
			if (parent == null || numValidNodes != 1 || firstChild == null || firstChild.nextSibling != null)
				return;
			if (childIndex == 0)
			{
				parent.firstChild = firstChild;
			}
			else
			{
				var prevSibling = parent.ChildAt(childIndex - 1);
				prevSibling.nextSibling = firstChild;
				firstChild.childIndex = childIndex;
			}
			firstChild.parent = parent;
			firstChild.nextSibling = nextSibling;
			
			parent = null;
			nextSibling = null;
			firstChild = null;
		}

		public void CleanUp()
		{
			var child = firstChild;
			if (numValidNodes == 0)
			{
				while (child != null)
				{
					var asNode = child as Node;
					child = child.nextSibling;
					if (asNode != null)
						asNode.Dispose();
				}
				firstChild = null;
				lastValid = null;
				return;
			}

			while (child != null)
			{
				var asNode = child as Node;
				if (asNode != null)
					asNode.CleanUp();
				
				if (child.childIndex >= numValidNodes - 1)
				{
					var lastValid = child;
					child = child.nextSibling;
					lastValid.nextSibling = null;
					
					while (child != null)
					{
						asNode = child as Node;
						child = child.nextSibling;
						if (asNode != null)
							asNode.Dispose();
					}
					break;
				}
				
				child = child.nextSibling;
			}
		}
		
		public void Dispose()
		{
			var child = firstChild;
			while (child != null)
			{
				var asNode = child as Node;
				child = child.nextSibling;
				if (asNode != null)
					asNode.Dispose();
			}
			
			if (declaration != null)// && declaration.scope != null)
			{
				//if (declaration.definition != null)
				//	Debug.Log("Removing " + declaration.definition.ReflectionName);
				//else
				//	Debug.Log("Removing null declaration! " + declaration.kind);
				if (declaration.scope != null)
					declaration.scope.RemoveDeclaration(declaration);
				++ParseTree.resolverVersion;
				if (ParseTree.resolverVersion == 0)
					++ParseTree.resolverVersion;
				declaration = null;
			}
		}
	}

	public Node root;

	public override string ToString()
	{
		var sb = new StringBuilder();
		root.Dump(sb, 0);
		return sb.ToString();
	}
}

[Flags]
public enum IdentifierCompletionsType
{
	None				= 1<<0,
	Namespace			= 1<<1,
	TypeName			= 1<<2,
	ArrayType			= 1<<3,
	NonArrayType		= 1<<4,
	ValueType			= 1<<5,
	SimpleType			= 1<<6,
	ExceptionClassType	= 1<<7,
	AttributeClassType	= 1<<8,
	Member				= 1<<9,
	Static				= 1<<10,
	Value				= 1<<11,
	ArgumentName		= 1<<12,
	MemberName          = 1<<13,
}

public abstract class FGGrammar
{
	public abstract Parser GetParser { get; }

	public abstract IdentifierCompletionsType GetCompletionTypes(ParseTree.BaseNode afterNode);

	public abstract class ErrorMessageProvider
	{
		protected TokenSet lookahead;
		protected Parser parser;
		
		protected ErrorMessageProvider(Parser parser, TokenSet lookahead)
		{
			this.parser = parser;
			this.lookahead = lookahead;
		}
		
		public abstract string GetErrorMessage();
	}
	
	public class MissingTokenErrorMessage : ErrorMessageProvider
	{
		public MissingTokenErrorMessage(Parser parser, TokenSet lookahead)
			: base(parser, lookahead)
		{}
		
		public override string GetErrorMessage()
		{
			return "Syntax error: Expected " + lookahead.ToString(parser);
		}
	}
	
	public class UnexpectedTokenErrorMessage : ErrorMessageProvider
	{
		public UnexpectedTokenErrorMessage(Parser parser, TokenSet lookahead)
			: base(parser, lookahead)
		{}
		
		public override string GetErrorMessage()
		{
			return "Unexpected token! Expected " + lookahead.ToString(parser);
		}
	}
	
	public abstract class IScanner : IEnumerator<SyntaxToken>
	{
		public SyntaxToken Current
		{
			get
			{
				return currentTokenCache ?? (tokens != null ? tokens[currentTokenIndex] : EOF);
			}
		}
		
		object System.Collections.IEnumerator.Current
		{
			get
			{
				return currentTokenCache ?? (tokens != null ? tokens[currentTokenIndex] : EOF);
			}
		}
		
		public void Dispose()
		{
		}

		public abstract bool MoveNext();

		public void Reset()
		{
			currentLine = -1;
			currentTokenIndex = -1;
			//	nonTriviaTokenIndex = 0;
			tokens = null;
		}


		protected string fileName;

		protected FGTextBuffer.FormatedLine[] lines;
		protected List<SyntaxToken> tokens;

		protected int currentLine = -1;
		protected int currentTokenIndex = -1;

		protected static SyntaxToken EOF;

		protected SyntaxToken currentTokenCache;

		protected int maxScanDistance;
		public bool KeepScanning { get { return maxScanDistance > 0; } }


		public int CurrentLine() { return currentLine + 1; }
		public int CurrentTokenIndex() { return currentTokenIndex; }
		
		public SyntaxToken CurrentToken()
		{
			return currentTokenCache ?? (tokens != null ? tokens[currentTokenIndex] : EOF);
		}
		
		public abstract IScanner Clone();
		public abstract void Delete();

		public bool Lookahead(Node node, int maxDistance = int.MaxValue)
		{
			if (tokens == null && currentLine > 0)
				return false;

			//			long laTime;
			//			if (!timeLookaheads.TryGetValue(node, out laTime))
			//				laTime = 0;
			//
			//			int numLAs;
			//			if (!numLookaheads.TryGetValue(node, out numLAs))
			//				numLAs = 0;
			//			numLookaheads[node] = numLAs + 1;
			//
			//			var timer = new Stopwatch();
			//			timer.Start();

			//bool memValue;
			//var id = node as Id;
			//if (id != null)
			//{
			//    if (memoizationTable.TryGetValue(id.peer, out memValue))
			//        return memValue;
			//}
			//else
			//{
			//    if (memoizationTable.TryGetValue(node, out memValue))
			//        return memValue;
			//}
				
			var line = currentLine;
			var index = currentTokenIndex;
			//	var realIndex = nonTriviaTokenIndex;

			var temp = maxScanDistance;
			maxScanDistance = maxDistance;
			var match = node.Scan(this);
			maxScanDistance = temp;

			for (var i = currentLine; i > line; --i)
				if (i < lines.Length)
					lines[i].laLines = Math.Max(lines[i].laLines, i - line);
			
			currentLine = line;
			currentTokenIndex = index;
			//	nonTriviaTokenIndex = realIndex;
			tokens = currentLine < lines.Length ? lines[currentLine].tokens : null;

			//if (id != null)
			//    memoizationTable[id.peer] = match;
			//else
			//    memoizationTable[node] = match;

			//			timer.Stop();
			//			laTime += timer.ElapsedTicks;
			//			timeLookaheads[node] = laTime;

			return match;
		}

		public SyntaxToken Lookahead(int offset, bool skipWhitespace = true)
		{
			if (!skipWhitespace)
			{
				return currentTokenIndex + 1 < tokens.Count ? tokens[currentTokenIndex + 1] : EOF;
			}
				
			var t = tokens;
			var cl = currentLine;
			var cti = currentTokenIndex;

			while (offset --> 0)
			{
				if (!MoveNext())
				{
					tokens = t;
					currentLine = cl;
					currentTokenIndex = cti;
					return EOF;
				}
			}
			var token = tokens[currentTokenIndex];
				
			for (var i = currentLine; i > cl; --i)
				if (i < lines.Length)
					lines[i].laLines = Math.Max(lines[i].laLines, i - cl);

			tokens = t;
			currentLine = cl;
			currentTokenIndex = cti;
			return token;
		}

		public FGGrammar.Node CurrentGrammarNode;
		public ParseTree.Node CurrentParseTreeNode;

		public ParseTree.Leaf ErrorToken;
		public ErrorMessageProvider ErrorMessage;
		public FGGrammar.Node ErrorGrammarNode;
		public ParseTree.Node ErrorParseTreeNode;

		public bool Seeking;

		public abstract void InsertMissingToken(ErrorMessageProvider errorMessage);

		public abstract void CollectCompletions(TokenSet tokenSet);

		public abstract void OnReduceSemanticNode(ParseTree.Node node);

		public abstract void SyntaxErrorExpected(TokenSet lookahead);
	}

	public abstract class Node
	{
		public Node parent;
		public int childIndex;

		public TokenSet lookahead;
		public TokenSet follow;

		public static implicit operator Node(string s)
		{
			return new Lit(s);
		}

		public static Node operator | (Node a, Node b)
		{
			return new Alt(a, b);
		}

		public static Node operator | (Alt a, Node b)
		{
			a.Add(b);
			return a;
		}

		public static Node operator - (Node a, Node b)
		{
			return new Seq(a, b);
		}

		public static Node operator - (Seq a, Node b)
		{
			a.Add(b);
			return a;
		}

		//public static implicit operator Predicate<IScanner> (Node node)
		//{
		//    return (IScanner scanner) =>
		//        {
		//            try
		//            {
		//                node.Parse(scanner.Clone(), new GoalAdapter());
		//            }
		//            catch
		//            {
		//                return false;
		//            }
		//            return true;
		//        };
		//}

		public virtual Node GetNode()
		{
			return this;
		}

		public virtual void Add(Node node)
		{
			throw new Exception(GetType() + ": cannot Add()");
		}

		//public virtual TokenSet GetLookahead()
		//{
		//    return lookahead;
		//}

		public virtual bool Matches(IScanner scanner)
		{
			return lookahead.Matches(scanner.Current);
		}

		public abstract TokenSet SetLookahead(Parser parser);
		
		public abstract TokenSet SetFollow(Parser parser, TokenSet succ);
  		
		public virtual void CheckLL1(Parser parser)
		{
			if (follow == null)
				throw new Exception(this + ": follow not set");
			if (lookahead.MatchesEmpty() && lookahead.Accepts(follow))
				throw new Exception(this + ": ambiguous\n"
					+ "  lookahead " + lookahead.ToString(parser) + "\n"
					+ "  follow " + follow.ToString(parser));
		}
		
		public abstract bool Scan(IScanner scanner);

		public void SyntaxError(IScanner scanner, ErrorMessageProvider errorMessage)
		{
			if (scanner.ErrorMessage != null)
				return;
			//Debug.LogError(errorMessage);
			scanner.ErrorMessage = errorMessage;
		}

		public virtual IEnumerable<Lit> EnumerateLitNodes() { yield break; }

		public virtual IEnumerable<Id> EnumerateIdNodes() { yield break; }

		public virtual IEnumerable<T> EnumerateNodesOfType<T>() where T : Node
		{
			if (this is T)
				yield return (T)this;
		}

		public abstract Node Parse(IScanner scanner);

		public Node Recover(IScanner scanner, out int numMissing)
		{
			numMissing = 0;

			var current = this;
			while (current.parent != null)
			{
				var next = current.parent.NextAfterChild(current, scanner);
				if (next == null)
					break;

				var nextId = next as Id;
				if (nextId != null && nextId.GetName() == "attribute")
					return nextId;

				var nextMatchesScanner = next.Matches(scanner);
				while (next != null && !nextMatchesScanner && next.lookahead.MatchesEmpty())
				{
					next = next.parent.NextAfterChild(next, scanner);
					nextMatchesScanner = next != null && next.Matches(scanner);
				}

				if (nextMatchesScanner && scanner.Current.text == ";" && next is Opt)
				{
					return null;
				}

				//if (next is Many)
				//{
				//    var currentToken = scanner.Current;
				//    var n = 0;
				//    var recoverOnMany = Recover(scanner, out n);
				//    if (recoverOnMany != null && currentToken != scanner.Current)
				//    {
				//        next = recoverOnMany;
				//    }
				//    else
				//    {
				//        next = next.parent.NextAfterChild(next, scanner);
				//    }
				//}
				//if (next == null)
				//    break;

				++numMissing;
				if (nextMatchesScanner)
				{
					//if (scanner.Current.text == ";" && next is Opt)
					//{
					//	Debug.Log(next);
					//	return null;
					//}

					//var clone = scanner.Clone();
					if (scanner.Current.text == "{" ||
						scanner.Current.text == "}" ||
						scanner.Lookahead(next, 3))//next.Scan(clone))
					{
						return next;
					}
//					else
//					{
//						nextMatchesScanner = false;
//					}
				}
				
				if (numMissing <= 1 && scanner.Current.text != "{" && scanner.Current.text != "}")
					using (var laScanner = scanner.Clone())
						if (//laScanner.CurrentLine() == scanner.CurrentLine() &&
							laScanner.MoveNext() && next.Matches(laScanner))
						{
							if (laScanner.Lookahead(next, 3))//.Scan(laScanner))
								return null;
						}

				current = next;
			}
			return null;
		}

		public virtual Node NextAfterChild(Node child, IScanner scanner)
		{
			return parent != null ? parent.NextAfterChild(this, scanner) : null;
		}

		public void CollectCompletions(TokenSet tokenSet, IScanner scanner, int identifierId)
		{
			var clone = scanner.Clone();

			var current = this;
			while (current != null && current.parent != null)
			{
				tokenSet.Add(current.lookahead);
				if (!current.lookahead.MatchesEmpty())
					break;
				current = current.parent.NextAfterChild(current, clone);
			}
			tokenSet.RemoveEmpty();
		}
	}

	public class NameId : Id
	{
		public NameId()
			: base("NAME")
		{}

		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				base.SetLookahead(parser);
				lookahead.Add(new TokenSet(parser.TokenToId("IDENTIFIER")));
			}
			return lookahead;
		}
	}

	public class Id : Node
	{
		protected string name;

		// Token or Rule.
		public Node peer { get; protected set; }

		public FGGrammar.Rule Rule { get { return peer as Rule; } }

		public Id(string name)
		{
			this.name = name;
		}

		public Id Clone()
		{
			var clone = new Id(name) { peer = peer, lookahead = lookahead, follow = follow };
			var token = peer as Token;
			if (token != null)
			{
				clone.peer = token.Clone();
				clone.peer.parent = this;
			}
			return clone;
		}

		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				peer = parser.GetPeer(name);
				if (peer == null)
					Debug.LogError("Parser rule \"" + name + "\" not found!!!");
				else
				{
					peer.parent = this;
					peer.childIndex = 0;
					lookahead = peer.SetLookahead(parser);
				}
			}
			return lookahead;
		}

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			SetLookahead(parser);
			if (peer is Rule)
				peer.SetFollow(parser, succ);

			return lookahead;
		}

		public override void CheckLL1(Parser parser) {}

		public override bool Scan(IScanner scanner)
		{
			return !scanner.KeepScanning || peer.Scan(scanner);
		}

		public override Node Parse(IScanner scanner)
		{
			peer.parent = this;
			var rule = peer as Rule;
			if (rule != null)
			{
				bool skip;
				scanner.CurrentParseTreeNode = scanner.CurrentParseTreeNode.AddNode(this, scanner, out skip);
				if (skip)
					return scanner.CurrentGrammarNode;
			}
			var result2 = peer.Parse(scanner);
			peer.parent = this;
			return result2;
		}

		public override Node NextAfterChild(Node child, IScanner scanner)
		{
			if (peer is Rule)
				scanner.CurrentParseTreeNode = scanner.CurrentParseTreeNode.parent;
			return base.NextAfterChild(this, scanner);
		}

		public override string ToString()
		{
			return name;
		}

		public string GetName()
		{
			return name;
		}
		
		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			yield return this;
		}
	}

	public class Lit : Node
	{
		public readonly string body;
		public string pretty;

		public Lit(string body)
		{
			pretty = body;
			this.body = body.Trim();
		}
		
		public override TokenSet SetLookahead(Parser parser)
		{
			return lookahead ?? (lookahead = new TokenSet(parser.TokenToId(body)));
		}

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			return SetLookahead(parser);
		}
		
		public override void CheckLL1(Parser parser)
		{
		}

		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			if (!lookahead.Matches(scanner.Current.tokenId))
				return false;

			scanner.MoveNext();
			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			if (!lookahead.Matches(scanner.Current.tokenId))
			{
				scanner.SyntaxErrorExpected(lookahead);
				return this;
			}

			scanner.CurrentParseTreeNode.AddToken(scanner).grammarNode = this;
			scanner.MoveNext();
			if (scanner.ErrorMessage == null)
			{
				scanner.ErrorParseTreeNode = scanner.CurrentParseTreeNode;
				scanner.ErrorGrammarNode = scanner.CurrentGrammarNode;
			}

			return parent.NextAfterChild(this, scanner);
		}

		public override string ToString()
		{
			return body; //  "\"" + body + "\"";
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			yield return this;
		}
	}

	// represents alternatives: node { "|" node }.

	public class Alt : Node
	{
		protected List<Node> nodes = new List<Node>();
		
		public Alt(params Node[] nodes)
		{
			foreach (var node in nodes)
				Add(node);
		}

		public override sealed void Add(Node node)
		{
			var idNode = node as Id;
			if (idNode != null)
				node = idNode.Clone();

			var altNode = node as Alt;
			if (altNode != null)
			{
				var count = altNode.nodes.Count;
				for (var i = 0; i < count; ++i)
				{
					var n = altNode.nodes[i];
					n.parent = this;
					nodes.Add(n);
				}
			}
			else
			{
				node.parent = this;
				nodes.Add(node);
			}
		}
		
		public override Node GetNode()
		{
			return nodes.Count == 1 ? nodes[0] : this;
		}
		
		// lookahead of each alternative must be
		// different, but more than one alternative
		// with empty input is allowed.
		// returns lookahead - union of alternatives.
		
		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				lookahead = new TokenSet();
				for (var i = 0; i < nodes.Count; ++i)
				{
					var t = nodes[i];
					if (t is If)
						continue;
					var set = t.SetLookahead(parser);
					if (lookahead.Accepts(set))
					{
						Debug.LogError(this + ": ambiguous alternatives");
						Debug.LogWarning(lookahead.Intersecton(set).ToString(parser));
					}
					lookahead.Add(set);
				}
				for (var i = 0; i < nodes.Count; ++i)
				{
					var t = nodes[i];
					if (t is If)
					{
						var set = t.SetLookahead(parser);
						lookahead.Add(set);
					}
				}
			}
			return lookahead;
		}
		
		// each alternative gets same succ.

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			SetLookahead(parser);
			follow = succ;
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				node.SetFollow(parser, succ);
			}
			return lookahead;
		}

		public override void CheckLL1(Parser parser)
		{
			base.CheckLL1(parser);
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				node.CheckLL1(parser);
			}
		}
   
		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				if (node.Matches(scanner))
					return node.Scan(scanner);
			}
			
			if (!lookahead.MatchesEmpty())
				return false; // throw new Exception(scanner + ": syntax error in Alt");
			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				if (node.Matches(scanner))
				{
					return node.Parse(scanner);
				}
			}
			if (lookahead.MatchesEmpty())
				return NextAfterChild(this, scanner);
			
			scanner.SyntaxErrorExpected(lookahead);
			return this;
		}

		public override string ToString()
		{
			var s = new StringBuilder("( " + nodes[0]);
			for (var n = 1; n < nodes.Count; ++n)
				s.Append(" | " + nodes[n]);
			s.Append(" )");
			return s.ToString();
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateLitNodes())
					yield return subnode;
			}
		}

		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateIdNodes())
					yield return subnode;
			}
		}

		public override IEnumerable<T> EnumerateNodesOfType<T>()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateNodesOfType<T>())
					yield return subnode;
			}
			base.EnumerateNodesOfType<T>();
		}
	}

	public class Many : Node
	{
		protected readonly Node node;
  
		public Many(Node node)
		{
			var idNode = node as Id;
			if (idNode != null)
				node = idNode.Clone();

			node.parent = this;
			this.node = node;
		}

		public override Node GetNode()
		{
			if (node is Opt)	// [{ [ n ] }] -> [{ n }]
				return new Many(node.GetNode());
			
			if (node is Many)	// [{ [{ n }] }] -> [{ n }]
				return node;
			
			return this;
		}

		// lookahead includes empty.
		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				lookahead = new TokenSet(node.SetLookahead(parser));
				lookahead.AddEmpty();
			}
			return lookahead;
		}
		
		// subtree gets succ.

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			SetLookahead(parser);
			follow = succ;
			node.SetFollow(parser, succ);
			return lookahead;
		}
		
		// subtree is checked.
		public override void CheckLL1(Parser parser)
		{
			// trust the predicate!
			//base.CheckLL1(parser);
			if (follow == null)
				throw new Exception(this + ": follow not set");
			node.CheckLL1(parser);
		}

		public override bool Matches(IScanner scanner)
		{
			return node.Matches(scanner);
		}

		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			var ifNode = node as If ?? node as IfNot;
			if (ifNode != null)
			{
				int tokenIndex, line;
				do
				{
					tokenIndex = scanner.CurrentTokenIndex();
					line = scanner.CurrentLine();
					if (!node.Scan(scanner))
						return false;
					if (!scanner.KeepScanning)
						return true;	
				} while (scanner.CurrentTokenIndex() != tokenIndex || scanner.CurrentLine() != line);
			}
			else
			{
				while (lookahead.Matches(scanner.Current.tokenId))
				{
					int tokenIndex = scanner.CurrentTokenIndex();
					int line = scanner.CurrentLine();
					if (!node.Scan(scanner))
						return false;
					if (!scanner.KeepScanning)
						return true;
					if (scanner.CurrentTokenIndex() == tokenIndex && scanner.CurrentLine() == line)
						throw new Exception("Infinite loop!!!");
				}
			}
			return true;
		}

		public override Node NextAfterChild(Node child, IScanner scanner)
		{
		//	if (scanner.ErrorMessage == null || Parse(scanner.Clone()))
				return this;
			
		//	return base.NextAfterChild(child, scanner);
		}

		public override Node Parse(IScanner scanner)
		{
			var ifNode = node as If;
			if (ifNode != null)
			{
				if (!ifNode.Matches(scanner))
					return parent.NextAfterChild(this, scanner);

				var tokenIndex = scanner.CurrentTokenIndex();
				var line = scanner.CurrentLine();
				var nextNode = node.Parse(scanner);
				if (nextNode != this || scanner.CurrentTokenIndex() != tokenIndex || scanner.CurrentLine() != line)
					return nextNode;
				//Debug.Log("Exiting Many " + this + " in goal: " + scanner.CurrentParseTreeNode);
			}
			else
			{
				if (!lookahead.Matches(scanner.Current.tokenId))
					return parent.NextAfterChild(this, scanner);

				var tokenIndex = scanner.CurrentTokenIndex();
				var line = scanner.CurrentLine();
				var nextNode = node.Parse(scanner);
				if (!(nextNode == this && scanner.CurrentTokenIndex() == tokenIndex && scanner.CurrentLine() == line))
					// throw new Exception("Infinite loop!!! while parsing " + scanner.Current + " on line " + scanner.CurrentLine());
					return nextNode;
			}
			return parent.NextAfterChild(this, scanner);
		}

		public override String ToString()
		{
			return "[{ "+node+" }]";
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			foreach (var n in node.EnumerateLitNodes())
				yield return n;
		}

		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			foreach (var n in node.EnumerateIdNodes())
				yield return n;
		}

		public override IEnumerable<T> EnumerateNodesOfType<T>()
		{
			foreach (var n in node.EnumerateNodesOfType<T>())
				yield return n;
			base.EnumerateNodesOfType<T>();
		}
	}

	//protected class Some : Many
	//{
	//    public Some(Node node)
	//        : base(node)
	//    {
	//    }

	//    public override Node GetNode()
	//    {
	//        if (node is Some)			// { { n } } -> { n }
	//            return node;
			
	//        if (node is Opt)		// { [ n ] } -> [{ n }]
	//            return new Many(node.GetNode());
			
	//        if (node is Many)		// { [{ n }] } -> [{ n }]
	//            return node;
			
	//        return this;
	//    }
		
	//    // lookahead results from subtree.
	//    public override TokenSet SetLookahead(Parser parser)
	//    {
	//        if (lookahead == null)
	//            lookahead = node.SetLookahead(parser);
			
	//        return lookahead;
	//    }
  		
	//    // lookahead != follow; check subtree.
	//    public override void CheckLL1(Parser parser)
	//    {
	//        if (follow == null)
	//            throw new Exception(this + ": follow not set");
	//        if (lookahead.Accepts(follow))
	//            throw new Exception(this + ": ambiguous\n"
	//                + "  lookahead " + lookahead.ToString(parser) + "\n"
	//                + "  follow " + follow.ToString(parser));
	//        node.CheckLL1(parser);
	//    }

	//    public override void Parse(IScanner scanner, Goal goal)
	//    {
	//        if (lookahead.Matches(scanner.Current.tokenId))
	//            do
	//                node.Parse(scanner, goal);
	//            while (lookahead.Matches(scanner.Current.tokenId));
	//        else if (!lookahead.MatchesEmpty())
	//            throw new Exception(scanner + ": syntax error in Some");
	//    }

	//    public override string ToString()
	//    {
	//        return "{ " + node + " }";
	//    }
	//}

	protected class Opt : Many
	{
		public Opt(Node node)
			: base(node)
		{
		}

		public override Node GetNode()
		{
		//	if (node is Some)	// [ { n } ] -> [{ n }]
		//		return new Many(node.GetNode());
			
			if (node is Opt)	// [ [ n ] ] -> [ n ]
				return node;
			
			if (node is Many)	// [ [{ n }] ] -> [{ n }]
				return node;
			
			return this;
		}
  
		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			if (lookahead.Matches(scanner.Current.tokenId))
				return node.Scan(scanner);
			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			if (lookahead.Matches(scanner.Current.tokenId))
				return node.Parse(scanner);
			return parent.NextAfterChild(this, scanner);
		}

		public override Node NextAfterChild(Node child, IScanner scanner)
		{
			return parent != null ? parent.NextAfterChild(this, scanner) : null;
		}

		public override string ToString()
		{
			return "[ " + node + " ]";
		}
	}

	protected class If : Opt
	{
		protected readonly string currentTokenText;
		protected readonly Predicate<IScanner> predicate;
		protected readonly Node nodePredicate;
		protected readonly bool debug;

		public If(Predicate<IScanner> pred, Node node, bool debug = false)
			: base(node)
		{
			predicate = pred;
			this.debug = debug;
		}

		public If(Node pred, Node node, bool debug = false)
			: base(node)
		{
			nodePredicate = pred;
			this.debug = debug;
		}

		public If(string currentText, Node pred, Node node, bool debug = false)
		: base(node)
		{
			currentTokenText = currentText;
			nodePredicate = pred;
			this.debug = debug;
		}

		public override Node GetNode()
		{
		//    if (node is Some)	// [ { n } ] -> [{ n }]
		//        return new Many(node.GetNode());

		//    if (node is Opt)	// [ [ n ] ] -> [ n ]
		//        return node;

		//    if (node is Many)	// [ [{ n }] ] -> [{ n }]
		//        return node;

			return this;
		}

		public virtual bool CheckPredicate(IScanner scanner)
		{
			//if (debug)
			//{
			//    var s = scanner.Clone();
			//    Debug.Log(s.Current.tokenKind + " " + s.Current.text);
			//    s.MoveNext();
			//    Debug.Log(s.Current.tokenKind + " " + s.Current.text);
			//}
			if (predicate != null)
				return predicate(scanner);
			else if (nodePredicate != null)
			{
				if (currentTokenText != null && scanner.Current.text != currentTokenText)
					return false;
				return scanner.Lookahead(nodePredicate);
			}
			return false;
		}

		public override bool Matches(IScanner scanner)
		{
			return lookahead.Matches(scanner.Current) && CheckPredicate(scanner);
		}

		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;

			if (lookahead.Matches(scanner.Current.tokenId) && CheckPredicate(scanner))
				return node.Scan(scanner);
			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			if (lookahead.Matches(scanner.Current.tokenId) && CheckPredicate(scanner))
				return node.Parse(scanner);
			else
				return parent.NextAfterChild(this, scanner); // .Parse2(scanner, goal);
		}

		// lookahead doesn't include empty.

		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
				lookahead = new TokenSet(node.SetLookahead(parser));
			return lookahead;
		}

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			if (nodePredicate != null && nodePredicate.follow == null)
				nodePredicate.SetFollow(parser, new TokenSet());
			return base.SetFollow(parser, succ);
		}

		public override string ToString()
		{
			return "[ ?(" + predicate + ") " + node + " ]";
		}
	}

	protected class IfNot : If
	{
		//public IfNot(Predicate<IScanner> pred, Node node)
		//    : base(pred, node)
		//{
		//}

		public IfNot(Node pred, Node node)
			: base(pred, node)
		{
		}

		public override bool CheckPredicate(IScanner scanner)
		{
			return !base.CheckPredicate(scanner);
		}
	}

	public class Seq : Node
	{
		// sequence of subtrees.
		// @serial nodes sequence of subtrees.
		
		private readonly List<Node> nodes = new List<Node>();
		//private readonly int debugLine = -1;

		public Seq(params Node[] nodes)
		{
			foreach (var t in nodes)
				Add(t);
		}

		//public Seq(int debugLine, params Node[] nodes)
		//{
		//	//this.debugLine = debugLine;
		//	foreach (var t in nodes)
		//		Add(t);
		//}

		public override sealed void Add(Node node)
		{
			var idNode = node as Id;
			if (idNode != null)
				node = idNode.Clone();

			var seqNode = node as Seq;
			if (seqNode != null)
				for (var i = 0; i < seqNode.nodes.Count; ++i)
					Add(seqNode.nodes[i]);
			else
			{
				node.parent = this;
				node.childIndex = nodes.Count;
				nodes.Add(node);
			}
		}
		
		// @return nodes[0] if only one.
		public override Node GetNode()
		{
			return nodes.Count == 1 ? nodes[0] : this;
		}
		
		// lookahead is union including first element
		// that does not accept empty input; it
		// includes empty input only if there is
		// no such element.
		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				lookahead = new TokenSet();
				if (nodes.Count == 0)
					lookahead.AddEmpty();
				else
					for (int i = 0; i < nodes.Count; ++i)
					{
						var t = nodes[i];
						var set = t.SetLookahead(parser);
						lookahead.Add(set);
						if (!set.MatchesEmpty())
						{
							lookahead.RemoveEmpty();
							//for (int j = i + 1; j < nodes.Count; ++j)
							//	nodes[j].SetLookahead(parser);
							break;
						}
					}
			}
			return lookahead;
		}
		
		// each element gets successor's lookahead.
		
		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			SetLookahead(parser);
			follow = succ;
			for (var n = nodes.Count; n-- > 0; )
			{
				var prev = nodes[n].SetFollow(parser, succ);
				if (prev.MatchesEmpty())
				{
					prev = new TokenSet(prev);
					prev.RemoveEmpty();
					prev.Add(succ);
				}
				succ = prev;
			}
			return lookahead;
		}

		public override void CheckLL1(Parser parser)
		{
			base.CheckLL1(parser);
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var t = nodes[i];
				t.CheckLL1(parser);
			}
		}

		public override bool Scan(IScanner scanner)
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var t = nodes[i];
				if (!scanner.KeepScanning)
					return true;
				if (!t.Scan(scanner))
					return false;
			}
			return true;
		}

		public override Node NextAfterChild(Node child, IScanner scanner)
		{
			var index = child.childIndex;
			if (++index < nodes.Count)
				return nodes[index];
			return base.NextAfterChild(this, scanner);
		}

		public override Node Parse(IScanner scanner)
		{
			return nodes[0].Parse(scanner);
		}

		public override string ToString()
		{
			var s = new StringBuilder("( ");
			foreach (var t in nodes)
				s.Append(" " + t);
			s.Append(" )");
			return s.ToString();
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateLitNodes())
					yield return subnode;
			}
		}

		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateIdNodes())
					yield return subnode;
			}
		}

		public override IEnumerable<T> EnumerateNodesOfType<T>()
		{
			var count = nodes.Count;
			for (var i = 0; i < count; ++i)
			{
				var node = nodes[i];
				foreach (var subnode in node.EnumerateNodesOfType<T>())
					yield return subnode;
			}
		}
	}

	public class Token : Node
	{
		protected string name;

		public Token(string name, TokenSet lookahead)
		{
			this.name = name;
			this.lookahead = lookahead;
		}

		public Token Clone()
		{
			var clone = new Token(name, lookahead);
			return clone;
		}
		
		// returns a one-element set,
		// initialized by the parser.
		public override TokenSet SetLookahead(Parser parser)
		{
			return lookahead;
		}
		
		// follow doesn't need to be set.
		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			return lookahead;
		}
		
		// follow is not set; nothing to check.
		public override void CheckLL1(Parser parser)
		{
		}
  
		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			if (!lookahead.Matches(scanner.Current.tokenId))
				return false;
			scanner.MoveNext();
			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			if (!lookahead.Matches(scanner.Current.tokenId))
			{
				scanner.SyntaxErrorExpected(lookahead);
				return this;
			}

			scanner.CurrentParseTreeNode.AddToken(scanner).grammarNode = this;
			scanner.MoveNext();
			if (scanner.ErrorMessage == null)
			{
				scanner.ErrorParseTreeNode = scanner.CurrentParseTreeNode;
				scanner.ErrorGrammarNode = scanner.CurrentGrammarNode;
			}

			return parent.NextAfterChild(this, scanner);
		}

		public override string ToString()
		{
			return name;
		}
	}

	public class Parser : Node
	{
		private readonly List<Rule> rules = new List<Rule>();
		public Rule Start { get { return rules[0]; } }

		// maps Rule.nt to rule.
		private readonly Dictionary<string, Rule> nts = new Dictionary<string, Rule>();

		// maps Id.name to Token or Rule; kept for scanner.
		protected Dictionary<string, Node> ids = new Dictionary<string, Node>();

		// maps token number to token name;
		protected string[] tokens;

		protected Dictionary<string, int> tokenToID = new Dictionary<string, int>();

		public Parser(Rule start, FGGrammar grammar)
		{
		//	Grammar = grammar;
			rules.Add(start);
		}

		// adds each rule.
		public void Add(Rule rule)
		{
			var nt = rule.GetNt();
			if (nts.ContainsKey(nt))
				throw new Exception(nt + ": duplicate");
			nts.Add(nt, rule);
			rules.Add(rule);
		}

		public void InitializeGrammar()
		{
			// maps Lit.body to set containing token
			var lits = new HashSet<string>();
			var t = new List<String>();

			foreach (Lit lit in EnumerateLitNodes())
			{
				if (lits.Add(lit.body))
					t.Add(lit.body);
			}
			foreach (var id in EnumerateIdNodes())
			{
				var idName = id.GetName();
				if (ids.ContainsKey(idName))
					continue;

				ids.Add(idName, nts.ContainsKey(idName) ? nts[idName] : null);
				t.Add(idName);
			}

			t.Sort();
			tokens = t.ToArray();
			for (var i = 0; i < tokens.Length; ++i)
			{
				var name = tokens[i];
				tokenToID[name] = i;
				
				if (!lits.Contains(name) && ids[name] == null)
				{
					ids[name] = new Token(name, new TokenSet(i));

					if (name == "NAME")
					{
						ids[name].lookahead.Add(ids["IDENTIFIER"].lookahead);
					}
				}
			}

			SetLookahead(this);
			SetFollow(this, null);
			CheckLL1(this);

			//var sb = new StringBuilder();
			//foreach (var rule in rules)
			//    if (rule.lookahead.Matches(FlipbookGames.ScriptInspector2.CsGrammar.Instance.tokenIdentifier))
			//        sb.AppendLine("Lookahead(" + rule.GetNt() + "): " + rule.lookahead.ToString(this));
			//UnityEngine.Debug.Log(sb.ToString());
		}
		
		public override TokenSet SetLookahead(Parser parser)
		{
			var count = rules.Count;
			for (var i = 0; i < count; ++i)
			{
				var rule = rules[i];
				rule.SetLookahead(this);
			}
			return null;
		}

		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			Start.SetFollow();
			bool followChanged;
			do
			{
				var count = rules.Count;
				for (var i = 0; i < count; ++i)
					rules[i].SetFollow(this);
				followChanged = false;
				count = rules.Count;
				for (var i = 0; i < count; ++i)
					followChanged |= rules[i].FollowChanged();
			} while (followChanged);
			return null;
		}

		public override void CheckLL1(Parser parser)
		{
			var count = rules.Count;
			for (var i = 0; i < count; ++i)
				rules[i].CheckLL1(this);
		}

		public override bool Scan(IScanner scanner)
		{
			throw new InvalidOperationException();
		}

		public override Node Parse(IScanner scanner)
		{
			throw new InvalidOperationException();
		}

		public ParseTree ParseAll(IScanner scanner)
		{
			if (!scanner.MoveNext())
				return null;

			var parseTree = new ParseTree();
			var rootId = new Id(Start.GetNt());
			ids[Start.GetNt()] = Start;
			rootId.SetLookahead(this);
			Start.parent = rootId;
			scanner.CurrentParseTreeNode = parseTree.root = new ParseTree.Node(rootId);
			scanner.CurrentGrammarNode = Start.Parse(scanner);

			scanner.ErrorParseTreeNode = scanner.CurrentParseTreeNode;
			scanner.ErrorGrammarNode = scanner.CurrentGrammarNode;

			while (scanner.CurrentGrammarNode != null)
			{
				var line = scanner.CurrentLine();
				var tokenIndex = scanner.CurrentTokenIndex();
				var rule = scanner.CurrentGrammarNode;
				var node = scanner.CurrentParseTreeNode;

				if (!ParseStep(scanner))
					break;
				
				if (scanner.ErrorMessage == null)
				{
					if (scanner.CurrentParseTreeNode == node && scanner.CurrentGrammarNode == rule && scanner.CurrentTokenIndex() == tokenIndex && scanner.CurrentLine() == line)
					{
						tryToRecover = false;
						//Debug.LogError("Cannot continue parsing - stuck at line " + line + ", token index " + tokenIndex);
						//break;
					}
				}
			}

			//if (scanner.MoveNext())
			//	Debug.LogWarning(scanner + ": trash at end");
			return parseTree;
		}

		public bool tryToRecover = true;

		public bool ParseStep(IScanner scanner)
		{
//			if (scanner.ErrorMessage == null && scanner.ErrorParseTreeNode == null)
//			{
//				scanner.ErrorParseTreeNode = scanner.CurrentParseTreeNode;
//				scanner.ErrorGrammarNode = scanner.CurrentGrammarNode;
//			}

			//scanner.CurrentParseTreeNode.AddToken(scanner).grammarNode = this;
			//scanner.MoveNext();

			//return parent.NextAfterChild(this, scanner);

			if (scanner.CurrentGrammarNode == null)
				return false;

			//var errorGrammarNode = scanner.CurrentGrammarNode;
			//var errorParseTreeNode = scanner.CurrentParseTreeNode;

			//var numValidNodes = scanner.CurrentParseTreeNode.numValidNodes;
			
			var token = scanner.Current;
			if (scanner.ErrorMessage == null)
			{
				while (scanner.CurrentGrammarNode != null)
				{
					scanner.CurrentGrammarNode = scanner.CurrentGrammarNode.Parse(scanner);
					if (scanner.ErrorMessage != null || token != scanner.Current)
						break;
				}

				//if (scanner.CurrentGrammarNode == null)
				//{
				//	Debug.LogError("scanner.CurrentGrammarNode == null");
				//	return false;
				//}

				//if (scanner.ErrorMessage != null)
				//{
				//    Debug.Log("ErrorGrammarNode: " + scanner.ErrorGrammarNode +
				//        "\nErrorParseTreeNode: " + scanner.ErrorParseTreeNode);
				//}

				if (scanner.ErrorMessage == null && token != scanner.Current)
				{
					scanner.ErrorParseTreeNode = scanner.CurrentParseTreeNode;
					scanner.ErrorGrammarNode = scanner.CurrentGrammarNode;
				}
			}
			if (scanner.ErrorMessage != null)
			{
				if (token.tokenKind == SyntaxToken.Kind.EOF)
				{
				//	Debug.LogError("Unexpected end of file in ParseStep");
					return false;
				}

				var missingParseTreeNode = scanner.CurrentParseTreeNode;
				var missingGrammarNode = scanner.CurrentGrammarNode;

				// Rolling back all recent parser state changes
				scanner.CurrentParseTreeNode = scanner.ErrorParseTreeNode;
				scanner.CurrentGrammarNode = scanner.ErrorGrammarNode;
				if (scanner.CurrentParseTreeNode != null)
				{
					var cpt = scanner.CurrentParseTreeNode;
					while (cpt.LastValid != null && !cpt.LastValid.HasLeafs())
						cpt.InvalidateFrom(cpt.LastValid.childIndex);
				}

				if (!tryToRecover)
                {
					tryToRecover = true;
					scanner.CurrentGrammarNode = null;
				}						
				else if (scanner.CurrentGrammarNode != null)
				{
					int numSkipped;
					scanner.CurrentGrammarNode = scanner.CurrentGrammarNode.Recover(scanner, out numSkipped);
				}

				if (scanner.CurrentGrammarNode == null)
				{
					if (token.parent != null)
						token.parent.ReparseToken();
//					if (scanner.ErrorToken == null)
//						scanner.ErrorToken = scanner.ErrorParseTreeNode.AddToken(scanner);
//					else
//						scanner.ErrorParseTreeNode.AddToken(scanner);
					new ParseTree.Leaf(scanner);

					if (cachedErrorGrammarNode == scanner.ErrorGrammarNode)
					{
						token.parent.syntaxError = cachedErrorMessage;
					}
					else
					{
						token.parent.syntaxError = new UnexpectedTokenErrorMessage(this, scanner.ErrorGrammarNode.lookahead);
						cachedErrorMessage = token.parent.syntaxError;
						cachedErrorGrammarNode = scanner.ErrorGrammarNode;
					}
				//	scanner.ErrorMessage = cachedErrorMessage;
				//	Debug.LogError("Skipped " + token + "added to " + token.parent + "\nparent: " + token.parent.parent);


					scanner.CurrentGrammarNode = scanner.ErrorGrammarNode;
					scanner.CurrentParseTreeNode = scanner.ErrorParseTreeNode;

				//	token = scanner.Current;
				//	token.parent = errorParseTreeNode;

					//Debug.Log("Skipping " + scanner.Current.tokenKind + " \"" + scanner.Current.text + "\"");
					if (!scanner.MoveNext())
					{
					//	Debug.LogError("Unexpected end of file");
						return false;
					}
					scanner.ErrorMessage = null;
				}
				else
				{
					//var sb = new StringBuilder();
					//scanner.CurrentParseTreeNode.Dump(sb, 0);
					//Debug.Log("Recovered on " + scanner.CurrentGrammarNode + " (current token: " + scanner.Current +
					//	" at line " + scanner.CurrentLine() + ":" + scanner.CurrentTokenIndex() +
					//	")" + //"\nnumSkipped: " + numSkipped +
					//	"\nin parent: " + scanner.CurrentGrammarNode.parent +
					//	"\nCurrentParseTreeNode is:\n" + sb);

					//if (scanner.ErrorToken == null)
					{
						//var n = scanner.ErrorGrammarNode;
						//while (n != null && !(n is Id))
						//    n = n.parent;
						//scanner.ErrorParseTreeNode.errors = scanner.ErrorParseTreeNode.errors ?? new List<string>();
						//scanner.ErrorParseTreeNode.errors.Add("Not a valid " + n + "! Expected " + scanner.ErrorGrammarNode.lookahead.ToString(this));

						if (missingGrammarNode != null && missingParseTreeNode != null)
						{
							scanner.CurrentParseTreeNode = missingParseTreeNode;
							scanner.CurrentGrammarNode = missingGrammarNode;
						}

						scanner.InsertMissingToken(scanner.ErrorMessage
							?? new MissingTokenErrorMessage(this, missingGrammarNode.lookahead));

						if (missingGrammarNode != null && missingParseTreeNode != null)
						{
							scanner.ErrorMessage = null;
							scanner.ErrorToken = null;
							scanner.CurrentParseTreeNode = missingParseTreeNode;
							scanner.CurrentGrammarNode = missingGrammarNode;
							scanner.CurrentGrammarNode = missingGrammarNode.parent.NextAfterChild(missingGrammarNode, scanner);
						}
					}
					scanner.ErrorMessage = null;
					scanner.ErrorToken = null;
				}
			}

			return true;
		}

		private static ErrorMessageProvider cachedErrorMessage;
		private static Node cachedErrorGrammarNode;

		public override string ToString()
		{
			var s = new StringBuilder(GetType().Name + " {\n");
			foreach (var rule in rules)
				s.AppendLine(rule.ToString(this));
			s.Append("}");
			return s.ToString();
		}

		public int TokenToId(string s)
		{
			int id;
			if (!tokenToID.TryGetValue(s, out id))
				id = -1;
			return id;
		}

		public string GetToken(int tokenId)
		{
			return tokenId >= 0 && tokenId < tokens.Length ? tokens[tokenId] : tokenId + "?";
		}

		// returns Token or Rule for Id.
		public Node GetPeer(string name)
		{
			var peer = ids[name];
			var token = peer as Token;
			if (token != null)
				peer = token.Clone();
			return peer;
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			var count = rules.Count;
			for (var i = 0; i < count; ++i)
				foreach (var node in rules[i].EnumerateLitNodes())
					yield return node;
		}

		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			var count = rules.Count;
			for (var i = 0; i < count; ++i)
				foreach (var node in rules[i].EnumerateIdNodes())
					yield return node;
		}

		public override IEnumerable<T> EnumerateNodesOfType<T>()
		{
			var count = rules.Count;
			for (var i = 0; i < count; ++i)
				foreach (var node in rules[i].EnumerateNodesOfType<T>())
					yield return node;
			base.EnumerateNodesOfType<T>();
		}
	}

	public class Rule : Node
	{
		public static bool debug;
		public SemanticFlags semantics;
		public bool autoExclude;
		public bool contextualKeyword;

		// nonterminal name.
		protected string nt;

		// right hand side subtree.
		protected Node rhs;

		public Rule(string nt, Node rhs)
		{
			var idNode = rhs as Id;
			if (idNode != null)
				rhs = idNode.Clone();

			this.nt = nt;

			rhs.parent = this;
			this.rhs = rhs;
		}
		
		// used to detect left recursion and to
		// flag if follow needs to be recomputed.
		protected bool inProgress;
				
		// gets lookahead from rhs, sets inProgress.
		public override TokenSet SetLookahead(Parser parser)
		{
			if (lookahead == null)
			{
				if (inProgress)
					throw new Exception(nt + ": recursive lookahead");
				inProgress = true;
				lookahead = rhs.SetLookahead(parser);

			}
			return lookahead;
		}
		
		// set if follow has changed.
		protected bool followChanged;

		// resets before recomputing follow.
		public bool FollowChanged()
		{
			inProgress = followChanged;
			followChanged = false;
			return inProgress;
		}
		
		// initializes follow with an empty set.
		// used only once, only for the start rule.
		public void SetFollow()
		{
			follow = new TokenSet();
		}

		// traverses rhs;
		// should reach all rules from start rule.
		public void SetFollow(Parser parser)
		{
			if (lookahead == null)
				throw new Exception(nt + ": lookahead not set");
			if (follow == null)
				return;// throw new Exception(nt + ": not connected");
			if (inProgress)
				rhs.SetFollow(parser, follow);
		}  
		
		// sets or adds to (new) follow set
		// and reports changes to parser.
		// returns lookahead.
		public override TokenSet SetFollow(Parser parser, TokenSet succ)
		{
			if (follow == null)
			{
				followChanged = true;
				follow = new TokenSet(succ);

			}
			else if (follow.Add(succ))
			{
				followChanged = true;
			}
			return lookahead;
		}
		
		public override void CheckLL1(Parser parser)
		{
			if (!contextualKeyword)
			{
				base.CheckLL1(parser);
				rhs.CheckLL1(parser);
			}
		}

		public override bool Scan(IScanner scanner)
		{
			if (!scanner.KeepScanning)
				return true;
			
			if (lookahead.Matches(scanner.Current))
				return rhs.Scan(scanner);
			else if (!lookahead.MatchesEmpty())
				return false; 

			return true;
		}

		public override Node Parse(IScanner scanner)
		{
			return RhsParse2(scanner);
		}

		public override Node NextAfterChild(Node child, IScanner scanner)
		{
			var temp = scanner.CurrentParseTreeNode;
			if (temp == null)
				return null;
			var res = temp.grammarNode != null ? temp.grammarNode.NextAfterChild(this, scanner) : null;

			if (scanner.Seeking)
				return res;

			if (contextualKeyword && temp.numValidNodes == 1)
			{
				var token = temp.LeafAt(0).token;
				token.tokenKind = SyntaxToken.Kind.ContextualKeyword;
			}

			/*if (autoExclude && temp.numValidNodes == 1)
			{
				temp.Exclude();
			}
			else*/ if (temp.semantics != SemanticFlags.None)
			{
				scanner.OnReduceSemanticNode(temp);
			}

			return res;
		}

		private Node RhsParse2(IScanner scanner)
		{
			bool wasError = scanner.ErrorMessage != null;
			Node res = null;
			if (lookahead.Matches(scanner.Current))
			{
				//try
				//{
					res = rhs.Parse(scanner);
				//}
				//catch (Exception e)
				//{
				//	throw new Exception(e.Message + "<=" + this.nt, e);
				//}
			}
			if ((res == null || !wasError && scanner.ErrorMessage != null) && !lookahead.MatchesEmpty())
			{
				scanner.SyntaxErrorExpected(lookahead);
				return res ?? this;
			}
			if (res != null)
				return res;

			return NextAfterChild(rhs, scanner); // ready to be reduced
		}

		public override string ToString()
		{
			return nt + " : " + rhs + " .";
		}

		public string ToString(Parser parser)
		{
			var result = new StringBuilder(nt + " : " + rhs + " .");
			if (lookahead != null)
				result.Append("\n  lookahead " + lookahead.ToString(parser));
			if (follow != null)
				result.Append("\n  follow " + follow.ToString(parser));
			return result.ToString();
		}

		public string GetNt()
		{
			return nt;
		}

		public sealed override IEnumerable<Lit> EnumerateLitNodes()
		{
			foreach (var node in rhs.EnumerateLitNodes())
				yield return node;
		}

		public sealed override IEnumerable<Id> EnumerateIdNodes()
		{
			foreach (var node in rhs.EnumerateIdNodes())
				yield return node;
		}

		public override IEnumerable<T> EnumerateNodesOfType<T>()
		{
			foreach (var node in rhs.EnumerateNodesOfType<T>())
				yield return node;
			base.EnumerateNodesOfType<T>();
		}
	}

	public class TokenSet
	{
		// true if empty input is acceptable.
		protected bool empty;

		// if != null: many elements.
		private BitArray set;

		// else if >= 0: single element.
		private int tokenId = -1;
  
		public int GetDataSet(out BitArray bitArray)
		{
			bitArray = set;
			return tokenId;
		}

		// empty set, doesn't accept even empty input.
		public TokenSet() {}

		public TokenSet(int tokenId)
		{
			this.tokenId = tokenId;
		}

		public TokenSet(TokenSet s)
		{
			empty = s.empty;
			if (s.set != null)
				set = new BitArray(s.set);
			else
				tokenId = s.tokenId;
		}

		public void AddEmpty()
		{
			empty = true;
		}

		public void RemoveEmpty()
		{
			empty = false;
		}

		public bool Remove(int token)
		{
			if (set == null)
			{
				if (token != tokenId)
					return false;
				tokenId = -1;
				return true;
			}
			if (token >= set.Count)
				Debug.LogError("Unknown token " + token);
			bool result = set[token];
			set[token] = false;
			return result;
		}

		// set to accept additional set of tokens.
		// returns true if set changed.
		public bool Add(TokenSet s)
		{
			var result = false;
			if (s.empty && !empty)
			{
				empty = true;
				result = true;
			}
			if (s.set != null)
			{
				if (set != null)
				{
					for (var n = 0; n < s.set.Count; ++n)
					{
						if (s.set[n] && !set[n])
						{
							set.Set(n, true);
							result = true;
						}
					}
				}
				else
				{
					set = new BitArray(s.set);
					if (tokenId >= 0)
					{
						set.Set(tokenId, true);
						tokenId = -1;
					}
					result = true;	// s.set cannot just contain one token
				}
			}
			else if (s.tokenId >= 0)
			{
				if (set != null)
				{
					if (!set.Get(s.tokenId))
					{
						set.Set(s.tokenId, true);
						result = true;
					}
				}
				else if (tokenId >= 0)
				{
					if (tokenId != s.tokenId)
					{
						set = new BitArray(700, false);
						set.Set(s.tokenId, true);
						set.Set(tokenId, true);
						tokenId = -1;
						result = true;
					}
				}
				else
				{
					tokenId = s.tokenId;
					result = true;
				}
			}
			return result;
		}

		// checks if lookahead accepts empty input.
		public bool MatchesEmpty()
		{
			return empty;
		}

		// checks if lookahead accepts input symbol.
		public bool Matches(TokenSet tokenSet)
		{
			if (tokenSet == null)
				return false;
			if (tokenSet.tokenId >= 0)
				return set != null ? set[tokenSet.tokenId] : tokenId == tokenSet.tokenId;
			throw new Exception("matches() botched");
		}

		// checks if lookahead accepts input symbol.
		public bool Matches(SyntaxToken token)
		{
			return set != null ? set[token.tokenId] : token.tokenId == tokenId;
		}

		// checks if lookahead accepts input symbol.
		public bool Matches(int token)
		{
			if (set == null)
				return token == tokenId;
			if (token >= set.Count)
				Debug.LogError("Unknown token " + token);
			return set[token];
		}

		// checks for ambiguous lookahead.
		public bool Accepts(TokenSet s)
		{
			if (s.set != null)
			{
				if (set != null)
				{
					var intersection = new BitArray(set);
					intersection = intersection.And(s.set);
					for (var n = 0; n < intersection.Count; ++n)
						if (intersection[n])
							return true;
				}
				else if (tokenId >= 0)
					return s.set[tokenId];
			}
			else if (s.tokenId >= 0)
			{
				if (set != null)
					return set[s.tokenId];
				if (tokenId >= 0)
					return tokenId == s.tokenId;
			}
			return false;
		}

		public TokenSet Intersecton(TokenSet s)
		{
			if (s.set != null)
			{
				if (set != null)
				{
					var intersection = new BitArray(set);
					intersection = intersection.And(s.set);
					var ts = new TokenSet();
					for (var i = 0; i < intersection.Length; ++i)
						if (intersection[i])
							ts.Add(new TokenSet(i));
					return ts;
				}
				else if (tokenId >= 0 && s.set[tokenId])
					return this;
			}
			else if (s.tokenId >= 0)
			{
				if (set != null && set[s.tokenId])
					return s;
				if (tokenId >= 0 && tokenId == s.tokenId)
					return this;
			}
			return new TokenSet();
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			var delim = "";
			if (empty)
			{
				result.Append("empty");
				delim = ", ";
			}
			if (set != null)
				result.Append(delim + "set " + set);
			else if (tokenId >= 0)
				result.Append(delim + "token " + tokenId);
			return "{" + result + "}";
		}
  
		private string cached;
		public string ToString(Parser parser)
		{
			if (cached != null)
				return cached;

			var result = new StringBuilder();
			var delim = string.Empty;
			if (empty)
			{
				result.Append("[empty]");
				delim = ", ";
			}
			if (set != null)
			{
				for (var n = 0; n < set.Count; ++n)
				{
					if (set.Get(n))
					{
						result.Append(delim + parser.GetToken(n));
						delim = n == set.Count - 2 ? ", or " : ", ";
					}	
				}
			}
			else if (tokenId >= 0)
			{
				result.Append(delim + parser.GetToken(tokenId));
			}
			return cached = result.ToString();
		}
	}

	public abstract int TokenToId(string s);

	public abstract string GetToken(int tokenId);
}

public static class FGGrammarExtensions
{
	public static FGGrammar.Lit ToLit(this string s)
	{
		return new FGGrammar.Lit(s);
	}
}

}
