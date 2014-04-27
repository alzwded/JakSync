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
		#region sending stuff
		
		/// <summary>
		/// Send whatever data is in List
		/// </summary>
		/// <returns>
		/// True if successful, false or exception for failure
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Send ()
		{
			if (tcpclnt_ == null || !(tcpclnt_.Connected)) {
#if DEBUG
				Console.WriteLine ("tcpclnt connected? {0}", 
					(tcpclnt_ != null) ? tcpclnt_.Connected : false);
#endif
				return false;
			}
			try
			{
				byte msg;
				byte type;
				int lenName;
				int lenData = 0;
				byte[] buffer = new byte[CommunicationsCommon.BUFFER_SIZE];
				byte[] bytes;
				
				byte[] data = new byte[0];
				int bufferJ;
				
				#region send number of entries
				msg = CommunicationsCommon.MSG_SEND;
				stream_.WriteByte(msg);

				// user and password
				bytes = Encoding.UTF8.GetBytes (user_);
				byte[] lenBytes = BitConverter.GetBytes (bytes.Length);
				stream_.Write (lenBytes, 0, lenBytes.Length);
				//stream_.Write (bytes.Length);
				stream_.Write (bytes, 0, bytes.Length);
				bytes = Encoding.UTF8.GetBytes (pass_);
				lenBytes = BitConverter.GetBytes (bytes.Length);
				stream_.Write (lenBytes, 0, lenBytes.Length);
				//stream_.Write (bytes.Length);
				stream_.Write (bytes, 0, bytes.Length);
				
				//wait for reply
				int reply = stream_.ReadByte();
				if(reply == -1 || (byte)reply != CommunicationsCommon.MSG_ACCEPTED)
				{
					return false;
				}
				
				bytes = BitConverter.GetBytes(list_.Count);
				
				stream_.Write(bytes, 0, bytes.Length);
				#endregion
				
				
				foreach(ClassElement i in list_)
				{
					type = CommunicationsCommon.TYPEOF_NULL;
					
					if(i.Payload != null)
					{
						type = CommunicationsCommon.switchType(i.MyType);
					}
					
					byte[] name = Encoding.UTF8.GetBytes(i.Name);
					lenName = name.Length;
					
					#region message header (action,type,lenName,name)
					bufferJ = 0;
					//buffer[bufferJ++] = msg;
					//buffer[bufferJ++] = type;
					stream_.WriteByte(type);

					bytes = BitConverter.GetBytes (lenName);
					/*for (int k = 0; k < bytes.Length; ++k) {
						buffer[bufferJ++] = bytes[k];
					}*/

					stream_.Write(bytes, 0, bytes.Length);
					stream_.Write(name, 0, lenName);
					
					/*for (int k = 0; k < lenName; ++k) {
						while (k < lenName && bufferJ < 
							CommunicationsCommon.BUFFER_SIZE) {
							buffer[bufferJ++] = name[k];
						}
						
						stream_.Write (buffer, 0, bufferJ);
						bufferJ = 0;
					}*/
					#endregion
					
					#region message body
					#region simple data
					if(type == CommunicationsCommon.TYPEOF_NULL)
					{
						// send msg,type,lenname,name
						// write nothing
						continue;
					}
					else if(type != CommunicationsCommon.TYPEOF_STRING 
						&& type != CommunicationsCommon.TYPEOF_COMPLEX
						&& type != CommunicationsCommon.TYPEOF_TYPE)
					{
						// get and call the correct GetBytes method that corresponds to i.MyType
						Type bitConverterType = typeof(BitConverter);
						MethodInfo getBytes = bitConverterType.GetMethod("GetBytes", 
							BindingFlags.Static | BindingFlags.Public, null,
							new Type[] {i.MyType}, null);
							//new Type[] {typeof(int)}, null);
						Console.WriteLine(i);
						//BitConverter.GetBytes((int)(i.Payload));
						bytes = (byte[])(getBytes.Invoke(null, new object[] {i.Payload}));
						
						stream_.Write(bytes, 0, bytes.Length);
						continue;
					}
					#endregion
					#region string or complex
					else if(type == CommunicationsCommon.TYPEOF_TYPE)
					{
						BinaryFormatter bf = new BinaryFormatter();
						MemoryStream ms = new MemoryStream();
						bf.Serialize(ms, i.Payload);
						data = ms.ToArray();
						lenData = data.Length;
						
						// send msg,type,lenname,name,lendata,data
					}
					else if(type == CommunicationsCommon.TYPEOF_STRING)
					{
						data = Encoding.UTF8.GetBytes((string)(i.Payload));
						lenData = data.Length;
							
						// send msg,type,lenname,name,lendata,data
					}
					else if(type == CommunicationsCommon.TYPEOF_COMPLEX)
					{
						data = (byte[])(i.Payload);
						lenData = data.Length;
						
						// send msg,type,lenname,name,lendata,data
					}
					bytes = BitConverter.GetBytes(lenData);
					stream_.Write(bytes, 0, bytes.Length);
					stream_.Write(data, 0, lenData);
					
//					for(int k = 0; k < lenData; ++k)
//					{
//						while(k < lenData && bufferJ < CommunicationsCommon.BUFFER_SIZE)
//						{
//							bytes[bufferJ++] = data[k];
//						}
//						
//						stream_.Write(bytes, 0, bufferJ);
//						bufferJ = 0;
//					}
					#endregion
					#endregion
				}
			}
			catch(IOException ex)
			{
				Console.WriteLine("Something happened to your connection: {0}",
					ex.ToString());
				
				Disconnect();
				
				return false;
			}
			
			return true;
		}
		
		#region Send overloads
		/// <summary>
		/// Disassemble o and serialize it to the database server
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_nameOfO">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_forceListFlush">
		/// Forces flushing the List. By default, the list contents, if any, are left intact.
		/// A <see cref="System.Boolean"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Send (object o, string _nameOfO, bool _forceListFlush)
		{
			if (tcpclnt_ == null || !(tcpclnt_.Connected))
			{
#if DEBUG
				Console.WriteLine("tcpclnt connected? {0}", 
					(tcpclnt_ != null) ? tcpclnt_.Connected : false);
#endif
				return false;
			}
			using (ClassParser cprs = new ClassParser ())
			{
				cprs.Traverse (o, _nameOfO);
				if (_forceListFlush) List = null;
				List = cprs.ListOfFields;
			}
			
			return Send ();
		}
		/// <summary>
		/// Disassemble o and serialize it to the database server
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_nameOfO">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// true if successful, false or exception otherwise
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Send (object o, string _nameOfO)
		{
			return Send (o, _nameOfO, false);
		}
		
		/// <summary>
		/// Disassemble o and serialize it to the database server
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <returns>
		/// true if successful, false or exception otherwise
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Send (object o)
		{
			return Send (o, o.GetType ().Name);
		}
		#endregion
		#endregion
	}
}