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

using UnityEngine;
using UnityEditor;
using System.Linq;
using ScriptInspector;


public class FGTooltip : FGPopupWindow
{
	private string _text;
	public string text { get { return _text; } private set { _text = value; textLines = null; } }
	private string[] textLines;

	private FGTextEditor textEditor;

	private SymbolDefinition[] overloads;
	private int currentOverload;

	private GUIStyle style;
	private GUIStyle boldStyle;
	
	private ParseTree.Leaf tokenLeaf;
	
	private bool isTokenTooltip = true;
	private int currentParameterIndex = -1;
	public int CurrentParameterIndex {
		set {
			if (currentParameterIndex != value)
			{
				currentParameterIndex = value;
				Repaint();
			}
		}
	}
	
	
	public static FGTooltip CreateTokenWidget(FGTextEditor editor, Rect tokenRect, ParseTree.Leaf leaf, bool horizontal = false)
	{
		var tool = Create(editor, tokenRect, leaf, horizontal, false);
		tool.isTokenTooltip = false;
		return tool;
	}
	
	public static FGTooltip Create(FGTextEditor editor, Rect tokenRect, ParseTree.Leaf leaf, bool horizontal = false, bool showError = true)
	{
		string tooltipText = null;
		var symbolDefinition = leaf.resolvedSymbol;
		SymbolDefinition[] overloads = null;
		int currentOverload = 0;
		if (symbolDefinition != null)
		{
			try
			{
				//Debug.Log("Creating tooltip: " + symbolDefinition.GetTooltipText());
				
				ConstructedTypeDefinition constructedType = symbolDefinition.parentSymbol as ConstructedTypeDefinition;

				if (symbolDefinition.kind == SymbolKind.MethodGroup)
				{
					var group = symbolDefinition as MethodGroupDefinition;
					if (group == null)
					{
						var constructedGroup = symbolDefinition as ConstructedSymbolReference;
						if (constructedGroup != null)
						{
							var genericGroup = constructedGroup.referencedSymbol as MethodGroupDefinition; 
							if (constructedType != null)
								symbolDefinition = constructedType.GetConstructedMember(genericGroup.methods.FirstOrDefault());
						}
					}
					if (group != null && group.methods != null)
						symbolDefinition = group.methods.FirstOrDefault() ?? symbolDefinition;
					//else
					//	Debug.Log("Can't convert to MethodGroupDefinition. " + symbolDefinition.GetTooltipText());
				}
				
				tooltipText = GetTooltipText(symbolDefinition, leaf);
				//tooltipText += symbolDefinition.IsValid();

				var methodGroup = symbolDefinition;
				if (methodGroup.parentSymbol != null)
				{
					if (methodGroup.parentSymbol.kind == SymbolKind.MethodGroup)
					{
						var constructedMethodGroup = methodGroup.parentSymbol as ConstructedMethodGroupDefinition;
						methodGroup = constructedMethodGroup ?? methodGroup.parentSymbol;
					}
					else
					{
						if (constructedType != null)
						{
							var constructedMethod = methodGroup as ConstructedSymbolReference;
							methodGroup = constructedMethod != null ?
								constructedMethod.referencedSymbol.parentSymbol as MethodGroupDefinition : null;
						}
					}
				}
				if (methodGroup != null && methodGroup.kind == SymbolKind.MethodGroup)
				{
					var group = methodGroup as MethodGroupDefinition;
					if (group != null && group.methods.Count > 1)
					{
						var methodOverloads = new MethodDefinition[group.methods.Count];
						//group.methods.CopyTo(methodOverloads);
						Scope leafScope = null;
						for (var i = leaf.parent; i != null; i = i.parent)
							if (i.scope != null)
							{
								leafScope = i.scope;
								break;
							}
						
						if (MethodGroupDefinition.methodCandidatesStack.Count != 0)
							throw new System.IndexOutOfRangeException();
						
						var numCandidates = group.CollectCandidates(-1, leafScope, null);
						if (numCandidates != 0)
						{
							methodOverloads = MethodGroupDefinition.methodCandidatesStack.ToArray();
							MethodGroupDefinition.methodCandidatesStack.Clear();
							if (constructedType != null)
							{
								overloads = new SymbolDefinition[methodOverloads.Length];
								for (int i = 0; i < overloads.Length; ++i)
									overloads[i] = constructedType.GetConstructedMember(methodOverloads[i]);
							}
							else
							{
								overloads = methodOverloads;
							}
							currentOverload = System.Array.IndexOf(overloads, symbolDefinition);
							//Debug.Log(currentOverload + " " + symbolDefinition.GetTooltipText());
							if (currentOverload == -1)
							{
								currentOverload = System.Array.IndexOf(overloads, symbolDefinition.GetGenericSymbol());
								//Debug.Log(currentOverload + " " + symbolDefinition.GetTooltipText());
							}
							currentOverload = Mathf.Clamp(currentOverload, 0, overloads.Length - 1);
						}
					}
					//else if (group == null)
					//	Debug.Log("Can't convert to MethodGroupDefinition. " + symbolDefinition);
				}
				//else if (methodGroup != null)
				//	Debug.Log("symbolDefinition: " + symbolDefinition.GetType());
				//if (overloads != null)
				//{
				//	var constructedMethodGroup = methodGroup as ConstructedMethodGroupDefinition;
				//	if (constructedMethodGroup != null)
				//	{
				//		var typeParameters = constructedMethodGroup.GetTypeParameters();
				//		Debug.Log(string.Join(", ", (from x in typeParameters select x.ToString()).ToArray()));
				//	}
				//}
				
				if (overloads == null && symbolDefinition.kind == SymbolKind.Method)
				{
					overloads = new SymbolDefinition[] { symbolDefinition };
					currentOverload = 0;
				}
			}
			catch (System.Exception e)
			{
				//	Debug.LogException(e);
				tooltipText = e.ToString();
			}
		}
		if (showError)
		{
			if (leaf.syntaxError != null)
			{
				tooltipText = leaf.syntaxError.GetErrorMessage();
			}
			else if (leaf.semanticError != null && (symbolDefinition == null || symbolDefinition.kind != SymbolKind.Error))
			{
				if (string.IsNullOrEmpty(tooltipText))
					tooltipText = leaf.semanticError;
				else
					tooltipText = string.Concat(tooltipText, "\n\nSemantic error:\n\t", leaf.semanticError);
			}
		}
		
		if (string.IsNullOrEmpty(tooltipText))
			return null;
		
		Rect position = horizontal
			? new Rect(tokenRect.xMax, tokenRect.y, 1f, 1f)
			: new Rect(tokenRect.x, tokenRect.yMax, 1f, 1f);
		
		var owner = EditorWindow.focusedWindow;

		var window = CreatePopup<FGTooltip>();
		window.wantsMouseMove = true;
		window.dropDownRect = tokenRect;
		window.horizontal = horizontal;
		window.hideFlags = HideFlags.HideAndDontSave;
		
		window.textEditor = editor;
		window.minSize = Vector2.one;
		window.owner = owner;
		window.tokenLeaf = leaf;
		window.text = tooltipText;
		window.overloads = overloads;
		window.currentOverload = currentOverload;

		window.style = editor.styles.tooltipTextStyle;
		window.style.normal.textColor = SISettings.useStandardColorInPopups ? editor.CurrentTheme.text : editor.CurrentTheme.tooltipText;
		window.style.font = EditorStyles.standardFont;
		window.style.fontSize = SISettings.fontSizeDelta + 11;
		
		window.boldStyle = new GUIStyle(window.style);
		window.boldStyle.font = EditorStyles.boldFont;
		
		window.position = position;
		
		window.ShowTooltip();

		return window;
	}
	
