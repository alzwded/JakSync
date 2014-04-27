using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using JakSyncCommon;

namespace JakSyncServer
{
	public partial class Server
	{
		public void handleClient (object _data)
		{
			ClientInfo ci = (ClientInfo)_data;
			byte b;
			int bb;
			NetworkStream stream = ci.socket_.GetStream ();
			
			/*while (!stopThread_) */{
				bool continue__;
				while (true)
				{
					continue__ = false;
					lock (clients_)
					{
						IDictionaryEnumerator i = clients_.GetEnumerator ();
						i.Reset ();
						while (i.MoveNext ())
						{
							ClientInfo ci2 = (ClientInfo)(i.Value);
								
							if (((int)(i.Key) != ci.id_) && ((IPEndPoint)(ci2.socket_.Client.RemoteEndPoint)).Address.ToString() ==
							((IPEndPoint)(ci.socket_.Client.RemoteEndPoint)).Address.ToString())
							{
								Console.WriteLine ("{0}:{2} waiting on {1}:{3}",
									ci.id_, ci2.id_, 
									((IPEndPoint)(ci2.socket_.Client.RemoteEndPoint)).Address,
									((IPEndPoint)(ci.socket_.Client.RemoteEndPoint)).Address);
								Thread.Sleep (1000);
								continue__ = true;
								break;
							}
						}
					}
					if (!continue__)
						break;
				}
				if (ci.socket_.Connected) {
					try
					{
						bb = stream.ReadByte ();
						if (bb == -1) {
							//break;
							return;
						}
					
						using (ci.sql_ = (IJakSQL)(Activator.CreateInstance (sql))) {
							b = (byte)bb;
							byte[] bytes = new byte[0];
							byte[] intBytes = BitConverter.GetBytes (new int ());
							int count;
						
						switch (b) {
							case CommunicationsCommon.MSG_SEND:
							case CommunicationsCommon.MSG_RECIEVE:
								// read user and pass
								//byte[] bytes;
								//byte[] intBytes = BitConverter.GetBytes (new int ());
								stream.Read (intBytes, 0, intBytes.Length);
								int len = BitConverter.ToInt32 (intBytes, 0);
								bytes = new byte[len];
								stream.Read (bytes, 0, len);
								string usr = System.Text.Encoding.UTF8.GetString (bytes);
							
							stream.Read (intBytes, 0, intBytes.Length);
								len = BitConverter.ToInt32 (intBytes, 0);
								bytes = new byte[len];
								stream.Read (bytes, 0, len);
								string pass = System.Text.Encoding.UTF8.GetString (bytes);
							
							// validate or deny
							
							ci.sql_.User = usr;
								ci.sql_.Password = pass;
								if (ci.sql_.Connect ()) {
									stream.WriteByte (CommunicationsCommon.MSG_ACCEPTED);
								} else {
									stream.WriteByte (CommunicationsCommon.MSG_DENIED);
									return;
								}
								break;
							default:
								stream.WriteByte (CommunicationsCommon.MSG_DENIED);
								return;
								break;
							}
						
						switch (b) {
							case CommunicationsCommon.MSG_SEND:
								// read count
								stream.Read (intBytes, 0, intBytes.Length);
								count = BitConverter.ToInt32 (intBytes, 0);
								int size;
							
							string name;
								byte type;
							
							for (int i = 0; i < count; ++i) {
									type = (byte)(stream.ReadByte ());
									stream.Read (intBytes, 0, intBytes.Length);
									size = BitConverter.ToInt32 (intBytes, 0);
									bytes = new byte[size];
									stream.Read (bytes, 0, size);
									name = System.Text.Encoding.UTF8.GetString (bytes);
								
								switch (type) {
									case CommunicationsCommon.TYPEOF_NULL:
										ci.sql_.UpdateOrInsert (name, type, null);
										break;
									case CommunicationsCommon.TYPEOF_STRING:
										stream.Read (intBytes, 0, intBytes.Length);
										size = BitConverter.ToInt32 (intBytes, 0);
										bytes = new byte[size];
										stream.Read (bytes, 0, size);
										ci.sql_.UpdateOrInsert (name, type, System.Text.Encoding.UTF8.GetString (bytes));
										break;
									case CommunicationsCommon.TYPEOF_COMPLEX:
									case CommunicationsCommon.TYPEOF_TYPE:
										stream.Read (intBytes, 0, intBytes.Length);
										size = BitConverter.ToInt32 (intBytes, 0);
										bytes = new byte[size];
										stream.Read (bytes, 0, size);
										ci.sql_.UpdateOrInsert (name, type, bytes);
										break;
									case CommunicationsCommon.TYPEOF_INTPTR:
									case CommunicationsCommon.TYPEOF_UINTPTR:
										Console.WriteLine ("(U)IntPtr not yet supported: {0} ; {1}", name, ci.id_);
										break;
									default:
										Type theType = CommunicationsCommon.switchTypeByte (type);
										size = System.Runtime.InteropServices.Marshal.SizeOf (theType);
										byte[] data = new byte[size];
									
									stream.Read (data, 0, size);
									
									MethodInfo mi = typeof(BitConverter).GetMethod ("To" + theType.ToString ().Substring (theType.ToString ().LastIndexOf (".") + 1), BindingFlags.Static | BindingFlags.Public);
										object theData = mi.Invoke (null, new object[] { data, 0 });
									
									ci.sql_.UpdateOrInsert (name, type, theData);
										break;
									}
								}

							
							
							break;
							case CommunicationsCommon.MSG_RECIEVE:
								// write count
								//int count;
								ci.sql_.Count (out count);
								//byte[] bytes;
								//byte[] intBytes;
								bytes = BitConverter.GetBytes (count);
								stream.Write (bytes, 0, bytes.Length);
							
							int len;
								foreach (object[] i in ci.sql_.SelectAll ()) {
									stream.WriteByte ((byte)(i[1]));
									
								bytes = System.Text.Encoding.UTF8.GetBytes ((string)i[0]);
									intBytes = BitConverter.GetBytes (bytes.Length);
									stream.Write (intBytes, 0, intBytes.Length);
									stream.Write (bytes, 0, bytes.Length);
								
								switch ((byte)(i[1])) {
									case CommunicationsCommon.TYPEOF_NULL:
										break;
									case CommunicationsCommon.TYPEOF_STRING:
										bytes = System.Text.Encoding.UTF8.GetBytes ((string)(i[2]));
										intBytes = BitConverter.GetBytes (bytes.Length);
										stream.Write (intBytes, 0, intBytes.Length);
										stream.Write (bytes, 0, bytes.Length);
										break;
									case CommunicationsCommon.TYPEOF_COMPLEX:
									case CommunicationsCommon.TYPEOF_TYPE:
										bytes = (byte[])(i[2]);
										intBytes = BitConverter.GetBytes (bytes.Length);
										stream.Write (intBytes, 0, intBytes.Length);
										stream.Write (bytes, 0, bytes.Length);
										break;
									case CommunicationsCommon.TYPEOF_INTPTR:
									case CommunicationsCommon.TYPEOF_UINTPTR:
										Console.WriteLine ("(U)IntPtr not yet supported: {0} ; {1}", i[0], ci.id_);
										break;
									default:
										// get and call the correct GetBytes method that corresponds to i.MyType
										Type bitConverterType = typeof(BitConverter);
										MethodInfo getBytes = bitConverterType.GetMethod ("GetBytes", BindingFlags.Static | BindingFlags.Public, null, new Type[] { CommunicationsCommon.switchTypeByte (((byte)(i[1]))) }, null);
										bytes = (byte[])(getBytes.Invoke (null, new object[] { i[2] }));
									
									//bytes = BitConverter.GetBytes (i[2]);
										stream.Write (bytes, 0, bytes.Length);
										break;
									}
								}

							
							break;
							case CommunicationsCommon.MSG_RECIEVE_SOME:
								Console.WriteLine ("Receiving some not yet supported {0}", ci.id_);
								break;
							}
							
							
							ci.sql_.Disconnect ();
						}
					}catch(Exception ex)
					{
						Console.WriteLine("{0}: {1}", ci.id_, ex.ToString());
						//break;
						goto breaked;
					}
				} else {
					//break;
					goto breaked;
				}
			}
			
		breaked:
			//Thread.Sleep(5000);
			Console.WriteLine("Lost client {0}:{1}", ci.id_, ci.socket_.Client.RemoteEndPoint);
			clients_.Remove (ci.id_);
		}
		
	}
}
