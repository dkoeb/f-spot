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
using FSpot.Database;
using FSpot.FileSystem;
using FSpot.Imaging;
using FSpot.Import;
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
			versions.Add (new FilePhotoVersion { Uri = uri, Name = name });

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

		public Photo Import (IDb db, DbItem roll, PhotoFileTracker tracker, MetadataImporter metadataImporter, IList<Tag> tagsToAttach, SafeUri destinationBase, bool duplicateDetect, bool copyFiles)
		{
			if (IsInvalid) {
				throw new Exception ("Failed to parse metadata, probably not a photo");
			}

			// Do duplicate detection
			if (duplicateDetect && db.Photos.HasDuplicate (this)) {
				return null;
			}

			if (copyFiles) {
				fileSystem.Directory.CreateDirectory (destinationBase);
				// Copy into photo folder.
				tracker.CopyIfNeeded (this, destinationBase);
			}

			// Import photo
			var photo = db.Photos.CreateFrom (this, false, roll.Id);

			bool needs_commit = false;

			// Add tags
			if (tagsToAttach.Count > 0) {
				photo.AddTag (tagsToAttach);
				needs_commit = true;
			}

			// Import XMP metadata
			needs_commit |= metadataImporter.Import (photo, this);

			if (needs_commit) {
				db.Photos.Commit (photo);
			}

			return photo;
		}

		void EnsureMetadataParsed ()
		{
			if (metadata_parsed)
				return;

			using (var image = imageFileFactory.Create (DefaultVersion.Uri)) {
				var metadata = image.Metadata;
				if (metadata != null) {
					var date = metadata.DateTime;
					time = date.HasValue ? date.Value : CreateDate;
					description = metadata.Comment;
				} else {
					throw new Exception ("Corrupt File!");
				}
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
			versions.Add (new FilePhotoVersion { Uri = uri, Name = name });
		}

		class FilePhotoVersion : IPhotoVersion
		{
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
			public SafeUri Uri { get; set; }

			string import_md5 = string.Empty;
			public string ImportMD5 {
				get {
					if (import_md5 == string.Empty)
						import_md5 = HashUtils.GenerateMD5 (Uri);
					return import_md5;
				}
			}
		}
	}
}
