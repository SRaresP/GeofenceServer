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
		private class TargetCode
		{
			[Key]
			public int Id { get; set; }
			[Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
			//this Id is also the user ID
			public int TargetUserId { get; set; }
			//THIS # IS SUPPOSED TO BE CODE_LENGTH
			[MaxLength(8, ErrorMessage = "Code was longer than the specified length")]
			public string Code { get; set; }

			public TargetCode() {
				TargetUserId = -1;
				Code = "-1";
			}
			public TargetCode(int id, string code)
			{
				TargetUserId = id;
				Code = code;
			}
		}
		private class TargetCodeDbContext : DbContext
		{
			public DbSet<TargetCode> Entries { get; set; }
		}
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
					using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
					{
						targetCodeDbContext.Entries.Attach(TargetCode);
						targetCodeDbContext.Entries.Remove(TargetCode);
						targetCodeDbContext.SaveChanges();
					}
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
				using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
				{
					var res = from entries in targetCodeDbContext.Entries
							  select entries;
					foreach (TargetCode entry in res)
					{
						targetCodeDbContext.Entries.Remove(entry);
					}
					targetCodeDbContext.SaveChanges();
				}
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
				using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
				using (TargetUserDbContext targetUserDbContext = new TargetUserDbContext())
				{
					var res = from codes in targetCodeDbContext.Entries
							  where codes.TargetUserId == user.Id
							  select codes;
					int count = res.Count();
					if (count != 0)
					{
						foreach (TargetCode targetCode in res)
						{
							targetCodeDbContext.Entries.Remove(targetCode);
						}
						targetCodeDbContext.SaveChanges();
					}
					string toHash = user.Email + user.NrOfCodeGenerations;
					code = Crypto.GetHash(toHash).Substring(0, CODE_LENGTH).ToUpper();
					targetUserDbContext.Users.Attach(user);
					++user.NrOfCodeGenerations;
					targetUserDbContext.SaveChanges();

					TargetCode newEntry = new TargetCode(user.Id, code);
					targetCodeDbContext.Entries.Add(newEntry);
					//1.800.000 miliseconds = 30 minutes
					DeletionTimer timer = new DeletionTimer(1800000, newEntry);
					timer.Elapsed += timer.DeleteEntry;
					timer.AutoReset = false;
					targetCodeDbContext.SaveChanges();
					timer.Start();
				}
				return code;
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
				return code;
			}
		}
		public static int Validate(string code)
		{
			using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
			{
				var res = from entries in targetCodeDbContext.Entries
							where entries.Code.Equals(code)
							select entries;
				int resultAmount = res.Count();
				switch (resultAmount)
				{
					case 0:
						throw new KeyNotFoundException("User matching passed unique code was not found.");
					case 1:
						int id = res.First().TargetUserId;
						targetCodeDbContext.Entries.Remove(res.First());
						targetCodeDbContext.SaveChanges();
						return id;
					default:
						foreach(TargetCode entry in res)
						{
							targetCodeDbContext.Entries.Remove(entry);
						}
						targetCodeDbContext.SaveChanges();
						throw new DuplicateCodesException("Found duplicate codes in database. Attempted to delete all of them.");
				}
			}
		}
	}
}
