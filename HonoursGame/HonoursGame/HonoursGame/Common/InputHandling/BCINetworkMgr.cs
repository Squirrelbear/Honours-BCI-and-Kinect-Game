using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

// http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server

namespace HonoursGame
{
    public class BCINetworkMgr
    {
        private BCIManager bciManager;
        private Thread dataRequestThread;
        private Thread reconnectAttemptThread;
        private bool _killThreads;

        private float[,] states;
        private float a1, t1;

        //private String localIP = "127.0.0.1";
        private String realIP = "192.168.1.8";//"169.254.130.154";
        private String targetIP;
        private int portNumber = 25000;

        private DataLog brainLog;

        private bool connected;

        public BCINetworkMgr(BCIManager bciManager, string gamePath, string sessionID)
        {
            brainLog = new DataLog(gamePath, sessionID, "brain");

            targetIP = realIP;
            this.bciManager = bciManager;
            states = new float[BCIManager.STATECOUNT, BCIManager.SUBSTATECOUNT];
            _killThreads = false;
            connected = false;

            try
            {
                TcpClient client = new TcpClient();
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(targetIP), portNumber);
                client.Connect(serverEndPoint);

                dataRequestThread = new Thread(new ParameterizedThreadStart(listenForData));
                dataRequestThread.Start(client);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("Server connection failed: \n" + e.Message + "\n");
                bciManager.setMode(BCIManager.Mode.GamePadInput);
                reconnectAttemptThread = new Thread(new ThreadStart(reconnectAttempt));
                reconnectAttemptThread.Start();
            }
        }

        private void listenForData(object clientObj)
        {
            brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "Connected to Server");
            connected = true;

            TcpClient client = (TcpClient)clientObj;
            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            //byte[] buffer = encoder.GetBytes("Hello Server!");

            //clientStream.Write(buffer, 0, buffer.Length);
            //clientStream.Flush();

            //byte[] message = new byte[4096];
            //int bytesRead;
            StreamReader streamReader = new StreamReader(clientStream);
            string lineInput;

            while (true)
            {
                if (_killThreads)
                {
                    brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "Terminated Connection To Server (Game Closed)");
                    clientStream.Close();
                    break;
                }
                //bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    //bytesRead = clientStream.Read(message, 0, 4096);
                    lineInput = streamReader.ReadLine();
                    //Console.WriteLine(lineInput);
                }
                catch (Exception e)
                {
                    //a socket error has occurred
                    System.Diagnostics.Debug.Write("Socket failed: \n" + e.Message + "\n");
                    brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "ERROR: SOCKET FAILED:" + e.Message);
                    bciManager.setMode(BCIManager.Mode.GamePadInput);
                    break;
                }

                /*if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    System.Diagnostics.Debug.Write("Server has disconnected\n");
                    bciManager.setMode(BCIManager.Mode.GamePadInput);
                    break;
                }*/

                string str = lineInput;//encoder.GetString(message, 0, bytesRead);
                brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.BCI, str);
                string[] tokens = str.Split(',');

                if (tokens.Length != 2 + BCIManager.STATECOUNT * BCIManager.SUBSTATECOUNT)
                {
                    System.Diagnostics.Debug.Write("Serious error has occurred indicating the network data count is incorrect.");
                    System.Diagnostics.Debug.Write("Failed to parse network input data.\n");
                    brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "ERROR: Unexpected number of tokens from server " + tokens.Length);
                    continue;
                }

                try
                {
                    //for(int i = 0; i < 10; i++)
                    a1 = (float)Convert.ToDouble(tokens[0]);
                    t1 = (float)Convert.ToDouble(tokens[1]);
                    //Console.Write(states[i] + " ");
                }
                catch
                {
                    System.Diagnostics.Debug.Write("Failed to parse network input data.\n");
                    brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "ERROR: Failed to parse a1 or t1");
                }

                for (int i = 0; i < BCIManager.STATECOUNT; i++)
                {
                    for (int j = 0; j < BCIManager.SUBSTATECOUNT; j++)
                    {
                        if (tokens[i*BCIManager.SUBSTATECOUNT+j].Length != 0)
                        {
                            try
                            {
                                //for(int i = 0; i < 10; i++)
                                states[i, j] = (float)Convert.ToDouble(tokens[2 + i * BCIManager.SUBSTATECOUNT + j]);
                                //Console.Write(states[i] + " ");
                            }
                            catch
                            {
                                System.Diagnostics.Debug.Write("Failed to parse network input data.\n");
                                brainLog.insert(DataLog.DataType.Brain, DataElement.DataType.Misc, "ERROR: Failed to parse a value");
                            }
                        }
                    }
                }
                //Console.WriteLine();
                //message has successfully been received
                //System.Diagnostics.Debug.Write(encoder.GetString(message, 0, bytesRead));
            }
            connected = false;
        }

        private void reconnectAttempt()
        {
            while (true)
            {
                if (_killThreads) break;

                if (dataRequestThread == null || !dataRequestThread.IsAlive)
                {
                    try
                    {
                        TcpClient client = new TcpClient();
                        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(targetIP), portNumber);
                        client.Connect(serverEndPoint);
                        
                        dataRequestThread = new Thread(new ParameterizedThreadStart(listenForData));
                        dataRequestThread.Start(client);
                        bciManager.setMode(BCIManager.Mode.Real);
                    }
                    catch
                    {
                       // System.Diagnostics.Debug.Write("Server reconnect attempt failed.\n");
                    }
                }
                System.Threading.Thread.Sleep(500);
            }
        }

        public void killThreads()
        {
            brainLog.save();
            _killThreads = true;
        }

        public float getState(int stateID, int subStateID)
        {
            return states[stateID, subStateID];
        }

        public float getAlpha1()
        {
            return a1;
        }

        public float getTheta1()
        {
            return t1;
        }

        public bool isConnected()
        {
            return connected;
        }
    }
}
