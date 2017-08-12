//
// BaseImageFile.cs
//
// Author:
//   Stephane Delcroix <stephane@delcroix.org>
//   Ruben Vermeersch <ruben@savanne.be>
//   Stephen Shaw <sshaw@decriptor.com>
//
// Copyright (C) 2007-2010 Novell, Inc.
// Copyright (C) 2007-2009 Stephane Delcroix
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

using System.IO;
using FSpot.Cms;
using Gdk;
using Hyena;

namespace FSpot.Imaging.FileTypes
{
	class BaseImageFile : IImageFile
	{
		#region fields

		bool disposed;

		#endregion

		#region props

		public SafeUri Uri { get; }

		public ImageOrientation Orientation { get; private set; }

		#endregion

		#region ctors

		public BaseImageFile (SafeUri uri)
		{
			Uri = uri;
			Orientation = ImageOrientation.TopLeft;

			using (var metadataFile = MetadataService.Parse (uri)) {
				ExtractMetadata (metadataFile);
			}
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

		// FIXME this need to have an intent just like the loading stuff.
		public virtual Profile GetProfile ()
		{
			return null;
		}

		public virtual Stream PixbufStream ()
		{
			Log.DebugFormat ("open uri = {0}", Uri);
			return new GLib.GioStream (GLib.FileFactory.NewForUri (Uri).Read (null));
		}

		#endregion

		#region protected API

		protected virtual void ExtractMetadata (IMetadata metadata)
		{
			if (metadata != null)
				Orientation = metadata.Orientation;
		}

		protected virtual void Close ()
		{
		}

		#endregion

		#region private

		Pixbuf TransformAndDispose (Pixbuf orig)
		{
			if (orig == null)
				return null;

			Pixbuf rotated = orig.TransformOrientation (Orientation);

			orig.Dispose ();

			return rotated;
		}

		#endregion

		#region IDisposable

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;
			disposed = true;

			if (disposing)
				Close ();
		}

		#endregion
	}
}
