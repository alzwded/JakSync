using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JakSyncCommon;

/// <summary>
/// TODO: add try blocks around everything that might ever throw an exception
/// TODO: fix extra big ULONG -- focking SQLs
/// </summary>
namespace JakSyncServer
{
	public class SQLite_JakSync : IJakSQL
	{
		public const string INVALID_USERNAME_OR_PASSWORD = "Invalid user name or password.";
		private const string USERS_TABLE = "_____users.db3";
		public const string NOT_IMPLEMENTED = "Not implemented. Stay tuned for the next version.";
		public const string NOT_CONNECTED = "User / password not validated.";
		public const string ALREADY_CONNECTED = "Already connected. Disconnect first.";
		public const string NAME_NOT_FOUND = "Name not found.";
		private string user_ = "";
		private string pass_ = "";
		private bool connected_ = false;
		private string lastError_ = "None.";
		
		private SQLiteConnection con;
		private SQLiteCommand ___insert;
		private SQLiteCommand ___update;
		private SQLiteCommand ___select;
		private SQLiteCommand ___delete;
		
		public SQLite_JakSync ()
		{
		}
		
		void IDisposable.Dispose ()
		{
			((IJakSQL)this).Disconnect ();
		}

		private bool clearParas ()
		{
			___insert.Parameters.Clear ();
			___update.Parameters.Clear ();
			___select.Parameters.Clear ();
			
			return true;
		}
		
		#region IJakSQL implementation
		bool IJakSQL.UpdateOrInsert (string _name, byte _type, object _data)
		{
			if (!connected_) {
				lastError_ = NOT_CONNECTED;
				return false;
			}
			
			// update
			___update.Parameters.Add (new SQLiteParameter("name", _name));
			___update.Parameters.Add (new SQLiteParameter("type", _type));
			___update.Parameters.Add (new SQLiteParameter("data", _data));
			int count = ___update.ExecuteNonQuery ();
			
			if (count < 1)
			{
				// insert
				___insert.Parameters.Add (new SQLiteParameter("name", _name));
				___insert.Parameters.Add (new SQLiteParameter("type", _type));
				___insert.Parameters.Add (new SQLiteParameter("data", _data));
				count = ___insert.ExecuteNonQuery ();
				
				if (count < 1)
				{
					lastError_ = "Unknown.";
					clearParas ();
					return false;
				}
			}
			
			return clearParas();
		}

		bool IJakSQL.Select (string _name, out byte _type, out object _data)
		{
			if (!connected_)
			{
				_type = 0;
				_data = null;
				lastError_ = NOT_CONNECTED;
				return false;
			}
			try
			{
				___select.Parameters.Add (new SQLiteParameter ("name", _name));
				SQLiteDataReader dr = ___select.ExecuteReader ();
				
				if (!(dr.HasRows))
				{
					_type = 0;
					_data = null;
					clearParas ();
					lastError_ = NAME_NOT_FOUND;
					return false;
				}
				
				_type = 0;
				_data = null;
				
				while (dr.Read ())
				{
					//_type = (byte)(dr["type"]);
					Byte.TryParse (dr["type"].ToString (), out _type);
					if (_type == CommunicationsCommon.TYPEOF_NULL)
					{
						_data = null;
					}
					else if (_type != CommunicationsCommon.TYPEOF_COMPLEX 
						&& _type != CommunicationsCommon.TYPEOF_TYPE
						&& _type != CommunicationsCommon.TYPEOF_INTPTR
						&& _type != CommunicationsCommon.TYPEOF_UINTPTR)
					{
						Type theType = CommunicationsCommon.switchTypeByte (_type);
						MethodInfo mi = typeof(SQLiteDataReader).GetMethod ("Get" + theType.ToString ().Substring (theType.ToString ().LastIndexOf (".") + 1));
						
						if (mi == null)
						{
							mi = typeof(SQLiteDataReader).GetMethod ("Get" + theType.ToString ().Substring (theType.ToString ().LastIndexOf (".") + 2));
							
							if (mi == null)
							{
								mi = typeof(SQLiteDataReader).GetMethod ("GetValue");
								Console.WriteLine (_name + "  got null");
							}
						}
						_data = mi.Invoke (dr, new object[] { 2 });
					}
					else
					{
						_data = dr["data"];
					}
				}
				
				dr.Close ();
			}
			catch (Exception ex)
			{
				clearParas ();
				lastError_ = "Unknown DB error";
				_type = 0;
				_data = null;
				return false;
			}
			
			return clearParas ();
		}
		
		bool IJakSQL.Count (out int count)
		{
			object o = (new SQLiteCommand ("SELECT COUNT(name) FROM a", con)).ExecuteScalar ();
			if (o is string)
			{
				int.TryParse ((string)o, out count);
			}
			else
			{
				count = (int)((long)o);
			}
			
			if (count > 0)
			{
				return true;
			}
			
			lastError_ = "No data in table";
			return false;
		}
		
