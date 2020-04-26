using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine("wang! /n");
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

            Console.WriteLine("\n Press Enter to continue...");
        }
        public void Write(String writeData)
        {
            Debug.Write(String.Format("{0} : Writing\n",client_Name));
            if (clientConnection!=null && ns!=null)
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(writeData);
                // Send the message to the connected TcpServer. 
                this.ns.Write(data, 0, data.Length);
                Console.WriteLine(String.Format("Sent: {0}\n", writeData));
            }
            catch(ConnectionError e)
            {
                Debug.Write(e.Message);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Unhandled Exception Error");
            }
            Debug.Write(String.Format("{0} : Finsihed Writing\n",client_Name));

        }
        public String Read()
        {
            Debug.Write(String.Format("{0} Reading\n",client_Name));
            if (clientConnection != null && ns != null)
            {
                String returnString = " ";
                try
                {
                    Debug.Write("Return String is :"+returnString+"\n");
                    while(!(returnString.Substring(returnString.Length - 1, 1).Equals("^")))
                    {
                        // Receive the TcpServer.response.
                        // Buffer to store the response bytes.
                        Byte[] data = Encoding.ASCII.GetBytes("a");
                        // String to store the response ASCII representation.
                        String responseData = String.Empty;
                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = ns.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        returnString += responseData;
                    }
                    Debug.WriteLine("Got the following input from client: {1}{0}", returnString, client_Name);
                    Debug.Write("{0} : Finsihed Reading\n", client_Name);
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
            Debug.Write(String.Format("{0} : Finsihed Reading with Error\n",client_Name));
            return String.Format("{0} had an Err^",client_Name);
            


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
            Debug.Write("{0} Ending connection ",client_Name);
            // Close everything.
            if (clientConnection!=null)
            {

                this.clientConnection.Close();
            }
            if (ns != null)
            {

                this.ns.Close();
            }
            Debug.Write("{0} Ended connection ", client_Name);
        }
    }
    
}
