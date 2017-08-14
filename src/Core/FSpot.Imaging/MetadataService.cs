//
// MetadataService.cs
//
// Author:
//   Ruben Vermeersch <ruben@savanne.be>
//
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

using System;
using System.IO;
using FSpot.Utils;
using Hyena;
using TagLib;
using TagLib.Xmp;
using FSpot.FileSystem;

namespace FSpot.Imaging
{
	static class MetadataService
	{
		public static ImageMetadata Parse (SafeUri uri, IFileSystem fileSystem)
		{
			// Detect mime-type
			string mime;
			try {
				mime = fileSystem.File.GetMimeType (uri);
			} catch (Exception e) {
				Hyena.Log.DebugException (e);
				return null;
			}

			if (mime.StartsWith ("application/x-extension-", StringComparison.Ordinal)) {
				// Works around broken metadata detection - https://bugzilla.gnome.org/show_bug.cgi?id=624781
				mime = string.Format ($"taglib/{mime.Substring (24)}");
			}

			// Parse file
			var res = new TagLibFileAbstraction (fileSystem) { Uri = uri };
			var sidecarUri = GetSidecarUri (uri, fileSystem);
			var sidecarRes = new TagLibFileAbstraction (fileSystem) { Uri = sidecarUri };

			TagLib.Image.File file;
			try {
				file = TagLib.File.Create (res, mime, ReadStyle.Average) as TagLib.Image.File;
			} catch (Exception) {
				Hyena.Log.DebugFormat ($"Loading of metadata failed for file: {uri}, trying extension fallback");

				try {
					file = TagLib.File.Create (res, ReadStyle.Average) as TagLib.Image.File;
				} catch (Exception e) {
					Hyena.Log.DebugFormat ($"Loading of metadata failed for file: {uri}");
					Hyena.Log.DebugException (e);
					return null;
				}
			}

			// Load XMP sidecar
			if (fileSystem.File.Exists (sidecarUri)) {
				ParseXmpSidecar (file, sidecarRes);
			}

			return new ImageMetadata (file);
		}

		delegate SafeUri GenerateSideCarName (SafeUri photoUri);
		static readonly GenerateSideCarName [] SidecarNameGenerators = {
			p => new SafeUri (p.AbsoluteUri + ".xmp"),
			p => p.ReplaceExtension (".xmp")
		};

		internal static SafeUri GetSidecarUri (SafeUri photoUri, IFileSystem fileSystem)
		{
			// First probe for existing sidecar files, use the one that's found.
			foreach (var generator in SidecarNameGenerators) {
				var name = generator (photoUri);
				if (fileSystem.File.Exists (name)) {
					return name;
				}
			}

			// Fall back to the default strategy.
			return SidecarNameGenerators [0] (photoUri);
		}

		/// <summary>
		///    Parses the XMP file identified by resource and replaces the XMP
		///    tag of file by the parsed data.
		/// </summary>
		public static bool ParseXmpSidecar (TagLib.Image.File file, TagLib.File.IFileAbstraction resource)
		{
			string xmp;

			try {
				using (var stream = resource.ReadStream) {
					using (var reader = new StreamReader (stream)) {
						xmp = reader.ReadToEnd ();
					}
				}
			} catch (Exception e) {
				Hyena.Log.DebugFormat ($"Sidecar cannot be read for file {file.Name}");
				Hyena.Log.DebugException (e);
				return false;
			}

			XmpTag tag = null;
			try {
				tag = new XmpTag (xmp, file);
			} catch (Exception e) {
				Hyena.Log.DebugFormat ($"Metadata of Sidecar cannot be parsed for file {file.Name}");
				Hyena.Log.DebugException (e);
				return false;
			}

			var xmp_tag = file.GetTag (TagTypes.XMP, true) as XmpTag;
			xmp_tag.ReplaceFrom (tag);
			return true;
		}

		public static bool SaveXmpSidecar (TagLib.Image.File file, TagLib.File.IFileAbstraction resource)
		{
			var xmp_tag = file.GetTag (TagTypes.XMP, false) as XmpTag;
			if (xmp_tag == null) {
				// TODO: Delete File
				return true;
			}

			var xmp = xmp_tag.Render ();

			try {
				using (var stream = resource.WriteStream) {
					stream.SetLength (0);
					using (var writer = new StreamWriter (stream)) {
						writer.Write (xmp);
					}
					resource.CloseStream (stream);
				}
			} catch (Exception e) {
				Hyena.Log.DebugFormat ($"Sidecar cannot be saved: {resource.Name}");
				Hyena.Log.DebugException (e);
				return false;
			}

			return true;
		}
	}
}
