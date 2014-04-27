using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace JakSync
{
	/// <summary>
	/// <para>Author: Vlad Meşco</para>
	/// <para>Rebuilds an object using the data obtained from ClassParser</para>
	/// </summary>
	public class ClassRebuilder : IDisposable
	{
		protected List<ClassElement> list_;
		protected Queue<ClassElement> queue_;
		
		public void Dispose ()
		{
			list_.Clear ();
			queue_.Clear ();
		}
		
		public static Version Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version; }
		}
		
		public void Add (ClassElement _e)
		{
			list_.Add (_e);
		}
		
		/// <summary>
		/// Get: Return the list_;
		/// <!--Set: if value is null, the list is cleared;-->
		/// Set: Add stuff to the list; if adding null, the list will be cleared
		/// </summary>
		public List<ClassElement> List
		{
			get { return list_; }
			set {
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
		
		/// <summary>
		/// Don't forget to add data through the List property
		/// </summary>
		public ClassRebuilder ()
		{
			list_ = new List<ClassElement> ();
			//queue_ = new Queue ();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="_l">
		/// A list generated earlier with ClassParser -- reconstruction will 
		/// be based off this
		/// A <see cref="List<ClassElement>"/>
		/// </param>
		public ClassRebuilder (List<ClassElement> _l)
		{
			list_ = _l;
		}
		
		/// <summary>
		/// Rebulds a class from the items added earlier through Add
		/// </summary>
		/// <param name="o">
		/// a ref to the object wherein the data will be added
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_name">
		/// the name of the root node as it appears in the db
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_type">
		/// typeof(o)
		/// A <see cref="Type"/>
		/// </param>
		public void Rebuild (ref object o, string _name, Type _type)
		{
			queue_ = new Queue<ClassElement> (list_.Count);
			
			foreach (ClassElement i in list_)
			{
				queue_.Enqueue (i);
			}
		
			rebuild (ref o, _name, _type);
		}
		
		protected void rebuild (ref object o, string _name, Type _type)
		{
			while (queue_.Count > 0)
			{
				ClassElement item = queue_.Peek ();
				string gigi  = item.ToString();
				if(item.Name.Contains(_name))
				{
					queue_.Dequeue();
				}
				else
				{
					break;
				}
			
#if DEBUG
				System.Console.WriteLine (item);
#endif
				// if is sexy and contains a $
				set(item, ref o, _name, _type);
				//set(item, ref o, _name, item.MyType);
			}
		}
		
		protected void set(ClassElement _listitem, 
			ref object o, string _name, Type _type)
		{	
#if DEBUG
			System.Console.WriteLine("\nEntering: " + _listitem);
			System.Console.WriteLine("\"" + _name + "\" ? \"" + _listitem.Name + "\" and " +  (_listitem.Name.Equals(_name)).ToString() );
			string gigi = _listitem.ToString();
			gigi = gigi;
#endif
			if(_listitem.Name.Equals(_name))
			{
				if(_listitem.MyType.IsPrimitive || _listitem.MyType == typeof(string) || _listitem.Payload == null)
				{
					o = _listitem.Payload;
				}
				else if(_listitem.MyType == typeof(ISerializable))
				{
					BinaryFormatter bf = new BinaryFormatter();
					
					MemoryStream ms = new MemoryStream((byte[])(_listitem.Payload));
			
					o = bf.Deserialize (ms);
				}
				else
				{
					o = Activator.CreateInstance(_listitem.MyType);
					o = _listitem.Payload;
				}
			}
			else if(_listitem.Name == _name + "$" )
			{
				ClassElement newListItem = queue_.Dequeue();
				set(newListItem, ref o, _name, ((Type)(_listitem.Payload)));
			}
			else if(_listitem.Name == (_name + "$##"))
			{
				Type t = ((Type)(_listitem.Payload));
				_listitem = queue_.Dequeue();
				o = Array.CreateInstance(t.GetElementType(), (int)(_listitem.Payload));
				Array a = (Array)o;
				
				for(int i = 0 ; i < a.Length ; ++i)
				{
					object instance = new object();
					try
					{
						// for whatever reason, strings are treated to such an exceptional
						// extent that it's the only type that I know of that does not 
						// accept instantiation without parameters. So basicly
						// the else is needed to treat strings period.
						if(t.GetElementType() != typeof(string))
						{
							instance = FormatterServices.GetUninitializedObject (t.GetElementType());
						}
						else
						{
							instance = "";
						}
					}
					catch(Exception ex)
					{
						throw(new Exception("Extra special case -- quick fix: fix the " +
								"type shown to have a parameterless constructor. Not the" +
								" most elegant of solutions, but the code will then work. " 
								+ ex.ToString()));
					}
					
					ClassElement checkCE = queue_.Peek();
					
					if(_name + "#$" + i.ToString() == checkCE.Name)
					{
						queue_.Dequeue();
						rebuild(ref instance, _name + "#" + i.ToString(), ((Type)(checkCE.Payload)));
					}
					else
					{
						rebuild(ref instance, _name + "#" + i.ToString(), t.GetElementType());
					}
					
					try
					{
						a.SetValue(instance, i);
					}
					catch(Exception ex)
					{
						System.Console.Error.WriteLine(ex.ToString() + "\n" + a.ToString() + " " + instance.ToString());
					}
					
					int xxxx = 2;
				}
				
				o = a;
			}
			else if(_listitem.Name == (_name + "##"))
			{
				throw(new Exception("You are using an older version of JakSync. "
						+ "If that is not so, contact the developer."));
			}
				
//			else if(_listitem.MyType == typeof(ISerializable))
//			{
//				throw(new NotImplementedException("Wait until the next iteration for serializable objects support."));
//			}
			else
			{
				try
				{
					if(o == null)
					{
						//o = Activator.CreateInstance(_type);
						o = FormatterServices.GetUninitializedObject(_type);
					}
					
					//should also include the .
					string atomNextField = _listitem.Name.Substring(_name.Length); 
					
					//strip fieldname of . and #*
					string atomNextFieldWithoutHash = atomNextField.Substring(1);
					if(atomNextFieldWithoutHash.Contains("."))
					{
						atomNextFieldWithoutHash = atomNextFieldWithoutHash.Substring(0, atomNextFieldWithoutHash.IndexOf("."));
					}
					if(atomNextFieldWithoutHash.Contains("#"))
					{
						atomNextFieldWithoutHash = atomNextFieldWithoutHash.Substring(0, atomNextFieldWithoutHash.IndexOf("#"));
					}
					if(atomNextFieldWithoutHash.Contains("$"))
					{
						atomNextFieldWithoutHash = atomNextFieldWithoutHash.Replace("$", "");
					}
					string nextFieldName = _name + "." + atomNextFieldWithoutHash;
						
					FieldInfo nextField = null;
					try
					{
					    nextField = _type.GetField(atomNextFieldWithoutHash, 
							BindingFlags.Public | BindingFlags.NonPublic |
							BindingFlags.Instance | BindingFlags.Static);
						if(nextField == null)
						{
							throw (new Exception("Something extraordinarily bad happened. "
									+ "Please contact the developer telling him the "
									+ "requested field could not be found. "
									+ _name + " ; " + _listitem.ToString()));
						}
					}
					catch(NullReferenceException ex)
					{
						int a = 2;
					}
					catch(Exception ex)
					{
						Console.WriteLine("Missed field {0}.", _listitem.Name);
					}
							
					if(_listitem.Name.IndexOf(nextFieldName) < 0)
							return;
						
					object fieldInstance = null;
					try
					{
						fieldInstance = nextField.GetValue(o);
					}
					catch(Exception ex)
					{
						string msg = "Something extraordinarily bad happened. "
								+ "Please contact the developer telling him the "
								+ "requested field was null. "
								+ _name + " ; " + _listitem.ToString();
	//					throw (new Exception("Something extraordinarily bad happened. "
	//							+ "Please contact the developer telling him the "
	//							+ "requested field was null. "
	//							+ _name + " ; " + _listitem.ToString()));
						Console.WriteLine(msg + " Either that, or consider flushing the "
								+ "database.");
					}

					set(_listitem, ref fieldInstance, nextFieldName, nextField.FieldType);
	#if DEBUG
					if(fieldInstance == null)
					{
						System.Console.WriteLine("flunked here {0} //// {1}", _name, gigi);
					}
	#endif
					try
					{
						nextField.SetValue(o, fieldInstance);
					}
					catch(System.FieldAccessException ex)
					{
						System.Console.Error.WriteLine("constant field {0} -- no need to panic", nextFieldName);
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine("Lost field {0}"
#if DEBUG
						+ ex.ToString()
#endif
						, _listitem.Name);
				}
			}
		}
	}
}

