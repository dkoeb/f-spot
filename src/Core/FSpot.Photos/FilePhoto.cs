//
// FilePhoto.cs
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
using System.Collections.Generic;
using System.Linq;
using FSpot.Core;
using FSpot.FileSystem;
using FSpot.Imaging;
using FSpot.Utils;
using Hyena;
using Mono.Unix.Native;

namespace FSpot.Photos
{
	public class FilePhoto : IPhoto, IInvalidPhotoCheck, IMediaFile
	{
		bool metadata_parsed;

		readonly IImageFileFactory imageFileFactory;
		readonly List<IPhotoVersion> versions;
		readonly IFileSystem fileSystem;

		public FilePhoto (SafeUri uri, IImageFileFactory imageFileFactory, IFileSystem fileSystem) : this (uri, null, imageFileFactory, fileSystem)
		{
		}

		public FilePhoto (SafeUri uri, string name, IImageFileFactory imageFileFactory, IFileSystem fileSystem)
		{
			versions = new List<IPhotoVersion> ();
			versions.Add (new FilePhotoVersion (uri, imageFileFactory) { Name = name });

			this.imageFileFactory = imageFileFactory;
			this.fileSystem = fileSystem;
		}

		public bool IsInvalid {
			get {
				if (metadata_parsed)
					return false;

				try {
					EnsureMetadataParsed ();
					return false;
				} catch (Exception) {
					return true;
				}
			}
		}

		void EnsureMetadataParsed ()
		{
			if (metadata_parsed)
				return;

			var metadata = DefaultVersion.ImageFile.Metadata;
			if (metadata != null) {
				var date = metadata.DateTime;
				time = date.HasValue ? date.Value : CreateDate;
				description = metadata.Comment;
			} else {
				throw new Exception ("Corrupt File!");
			}

			metadata_parsed = true;
		}

		DateTime CreateDate {
			get {
				var cTime = fileSystem.File.GetCTime (DefaultVersion.Uri);
				return NativeConvert.ToDateTime ((long)cTime);
			}
		}

		public Tag[] Tags {
			get { return null; }
		}

		DateTime time;
		public DateTime Time {
			get {
				EnsureMetadataParsed ();
				return time;
			}
		}

		public IPhotoVersion DefaultVersion {
			get {
				return versions.First ();
			}
		}

		public IEnumerable<IPhotoVersion> Versions {
			get {
				return versions;
			}
		}

		string description;
		public string Description {
			get {
				EnsureMetadataParsed ();
				return description;
			}
		}

		public string Name {
			get { return DefaultVersion.Uri.GetFilename (); }
		}

		public uint Rating {
			//FIXME ndMaxxer: correct?
			get { return 0; }
		}

		public void AddVersion(SafeUri uri, string name)
		{
			versions.Add (new FilePhotoVersion (uri, imageFileFactory) { Name = name });
		}

		class FilePhotoVersion : IPhotoVersion
		{
			readonly IImageFileFactory imageFileFactory;

			public string Name { get; set; }

			public bool IsProtected {
				get { return true; }
			}

			public SafeUri BaseUri {
				get { return Uri.GetBaseUri (); }
			}
			public string Filename {
				get { return Uri.GetFilename (); }
			}
			public SafeUri Uri {
				get { return ImageFile.Uri; }
				set { ImageFile = imageFileFactory.Create (value); }
			}

			string import_md5 = string.Empty;
			public string ImportMD5 {
				get {
					if (import_md5 == string.Empty)
						import_md5 = HashUtils.GenerateMD5 (Uri);
					return import_md5;
				}
			}

			public IImageFile ImageFile { get; private set; }

			public FilePhotoVersion (SafeUri uri, IImageFileFactory imageFileFactory)
			{
				this.imageFileFactory = imageFileFactory;
				Uri = uri;
			}
		}
	}
}
