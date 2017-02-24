//
// FileImportSource.cs
//
// Author:
//   Daniel Köb <daniel.koeb@peony.at>
//   Ruben Vermeersch <ruben@savanne.be>
//
// Copyright (C) 2014 Daniel Köb
// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
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
using System.Linq;
using FSpot.Core;
using FSpot.FileSystem;
using Hyena;

namespace FSpot.Import
{
	public class FileImportSource : IImportSource
	{
		#region fields

		readonly SafeUri root;
		readonly IEnumerable<IFileImporter> fileImporters;
		readonly IFileSystem fileSystem;

		#endregion

		#region ctors

		public FileImportSource (SafeUri root, IEnumerable<IFileImporter> fileImporters, IFileSystem fileSystem)
		{
			this.root = root;
			this.fileImporters = fileImporters;
			this.fileSystem = fileSystem;
		}

		#endregion

		#region IImportSource

		public virtual IEnumerable<FilePhoto> ScanPhotos (bool recurseSubdirectories)
		{
			return ScanPhotoDirectory (recurseSubdirectories, root);
		}

		#endregion

		#region private

		protected IEnumerable<FilePhoto> ScanPhotoDirectory (bool recurseSubdirectories, SafeUri uri)
		{
			IEnumerable<SafeUri> files = new RecursiveFileEnumerator (uri, fileSystem) {
				Recurse = recurseSubdirectories,
				CatchErrors = true,
				IgnoreSymlinks = true
			};

			IEnumerable<FilePhoto> result = Enumerable.Empty<FilePhoto> ();

			foreach (var importer in fileImporters) {
				IEnumerable<SafeUri> remainingFiles;
				result = result.Concat (importer.Import (files, out remainingFiles));
				files = remainingFiles;
			}

			return result;
		}

		#endregion
	}
}
