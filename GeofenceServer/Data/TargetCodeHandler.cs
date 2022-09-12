using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ServerExemplu.Data
{
	class TargetCodeHandler
	{
		private class TargetCode
		{
			[Key]
			[Required(AllowEmptyStrings = false, ErrorMessage = "Email missing while manipulating database", ErrorMessageResourceName = "Email")]
			[MaxLength(50, ErrorMessage = "Email adress was over 50 characters.")]
			public string Email;
			public int Code;

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
						targetCodeDbContext.Entries.Remove(TargetCode);
						targetCodeDbContext.SaveChangesAsync();
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
					targetCodeDbContext.SaveChangesAsync();
				}
			} catch (Exception e)
			{
				Trace.TraceError(e.Message);
			}
		}
		public static void Add(string email, int code)
		{
			try
			{
				using (TargetCodeDbContext targetCodeDbContext = new TargetCodeDbContext())
				{
					TargetCode newEntry = new TargetCode(email, code);
					targetCodeDbContext.Entries.Add(newEntry);
					//1.800.000 miliseconds = 30 minutes
					DeletionTimer timer = new DeletionTimer(10000, newEntry);
					timer.Elapsed += timer.DeleteEntry;
					timer.AutoReset = false;
					targetCodeDbContext.SaveChangesAsync();
				}
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
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
