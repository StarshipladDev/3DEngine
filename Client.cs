using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    class Client
    {
        TcpClient clientConnection=null;
        NetworkStream ns = null;
        String port;
        String address;
        String client_Name;
        public Client(String port,String Address,String client_Name)
        {
            this.address = Address;
            this.port = port;
            this.client_Name = client_Name;
            Connect();
            Debug.WriteLine("Finished Setup Of Client! /n");
        }
        public String GetName()
        {
            return this.client_Name;
        }
        public void Print(String s)
        {
            Debug.WriteLine(GetName() + ":" + s);
        }
        /// <summary>
        /// Connect to a given server, to use as the location to write any further data using the <see cref="Write"/> method
        ///Built from a base found at https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=netcore-3.1
        /// </summary>
        /// <param name="server"></param>
        /// <param name="message"></param>
        public void Connect()
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                this.clientConnection = new TcpClient();
                clientConnection.Connect(address, Int32.Parse(port));
                ns = clientConnection.GetStream();

                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }
        public void Write(String writeData)
        {
            writeData += "^";
            Print("Begining Write Action");
            if (clientConnection!=null && ns!=null)
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(writeData);
                    if (data == null)
                    {
                        throw (new ConnectionError ("Starshiplad's error!"));
                    }
                // Send the message to the connected TcpServer. 
                this.ns.Write(data, 0, data.Length);
                Print(String.Format("Sent the following on Network Stream: {0}",writeData));
                }
            catch(ConnectionError e)
            {
                    Print(e.Message+" As no Data");
            }
            catch(Exception e)
            {
                Debug.WriteLine("Unhandled Exception Error");
            }
            Print("Finished Write");

        }
        public String Read()
        {
            if (clientConnection != null && ns != null)
            {
                String returnString = " ";
                try
                {

                    Byte[] data = Encoding.ASCII.GetBytes("a");
                    Print("Return String from server is :"+returnString+"\n");
                    while(!(returnString.Substring(returnString.Length - 1, 1).Equals("^")))
                    {
                        // Receive the TcpServer.response.
                        // Buffer to store the response bytes.
                        // String to store the response ASCII representation.
                        String responseData = String.Empty;
                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = ns.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        returnString += responseData;
                    }
                    Print(String.Format("Got the following input from Server: {0}",returnString));
                    Debug.Write("{0} : Finsihed Reading\n", client_Name);
                    Globals.flags[5] = true;
                    Globals.Message = returnString;
                    return returnString;

                }
                catch (ConnectionError e)
                {
                    Debug.Write(e.Message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unhandled Exception Error: "+e.Message);
                }
            }
            return String.Empty;
            


        }
        private class ConnectionError : Exception
        {
            public ConnectionError()
            {
                
            }
            public ConnectionError(String message) : base("There was an error trying to perform a connection action")
            { }

        }
        public void CloseClient()
        {
            Print("Ending Connection");
            // Close everything.

            if (ns != null)
            {
                this.ns.Close();
            }
            if (clientConnection!=null)
            {
                this.clientConnection.Close();
            }
            Print("Ended COnnection");
        }
    }
    
}
