using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using JakSync;
using JakSyncCommon;

namespace Client
{
	public partial class Client 
	{
		#region getting stuff
		

		/// <summary>
		/// retrieves everything
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Get ()
		{
			if (tcpclnt_ == null || !(tcpclnt_.Connected)) {
				#if DEBUG
				Console.WriteLine ("tcpclnt connected? {0}", (tcpclnt_ != null) ? tcpclnt_.Connected : false);
				#endif
				return false;
			}
			try {
				int bb;
				int count = 0;
				byte b;
				byte[] bytes;
				int lenName;
				byte[] name;
				string nameStr;
				int lenData;
				byte[] data;
				
				List = null;
				// tell server to send me everything
				stream_.WriteByte (CommunicationsCommon.MSG_RECIEVE);

				// user and password
				bytes = Encoding.UTF8.GetBytes (user_);
				byte[] lenBytes = BitConverter.GetBytes(bytes.Length);
				stream_.Write(lenBytes, 0, lenBytes.Length);
				//stream_.Write (bytes.Length);
				stream_.Write (bytes, 0, bytes.Length);
				bytes = Encoding.UTF8.GetBytes (pass_);
				lenBytes = BitConverter.GetBytes (bytes.Length);
				stream_.Write (lenBytes, 0, lenBytes.Length);
				//stream_.Write (bytes.Length);
				stream_.Write (bytes, 0, bytes.Length);

				// read okay / error
				// read count
				bb = stream_.ReadByte ();
				if ((byte)bb != CommunicationsCommon.MSG_ACCEPTED || bb == -1)
					return false;
				bytes = BitConverter.GetBytes (new int ());
				stream_.Read (bytes, 0, bytes.Length);
				count = BitConverter.ToInt32 (bytes, 0);
				
				for (int ij = 0; ij < count; ++ij) {
					bb = stream_.ReadByte ();
					b = (byte)bb;
					
					//bytes = BitConverter.GetBytes (new int ());
					stream_.Read (bytes, 0, bytes.Length);
					lenName = BitConverter.ToInt32 (bytes, 0);
					name = new byte[lenName];
					
//					for (int i = 0; i < lenName; ++i) {
//						bb = stream_.ReadByte ();
//						b = (byte)bb;
//						name[i] = b;
//					}
					stream_.Read(name, 0, lenName);
					
					nameStr = Encoding.UTF8.GetString (name);

//					bb = stream_.ReadByte();
//					b = (byte)bb;
					
					if (b == CommunicationsCommon.TYPEOF_NULL) {
						list_.Add (new ClassElement (nameStr, null, typeof(object)));
						
						continue;
					}
					if (b != CommunicationsCommon.TYPEOF_STRING 
						&& b != CommunicationsCommon.TYPEOF_COMPLEX 
						&& b != CommunicationsCommon.TYPEOF_TYPE) {
						int size = System.Runtime.InteropServices
							.Marshal.SizeOf (CommunicationsCommon.switchTypeByte (b));
						data = new byte[size];
						
						stream_.Read (data, 0, size);
						Type theType = CommunicationsCommon.switchTypeByte(b);
						
						MethodInfo mi = typeof(BitConverter).GetMethod ("To" 
							+ theType.ToString ().Substring (theType.ToString ()
								.LastIndexOf (".") + 1),
							BindingFlags.Static | BindingFlags.Public);
						object theData = mi.Invoke (null, new object[] { data, 0 });
						
						list_.Add (new ClassElement (nameStr, theData, 
								CommunicationsCommon.switchTypeByte (b)));
						
						continue;
					}
					if (b == CommunicationsCommon.TYPEOF_TYPE) {
						stream_.Read (bytes, 0, bytes.Length);
						lenData = BitConverter.ToInt32 (bytes, 0);
						data = new byte[lenData];
						stream_.Read (data, 0, lenData);
						
						MemoryStream ms = new MemoryStream (data);
						BinaryFormatter bf = new BinaryFormatter ();
						object theType = bf.Deserialize (ms);
						
						list_.Add (new ClassElement (nameStr, theType, typeof(Type)));
						
						continue;
					}
					if (b == CommunicationsCommon.TYPEOF_STRING) {
						stream_.Read (bytes, 0, bytes.Length);
						lenData = BitConverter.ToInt32 (bytes, 0);
						data = new byte[lenData];
						stream_.Read (data, 0, lenData);
						
						list_.Add (new ClassElement (nameStr, Encoding.UTF8.GetString (data), typeof(string)));
						
						continue;
					}
					if (b == CommunicationsCommon.TYPEOF_COMPLEX) {
						stream_.Read (bytes, 0, bytes.Length);
						lenData = BitConverter.ToInt32 (bytes, 0);
						data = new byte[lenData];
						stream_.Read (data, 0, lenData);
						
						list_.Add (new ClassElement (nameStr, data, typeof(ISerializable)));
						
						continue;
					}
				}
				
				// read data
				// read type:1 lenname:lengthof(int) name:lenname
				//      null
				//      data:switch(type)
				//      lendata:lengthof(int) data:lendata
				
				
				return true;
			} catch (IOException ex) {
				System.Console.WriteLine ("error happened: {0}", ex.ToString ());
			}
			
			return false;
		}

		public bool Get (ref object o, string _name, Type _type)
		{
			bool got = Get ();
			
			if (got) {
				using (ClassRebuilder cr = new ClassRebuilder (List)) {
					cr.Rebuild (ref o, _name, _type);
				}
				
				return true;
			}
			
			return false;
		}

		/// <summary>
		/// Gets a List which coresponds to _nodeName and puts it in List which 
		/// can then be retrieved and used as the data in a ClassRebuilder
		/// </summary>
		/// <param name="_nodeName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Get (string _nodeName)
		{
			throw (new NotImplementedException("Not YET implemented. Check updates."));
			if (tcpclnt_ == null || !(tcpclnt_.Connected)) {
				#if DEBUG
				Console.WriteLine ("tcpclnt connected? {0}", (tcpclnt_ != null) ? tcpclnt_.Connected : false);
				#endif
				return false;
			}

			return false;
		}

		/// <summary>
		/// Combines the efforts of Get(string) and of ClassRebuilder and 
		/// retrives the instance of the object
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_type">
		/// A <see cref="Type"/>
		/// </param>
		/// <param name="_nodeName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Get (ref object o, string _name, Type _type, string _nodeName)
		{
			bool got = Get (_nodeName);
			
			if (got) {
				using (ClassRebuilder cr = new ClassRebuilder (List)) {
					cr.Rebuild (ref o, _name, _type);
				}
				
				return true;
			}
			
			return false;
		}
		#endregion	
	}
}

