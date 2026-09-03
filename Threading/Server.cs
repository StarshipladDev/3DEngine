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
        int counter = 0;
        public bool yeet = true;
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
                while (yeet)
                {
                    Debug.WriteLine("Listening on address/port " + address + "/" + port + " ...");
                    Console.WriteLine("Listner"+counter+" Listening successfully on address/port " + address + "/" + port + " ...");
                    //---incoming client connected---
                    TcpClient incomingClient = listener.AcceptTcpClient();
                    if (counter >= clients.Length)
                    {
                        Debug.WriteLine("Server is full (" + clients.Length + " clients), rejecting new connection");
                        incomingClient.Close();
                        continue;
                    }
                    clients[counter] = incomingClient;
                    Debug.WriteLine("Listner"+counter+" accepted TCP client");
                    object[] args = new object[2];
                    args[0] = clients[counter];
                    args[1] = this;
                    thread[counter] = new Thread(ThreadFunctions.Listen);
                    thread[counter].IsBackground = true;
                    thread[counter].Start(args);
                    Thread.Sleep(1000);
                    SendClientDetails(this);
                    counter++;

                }
                Stop();
            }
            catch(Exception e)
            {
                Stop();
                System.Diagnostics.Debug.WriteLine("error: "+e.Message);
            }
           
        }
        public static void SendMessage(String s,Server serv)
        {
            if (s.Equals("KillTheSever^"))
            {
                serv.Stop();
                return;
            }
            Debug.WriteLine("Sending a Message via Server");
            //Globals.flags[5] = true;
            //Globals.Message = s;
            int i = 0;
            while (i < serv.counter)
            {
                try
                {
                    NetworkStream nws = serv.clients[i].GetStream();
                    //---write back the text to the client---
                    Debug.WriteLine("Server: Sending to client"+i+" : " + s);
                    Globals.flags[6] = true;
                    Globals.ServerMessage = s;
                    Byte[] ba = Encoding.ASCII.GetBytes(s);
                    nws.Write(ba, 0, ba.Length);
                }
                catch (Exception e)
                {
                    //A single dead/disconnected client shouldn't take the whole server down.
                    Debug.WriteLine("Server: Failed to send to client " + i + ": " + e.Message);
                }
                i++;
            }
        }
        public void Stop()
        {
            yeet = false;
            int i = 0;
            while (i < counter)
            {
                try { clients[i].Close(); } catch (Exception e) { Debug.WriteLine("Server: Error closing client " + i + ": " + e.Message); }
                try { thread[i].Abort(); } catch (Exception e) { Debug.WriteLine("Server: Error aborting thread " + i + ": " + e.Message); }
                i++;
            }
            try { listener.Stop(); } catch (Exception e) { Debug.WriteLine("Server: Error stopping listener: " + e.Message); }
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