	private static string GetTooltipText(SymbolDefinition symbol, ParseTree.Leaf leaf)
	{
		if (symbol.kind == SymbolKind.Method)
		{
			var method = symbol as MethodDefinition;
			if (method != null && method.IsExtensionMethod)
			{
				var nodeLeft = leaf.parent;
				if (nodeLeft != null && nodeLeft.RuleName == "accessIdentifier")
				{
					nodeLeft = nodeLeft.FindPreviousNode() as ParseTree.Node;
					if (nodeLeft != null &&
						(nodeLeft.RuleName == "primaryExpressionPart" || nodeLeft.RuleName == "primaryExpressionStart"))
					{
						var symbolLeft = FGResolver.GetResolvedSymbol(nodeLeft);
						if (symbolLeft != null && symbolLeft.kind != SymbolKind.Error && !(symbolLeft is TypeDefinitionBase))
							return symbol.GetTooltipTextAsExtensionMethod();
					}
				}
			}
		}
		return symbol.GetTooltipText();
	}
	
	public void Hide()
	{
		overloads = null;
		currentOverload = 0;
		text = null;
		if (this == textEditor.tokenTooltip)
		{
			textEditor.mouseHoverTime = default(System.DateTime);
			textEditor.mouseHoverToken = null;
			textEditor.tokenTooltip = null;
		}
		else if (this == textEditor.argumentsHint)
		{
			textEditor.argumentsHint = null;
			textEditor.CloseArgumentsHint();
		}
		if (!this)
			return;
		
//#if UNITY_2018_3_OR_NEWER
//		Close();
//#else
		EditorApplication.delayCall += Close;
//#endif
		//DestroyImmediate(this);
	}

	public void OnGUI()
	{
		OnExternalGUI(Vector2.zero);
	}
	
