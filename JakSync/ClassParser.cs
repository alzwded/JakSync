using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections.Generic;

// doesn't work for intptr or uintptr yet

/// <summary>
/// <para>Author: Vlad Meşco</para>
/// <para>Automated object serializer with over the air database persistance</para>
/// </summary>

namespace JakSync
{
	public struct ClassElement
	{
		private string name_;
		private object payload_;
		private Type type_;
		
		public override string ToString ()
		{
			return string.Format ("[ClassElement: Name={0}, Payload={1}, MyType={2}]", Name, Payload, MyType);
		}
		
		public static explicit operator object[] (ClassElement o)
		{
			return new object[] { o.Name, o.MyType, o.Payload };
		}
		
		public ClassElement (string _name, object _payload, Type _type)
		{
			name_ = _name;
			payload_ = _payload;
			type_ = _type;
		}
		
		public string Name 
		{
			get { return name_; }
		}
		
		public object Payload
		{
			get { return payload_; }
		}
		
		public Type MyType
		{
			get { return type_; }
		}
		
		
	}
	
	/// <summary>
	/// <para>Author: Vlad Meşco</para>
	/// <para>Saves all the data an object holds into an ArrayList (in an automated 
	/// fashion), which can then be stored into a database using the Client and
	/// server.</para>
	/// </summary>
	public class ClassParser : IDisposable
	{
		protected string className_ = "";
		//protected ArrayList list_ = new ArrayList();
		protected List<ClassElement> list_ = new List<ClassElement>();
		
		public void Dispose ()
		{
			list_.Clear ();
		}
		
		public static Version Version {
			get { return System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version; }
		}
		
		/// <summary>
		/// <para>Returns the list of fields of the parsed classes in
		///  (name, value, type) format.</para>
		/// <para>If the value set is null, the list contents will be
		/// cleared, otherwise it will be ignored.</para>
		/// </summary>
		public List<ClassElement> ListOfFields
		{
			get { return list_; }
			set 
			{
				if (value == null)
					list_.Clear ();
			}
		}
		
		/// <summary>
		/// Calls Traverse(o, classnameof(o))
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		public void Traverse (object o)
		{
			Traverse (o, o.GetType ().Name);
		}
		
		protected void handleArray (Array o, string _name)
		{
			// [classnamer]
			list_.Add (new ClassElement (_name + "$##", o.GetType (), typeof(Type)));
			
			list_.Add (new ClassElement (_name + "##", o.Length, o.Length.GetType()));
			for (int i = 0; i < o.Length; ++i)
			{
				object item = o.GetValue (i);
				Type itemType = typeof(object);
				
				if (item != null)
					itemType = item.GetType ();
				
				// uncomment the following lines if it still doesn't work
				// [classnamer]
				if (o.GetType ().GetElementType () != itemType && !(itemType.IsPrimitive)
					&& !(itemType == typeof(String))) {
					list_.Add (new ClassElement (_name + "#$" + i.ToString (), 
							itemType, typeof(Type)));
				}
				
				if (item == null)
				{
					list_.Add (new ClassElement (_name + "#" + i.ToString (), 
							null, itemType));
				}
				else if (itemType.IsPrimitive || itemType == typeof(string))
				{
					list_.Add (new ClassElement (_name + "#" + i.ToString (), 
							item, itemType));
				}
				else if (itemType.IsArray)
				{
					Traverse (item, _name + "#" + i.ToString ());
				}
				else if (item is ISerializable)
				{
					MemoryStream ms = new MemoryStream ();
					BinaryFormatter bf = new BinaryFormatter ();
					bf.Serialize (ms, item);
					
					//                                                typeof(byte[])
					list_.Add (new ClassElement (_name + "#" + i.ToString ()
							, ms.ToArray (), typeof(ISerializable)));
				}
				else
				{
					foreach (FieldInfo fi in itemType.GetFields (
							BindingFlags.Public | BindingFlags.NonPublic 
							| BindingFlags.Instance | BindingFlags.Static))
					{
						object fieldInstance = fi.GetValue (item);
						Type fieldType = fi.FieldType;
						
						if (fieldInstance == null)
						{
							list_.Add (new ClassElement (_name + "#" + i.ToString () + 
									"." + fi.Name, 
									null, fieldType));
						}
						else if (fieldType.IsPrimitive || fieldType == typeof(string))
						{
							list_.Add (new ClassElement (_name + "#" + i.ToString ()
									+ "." + fi.Name, 
									fieldInstance, fieldType));
						}
						else if (fieldInstance is ISerializable)
						{
							MemoryStream ms = new MemoryStream ();
							BinaryFormatter bf = new BinaryFormatter ();
							bf.Serialize (ms, fieldInstance);
							
							//                                                typeof(byte[])
							list_.Add (new ClassElement (_name + "#" + i.ToString ()
									+ "." + fi.Name
									, ms.ToArray (), typeof(ISerializable)));
						}
						else
						{
							Traverse (fieldInstance, _name + "#" + i.ToString () + 
								"." + fi.Name);
						}
					}
				}
			}
			
		}
		
