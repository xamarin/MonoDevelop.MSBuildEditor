// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.StandardClassification;
using MonoDevelop.MSBuild.Language;
using MonoDevelop.Xml.Dom;

namespace MonoDevelop.MSBuild.Editor.Host
{
	public class FoundReference
	{
		private string filename;
		private int offset;
		private int length;
		private ReferenceUsage usage;

		public FoundReference (string filename, int offset, int length, ReferenceUsage usage)
		{
			this.filename = filename;
			this.offset = offset;
			this.length = length;
			this.usage = usage;
		}

		FoundReference () { }

		public SourceLocation Location { get; set; }
		public ImageId ImageId { get; set; }
		public ImmutableArray<TaggedText> DisplayParts { get; set; }
		public ImmutableArray<TaggedText> NameDisplayParts { get; set; }
		public ReferenceUsage Kind { get; set; }

		public bool CanNavigateTo (IMSBuildEditorHost host) => Location.CanNavigateTo (host);
		public bool TryNavigateTo (IMSBuildEditorHost host, bool isPreview) => Location.TryNavigateTo (host, isPreview);

		public static FoundReference NoResults { get; } = new FoundReference {
			ImageId = KnownImages.StatusInformation.ToImageId (),
			DisplayParts = ImmutableArray.Create (new TaggedText(
                    "Search found no results",
                PredefinedClassificationTypeNames.NaturalLanguage))
			};

		public bool DisplayIfNoReferences { get; set; }
	}

	public struct TaggedText
	{
		public string Text { get; }
		public string ClassificationType { get; }

		public TaggedText(string text, string classificationType)
        {
			Text = text;
			ClassificationType = classificationType;
		}
	}

	public static class TaggedTextExtensions
	{
		public static string JoinText (this ImmutableArray<TaggedText> taggedText)
			=> string.Concat (taggedText.Select (t => t.Text));

		public static ImmutableArray<T> WhereAsArray<T> (this ImmutableArray<T> array, Func<T, bool> predicate)
			=> ImmutableArray<T>.Empty.AddRange (array.Where (predicate));

		public static ImmutableArray<TOut> SelectAsArray<TIn, TOut> (this ImmutableArray<TIn> array, Func<TIn, TOut> selector)
			=> ImmutableArray<TOut>.Empty.AddRange (array.Select (selector));
	}

	public struct SourceLocation
	{
		public TextSpan SourceSpan { get; internal set; }
		public string FilePath { get; internal set; }
		public int StartOffset { get; internal set; }
		public int StartLine { get; internal set; }
		public int StartCol { get; internal set; }
		public string ProjectName { get; internal set; }
		public string LineText { get; internal set; }
		public ImmutableArray<TaggedText> ClassifiedSpans { get; internal set; }
		public TextSpan Highlight { get; set; }

		public bool CanNavigateTo (IMSBuildEditorHost host)
		{
			return host !=null && System.IO.File.Exists (FilePath);
		}

		public bool TryNavigateTo (IMSBuildEditorHost host, bool isPreview)
		{
			return host.OpenFile (FilePath, StartOffset, isPreview);
		}
	}
}