// Copyright (c) 2016 Xamarin Inc.
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.MSBuild.Language;
using MonoDevelop.MSBuild.Util;

namespace MonoDevelop.MSBuild.Evaluation
{
	/// <summary>
	/// Provides MSBuild properties built into the runtime itself
	/// </summary>
	class MSBuildRuntimeEvaluationContext : IMSBuildEvaluationContext
	{
		readonly Dictionary<string, MSBuildPropertyValue> values
			= new Dictionary<string, MSBuildPropertyValue> (StringComparer.OrdinalIgnoreCase);

		public MSBuildRuntimeEvaluationContext (IRuntimeInformation runtime)
		{
			string tvString = MSBuildToolsVersion.Unknown.ToVersionString ();
			string binPath = MSBuildEscaping.ToMSBuildPath (null, runtime.GetBinPath ());
			string toolsPath = MSBuildEscaping.ToMSBuildPath (null, runtime.GetToolsPath ());

			var searchPaths = runtime.GetSearchPaths ();

			Convert ("MSBuildExtensionsPath");
			Convert ("MSBuildExtensionsPath32");
			Convert ("MSBuildExtensionsPath64");

			void Convert (string name)
			{
				if (searchPaths.TryGetValue (name, out var vals)) {
					values[name] = new MSBuildPropertyValue (vals.ToArray ());
				}
			}

			values["MSBuildBinPath"] = binPath;
			values["MSBuildToolsPath"] = toolsPath;
			values["MSBuildToolsPath32"] = toolsPath;
			values["MSBuildToolsPath64"] = toolsPath;
			values["RoslynTargetsPath"] = $"{binPath}\\Roslyn";
			values["MSBuildToolsVersion"] = tvString;
			values["VisualStudioVersion"] = "15.0";

			var defaultSdksPath = runtime.GetSdksPath ();
			if (defaultSdksPath != null) {
				values["MSBuildSDKsPath"] = MSBuildEscaping.ToMSBuildPath (null, defaultSdksPath);
			}
		}

		public bool TryGetProperty (string name, out MSBuildPropertyValue value) => values.TryGetValue (name, out value);
	}
}