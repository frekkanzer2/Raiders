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
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(MonoScript))]
public class ScriptInspector : Editor
{
	[SerializeField, HideInInspector]
	protected FGTextEditor textEditor = new FGTextEditor();

	[System.NonSerialized]
	protected bool enableEditor = true;

	public static string GetVersionString()
	{
		return "3.0.28, March 2021";
	}
	
	public void OnDisable()
	{
		textEditor.onRepaint = null;
		textEditor.OnDisable();
	}
	
	public override void OnInspectorGUI()
	{
		if (enableEditor)
		{
			textEditor.onRepaint = Repaint;
			textEditor.OnEnable(target);
			enableEditor = false;
		}

		if (Event.current.type == EventType.ValidateCommand)
		{
			if (Event.current.commandName == "ScriptInspector.AddTab")
			{
				Event.current.Use();
				return;
			}
		}
		else if (Event.current.type == EventType.ExecuteCommand)
		{
			if (Event.current.commandName == "ScriptInspector.AddTab")
			{
				FGCodeWindow.OpenNewWindow(null, null, false);
				Event.current.Use();
				return;
			}
		}
		
		DoGUI();
	}

	protected virtual void DoGUI()
	{
		if (textEditor == null)
			return;
		var currentInspector = GetCurrentInspector();
		textEditor.OnInspectorGUI(true, new RectOffset(0, -6, -4, 0), currentInspector);
	}
	
	private static System.Type spotlightWindowType;
	private static System.Type inspectorWindowType;
	private static FieldInfo currentInspectorWindowField;
	private static PropertyInfo currentSpotlightWindowProperty;
	private static System.Type guiViewType;
	private static System.Reflection.PropertyInfo guiViewCurrentProperty;
	private static System.Type hostViewType;
	private static System.Reflection.PropertyInfo hostViewActualViewProperty;

	public static bool IsFocused()
	{
		var windowType = EditorWindow.focusedWindow.GetType();
		return
			windowType.ToString() == "UnityEditor.InspectorWindow" ||
			spotlightWindowType != null && windowType == spotlightWindowType;
	}
 
	static ScriptInspector()
	{
		var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		
		var spotlightAssembly = assemblies.FirstOrDefault(a => a.FullName.StartsWith("Spotlight,"));
		if (spotlightAssembly == null)
		{
			spotlightAssembly = assemblies.FirstOrDefault(a => a.FullName.StartsWith("Assembly-CSharp-Editor,"));
		}
		
		if (spotlightAssembly != null)
		{
			spotlightWindowType = spotlightAssembly.GetType("TakionStudios.Spotlight.Helper");
			if (spotlightWindowType != null)
			{
				currentSpotlightWindowProperty = spotlightWindowType.GetProperty("CurrentWindow",
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			}
		}
		
		inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
		if (inspectorWindowType != null)
		{
			currentInspectorWindowField = inspectorWindowType.GetField("s_CurrentInspectorWindow",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
		}

		guiViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GUIView");
		if (guiViewType != null)
		{
			guiViewCurrentProperty = guiViewType.GetProperty("current");
		}

		hostViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.HostView");
		if (hostViewType != null)
		{
			hostViewActualViewProperty = hostViewType.GetProperty("actualView",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}
	}
	
	public static EditorWindow GetCurrentInspector()
	{
		if (currentSpotlightWindowProperty != null)
		{
			var currentInspector = currentSpotlightWindowProperty.GetValue(null, null) as EditorWindow;
			if (currentInspector != null)
				return currentInspector;
		}
		
		if (currentInspectorWindowField != null)
		{
			return currentInspectorWindowField.GetValue(null) as EditorWindow;
		}
		
		if (guiViewCurrentProperty != null)
		{
			object currentView = guiViewCurrentProperty.GetValue(null, null);
			if (currentView != null && currentView.GetType().IsSubclassOf(hostViewType) && hostViewActualViewProperty != null)
			{
				var actualView = hostViewActualViewProperty.GetValue(currentView, null) as EditorWindow;
				return actualView;
			}
		}
		
		return null;
	}
}

}