	public void OnExternalGUI(Vector2 offset)
	{
		if (isTokenTooltip && Event.current.type == EventType.MouseMove ||
			Event.current.type == EventType.ScrollWheel ||
			Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape ||
			textEditor.styles.tooltipFrameStyle == null)
		{
			EditorApplication.delayCall -= Hide;
			EditorApplication.delayCall += Hide;
			return;
		}
		if (this.text == null)
			return;

		var text = this.text;
		if (overloads != null && overloads.Length > 1)
		{
			if (horizontal)
				text = "\u25c4" + (currentOverload + 1).ToString() + " of " + overloads.Length.ToString() + "\u25ba " + text;
			else
				text = "\u25b2" + (currentOverload + 1).ToString() + " of " + overloads.Length.ToString() + "\u25bc " + text;
		}

		//wantsMouseMove = true;

		//if (focusedWindow == this && owner != null)
		//{
		//	owner.Focus();
		////	GUIUtility.ExitGUI();
		//}
		
		if (Event.current.type == EventType.Layout)
		{
			var content = new GUIContent(text);
			style.fixedWidth = 0f;
			Vector2 size = style.font != null ? style.CalcSize(content) : Vector2.zero;
			if (size.x > 700f)
			{
				size.x = 700f;
				size.y = style.CalcHeight(content, size.x);
				style.fixedWidth = size.x;
				//size = style.CalcSize(content);
			}
			SetSize(size.x + 10f, size.y + 10f);
			return;
		}

	//	if (Event.current.type == EventType.Repaint)
		{
			if (SISettings.useStandardColorInPopups)
			{
				//GUI.Label(new Rect(0f, 0f, position.width, position.height), GUIContent.none, textEditor.styles.lineNumbersSeparator);
				var rc1 = new Rect(offset.x + 1f, offset.y + 1f, position.width - 2f, position.height - 2f);
				GUI.Label(rc1, GUIContent.none, textEditor.styles.scrollViewStyle);
			}
			else
			{
				var rc1 = new Rect(offset.x, offset.y, position.width, position.height);
				GUI.Label(rc1, GUIContent.none, textEditor.styles.tooltipFrameStyle);
				rc1 = new Rect(offset.x + 1f, offset.y + 1f, position.width - 2f, position.height - 2f);
				GUI.Label(rc1, GUIContent.none, textEditor.styles.tooltipBgStyle);
			}
			
			if (!isTokenTooltip && textLines == null)
				textLines = text.Split(new [] {'\n'});
			
			var rc = new Rect(4f + offset.x, 4f + offset.y, position.width - 4f, position.height - 4f);
			if (isTokenTooltip || currentParameterIndex < 0 ||
				overloads == null || currentOverload < 0 || currentOverload >= overloads.Length ||
				overloads[currentOverload].GetParameters().Count <= currentParameterIndex)
			{
				GUI.Label(rc, text, style);
			}
			else
			{
				for (int i = 0; i < textLines.Length; ++i)
				{
					var lineContent = new GUIContent(textLines[i]);
					if (i != currentParameterIndex + 1)
					{
						GUI.Label(rc, lineContent, style);
						rc.yMin += style.CalcHeight(lineContent, rc.width);
					}
					else
					{
						GUI.Label(rc, lineContent, boldStyle);
						rc.yMin += boldStyle.CalcHeight(lineContent, rc.width);
					}
				}
			}
		}
	}
	
	public void OnOwnerGUI()
	{
		if (isTokenTooltip && Event.current.type == EventType.Layout)
		{
			var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			//Debug.Log(dropDownRect + "\n" + mousePos);
			if (!dropDownRect.Contains(mousePos))
			{
				Hide();
				return;
			}
		}
		
		if (Event.current.type == EventType.ScrollWheel)
		{
			Hide();
			return;
		}
		
		if (Event.current.type == EventType.KeyDown)
		{
			if (!(Event.current.alt || Event.current.command || Event.current.control || Event.current.shift))
			{
				if (overloads != null && overloads.Length > 1)
				{
					var showNext = horizontal ? Event.current.keyCode == KeyCode.RightArrow : Event.current.keyCode == KeyCode.DownArrow;
					var showPrev = horizontal ? Event.current.keyCode == KeyCode.LeftArrow : Event.current.keyCode == KeyCode.UpArrow;
					
					if (showPrev || showNext)
					{
						Event.current.Use();
						currentOverload = (currentOverload + overloads.Length + (showNext ? 1 : -1)) % overloads.Length;
						text = GetTooltipText(overloads[currentOverload], tokenLeaf);
						//text += overloads[currentOverload].IsValid();
						RepaintOnUpdate();
						return;
					}
				}

				if (Event.current.keyCode == KeyCode.Escape)
				{
					Event.current.Use();
					Hide();
					return;
				}
			}
			else if (!isTokenTooltip && (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.DownArrow))
			{
				Event.current.modifiers = Event.current.modifiers & ~(EventModifiers.Control | EventModifiers.Command);
				Hide();
			}
			
		    if (isTokenTooltip)
				Hide();
		}
	}
	
	private void RepaintOnUpdate()
	{
		EditorApplication.update += DelayedRepaint;
	}
	
	private void DelayedRepaint()
	{
		EditorApplication.update -= DelayedRepaint;
		Repaint();
	}
}
