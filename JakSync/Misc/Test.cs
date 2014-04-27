using System;
using System.Reflection;
namespace JakSync
{
	public class AlaBala
	{
		int x = 1;
		string y = "2";
	}
	
	public class Test
	{
		AlaBala[][] a;
		
		
		public Test ()
		{
			a = new AlaBala[3][];
			for (int i = 0; i < a.Length; ++i)
			{
				a[i] = new AlaBala[4];
			}
		}
		
		public void doS ()
		{
			System.Console.WriteLine (a.GetType().Name + " " + a.GetType ().IsPrimitive);
			System.Console.WriteLine (a.GetType ().Name + " " + a.GetType ().IsArray);
			
			object instance = a;
			Type t = a.GetType ();
			
			object[] lol = (object[])instance;
			
			System.Console.WriteLine (lol[0].GetType ().Name + " " + lol[0].GetType ().IsPrimitive);
			
			object[] lol2 = (object[])lol[0];
			
			System.Console.WriteLine (lol2[0].GetType ().Name + " " + lol2[0].GetType ().IsPrimitive);
		}
		
		[STAThread]
		public static void Main (string[] args)
		{
			Test t = new Test ();
			
			t.doS ();
		}
	}
}

