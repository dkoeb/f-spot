﻿//
// IMetadata.cs
//
// Author:
//   Daniel Köb <daniel.koeb@peony.at>
//
// Copyright (C) 2017 Daniel Köb
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Hyena;
using TagLib;

namespace FSpot.Imaging
{
	public interface IMetadata : IDisposable
	{
		int Width { get; }
		int Height { get; }
		ImageOrientation Orientation { get; set; }
		DateTime? DateTime { get; set; }
		string Comment { get; set; }
		string[] Keywords { get; set; }
		uint? Rating { get; set; }
		string Software { get; set; }
		double? FNumber { get; }
		double? ExposureTime { get; }
		uint? ISOSpeedRatings { get; }
		double? FocalLength { get; }
		string Model { get; }
		string Creator { get; }
		uint? Flash { get; }

		void CopyFrom (IMetadata other);

		void Save ();

		Tag GetTag (TagTypes type);

		void EnsureAvailableTags ();

		void SaveSafely (SafeUri photoUri, bool alwaysSidecar);
	}
}