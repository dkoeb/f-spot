//
// JobStore.cs
//
// Author:
//   Mike Gemünde <mike@gemuende.de>
//   Stephane Delcroix <sdelcroix@src.gnome.org>
//
// Copyright (C) 2007-2010 Novell, Inc.
// Copyright (C) 2010 Mike Gemünde
// Copyright (C) 2007-2008 Stephane Delcroix
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
using Banshee.Kernel;
using FSpot.Core;
using FSpot.Jobs;

namespace FSpot.Database
{
	public abstract class Job : DbItem, IJob
	{
		protected Job (IDb db, uint id, string job_options, JobPriority job_priority, DateTime run_at, bool persistent)
			: base (id)
		{
			JobOptions = job_options;
			JobPriority = job_priority;
			RunAt = run_at;
			Persistent = persistent;
			Db = db;
		}

		public string JobOptions { get; set; }
		internal JobPriority JobPriority { get; set; }
		//Not in use yet !
		public DateTime RunAt { get; private set; }
		public bool Persistent { get; private set; }
		protected IDb Db { get; private set; }

		public event EventHandler Finished;

		JobStatus status;
		public JobStatus Status
		{
			get { return status; }
			set {
				status = value;
				switch (value) {
				case JobStatus.Finished:
				case JobStatus.Failed:
					if (Finished != null)
						Finished (this, new EventArgs ());
					break;
				}
			}
		}

		public void Run ()
		{
			Status = JobStatus.Running;
			Status = Execute () ? JobStatus.Finished : JobStatus.Failed;
		}

		protected abstract bool Execute ();
	}
}
