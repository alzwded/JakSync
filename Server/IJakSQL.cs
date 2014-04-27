using System;
using System.Collections.Generic;
namespace JakSync_Server
{
	/// <summary>
	/// The server attaches itself to an assembly implementing this interface.
	/// information about how to implement this:
	/// 	Database -- the name of the database to which we will connect. This will
	/// 		be just the name, not a full link
	/// 	User & Password -- the username and password used to connect
	/// 	UsersDataTable -- not used, but someone might be curious what the name of
	/// 		the table where the user's data is stored is
	/// 	UpdateOrInsert -- either updates old data or inserts new data if there no
	/// 		old data available
	/// 		:_name -- the node's name, TEXT
	/// 		:_type -- one BYTE
	/// 		:_data -- BLOB
	/// 	Select(in, out, out) -- retrieves the data with key _name
	/// 	LastError -- all exceptions should be caught and all functions return
	/// 		false in the event that an error occurs. For other people who might 
	/// 		want to throw some friendly messages, this is where you can store
	/// 		that friendly message.
	/// 	DeleteData -- not used, but might be in the future -- delete data
	/// 		from node _name down
	/// </summary>
	public interface IJakSQL : IDisposable, IEnumerator
	{			
		string Database { get; set; }
		// string UsersTable { get; set; }
		string User { get; set; }
		string Password { get; set; }
		/// <summary>
		/// informative purpose only ; the name of the table where the user's data
		/// will be written to
		/// </summary>
		string UsersDataTable { get; }
		
		/// <summary>
		/// updates existing data or inserts new data into the user's appointed data
		/// table
		/// </summary>
		/// <param name="_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_type">
		/// A <see cref="System.Byte"/>
		/// </param>
		/// <param name="_data">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool UpdateOrInsert (string _name, byte _type, object _data);
		/// <summary>
		/// retrieve data from the user's data table
		/// </summary>
		/// <param name="_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_type">
		/// A <see cref="System.Byte"/>
		/// </param>
		/// <param name="_data">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool Select (string _name, out byte _type, out object _data);
		/// <summary>
		/// Returns the number of records in the user's table
		/// </summary>
		/// <param name="count">
		/// A <see cref="System.Int64"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool Count (out int count);
		/// <summary>
		/// Performs a select all and returns the data through an iterator.
		/// The return value is of type object[] { string _name, byte _type,
		/// object _data };
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		IEnumerator<object[]> SelectAll();
		/// <summary>
		/// you can set additional parameters (address, port) in the project's 
		/// xml config file
		/// </summary>
		/// <returns>
		/// true if succsessful, false if fail
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool Connect ();
		bool Disconnect ();
		/// <summary>
		/// get the last error thrown (e.g. if Connect returned false)
		/// </summary>
		string LastError { get; }
		/// <summary>
		/// Deletes data from _name node down
		/// </summary>
		/// <param name="_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool DeleteData(string _name);
	}
}

