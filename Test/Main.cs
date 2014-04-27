//#define STRESS
using System;
using JakSync;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using JakSyncCommon;
using JakSyncServer;

namespace Test
{
	public class Innerer
	{
		asd[] d;
		string ss;
		Type tyype;
		
		public Innerer (bool asd)
		{
			d = new asd[2];
			d[0]._asd = "ala";
			d[1]._asd = "bala";
			ss = "nazi";
			tyype = typeof(string);
		}
		
		public override string ToString ()
		{
			return string.Format ("[Innerer: d={0} ss={1} tyype={2}]", 
				(d == null) ? "<NULL>" : d.ToString (), 
				(ss == null) ? "<NULL>" : ss,
				TestClass.nullOrBust(tyype));
		}
	}
	
	public class Inner
	{
		asd[] d = new asd[2];
		string mm = "bad show";
		ArrayList ula = new ArrayList();
		string[] strings = new string[1];
		int[][] x;
		Innerer inrr;
		Type tyype;
		
		public Inner ()
		{
			d[0]._asd = "element#0";
			d[1]._asd = "element#1";
			ula.Add (d);
			ula.Add (mm);
			ula.Add (new double[] { 5.55, 6.66 });
			ula.Add ("asd");
			strings[0] = "bad";
		}
		
		public Inner (bool p0p)
		{
			d[0]._asd = "asd0";
			d[1]._asd = "asd1";
			mm = "good show";
			ula.Add (d);
			ula.Add (mm);
			ula.Add (new object[2]);
			ula.Add ("yes?");
			strings[0] = "good";
			x = new int[1][];
			x[0] = new int[] { 472, 583 };
			inrr = new Innerer (true);
			tyype = typeof(Inner);
		}
		
		public override string ToString ()
		{
			return string.Format ("[inner: d0={0} d1={1} ula0={2} ula1=/{4}/ " + 
				"mm=/{3}/ ula2=/{5}/ ula3=/{6}/ string[0]=/{7}/" +
				" innrr=/{8}/ tyype=/{9}/]", d[0]._asd, d[1]._asd, ula[0], mm, 
				ula[1], ula[2], ula[3], strings[0], TestClass.nullOrBust(inrr),
				TestClass.nullOrBust(tyype));
		}
	}
	
	public struct asd
	{
		public string _asd;
	}
	
	public class TestClass
	{
		public static string nullOrBust (object x)
		{
			if (x == null)
				return "<NULL>";
			return x.ToString ();
		}
		public int a = 1;
		public double[] b = new double[] {1.11, 3.12e-4};
		public asd dsa;
		public Inner[] inn = new Inner[17];
		public Innerer inrr;
		public Type typeField;
		
		public TestClass ()
		{
			dsa._asd = "inside asd";
		}
		
		public TestClass (bool ___b) : this()
		{
			dsa._asd = "runt";
			a = 0;
			b = new double[] { 0.0, 0.0 };
			for (int i = 0; i < 10; ++i) {
				inn[i] = new Inner (true);
			}
			inn[10] = new Inner ();
			inrr = new Innerer (true);
			typeField = typeof(TestClass);
		}
		
		public override string ToString ()
		{
			return string.Format ("[TestClass: a={0} b[0]={1} b[1]={2} " + 
				"dsa._asd={3} inner#9={4} inner#10={5} inrr=/{6}/ typeField=/{7}/]"
				, a, b[0], b[1], dsa._asd, inn[9], inn[10], nullOrBust(inrr),
				nullOrBust(typeField));
		}
	}
	
	class Window : Form
	{
		DataGridView dgr;
		
		
		public object DataSource
		{
			get { return dgr.DataSource; }
			set { dgr.DataSource = value; }
		}
		public new int Width
		{
			set {
				for (int i = 0; i < dgr.Columns.Count; ++i)
				{
					dgr.Columns[i].Width = value;
				}
			}
		}
		
		public Window ()
		{
			this.Size = new Size (Screen.PrimaryScreen.WorkingArea.Width, 300);
			dgr = new DataGridView ();
			dgr.Dock = DockStyle.Fill;
			dgr.ColumnHeadersVisible = true;
			dgr.ReadOnly = false;
			dgr.DoubleClick += (o, e) => dgr_dblClick (o, e);
			this.Controls.Add (dgr);
			
			dgr.DataSourceChanged += delegate(object o, EventArgs e) {
				dgr.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				dgr.Columns[0].FillWeight = 500f;
				dgr.Columns[1].FillWeight = 250f;
				dgr.Columns[2].FillWeight = 250f;
			};
			
			Test ();
		}
		
