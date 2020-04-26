using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    class Server
    {
        public Server(String address,String port)
        {
            try
            {
                //---listen at the specified IP and port no.---
                // IPAddress localAdd = IPAddress.Parse(address);
                IPAddress localAdd = IPAddress.Any;
                TcpListener listener = new TcpListener(localAdd, Int32.Parse(port));
                Console.WriteLine("Listening on address/port "+address+"/"+port+" ...");
                listener.Start();

                Console.WriteLine("Listening successfully on address/port " + address + "/" + port + " ...");
                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received : " + dataReceived);

                //---write back the text to the client---
                Console.WriteLine("Sending back : " + dataReceived);
                nwStream.Write(buffer, 0, bytesRead);
                client.Close();
                listener.Stop();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("error: "+e.Message);
            }
           
        }
    }
}