		IEnumerable<object[]> IJakSQL.SelectAll ()
		{
			if (!connected_) {
				lastError_ = NOT_CONNECTED;
				return false;
			}
			
			//int number = (int)((new SQLiteCommand ("SELECT COUNT(name) FROM a", con)).ExecuteScalar ());
			SQLiteDataReader dr = (new SQLiteCommand ("SELECT name FROM a", con)).ExecuteReader ();
			
			int number;
			IJakSQL __this = this;
			__this.Count (out number);
			
			string[] names = new string[number];
			#if DEBUG
			Console.WriteLine (number);
			#endif
			int i = 0;
			while (dr.Read ()) {
				names[i++] = dr.GetString (0);
			}
			
			dr.Close ();
			
			for (int ij = 0; ij < names.Length; ++ij)
			{
				byte type;
				object data;
				__this.Select (names[ij], out type, out data);
				yield return new object[] { names[ij], type, data };
			}
		}

		bool IJakSQL.Connect ()
		{
			if (connected_)
			{
				lastError_ = ALREADY_CONNECTED;
				return false;
			}
			
			if (user_.Contains ("\b"))
			{
				lastError_ = "Very funny.";
				return false;
			}
			
			if (!(File.Exists (USERS_TABLE)))
			{
				SQLiteConnection.CreateFile (USERS_TABLE);
				SQLiteConnection cona = new SQLiteConnection (USERS_TABLE);
				cona.Open ();
				(new SQLiteCommand ("CREATE TABLE users(name TEXT PRIMARY KEY,"
						+ " password TEXT)"
						)).ExecuteNonQuery ();
				cona.Close ();
				
				lastError_ = "Users Table was blank / non-existant. Please "
					+ "add users to your database(" 
					+ USERS_TABLE + ") lest no one will use your program.";
				return false;
			}
			
			SQLiteConnection lcon = new SQLiteConnection ("Data Source=" + USERS_TABLE + ";Synchronous=FULL");
			lcon.Open ();
			SQLiteCommand cmd = lcon.CreateCommand ();
			cmd.CommandText = "SELECT * FROM users WHERE name=@name AND password=@pass";
			cmd.Parameters.Add (new SQLiteParameter ("name", user_));
			cmd.Parameters.Add (new SQLiteParameter ("pass", pass_));
			SQLiteDataReader dr = cmd.ExecuteReader ();
			
			if (dr.HasRows)
			{
				connected_ = true;
				
				
				if (!(File.Exists ("data_" + user_ + ".db3"))) {
					try {
						SQLiteConnection.CreateFile ("data_" + user_ + ".db3");
					} catch (Exception ex) {
						lastError_ = "DB error.";
						return false;
					}
				}
				con = new SQLiteConnection ("Data Source=" + "data_" + user_ + ".db3");
				con.Open ();
				SQLiteDataReader dr___ = (new SQLiteCommand
					("SELECT name FROM sqlite_master WHERE name='a'", con)).ExecuteReader ();
				if (!(dr___.HasRows))
				{
					(new SQLiteCommand ("CREATE TABLE a(name TEXT PRIMARY KEY,"
							+ "type INTEGER,data BLOB)", con)).ExecuteNonQuery ();
				}
				___select = con.CreateCommand ();
				___select.CommandText = "SELECT * FROM a WHERE name=@name";
				___insert = con.CreateCommand ();
				___insert.CommandText = "INSERT INTO a(name,type,data) VALUES " 
					+ "(@name,@type,@data)";
				___update = con.CreateCommand ();
				___update.CommandText = "UPDATE a SET type=@type,data=@data " 
					+ "WHERE name=@name";
				___delete = con.CreateCommand ();
				___delete.CommandText = "DELETE FROM a WHERE name LIKE @name";
			}
			else
			{
				lastError_ = INVALID_USERNAME_OR_PASSWORD;
			}
			
			lcon.Close ();
			
			return connected_;
			
		}
		
		bool IJakSQL.Disconnect ()
		{
			connected_ = false;
			___insert = ___select = ___update = null;
			if (con != null) {
				con.Close ();
			}
			con = null;
			
			return true;
		}

		bool IJakSQL.DeleteData (string _name)
		{
			if (!connected_)
			{
				lastError_ = NOT_CONNECTED;
				return false;
			}
			
			___delete.Parameters.Add (new SQLiteParameter ("name", _name + "%"));
			int count = ___delete.ExecuteNonQuery ();
			
			if (count < 1)
			{
				lastError_ = NAME_NOT_FOUND;
				clearParas ();
				return false;
			}
			
			return clearParas ();
		}

		[Obsolete("Not used.", true)]
		string IJakSQL.Database {
			get {
				return "Not used.";
			}
			set {
			}
		}

		string IJakSQL.User {
			get {
					return user_;
			}
			set {
					user_ = value;
			}
		}

		string IJakSQL.Password {
			get {
					return pass_;
			}
			set {
					pass_ = value;
			}
		}

		string IJakSQL.UsersDataTable {
			get {
					return (connected_) ? "data_" + user_ + ".db3" : "";
			}
		}

		string IJakSQL.LastError {
			get {
					return lastError_;
			}
		}
		#endregion
	}
}

