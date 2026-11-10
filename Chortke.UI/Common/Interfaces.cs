using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chortke.UI.Common
{
	public interface ITrackable
	{
		public DateTime CreateDate { get; set; }
	}
	public interface IStorable
	{
		string Save();
		void Restore(string text);
	}
}
