//
// ImageMetadata.cs
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
using File = TagLib.Image.File;

namespace FSpot.Utils
{
	class ImageMetadata : IMetadata
	{
		readonly File metadata;

		#region ctors

		public ImageMetadata (File metadata)
		{
			this.metadata = metadata;
		}
		
		#endregion

		#region props

		public int Width => metadata.Properties?.PhotoWidth ?? 0;

		public int Height => metadata.Properties?.PhotoHeight ?? 0;

		public ImageOrientation Orientation
		{
			get { return metadata.ImageTag.Orientation.ConvertTo (); }
			set { metadata.ImageTag.Orientation = value.ConvertFrom (); }
		}

		public DateTime? DateTime
		{
			get { return metadata.ImageTag.DateTime; }
			set { metadata.ImageTag.DateTime = value; }
		}

		public string Comment
		{
			get { return metadata.ImageTag.Comment; }
			set { metadata.ImageTag.Comment = value; }
		}

		public string[] Keywords
		{
			get { return metadata.ImageTag.Keywords; }
			set { metadata.ImageTag.Keywords = value; }
		}

		public uint? Rating
		{
			get { return metadata.ImageTag.Rating; }
			set { metadata.ImageTag.Rating = value; }
		}

		public string Software
		{
			get { return metadata.ImageTag.Software; }
			set { metadata.ImageTag.Software = value; }
		}

		public double? FNumber => metadata.ImageTag.FNumber;

		public double? ExposureTime => metadata.ImageTag.ExposureTime;

		public uint? ISOSpeedRatings => metadata.ImageTag.ISOSpeedRatings;

		public double? FocalLength => metadata.ImageTag.FocalLength;

		public string Model => metadata.ImageTag.Model;

		public string Creator => metadata.ImageTag.Creator;

		public uint? Flash => metadata.ImageTag.Exif?.ExifIFD.GetLongValue (0, (ushort) TagLib.IFD.Tags.ExifEntryTag.Flash);

		#endregion

		#region public API

		public void CopyFrom (IMetadata other)
		{
			metadata.CopyFrom (((ImageMetadata) other).metadata);
		}

		public void Save ()
		{
			metadata.Save ();
		}

		public Tag GetTag (TagTypes type)
		{
			return metadata.GetTag (type);
		}

		public void EnsureAvailableTags ()
		{
			metadata.EnsureAvailableTags ();
		}

		public void SaveSafely (SafeUri photoUri, bool alwaysSidecar)
		{
			if (alwaysSidecar || !metadata.Writeable || metadata.PossiblyCorrupt) {
				if (!alwaysSidecar && metadata.PossiblyCorrupt) {
					Log.WarningFormat (
						$"Metadata of file {photoUri} may be corrupt, refusing to write to it, falling back to XMP sidecar.");
				}

				var sidecarRes = new GIOTagLibFileAbstraction {Uri = MetadataService.GetSidecarUri (photoUri)};

				metadata.SaveXmpSidecar (sidecarRes);
			}
			else {
				metadata.Save ();
			}
		}

		#endregion

		#region IDisposable

		public void Dispose ()
		{
			metadata?.Dispose ();
		}

		#endregion
	}
}