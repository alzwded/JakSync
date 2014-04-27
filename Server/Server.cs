#define ENABLE_TIMEOUTS
using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using JakSyncCommon;
using System.Reflection;

namespace JakSyncServer
{
	struct ClientInfo
	{
		/// <summary>
		/// DO NOT modify this after handler_ started running
		/// </summary>
		public int id_; // ReadOnly, PLEASE
		public TcpClient socket_;
		public Thread handler_; // new Thread(handleClient);
		public JakSyncCommon.IJakSQL sql_; // = Activator.CreateInstance(<Server>.sql);
	}
	
	public partial class Server
	{
		Type sql;
		
		private static int connectionID = 0;
		Hashtable clients_ = new Hashtable();
		TcpListener listener_;
		private bool stopThread_ = false;
		private Thread running;
		
		public Server (int port, string sqlAssemblyName)
		{
			//IPAddress ip = Dns.Resolve ("localhost").AddressList[0];
			string myName = System.Net.Dns.GetHostName();
			IPAddress ip = System.Net.Dns.GetHostEntry (myName).AddressList[0];
			if ((port <= 0 && sqlAssemblyName == "") 
				&& File.Exists ("Server.settings"))
			{
				StreamReader sr = new StreamReader ("Server.settings");
				string line;
				
				do {
					line = sr.ReadLine ();
					if (line == null)
						break;
					if (line.Trim ()[0] == ';')
						continue;
					if (line.Trim ()[0] == '[')
						continue;
					if (port <= 0 && line.Trim ().ToLower ().StartsWith ("port="))
					{
						int x;
						
						line = line.Trim ().Substring (line.IndexOf ("=") + 1);
						
						if (int.TryParse (line, out x))
						{
							port = x;
						}
						
						continue;
					}
					if (sqlAssemblyName == "" && line.Trim ().ToLower ().StartsWith ("sqlassemblyname"))
					{
						line = line.Trim ().Substring (line.IndexOf ("=") + 1);
						if (File.Exists (line))
						{
							sqlAssemblyName = line;
						}
						
						continue;
					}
				} while (line != null);
			}
			
			if (sqlAssemblyName != "")
			{
				try
				{
					System.Reflection.Assembly sqlAsm = 
						System.Reflection.Assembly.LoadFile (sqlAssemblyName);
					
					Type[] types = sqlAsm.GetTypes ();
					foreach (Type i in types)
					{
						if (i.GetInterface ("IJakSQL") != null)
						{
							//sql = (IJakSQL)(Activator.CreateInstance (i));
							sql = i;
						
							goto doneSQLInit;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ("Assembly load failed, reverting to sqlite");
				}
			}
			
			sql = typeof(SQLite_JakSync);
			
		doneSQLInit:
			if(port <= 0) port = 5255;
			listener_ = new TcpListener (ip, port);
			listener_.Start ();
			
			Console.WriteLine(listener_.LocalEndpoint);
			
			running = new Thread (new ThreadStart (this.listen));
			running.Start ();
			
			Console.WriteLine ("Started JakSync Server. Press ^D or Q to quit.");
			
			while (true)
			{
				ConsoleKeyInfo c = Console.ReadKey (true);
				if (c.Key == ConsoleKey.Q || 
					(c.Modifiers == ConsoleModifiers.Control && c.Key == ConsoleKey.C) ||
					(c.Modifiers == ConsoleModifiers.Control && c.Key == ConsoleKey.D) ||
					c.Key == ConsoleKey.Delete)
				{
					stop ();
					break;
				}
			}
		}
		
		public void stop ()
		{
			Console.WriteLine ("Stopping Server... please be patient.");
			stopThread_ = true;
			
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
			
			Thread.Sleep (5000);
			if (running.IsAlive)
			{
				running.Interrupt ();
				//running.Abort ();
			}
			
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			Thread.Sleep (1000);
//			if (!(running.IsAlive))
//				return;
//			
//			Console.WriteLine ("Shit just got real: forcing shutdown...");
			running.Abort ();
		}
		
		public void listen ()
		{
			ClientInfo ci = new ClientInfo ();
			
			while (!stopThread_)
			{
				ci.socket_ = listener_.AcceptTcpClient ();
				Interlocked.Increment (ref connectionID);
				ci.handler_ = new Thread (new ParameterizedThreadStart (handleClient));
				ci.id_ = connectionID;
#if ENABLE_TIMEOUTS
				ci.socket_.SendTimeout = 60000;
				ci.socket_.ReceiveTimeout = 60000;
#endif
#if DEBUG
				ci.socket_.SendTimeout = 0;
				ci.socket_.ReceiveTimeout = 0;
#endif
				
				lock (this)
				{
					clients_.Add (connectionID, ci);
				}
				
				ci.handler_.Start (ci);
				Console.WriteLine ("New client: {0} {1}", ci.id_, ci.socket_.Client.RemoteEndPoint.ToString());
			}
		}
		
		public static Version Version {
			get { return System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version; }
		}
		
		public static void Main (string[] args)
		{
			int port = 0;
			string sqlAssemblyName = "";
			bool quit = false;
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; ++i)
				{
					if (args[i] == "--help" || args[i].StartsWith ("-") 
						&& args[i].Contains ("h"))
					{
						Console.WriteLine ("JakSync Server\nusage:\tServer [-hv] [port] " +
							"\n\t--help\t-h\tprint this message" +
							"\n\t--version\t-v\tprint the version string" +
							"\n\t\tport\tspecifying a port override the settings file" +
							"\n\t--sql-assembly <file>\tthe path to the desired sql backend. If it's" +
							"\n\t\t\tinvalid, the default built-in sqlite engine will be used.");
						quit = true;
						continue;
					}
					if (args[i] == "--sql-assembly")
					{
						sqlAssemblyName = args[++i];
						continue;
					}
					if (args[i] == "--version" || args[i].StartsWith ("-")
						&& args[i].Contains ("v"))
					{
						Console.WriteLine ("JakSync Server {0}", Version.ToString ());
						
						quit = true;
						continue;
					}
					int x;
					if (int.TryParse (args[i], out x))
					{
						if (x > 1024 && x <= 65535)
						{
							port = x;
							break;
						}
					}
				}
			}
			
			if (quit)
				return;
			
			new Server (port, sqlAssemblyName);
		}
	}
}

