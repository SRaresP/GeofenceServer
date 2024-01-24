using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GeofenceServer.Data
{
	public class TargetCodeHandler
	{
		private static readonly int CODE_LENGTH = 8;
		private class DeletionTimer : Timer
		{
			private readonly TargetCode TargetCode;
			public DeletionTimer(double interval, TargetCode entryToDelete) : base(interval)
			{
				TargetCode = entryToDelete;
			}
			public void DeleteEntry(Object source, System.Timers.ElapsedEventArgs e)
			{
				try
				{
					TargetCode.Delete();
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.Message);
				}
				Dispose();
			}
		}
		public class DuplicateCodesException : Exception
		{
			public DuplicateCodesException() : base() { }
			public DuplicateCodesException(string message) : base(message) { }
			public DuplicateCodesException(string message, Exception innerException) : base(message, innerException) { }
			public DuplicateCodesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}
		private static CryptoHashHelper Crypto = new CryptoHashHelper();
		public static void Clear()
		{
			try
			{
				TargetCode.ExecuteNonQuery($"DELETE FROM {TargetCode.TableName} WHERE 1;");
			} catch (Exception e)
			{
				Trace.TraceError(e.Message);
			}
		}
		public static string Get(TargetUser user)
		{
			string code = "-1";
			try
			{
				TargetCode targetCode1 = new TargetCode()
				{
					TargetUserId = user.Id
				};
				TargetCode[] result = targetCode1.LoadMultipleUsingAvailableData().Cast<TargetCode>().ToArray();
				foreach (TargetCode targetCode in result)
				{
					targetCode.Delete();
				}
				string toHash = user.Email + user.NrOfCodeGenerations;
				code = Crypto.GetHash(toHash).Substring(0, CODE_LENGTH).ToUpper();
				++user.NrOfCodeGenerations;
				user.Save();

				TargetCode newEntry = new TargetCode()
				{
					TargetUserId = user.Id,
					Code = code
				};
				newEntry.Save();
				//1.800.000 miliseconds = 30 minutes
				DeletionTimer timer = new DeletionTimer(1800000, newEntry);
				timer.Elapsed += timer.DeleteEntry;
				timer.AutoReset = false;
				timer.Start();
				return code;
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
				return code;
			}
		}
		public static long Validate(string code)
		{
			TargetCode targetCode = new TargetCode()
			{
				Code = code
			};
			TargetCode[] results = targetCode.LoadMultipleUsingAvailableData().Cast<TargetCode>().ToArray();
			switch (results.Count())
			{
				case 0:
					throw new KeyNotFoundException("TargetUser matching passed unique code was not found.");
				case 1:
					long id = results[0].TargetUserId;
					results[0].Delete();
					return id;
				default:
					foreach(TargetCode entry in results)
					{
						entry.Delete();
					}
					throw new DuplicateCodesException("Found duplicate codes in database. Attempted to delete all of them.");
			}
		}
	}
}
