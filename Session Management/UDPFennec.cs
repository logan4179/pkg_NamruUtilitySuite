using System.Runtime.InteropServices;

namespace NamruUtilitySuite
{
	public class UDPFennec
	{
		static int FENNEC_SENDPORT = 7868;
		static int FENNEC_LOCALPORT = 7867;

		const short FEN_PACKET_SIZE = 200;
		const short NAME_SIZE = 64;

		//struct FenPacketStruct
		//{
		private int FenPacketSize;                                           /// Number of items in the data packet
		//private char[,] SpNames = new char[FEN_PACKET_SIZE, NAME_SIZE];  /// Data labels
		//private double[] SpValues = new double[FEN_PACKET_SIZE];                       /// Data packet values
		//};

		struct FenPacketStruct
		{
			public int Size;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = FEN_PACKET_SIZE)]
			private fixedString[] name;
			public fixedString[] Name { get { return name ?? (name = new fixedString[FEN_PACKET_SIZE]); } }
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = FEN_PACKET_SIZE)]
			private double[] value;
			public double[] Value { get { return value ?? (value = new double[FEN_PACKET_SIZE]); } }
		}

		struct fixedString
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NAME_SIZE)]
			public char[] Name;
		}

		private FenPacketStruct m_fenPacket;

		private UDPSender fennec_udp;

		public UDPFennec(string IPAddress = "127.0.0.1")
		{
			InitSender(IPAddress);
		}

		public static byte[] GetBytes<T>(T str)
		{
			int size = Marshal.SizeOf(str);
			byte[] arr = new byte[size];
			GCHandle h = default(GCHandle);

			try
			{
				h = GCHandle.Alloc(arr, GCHandleType.Pinned);
				Marshal.StructureToPtr(str, h.AddrOfPinnedObject(), false);
			}
			finally
			{
				if (h.IsAllocated)
				{
					h.Free();
				}
			}

			return arr;
		}

		public void InitSender(string IPAddress)
		{
			fennec_udp = new UDPSender(IPAddress, FENNEC_SENDPORT);
		}

		public void AddData(string name, double value)
		{
			char[] thisname = new char[NAME_SIZE];
			char[] arr;
			arr = name.ToCharArray(0, name.Length);
			for (int i = 0; i < name.Length; i++)
			{
				thisname[i] = arr[i];
			}
			thisname[name.Length] = '\0';

			m_fenPacket.Name[FenPacketSize].Name = thisname;
			m_fenPacket.Value[FenPacketSize] = value;

			FenPacketSize++;
		}

		public void SendData()
		{
			m_fenPacket.Size = FenPacketSize;
			byte[] buffer;

			buffer = GetBytes(m_fenPacket);

			fennec_udp.Send(buffer);
			//udpclient.Send(buffer, buffer.Length, remoteep);

			FenPacketSize = 0;
		}
	}
}
