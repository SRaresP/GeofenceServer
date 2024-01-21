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
			Id = DEFAULT_ID;
			TargetUserId = DEFAULT_ID;
			Code = "-1";
		}
		public TargetCode(TargetCode toCopy) : base(toCopy) { }
	}
}
