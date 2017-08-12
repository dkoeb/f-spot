//
// ImageFileFactory.cs
//
// Author:
//7  Daniel Köb <daniel.koeb@peony.at>
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

using FSpot.FileSystem;
using Gdk;
using Hyena;

namespace FSpot.Imaging
{
	class ImageFileFactory : IImageFileFactory
	{
		readonly FileTypeFactory fileTypeFactory;

		public ImageFileFactory (IFileSystem fileSystem)
		{
			fileTypeFactory = new FileTypeFactory (fileSystem);
		}

		public IImageFile Create (SafeUri uri)
		{
			return new ImageFile (uri, fileTypeFactory);
		}

		public bool HasLoader (SafeUri uri)
		{
			return fileTypeFactory.HasLoader (uri);
		}

		public bool IsJpeg (SafeUri uri)
		{
			return fileTypeFactory.IsJpeg (uri);
		}

		public bool IsRaw (SafeUri uri)
		{
			return fileTypeFactory.IsRaw (uri);
		}

		public bool IsJpegRawPair (SafeUri file1, SafeUri file2)
		{
			return fileTypeFactory.IsJpegRawPair (file1, file2);
		}

		public void CreateDerivedVersion (SafeUri source, SafeUri destination, uint jpegQuality, Pixbuf pixbuf)
		{
			fileTypeFactory.CreateDerivedVersion (source, destination, jpegQuality, pixbuf);
		}
	}
}
