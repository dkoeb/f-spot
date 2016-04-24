//
// FileFactoryBase.cs
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

using System;
using System.Collections.Generic;
using FSpot.FileSystem;
using FSpot.Utils;
using Hyena;

namespace FSpot.FileSupport
{
	public abstract class FileFactoryBase<T> : IFileFactory<T>
		where T: class
	{
		#region fields

		readonly TinyIoCContainer container;
		protected IFileSystem FileSystem { get; private set; }

		#endregion

		#region ctors

		protected FileFactoryBase (IFileSystem fileSystem)
		{
			container = new TinyIoCContainer ();
			FileSystem = fileSystem;
		}

		#endregion

		#region IFileFactory

		public T Create (SafeUri uri)
		{
			var name = GetLoaderType (uri);
			if (name == null)
				throw new Exception (String.Format ("Unsupported file: {0}", uri));

			try {
				return container.Resolve<T> (name, UriAsParameter (uri));
			} catch (Exception e) {
				Log.DebugException (e);
				throw e;
			}
		}

		public bool HasLoader (SafeUri uri)
		{
			return GetLoaderType (uri) != null;
		}

		#endregion

		#region private/protected

		protected void Register<TImpl> (string type)
			where TImpl: class, T
		{
			container.Register<T, TImpl> (type).AsMultiInstance ();
		}

		string GetLoaderType (SafeUri uri)
		{
			// check if GIO can find the file, which is not the case
			// with filenames with invalid encoding
			if (!FileSystem.File.Exists (uri))
				return null;

			string extension = uri.GetExtension ().ToLower ();

			// Ignore video thumbnails
			if (extension == ".thm")
				return null;

			// Ignore empty files
			if (FileSystem.File.GetSize (uri) == 0)
				return null;

			var param = UriAsParameter (uri);

			// Get loader by mime-type
			string mime = FileSystem.File.GetMimeType (uri);
			if (container.CanResolve<T> (mime, param))
				return mime;

			// Get loader by extension
			return container.CanResolve<T> (extension, param) ? extension : null;
		}

		protected virtual NamedParameterOverloads UriAsParameter (SafeUri uri) {
			return new NamedParameterOverloads (new Dictionary<string, object> {
				{ "uri", uri }
			});
		}

		#endregion
	}
}
