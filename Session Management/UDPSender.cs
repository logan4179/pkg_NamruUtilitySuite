using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NamruUtilitySuite
{
	public class UDPSender
	{
		private Socket sendingSocket;
		private IPAddress address;
		private IPEndPoint sendingEndPoint;

		public event EventHandler ErrorOccured;

		public UDPSender(string ipAddress, int portNumber)
		{
			sendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sendingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			address = IPAddress.Parse(ipAddress);
			sendingEndPoint = new IPEndPoint(address, portNumber);
		}

		public void Send(string dataToSend)
		{
			byte[] sendBuffer = Encoding.ASCII.GetBytes(dataToSend);

			try
			{
				sendingSocket.SendTo(sendBuffer, sendBuffer.Length, SocketFlags.None, sendingEndPoint);
			}
			catch (Exception e)
			{
				if (ErrorOccured != null)
				{
					ErrorOccured(e.Message, new EventArgs());
				}
			}
		}
		public void Send(byte[] dataToSend)
		{
			try
			{
				sendingSocket.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, sendingEndPoint);
			}
			catch (Exception e)
			{
				if (ErrorOccured != null)
				{
					ErrorOccured(e.Message, new EventArgs());
				}
			}
		}
	}
}
