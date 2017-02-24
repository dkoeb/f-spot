//
// ImageImporter.cs
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

using System.Collections.Generic;
using FSpot.Core;
using FSpot.FileSystem;
using FSpot.Imaging;
using FSpot.Import;
using Hyena;
using Mono.Unix;

namespace FSpot.Photos
{
	public class PhotoImporter : IFileImporter
	{
		readonly IFileSystem fileSystem;
		readonly IImageFileFactory factory;
		readonly bool mergeRawAndJpeg;

		public PhotoImporter (IFileSystem fileSystem, IImageFileFactory factory, bool mergeRawAndJpeg)
		{
			this.fileSystem = fileSystem;
			this.factory = factory;
			this.mergeRawAndJpeg = mergeRawAndJpeg;
		}

		public IEnumerable<IMediaFile> Import (IEnumerable<SafeUri> files, out IEnumerable<SafeUri> remainingFiles)
		{
			var list = new List<SafeUri> ();
			remainingFiles = list;
			return ImportInternal (files, list);
		}

		IEnumerable<IMediaFile> ImportInternal (IEnumerable<SafeUri> files, List<SafeUri> remainingFiles)
		{
			var enumerator = files.GetEnumerator ();
			SafeUri file = null;

			while (true) {
				if (file == null) {
					file = NextImageFileOrNull (enumerator, remainingFiles);
					if (file == null)
						break;
				}

				// peek the next file to see if we have a RAW+JPEG combination
				// skip any non-image files
				var nextFile = NextImageFileOrNull (enumerator, remainingFiles);

				SafeUri original;
				SafeUri version = null;
				if (mergeRawAndJpeg && nextFile != null && factory.IsJpegRawPair (file, nextFile)) {
					// RAW+JPEG: import as one photo with versions
					original = factory.IsRaw (file) ? file : nextFile;
					version = factory.IsRaw (file) ? nextFile : file;
					// current and next files consumed in this iteration,
					// prepare to get next file on next iteration
					file = null;
				} else {
					// import current file as single photo
					original = file;
					// forward peeked file to next iteration of loop
					file = nextFile;
				}

				FilePhoto info;
				if (version == null) {
					info = new FilePhoto (original, Catalog.GetString ("Original"), factory, fileSystem);
				} else {
					info = new FilePhoto (original, Catalog.GetString ("Original RAW"), factory, fileSystem);
					info.AddVersion (version, Catalog.GetString ("Original JPEG"));
				}

				yield return info;
			}
		}

		SafeUri NextImageFileOrNull (IEnumerator<SafeUri> enumerator, List<SafeUri> remainingFiles)
		{
			SafeUri nextImageFile = null;
			do {
				if (nextImageFile != null) {
					remainingFiles.Add (nextImageFile);
				}
				if (enumerator.MoveNext ())
					nextImageFile = enumerator.Current;
				else
					return null;
			} while (!factory.HasLoader (nextImageFile));
			return nextImageFile;
		}
	}
}
