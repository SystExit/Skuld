using System;

namespace Skuld.Models
{
    public struct SqlResult
	{
		public bool Successful;
		public string Error;
		public object Data;
		public Exception Exception;
    }
}