		private void dgr_dblClick (object o, EventArgs e)
		{
			Form f = new Form ();
			f.Text = "Details";
			f.Size = new Size (400, 200);
			f.StartPosition = FormStartPosition.CenterParent;
			
			TextBox tb = new TextBox ();
			tb.Dock = DockStyle.Fill;
			tb.ReadOnly = true;
			tb.Multiline = true;
			tb.WordWrap = true;
			if (dgr.SelectedCells.Count > 0)
			{
				tb.Text = dgr.SelectedCells[0].Value.ToString ();
			}
			f.Controls.Add (tb);
			
			Button btn = new Button ();
			btn.Text = "Okay";
			btn.Dock = DockStyle.Bottom;
			btn.Click += (oo, oe) => f.Close ();
			f.Controls.Add (btn);
			
			f.ShowDialog();
		}
		
		protected List<ClassElement> al;
		
		
		object oldInst;
		/// <summary>
		/// Modify this to suit your needs
		/// </summary>
		public void Test ()
		{
			long then = DateTime.Now.Ticks;
			
			asd ass = new asd ();
			ass._asd = "123";
#if DEBUG
			System.Console.WriteLine (ass is ISerializable);
#endif
			
			TestClass tcls = new TestClass (true);
			tcls.a = 999;
			tcls.b = new double[] { 999.123, 999e-4 };
			tcls.dsa._asd = "better show";
			oldInst = tcls;
			ClassParser cprs = new ClassParser ();
			cprs.Traverse (tcls);
			al = cprs.ListOfFields;

			Console.WriteLine("Done. Phase 1 too {0}00ns", DateTime.Now.Ticks - then);
		
//			ArrayList testArrayList = new ArrayList ();
//			testArrayList.Add ("asd");
//			testArrayList.Add (2.0);
//			//testArrayList.Add (testArrayList);
//			ArrayList dsa = new ArrayList ();
//			dsa.Add ("blabla");
//			dsa.Add (false);
//			testArrayList.Add (dsa);
//			cprs.Traverse (testArrayList);
//			al = cprs.ListOfFields;
//			oldInst = testArrayList;
			
			DataSource = al.ToArray ();
			
			Panel bottom = new Panel();
			bottom.Height = 25;
			bottom.Dock = DockStyle.Bottom;
			
			Button btnSave = new Button ();
//			btnSave.Dock = DockStyle.Left;
			btnSave.Click += (o, e) => save (o, e);
			btnSave.Text = "S&ave";
			btnSave.UseMnemonic = true;
			btnSave.Left = this.Size.Width / 2 - 10 - btnSave.Width;
			bottom.Controls.Add (btnSave);
			
			Button btn = new Button ();
//			btn.Dock = DockStyle.Right;
			btn.Click += (o, e) => secondPhase (o, e);
			btn.Text = "&Next";
			btn.UseMnemonic = true;
			btn.Left = this.Size.Width / 2 + 10;
			bottom.Controls.Add(btn);
			
			this.Controls.Add (bottom);
		}
		
		void save (object o, EventArgs e)
		{
			try
			{
				ArrayList al = new ArrayList (((ClassElement[])(dgr.DataSource)));
				string fileName = "output"
					+ DateTime.UtcNow.ToString ().Replace("/","-").Replace(":", ";") 
						+ ".txt";
			
				System.IO.StreamWriter sw = new System.IO.StreamWriter (
					fileName);
				
				sw.WriteLine (new String('-', 78));
				sw.WriteLine (" {0, -45} | {1, -13} | {2, -13}", "Name", "Type", "Data");
				sw.WriteLine (new String ('-', 78));
				
				foreach (ClassElement i in al)
				{
					sw.WriteLine (" {0,-45} | {1,-13} | {2,-13}", 
						i.Name, i.MyType.ToString(), 
						(i.Payload != null) ? i.Payload.ToString() : 
						"<NULL>");
				}
				
				sw.Close ();
				
				MessageBox.Show ("Saved to " + fileName);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not save because: " + ex.ToString());
			}
		}
		
		void secondPhase (object o, EventArgs e)
		{
			//			ArrayList rebuilts = new ArrayList();
			//			System.Console.Write ("empty becomes ");
			//			ClassRebuilder cRebuilder = new ClassRebuilder ();
			//			cRebuilder.List = al;
			//			object capsule = rebuilts;
			//			cRebuilder.Rebuild (ref capsule, "ArrayList", typeof(ArrayList));
			//			System.Console.WriteLine ("\n" + ((ArrayList)capsule)[0] + " " + ((ArrayList)capsule)[1] + " and should be\n" + ((ArrayList)oldInst)[0] + " " + ((ArrayList)oldInst)[1]);
			//			return;

//			MessageBox.Show ("Please draw your attention to the console.");

			long then = DateTime.Now.Ticks;
			TestClass rebuilt = new TestClass ();
			System.Console.Write (rebuilt + " becomes ");
			ClassRebuilder cRebuilder = new ClassRebuilder ();
			cRebuilder.List = al;
			object capsule = rebuilt;
			cRebuilder.Rebuild (ref capsule, "TestClass", typeof(TestClass));
			
			System.Console.WriteLine ("\n" + capsule + " and should be\n" + oldInst);
			
			Console.WriteLine ("DOne. Phase 2 took {0}00ns\n\n\n\n\n",
				DateTime.Now.Ticks - then);
			
			thirdPhase ();
		}
		
