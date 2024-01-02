using System;
using System.Linq;
using System.Reflection;
using GeofenceServer.Util;

namespace GeofenceServer.Data
{
    public partial class TargetCode : DatabaseClient
    {
		public TargetCode()
		{
			Id = -1;
			TargetUserId = -1;
			Code = "-1";
		}
		public TargetCode(long id, string code)
		{
			Id = -1;
			TargetUserId = id;
			Code = code;
		}
		public TargetCode(TargetCode toCopy) : base(toCopy) { }
	}
}
