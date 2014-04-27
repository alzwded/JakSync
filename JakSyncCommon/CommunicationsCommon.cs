using System;
namespace JakSyncCommon
{
	public abstract class CommunicationsCommon
	{
		#region variousConstants
		public const byte MSG_SEND = 0xFF;
		public const byte MSG_SEND_OKAY = 0xFE;
		public const byte MSG_SEND_ERROR = 0xFD;
		public const byte MSG_RECIEVE = 0xF7;
		public const byte MSG_RECIEVE_SOME = 0xF6;
		public const byte MSG_RECIEVE_OKAY = 0xF5;
		public const byte MSG_RECIEVE_ERROR = 0xF4;
		public const byte MSG_ACCEPTED = 0xF3;
		public const byte MSG_DENIED = 0xF2;
		#region integral types
		public const byte TYPEOF_NULL = 0x1;
		public const byte TYPEOF_SBYTE = 0x2;
		public const byte TYPEOF_BYTE = 0x3;
		public const byte TYPEOF_CHAR = 0x4;
		public const byte TYPEOF_SHORT = 0x5;
		public const byte TYPEOF_USHORT = 0x6;
		public const byte TYPEOF_INT = 0x7;
		public const byte TYPEOF_UINT = 0x8;
		public const byte TYPEOF_LONG = 0x9;
		public const byte TYPEOF_ULONG = 0xA;
		public const byte TYPEOF_INTPTR = 0xB;
		public const byte TYPEOF_UINTPTR = 0xC;
		#endregion
		#region float types
		public const byte TYPEOF_FLOAT = 0x11;
		public const byte TYPEOF_DOUBLE = 0x12;
		public const byte TYPEOF_DECIMAL = 0x13;
		#endregion
		#region misc
		public const byte TYPEOF_BOOL = 0x21;
		public const byte TYPEOF_STRING = 0x22;
		public const byte TYPEOF_COMPLEX = 0x31;
		public const byte TYPEOF_TYPE = 0x38;
		#endregion
		#endregion
		
		public const int BUFFER_SIZE = 100;
		
		public static byte switchType (Type t)
		{
			#region integral types
			if (t == typeof(System.SByte) || t == typeof(sbyte))
				return TYPEOF_SBYTE;
			if (t == typeof(System.Byte) || t == typeof(byte))
				return TYPEOF_BYTE;
			if (t == typeof(System.Int16) || t == typeof(short))
				return TYPEOF_SHORT;
			if (t == typeof(System.UInt16) || t == typeof(ushort))
				return TYPEOF_USHORT;
			if (t == typeof(System.Int32) || t == typeof(int))
				return TYPEOF_INT;
			if (t == typeof(System.UInt32) || t == typeof(uint))
				return TYPEOF_UINT;
			if (t == typeof(System.Int64) || t == typeof(long))
				return TYPEOF_LONG;
			if (t == typeof(System.UInt64) || t == typeof(ulong))
				return TYPEOF_ULONG;
			if (t == typeof(System.IntPtr))
				return TYPEOF_INTPTR;
			if (t == typeof(System.UIntPtr))
				return TYPEOF_UINTPTR;
			#endregion
			#region floats
			if (t == typeof(System.Single) || t == typeof(float))
				return TYPEOF_FLOAT;
			if (t == typeof(System.Double) || t == typeof(double))
				return TYPEOF_DOUBLE;
			if (t == typeof(System.Decimal) || t == typeof(decimal))
				return TYPEOF_DECIMAL;
			#endregion
			#region misc
			if (t == typeof(System.Boolean) || t == typeof(bool))
				return TYPEOF_BOOL;
			if (t == typeof(System.String) || t == typeof(string))
				return TYPEOF_STRING;
			if (t == typeof(System.Type)) {
				return TYPEOF_TYPE;
			}
			return TYPEOF_COMPLEX;
			#endregion
		}



		
		public static Type switchTypeByte (byte t)
		{
			#region ints
			if (t ==  TYPEOF_SBYTE)
				return typeof(System.SByte);
			if (t ==  TYPEOF_BYTE)
				return typeof(System.Byte);
			if (t ==  TYPEOF_USHORT)
				return typeof(System.UInt16);
			if (t ==  TYPEOF_INT)
				return typeof(System.Int32);
			if (t ==  TYPEOF_UINT)
				return typeof(System.UInt32);
			if (t ==  TYPEOF_LONG)
				return typeof(System.Int64);
			if (t ==  TYPEOF_ULONG)
				return typeof(System.UInt64);
			if (t ==  TYPEOF_INTPTR)
				return typeof(System.UIntPtr);
			#endregion
			#region floats
			if (t ==  TYPEOF_FLOAT)
				return typeof(System.Single);
			if (t ==  TYPEOF_DOUBLE)
				return typeof(System.Double);
			if (t ==  TYPEOF_DECIMAL)
				return typeof(System.Decimal);
			#endregion
			#region misc
			if (t ==  TYPEOF_BOOL)
				return typeof(System.Boolean);
			if (t ==  TYPEOF_STRING)
				return typeof(System.String);
			if (t ==  TYPEOF_COMPLEX)
				return typeof(System.Object);
			if (t ==  TYPEOF_TYPE)
				return typeof(System.Type);
			if (t ==  TYPEOF_NULL)
				return typeof(System.Object);
			#endregion
			return typeof(object);
		}

	}
}

