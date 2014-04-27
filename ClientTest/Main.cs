using System;
using System.Net.Sockets;
using System.Collections.Generic;
using JakSync;
using JakSyncCommon;
using Test;

namespace ClientTest
{
	class MainClass
	{
		public static void pitch (string _ip)
		{


			Client.Client cl = new Client.Client (_ip, 5255, "jak", "kaj");
			cl.Connect ();

			TestClass tcl1 = new TestClass (true);
			tcl1.b = new double[] { 666.66, 777.777 };
			tcl1.inrr = new Innerer (true);
			tcl1.a = 871;
			tcl1.dsa._asd = "merge";

			List<ClassElement> al = new List<ClassElement> ();
			ClassParser cp = new ClassParser ();
			cp.Traverse (tcl1, "gigel");
			al = cp.ListOfFields;

			cl.List = al;
			bool b = cl.Send ();

			if (!b)
			{
				Console.WriteLine ("Nope");
			}
		}

		public static void bat (string _ip)
		{

			Client.Client cl = new Client.Client (_ip, 5255, "jak", "kaj");
			cl.Connect ();

			object tcl2 = new TestClass ();
			bool b = cl.Get (ref tcl2, "gigel", typeof(TestClass));

			if (!b)
				Console.WriteLine ("Nope.");

			Console.WriteLine (tcl2);
			Console.WriteLine ("and should be : ");

			TestClass tcl1 = new TestClass (true);
			tcl1.b = new double[] { 666.66, 777.777 };
			tcl1.inrr = new Innerer (true);
			tcl1.a = 871;
			tcl1.dsa._asd = "merge";
			Console.WriteLine (tcl1.ToString());
		}

		public static void Main (string[] args)
		{
			String ip = args[0];
			pitch (ip);
			Console.WriteLine ("Sleeping .2 seconds . . .");
			System.Threading.Thread.Sleep (200);
			bat (ip);

			Console.WriteLine ("Sent and recieved an item.");
		}
	}
}

