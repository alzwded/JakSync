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

namespace Client
{
	public partial class Client : IDisposable
	{
		/// <summary>
		/// cognate with Server.BUFFER_SIZE
		/// </summary>
		
		#region coreClient
		private string ip_ = "127.0.0.1";
		private int port_ = 5255;
		private string user_ = "";
		private string pass_ = "";
		private List<ClassElement> list_;
		
		private NetworkStream stream_;
		private TcpClient tcpclnt_;
		
		public string IP
		{
			get { return ip_; }
			set { ip_ = value; }
		}
		
		public int Port {
			get { return port_; }
			set { port_ = value; }
		}
		
		public string UserName {
			get { return user_; }
			set { user_ = value; }
		}
		
		public string Password {
			get { return pass_; }
			set { pass_ = value; }
		}
		
		/// <summary>
		/// The list of requests / object data. Setting a value of null 
		/// clears the list, anything else gets added to the preexisting list
		/// </summary>
		public List<ClassElement> List
		{
			get { return list_; }
			set 
			{
				if (value == null)
				{
					list_.Clear ();
				}
				else
				{
					list_.AddRange (value.ToArray ());
				}
			}
		}
		
		public Client ()
		{
			list_ = new List<ClassElement> ();
		}
		
		public Client (string _ip, int _port, string _usr, string _pwd)
		{
			list_ = new List<ClassElement> ();
			ip_ = _ip;
			port_ = _port;
			user_ = _usr;
			pass_ = _pwd;
		}
		
		public void Connect (string _ip, int _port, string _usr, string _pwd)
		{
			ip_ = _ip;
			port_ = _port;
			user_ = _usr;
			pass_ = _pwd;
			Connect ();
		}
		
		public void Connect ()
		{
			try
			{
				tcpclnt_ = new TcpClient (ip_.Trim (), port_);
				string st = ip_.Trim ();
				stream_ = tcpclnt_.GetStream ();
				
				// tell server I'm a pretty cool guy and usr, pass
				// read response
			}
			catch (SocketException ex)
			{
				Console.WriteLine ("Connectino error: {0}; Err Code: {1}",
					ex.ToString (), ex.SocketErrorCode.ToString());
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine ("Something fishy is going on... this exception " +
					"should not have been raised... {0}", ex.ToString());
			}
		}
		
		public void Disconnect ()
		{
			if (tcpclnt_ == null || !(tcpclnt_.Connected))
				return;
			
			try
			{
				tcpclnt_.Close ();
			}
			catch (SocketException ex)
			{
				Console.WriteLine ("Something went wrong when closing the socket: "
					+ "{0}; err code: {1}",
					ex.ToString (), ex.SocketErrorCode.ToString ());
			}
		}
		
		public void Dispose ()
		{
			Disconnect ();
			List = null;
		}
		#endregion
		
		
		
	}
}