		void thirdPhase ()
		{
			long then = DateTime.Now.Ticks;
			Console.WriteLine ("Phase Third:::::");
			//sqlite_jaksync a = new sqlite_jaksync ();
			IJakSQL a = new SQLite_JakSync ();
			a.User = "jak";
			a.Password = "kaj";
			a.Connect ();
			a.UpdateOrInsert ("A", CommunicationsCommon.TYPEOF_STRING, (object)"yomamma");
			a.UpdateOrInsert ("B.a", CommunicationsCommon.TYPEOF_INT, 1);
			a.UpdateOrInsert ("B.b", CommunicationsCommon.TYPEOF_INT, 1);
			a.DeleteData ("B");
			a.UpdateOrInsert ("A.j", CommunicationsCommon.TYPEOF_DECIMAL, (object)3e-10);
			a.UpdateOrInsert ("A.q", CommunicationsCommon.TYPEOF_ULONG, (object)Int64.MaxValue);
			a.UpdateOrInsert ("A.nullVal", CommunicationsCommon.TYPEOF_NULL, null);
			Type test = typeof(Window);
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, test);
			byte[] bsz = ms.ToArray ();
			a.UpdateOrInsert ("A.TypeTest", CommunicationsCommon.TYPEOF_TYPE, bsz);

			byte b;
			object o;
			a.Select ("A", out b, out o);
			Console.WriteLine ("Recieved: a string: {0} ; and the data is /{1}/", 
				(b == CommunicationsCommon.TYPEOF_STRING), o);
			a.Select ("A.j", out b, out o);
			Console.WriteLine ("Recieved: a decimal: {0} ; and the data is {1}", 
				(b == CommunicationsCommon.TYPEOF_DECIMAL), o);
			a.Select ("A.q", out b, out o);
			Console.WriteLine ("Recieved: a long: {0} ; and the data is {1} = {2}", 
				(b == CommunicationsCommon.TYPEOF_ULONG), o.GetType (), UInt64.Parse (o.ToString ()));
			a.Select ("A.nullVal", out b, out o);
			Console.WriteLine ("Recieved: a null: {0} ; and the data is {1}",
				(b == CommunicationsCommon.TYPEOF_NULL), (o == null) ? "NULL" : "\"" + o.ToString () + "\"");

			a.Select ("A.TypeTest", out b, out o);
			bsz = (byte[])o;
			test = (Type)(bf.Deserialize (new MemoryStream (bsz)));
			Console.WriteLine ("Recieved: a type: {0} ; and the data is {1} = {2}",
				(b == CommunicationsCommon.TYPEOF_TYPE), o.GetType (), test.ToString ());

			Console.WriteLine ("\nSelect all:");
			Console.WriteLine ("{0, -10}{1, -20}{2, -20}", "Name", "Type", "Data");
			foreach (object[] i in a.SelectAll ())
			{
				Console.WriteLine ("{0, -10}{1, -20}{2, -20}", i[0],
					CommunicationsCommon.switchTypeByte ((byte)(i[1])),
					(i[2] != null) ? i[2] : "NULL");
			}

			//stress test
			#if STRESS
			string sts = "";
			int x = 10000;
			//x * (5 + 1) select statements
			//on a phenom 3x2.3GHz ; 800MB free RAM, no swap ; sqlite3 ;
			//		linux 2.6.35 32bit
			//x = 100: ~300ms
			//x = 1000: ~1s
			//x = 10000: ~33s
			for (int st = 0; st < x; ++st)
			{
				foreach (object[] i in a.SelectAll ())
				{
					sts += i[0].ToString ();
				}
			}
			#endif
			
			Console.WriteLine ("\nAttempting illicit access...");
			a.Disconnect ();
			a.User = "\b\b\b\b\b_____users";
			a.Password = "";
			Console.WriteLine (a.Connect () + "    " + a.LastError);
			a.User = "Invalid user";
			a.Password = "falala";
			Console.WriteLine (a.Connect () + "    " + a.LastError);

			Console.WriteLine ("Done. Phase 3 took {0}00 ns", DateTime.Now.Ticks - then);

#if STRESS
			Console.Error.Write (sts.Substring(0, sts.IndexOf(".")));
#endif
		}
	}
	
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Run(new Window());
		}
	}
}

