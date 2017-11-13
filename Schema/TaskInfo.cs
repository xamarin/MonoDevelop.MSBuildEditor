// Copyright (c) 2016 Xamarin Inc.
// Copyright (c) Microsoft. ALl rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MonoDevelop.MSBuildEditor.Schema
{
	class TaskInfo : BaseInfo
	{
		public HashSet<string> Parameters { get; internal set; }

		public TaskInfo (string name, string description)
			: base (name, description)
		{
			Parameters = new HashSet<string> ();
		}
	}
}