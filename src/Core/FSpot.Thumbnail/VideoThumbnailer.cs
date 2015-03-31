//
// VideoThumbnailer.cs
//
// Author:
//   Daniel Köb <daniel.koeb@peony.at>
//
// Copyright (C) 2016 Daniel Köb
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

using System.Diagnostics;
using Hyena;

namespace FSpot.Thumbnail
{
	class VideoThumbnailer : IThumbnailer
	{
		#region consts

		const string TotemVideoThumbnailer = "totem-video-thumbnailer";

		#endregion

		#region fields

		readonly SafeUri fileUri;

		#endregion

		#region ctors

		public VideoThumbnailer (SafeUri fileUri)
		{
			this.fileUri = fileUri;
		}

		#endregion

		#region IThumbnailer implementation

		public bool TryCreateThumbnail (SafeUri thumbnailUri, ThumbnailSize size)
		{
			var startInfo =
				new ProcessStartInfo (
					TotemVideoThumbnailer,
					string.Format ("-s {0} \"{1}\" \"{2}\"", size == ThumbnailSize.Large ? "256" : "128",
						SafeUri.UriToFilename(fileUri), SafeUri.UriToFilename(thumbnailUri)))
				{
					UseShellExecute = false,
					RedirectStandardOutput = true
				};

			var process = Process.Start (startInfo);

			// totem video thumbnailer writes errors to stdout and sets exit code to 0
			// e.g., when target file name is invalid
			var output = process.StandardOutput.ReadToEnd ();
			var success = string.IsNullOrEmpty (output);

			process.WaitForExit ();
			success &= process.ExitCode == 0;
			process.Dispose ();

			return success;
		}

		#endregion
	}
}
