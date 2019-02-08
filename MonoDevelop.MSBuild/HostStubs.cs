// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using MonoDevelop.MSBuild.Language;
using MonoDevelop.MSBuild.Schema;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo ("MonoDevelop.MSBuild.Tests")]

namespace MonoDevelop.MSBuild
{
	static class LoggingService
	{
		public static void LogDebug (string message) => throw new NotImplementedException ();
		public static void LogError (string message, Exception ex) => throw new NotImplementedException ();
		public static void LogError (string message) => throw new NotImplementedException ();
		internal static void LogWarning (string v) => throw new NotImplementedException ();
	}

	static class Ambience
	{
		public static string GetSummaryMarkup (IMethodSymbol symbol) => throw new NotImplementedException ();
		public static string GetSummaryMarkup (IParameterSymbol symbol) => throw new NotImplementedException ();
		public static string GetSummaryMarkup (ITypeSymbol symbol) => throw new NotImplementedException ();
		public static string GetSummaryMarkup (IPropertySymbol symbol) => throw new NotImplementedException ();
	}

	static class Extensions
	{
		public static ITypeSymbol GetReturnType (this IPropertySymbol property) => throw new NotImplementedException ();
		public static ITypeSymbol GetReturnType (this IMethodSymbol method) => throw new NotImplementedException ();
	}

	static class Markup
	{
		public static string EscapeText (string text) => throw new NotImplementedException ();
	}

	static class MSBuildProjectService
	{
		public static string FromMSBuildPath (string directory, string file) => throw new NotImplementedException ();
	}

	static class TextEditorFactory
	{
		public static ITextSource CreateNewDocument (string filename) => new StringTextSource (File.ReadAllText (filename), filename);
		public static ITextSource CreateNewDocument (string content, string filename) => new StringTextSource (content, filename);

		class StringTextSource : ITextSource
		{
			string content;

			public StringTextSource (string content, string filename)
			{
				this.content = content;
				FileName = filename;
			}

			public string FileName { get; set; }
			public int Length => content.Length;
			public TextReader CreateReader () => new StringReader (content);
			public char GetCharAt (int offset) => content[offset];
			public string GetTextBetween (int begin, int end) => content.Substring (begin, end - begin);
		}
	}

	static class MSBuildHost
	{
		public static Compilation GetMSBuildCompilation () => throw new NotImplementedException ();

		public static class Options
		{
			public static bool ShowPrivateSymbols => false;
		}

		public static IMSBuildEvaluationContext CreateEvaluationContext (IRuntimeInformation runtimeInformation, string projectPath, string thisFilePath) => throw new NotImplementedException ();

		public static TaskMetadataBuilder CreateTaskMetadataBuilder (MSBuildRootDocument doc) => throw new NotImplementedException ();
	}

	interface IMSBuildEvaluationContext
	{
		IEnumerable<string> EvaluatePathWithPermutation (string pathExpression, string baseDirectory, PropertyValueCollector propVals);
	}
}