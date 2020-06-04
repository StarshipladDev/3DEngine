using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    class Server
    {
        Thread[] thread = new Thread[10];
        TcpClient[] clients = new TcpClient[10];
        TcpListener[] listeners = new TcpListener[10];
        int counter = 0;
        bool yeet = true;
        TcpListener listener;
        public Server(String address,String port)
        {
            try
            {
                //---listen at the specified IP and port no.---
                // IPAddress localAdd = IPAddress.Parse(address);
                IPAddress localAdd = IPAddress.Any;

                listener = new TcpListener(localAdd, Int32.Parse(port));
                listener.Start();
                listener = listener;
                while (yeet)
                {
                    Debug.WriteLine("Listening on address/port " + address + "/" + port + " ...");
                    Console.WriteLine("Listner"+counter+" Listening successfully on address/port " + address + "/" + port + " ...");
                    //---incoming client connected---
                    clients[counter] = listener.AcceptTcpClient();
                    Debug.WriteLine("Listner"+counter+" accepted TCP client");
                    object[] args = new object[2];
                    args[0] = clients[counter];
                    args[1] = this;
                    thread[counter] = new Thread(ThreadFunctions.Listen);
                    thread[counter].Start(args);
                    Thread.Sleep(1000);
                    SendClientDetails(this);
                    counter++;

                }
                Stop();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("error: "+e.Message);
            }
           
        }
        public static void SendMessage(String s,Server serv)
        {

            Debug.WriteLine("Sending a Message via Server");
            //Globals.flags[5] = true;
            //Globals.Message = s;
            int i = 0;
            while (i < serv.counter)
            {
                NetworkStream nws =serv.clients[i].GetStream();
                //---write back the text to the client---
                Debug.WriteLine("Server: Sending to client"+i+" : " + s);
                Globals.flags[6] = true;
                Globals.ServerMessage = s;
                Byte[] ba = Encoding.ASCII.GetBytes(s);
                nws.Write(ba, 0, ba.Length);
                i++;
            }
        }
        public void Stop()
        {
            int i = 0;
            while (i < counter)
            {
                listeners[i].Stop();
                clients[i].Close();
                i++;
            }
        }
        /// <summary>
        /// SendClientDetails sends a command to each client that isn't null and sets it's ID as server's 'counter'
        /// </summary>
        /// <param name="serv">The server to send details to client</param>
        public static void SendClientDetails(Server serv)
        {
            int i = 0;
            while (i <= serv.counter)
            {
                NetworkStream nws = serv.clients[i].GetStream();
                String clientNumber = "COP" + String.Format("{0:00}",i)+"^";
                Debug.WriteLine("Server: Sending to client" + i + " : " + clientNumber);
                Byte[] ba = Encoding.ASCII.GetBytes(clientNumber);
                Globals.flags[6] = true;
                Globals.ServerMessage = clientNumber;
                nws.Write(ba, 0, ba.Length);
                i++;
            }
        }
    }
}
