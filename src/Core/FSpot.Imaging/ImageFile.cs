//
// ImageFile.cs
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

using System.IO;
using FSpot.Cms;
using FSpot.Imaging.FileTypes;
using Gdk;
using Hyena;

namespace FSpot.Imaging
{
	class ImageFile : IImageFile
	{
		#region fields

		readonly FileTypeFactory fileTypeFactory;
		BaseImageFile file;
		IMetadata metadata;

		#endregion

		#region props

		BaseImageFile File => file ?? (file = fileTypeFactory.Create (Uri));

		public SafeUri Uri { get; }

		public ImageOrientation Orientation => Metadata?.Orientation ?? ImageOrientation.TopLeft;

		public IMetadata Metadata => metadata ?? (metadata = MetadataService.Parse (Uri));

		#endregion

		#region ctors

		public ImageFile (SafeUri uri, FileTypeFactory fileTypeFactory)
		{
			Uri = uri;
			this.fileTypeFactory = fileTypeFactory;
		}

		#endregion

		#region public API

		public Pixbuf Load ()
		{
			using (Stream stream = PixbufStream ()) {
				var orig = new Pixbuf (stream);
				return TransformAndDispose (orig);
			}
		}

		public Pixbuf Load (int maxWidth, int maxHeight)
		{
			using (var full = Load ()) {
				return full.ScaleToMaxSize (maxWidth, maxHeight);
			}
		}

		public Profile GetProfile ()
		{
			return File.GetProfile ();
		}

		public Stream PixbufStream ()
		{
			return File.PixbufStream (Uri, Metadata);
		}

		#endregion

		#region private

		Pixbuf TransformAndDispose (Pixbuf orig)
		{
			if (orig == null)
				return null;

			try {
				return orig.TransformOrientation (Orientation);
			}
			finally {
				orig.Dispose ();
			}
		}

		#endregion

		#region IDisposable

		public void Dispose ()
		{
			metadata?.Dispose ();
			metadata = null;
		}

		#endregion
	}
}