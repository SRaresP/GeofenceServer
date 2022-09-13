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
		private class TargetCode
		{
			[Key]
			[Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
			[MaxLength(50, ErrorMessage = "Email adress was over 50 characters.")]
			public string Email { get; set; }
			public int Code { get; set; }

			public TargetCode() {
				Email = "";
				Code = -1;
			}
			public TargetCode(string email, int code)
			{
				Email = email;
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
		public static int Get(TargetUser user)
		{
			int code = -1;
			try
			{
				using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
				using (TargetUserDbContext targetUserDbContext = new TargetUserDbContext())
				{
					var res = from codes in targetCodeDbContext.Entries
							  where codes.Email == user.Email
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
					//get an 8 digit code
					code = Math.Abs(toHash.GetHashCode() % 100000000);
					targetUserDbContext.Users.Attach(user);
					++user.NrOfCodeGenerations;
					targetUserDbContext.SaveChanges();

					TargetCode newEntry = new TargetCode(user.Email, code);
					targetCodeDbContext.Entries.Add(newEntry);
					//1.800.000 miliseconds = 30 minutes
					DeletionTimer timer = new DeletionTimer(5000, newEntry);
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
		public static string Validate(int code)
		{
			try
			{
				using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
				{
					var res = from entries in targetCodeDbContext.Entries
							  where entries.Code == code
							  select entries;
					int resultAmount = res.Count();
					switch (resultAmount)
					{
						case 0:
							return null;
						case 1:
							return res.First().Email;
						default:
							foreach(TargetCode entry in res)
							{
								targetCodeDbContext.Entries.Remove(entry);
							}
							targetCodeDbContext.SaveChangesAsync();
							throw new DuplicateCodesException("Found duplicate codes in database. Attempted to delete both of them.");
					}
				}
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
				return null;
			}
		}
	}
}
