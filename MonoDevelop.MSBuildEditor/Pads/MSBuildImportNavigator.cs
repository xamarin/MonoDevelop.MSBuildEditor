// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Core;
using MonoDevelop.Ide.Composition;
using MonoDevelop.MSBuild.Editor;
using MonoDevelop.MSBuild.Editor.Completion;
using MonoDevelop.MSBuild.Language;
using MonoDevelop.Xml.Editor.Completion;
using Xwt;

namespace MonoDevelop.MSBuildEditor.Pads
{
	class MSBuildImportNavigator : TreeView
	{
		ActiveEditorTracker editorTracker;
		readonly DataField<string> markupField = new DataField<string> ();
		readonly DataField<Import> importField = new DataField<Import> ();
		readonly DataField<bool> isGroupField = new DataField<bool> ();
		readonly TreeStore store;

		MSBuildParserProvider parserProvider;
		DisplayElementFactory displayElementFactory;
		MSBuildBackgroundParser parser;

		static readonly string colorGroup = "#2CC775";
		static readonly string colorReplacement = "#7DA7FF";
		static readonly string colorUnresolved = "#FF5446";

		public MSBuildImportNavigator ()
		{
			Columns.Add ("Text", new TextCellView { MarkupField = markupField });
			HeadersVisible = false;
			BorderVisible = false;

			store = new TreeStore (markupField, importField, isGroupField);
			DataSource = store;

			editorTracker = new ActiveEditorTracker ();
			editorTracker.ActiveEditorChanged += ActiveEditorChanged;

			ActiveEditorChanged (editorTracker,
				new ActiveEditorChangedEventArgs (
					editorTracker.TextView, null,
					editorTracker.Document, null
				)
			);
		}

		MSBuildParserProvider GetParserProvider () => parserProvider ?? (
			parserProvider = CompositionManager.Instance.GetExportedValue<MSBuildParserProvider> ()
		);

		DisplayElementFactory GetDisplayElementFactory () => displayElementFactory ?? (
			displayElementFactory = CompositionManager.Instance.GetExportedValue<DisplayElementFactory> ()
		);

		void ActiveEditorChanged (object sender, ActiveEditorChangedEventArgs e)
		{
			if (parser != null) {
				parser.ParseCompleted -= ParseCompleted;
			}

			if (e.NewView is ITextView t && t.TextBuffer.ContentType.IsOfType (MSBuildContentType.Name)) {
				parser = GetParserProvider ().GetParser (t.TextBuffer);
				parser.ParseCompleted += ParseCompleted;
			}

			var output = parser?.LastOutput;
			if (output?.MSBuildDocument is MSBuildRootDocument doc) {
				Runtime.RunInMainThread (() => Update (doc));
			} else {
				Runtime.RunInMainThread (() => store.Clear ());
			}
		}

		void ParseCompleted (object sender, ParseCompletedEventArgs<MSBuildParseResult> e)
		{
			var doc = e.ParseResult.MSBuildDocument;
			if (e.ParseResult.MSBuildDocument.ImportsHash == lastImportsHash) {
				return;
			}
			lastImportsHash = doc.ImportsHash;

			Runtime.RunInMainThread (() => Update (doc));
		}

		int lastImportsHash;

		void Update (MSBuildRootDocument doc)
		{
			store.Clear ();
			var shorten = GetDisplayElementFactory ().CreateFilenameShortener (doc.RuntimeInformation);
			AddNode (store.AddNode (), doc, shorten);
			ExpandAll ();
		}

		void AddNode (TreeNavigator treeNavigator, MSBuildDocument document, Func<string,(string prefix, string remaining)?> shorten)
		{
			bool first = true;

			string group = null;

			foreach (var import in document.Imports) {
				bool needsInsert = !first;
				first = false;

				void CloseGroup ()
				{
					if (group != null) {
						treeNavigator.MoveToParent ();
						group = null;
					}
				}

				if (import.OriginalImport.IndexOf('*') > -1 || import.OriginalImport[0] == '(') {
					if (import.OriginalImport != group) {
						CloseGroup ();
						if (needsInsert) {
							treeNavigator.InsertAfter ();
							needsInsert = false;
						}
						treeNavigator.SetValues (
							markupField, $"<span color='{colorGroup}'>{GLib.Markup.EscapeText(import.OriginalImport)}</Span>",
							importField, import,
							isGroupField, true
						);
						if (import.IsResolved) {
							group = import.OriginalImport;
							treeNavigator.AddChild ();
						} else {
							continue;
						}
					}
				} else {
					CloseGroup ();
				}

				if (needsInsert) {
					treeNavigator.InsertAfter ();
				}

				if (import.IsResolved) {
					var shortened = shorten (import.Filename);
					if (shortened.HasValue) {
						treeNavigator.SetValues (
							markupField, $"<span color='{colorReplacement}'>{GLib.Markup.EscapeText (shortened.Value.prefix)}</span>{shortened.Value.remaining}",
							importField, import,
							isGroupField, false
						);
					} else {
						treeNavigator.SetValues (
							markupField, GLib.Markup.EscapeText (import.Filename),
							importField, import,
							isGroupField, false
						);
					}
				} else {
					treeNavigator.SetValues (
						markupField, $"<span color='{colorUnresolved}'>{GLib.Markup.EscapeText (import.OriginalImport)}</span>",
						importField, import,
						isGroupField, false
					);
				}

				if (import.IsResolved && import.Document.Imports.Count > 0) {
					treeNavigator.AddChild ();
					AddNode (treeNavigator, import.Document, shorten);
					treeNavigator.MoveToParent ();
				}
			}
		}

		protected override void OnRowActivated (TreeViewRowEventArgs a)
		{
			var nav = store.GetNavigatorAt (a.Position);
			if (nav != null) {
				var isGroup = nav.GetValue (isGroupField);
				if (!isGroup) {
					var import = nav.GetValue (importField);
					Ide.IdeApp.Workbench.OpenDocument (import.Filename, null, true);
				}
			}
			base.OnRowActivated (a);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (parser != null) {
					parser.ParseCompleted -= ParseCompleted;
					parser = null;
				}
				editorTracker?.Dispose ();
				editorTracker = null;
			}

			base.Dispose (disposing);
		}
	}
}