		/// <summary>
		/// Parses the class and returns the data in a state ready to be
		/// written to a db.
		/// </summary>
		/// <param name="o">
		/// An instance of an object to be parsed and thrown away.
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_name">
		/// The name you would want it to appear (use this when you are
		/// remembering multiple objects of the same type, 'cause otherwise
		/// their names will coincide)
		/// A <see cref="System.String"/>
		/// </param>
		public void Traverse (object o, string _name)
		{
			Type myType = o.GetType ();
			
			// primitive
			if (myType.IsPrimitive || myType == typeof(string))
			{
				list_.Add (new ClassElement (_name, o, myType));
			}
			// array
			else if (myType.IsArray)
			{
				handleArray ((Array)o, _name);
			}
			// has been made to serialize properly
			else if (o is ISerializable)
			{
				MemoryStream ms = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, o);
				
				//                                                typeof(byte[])
				list_.Add (new ClassElement (_name, ms.ToArray (), typeof(ISerializable)));
			}
			// complex object, running through its fields
			else
			{
				list_.Add(new ClassElement(_name + "$", o.GetType(), typeof(Type)));
				foreach(FieldInfo fi in myType.GetFields (
							BindingFlags.Public | BindingFlags.NonPublic 
							| BindingFlags.Instance | BindingFlags.Static))
				{
					object fieldInstance = fi.GetValue(o);
					Type fieldType = fi.FieldType;
					
					if(fieldInstance == null)
					{
						list_.Add(new ClassElement(_name + "." + fi.Name, 
								null, fieldType));
					}
					else if(fieldType.IsPrimitive || fieldType == typeof(string))
					{
						list_.Add(new ClassElement(_name + "." + fi.Name,
								fieldInstance, fieldType));
					}
					else if(fieldType.IsArray)
					{
						handleArray( (Array) fieldInstance, _name + "." + fi.Name);
					}
					else if(fieldInstance is ISerializable)
					{
						//System.Console.WriteLine(fieldType + " is serializable? " + (fieldType == typeof(ISerializable)).ToString());
						MemoryStream ms = new MemoryStream ();
						BinaryFormatter bf = new BinaryFormatter ();
						bf.Serialize (ms, fieldInstance);
				                         
						//                       typeof(byte[])
						list_.Add (new ClassElement (_name + "." + fi.Name
								, ms.ToArray (), typeof(ISerializable)));	
					}
					// complex
					else
					{
						Traverse(fieldInstance, _name + "." + fi.Name);
					}
				}
				
			}
		}
		
		[Obsolete("Not used anymore",true)]
		public void OriginalTraverse (object o, string _name)
		{
			Type T = o.GetType ();
			
			if (o == null)
			{
				list_.Add (new ClassElement (_name, null, o.GetType ()));
			}
			else if (T.IsPrimitive || T == typeof(string))
			{
				list_.Add (new ClassElement (_name, o, o.GetType ()));
			}
			else if (T.IsArray)
			{
				object[] a = (object[])o;
				for (int i = 0; i < a.Length; ++i)
				{
					OriginalTraverse (a[i], _name + "#" + i.ToString ());
				}
			}
			else if (o is ISerializable)
			{
				MemoryStream ms = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, o);
				
				//                       typeof(byte[])
				list_.Add (new ClassElement (_name
						, ms.ToArray (), typeof(ISerializable)));
			}
			else
			{
				foreach (FieldInfo fi in T.GetFields (
							BindingFlags.Public | BindingFlags.NonPublic 
							| BindingFlags.Instance | BindingFlags.Static))
				{
					OriginalTraverse (fi.GetValue (o), _name + "." + fi.Name);
				}
			}
		}
		
		protected bool failLoopTest (object o, string _name, 
			object _elder, string _elderName, bool _aggressive)
		{
			if (_name.Contains (_elderName))
			{
				if (o.Equals (_elder))
				{
					if (_aggressive)
						return true;
					else
					{
						foreach (FieldInfo fi in o.GetType ().GetFields (
								BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Instance))
						{
							if (fi.FieldType == o.GetType () &&
								fi.Name == _name.Substring (_name.LastIndexOf (".") + 1))
								return true;
						}
					}
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Has a basic loop detection
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="_aggressive">
		/// If true, it will not check if two objects are coincidental copies or are
		/// exact copies. Slightly faster, relatively better detection (over detects!).
		/// If false, it will check fields to see if there is a reference to the same
		/// type, meaning it's likely there is a circular list.
		/// A <see cref="System.Boolean"/>
		/// </param>
		[Obsolete("Needs heave updating -- not recommended for actual usage yet", true)]
		public void TraverseWithLoopProtection (object o, string _name, bool _aggressive)
		{
			foreach (ClassElement i in list_)
			{
				if (failLoopTest (o, _name, i.Payload, i.Name, _aggressive))
				{
					return;
				}
			}
			
			Type T = o.GetType ();
			
			if (o == null)
			{
				list_.Add (new ClassElement (_name, null, o.GetType ()));
			}
			else if (T.IsPrimitive || T == typeof(string))
			{
				list_.Add (new ClassElement (_name, o, o.GetType ()));
			}
			else if (T.IsArray)
			{
				object[] a = (object[])o;
				for (int i = 0; i < a.Length; ++i)
				{
					TraverseWithLoopProtection (a[i], _name + "#" + i.ToString (), _aggressive);
				}
			}
			else if (o is ISerializable)
			{
				MemoryStream ms = new MemoryStream ();
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, o);
				
				//                       typeof(byte[])
				list_.Add (new ClassElement (_name
						, ms.ToArray (), typeof(ISerializable)));
			}
			else
			{
				foreach (FieldInfo fi in T.GetFields (
							BindingFlags.Public | BindingFlags.NonPublic 
							| BindingFlags.Instance | BindingFlags.Static))
				{
					TraverseWithLoopProtection (fi.GetValue (o), _name + "." + fi.Name, _aggressive);
				}
			}
		}
	}
}

