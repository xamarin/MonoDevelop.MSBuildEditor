//
// MSBuildParsedDocument.cs
//
// Author:
//       mhutch <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2016 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Xml.Dom;
using MonoDevelop.Xml.Editor;
using System.Collections.Generic;
using System;
using MonoDevelop.Xml.Parser;

namespace MonoDevelop.MSBuildEditor
{
	class MSBuildParsedDocument : XmlParsedDocument
	{
		AnnotationTable<XObject, object> annotations = new AnnotationTable<XObject, object> ();
		string toolsVersion;
		Dictionary<string, ParsedImport> imports = new Dictionary<string, ParsedImport> (StringComparer.OrdinalIgnoreCase);

		Dictionary<string,PropertyInfo> knownProperties = new Dictionary<string,PropertyInfo> (StringComparer.OrdinalIgnoreCase);
		Dictionary<string,ItemInfo> knownItems = new Dictionary<string,ItemInfo> (StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string,TaskInfo> knownTasks = new Dictionary<string,TaskInfo> (StringComparer.OrdinalIgnoreCase);

		public MSBuildParsedDocument (string filename) : base (filename)
		{
		}

		public string ToolsVersion
		{
			get {
				if (toolsVersion != null)
					return toolsVersion;
				if (XDocument.RootElement != null) {
					var att = XDocument.RootElement.Attributes [new XName ("ToolsVersion")];
					if (att != null) {
						var val = att.Value;
						if (!string.IsNullOrEmpty (val))
							return toolsVersion = val;
					}
				}
				return toolsVersion = "2.0";
			}
		}

		void ResolveImport (MSBuildParsedDocument previous, XElement el, string basePath = null)
		{
			var importAtt = el.Attributes [new XName ("Project")];
			string import = importAtt?.Value;
			if (string.IsNullOrWhiteSpace (import)) {
				Add (new Error (ErrorType.Warning, "Empty value", importAtt.Region));
				return;
			}

			//TODO: use property values when resolving imports
			var importEvalCtx = MSBuildResolveContext.CreateImportEvalCtx (ToolsVersion);

			string filename = importEvalCtx.Evaluate (import);

			if (basePath != null) {
				filename = Path.Combine (basePath, filename);
			}

			if (!Platform.IsWindows) {
				filename = filename.Replace ('\\', '/');
			}

			var fi = new FileInfo (filename);

			if (!fi.Exists) {
				Add (new Error (ErrorType.Warning, "Could not resolve import", importAtt.Region));
				return;
			}

			if (imports.ContainsKey (filename)) {
				return;
			}

			//TODO: reparse this when any imports' mtimes change
			ParsedImport parsedImport;
			if (previous != null && previous.imports.TryGetValue (filename, out parsedImport) && parsedImport.TimeStampUtc <= fi.LastWriteTimeUtc) {
				imports [filename] = parsedImport;
				return;
			}

			var xmlParser = new XmlParser (new XmlRootState (), true);
			try {
				bool useBom;
				System.Text.Encoding encoding;
				string text = Core.Text.TextFileUtility.ReadAllText (filename, out useBom, out encoding);
				xmlParser.Parse (new StringReader (text));
			}
			catch (Exception ex) {
				LoggingService.LogError ("Unhandled error parsing xml document", ex);
			}

			imports [filename] = new ParsedImport (filename, xmlParser.RootState.CreateDocument (), fi.LastWriteTimeUtc);
		}
	}
}
