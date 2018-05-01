using System;

namespace Skuld.Models
{
    public struct SqlResult : IEquatable<SqlResult>
	{
		public bool Successful;
		public string Error;
		public object Data;
		public Exception Exception;

		public override bool Equals(Object obj)
		{
			return obj is SqlResult && this == (SqlResult)obj;
		}
		public bool Equals(SqlResult obj)
		{
			return this == obj;
		}

		public static bool operator ==(SqlResult x, SqlResult y)
		{
			if (x.Successful == y.Successful &&
				x.Error == y.Error &&
				x.Data == y.Data &&
				x.Exception == y.Exception)
				return true;
			return false;
		}
		public static bool operator !=(SqlResult x, SqlResult y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Successful.GetHashCode();
				hash = hash * 23 + Error?.GetHashCode() ?? 0;
				hash = hash * 23 + Data?.GetHashCode() ?? 0;
				hash = hash * 23 + Exception?.GetHashCode() ?? 0;
				return hash;
			}
		}
	}
}
