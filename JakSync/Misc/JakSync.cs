using System;
namespace JakSync
{
	public class JakSync
	{
		/// <summary>
		/// Creates a new instance of the JakSync engine
		/// </summary>
		/// <param name="_SerDataBase">
		/// The name of the database to which the data will be written to / read from
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_SerDBPort">
		/// The port of the db to which the data will be written to / read from
		/// A <see cref="Int32"/>
		/// </param>
		/// <param name="_SerUser">
		/// The username of the db to which the data will be written to / read from
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_SerPass">
		/// The password of the db to which the data will be written to / read from
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_SerTableName">
		/// The table name blabla
		/// </param>
		/// <param name="_IncomingPort">
		/// The port through which the client-server com flows through
		/// A <see cref="Int32"/>
		/// </param>
		/// <param name="_UsrLookUpDB">
		/// The name of the database containing the allowed users
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_UsrLookUpPort">
		/// The port of the database containing the allowed users
		/// A <see cref="Int32"/>
		/// </param>
		/// <param name="_UsrLookUpUsr">
		/// The username of said db
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_UsrLookUpPass">
		/// The password of said db
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_UsrLookUpTableName">
		/// The table name of the blablabla
		/// </param>
		/// <param name="_StayConnected">
		/// If true, the connection will be maintained and not interrupted after
		/// each request.
		/// A <see cref="bool"/>
		/// </param>
		/// <param name="_BreakOnFail">
		/// If true, methods will crash on connection errors, if false, it will just 
		/// write a debug message and try again after a while for N attempts
		/// </param>
		public JakSync (string _SerDataBase, 
			Int32 _SerDBPort, 
			string _SerUser, 
			string _SerPass, 
			string _SerTableName,
			Int32 _IncomingPort, 
			string _UsrLookUpDB, 
			Int32 _UsrLookUpPort, 
			string _UsrLookUpUsr,
			string _UsrLookUpPass,
		string _UsrLookUpTableName,
			bool _StayConnected,
			bool _BreakOnFail)
		{
			//check ports are open
			//-if fail, throw exception
			
			//if the given sertable is incorrect or nonexistant, try to create it
			//-if fail, throw exception
			
			//if _StayConnected connect to databases
			//-if connection failed, invalid data was provided, throw exception
			//-alternatively, write debug message without failing
		}
	}
}

