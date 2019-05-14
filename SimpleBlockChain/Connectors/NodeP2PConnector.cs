using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleBlockChain.Connectors
{
    public class NodeP2PConnector : INodeConnector
    {
        
        private class Connection
        {
            public IPAddress RemoteIP;
            public int? RemotePort;
            public TcpClient RemoteSocket;

            /// <summary>
            /// Indicates whether this node has been checked to see wether it knows of any other nodes
            /// that we might try and connect to.
            /// </summary>
            public readonly bool Inspected;
            public enum State
            {
                disconnected,
                connecting,
                connected,
                unreachable,
                unattempted
            }

            public State ConnectionState { get; set; }

            public Connection(IPAddress RemoteIP, int? RemotePort)
            {
                this.RemoteIP = RemoteIP;
                this.RemotePort = RemotePort;
                this.ConnectionState = State.unattempted;
            }

            public bool Connect()
            {


                if (this.RemotePort.HasValue)
                {
                    this.RemoteSocket = new TcpClient(this.RemoteIP.ToString(), (int)this.RemotePort);
                }

                this.ConnectionState = State.connected;
                return true;
            }

            public List<Connection> GetOtherKnownNodes()
            {



                return null; //todo: implement
            }

            internal bool disconnect()
            {
                this.ConnectionState = State.disconnected;
                return true;
            }
        }

        private Dictionary<string,Connection> Connections;
        private TcpClient ListeningSocket;
        private BlockChainNode MyNode;

        public string ListeningIP { get; }
        public int ListeningPort { get; }

        private enum P2Pcommand
        {
            GetBlockChain,
            GetBlockChainResponse,
            PutBlock
        }
        
        public NodeP2PConnector(IPAddress RemoteIP, int? RemotePort, string ListeningIP, int ListeningPort)
        {
            this.ListeningIP = ListeningIP;
            this.ListeningPort = ListeningPort;

            var t = new System.Threading.Thread(StartListening);
            t.Start();

            Connections = new Dictionary<string, Connection>();

            if (RemoteIP != null)
                AddP2PNode(RemoteIP, RemotePort);


        }

        public void AddP2PNode(IPAddress RemoteIP, int? RemotePort)
        {
            Connections.Add($"{RemoteIP.Address}:{RemotePort}", new Connection(RemoteIP, RemotePort));
        }
              
        /// <summary>
        /// setup socket to listen for incoming messages
        /// </summary>
        private void StartListening()
        {

          

            // bind the listening socket to the port
            IPAddress hostIP = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[0];
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(this.ListeningIP),this.ListeningPort);

            TcpListener listener=TcpListener.Create(this.ListeningPort);
            listener.Start();
            ListeningSocket = listener.AcceptTcpClient();
                

            NetworkStream stream = ListeningSocket.GetStream();

            while (1 == 1)
            {
                try
                {
                    // Buffer to store the response bytes.
                    var data = new Byte[2048];

                    // String to store the response ASCII representation.
                    P2Pmessage responseData;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                    responseData = (P2Pmessage)ByteArrayToObject(data); //(**This converts it to string**)

                    ProcessInboundP2PCommand(responseData);

                    System.Threading.Thread.Sleep(30);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
               
            }


        }

        /// <summary>
        /// All incoming messages from other peers are passed to this function for processing
        /// </summary>
        /// <param name="responseData"></param>
        private void ProcessInboundP2PCommand(P2Pmessage responseData)
        {
            Console.WriteLine($"Processing {responseData.Command.ToString()} command");

            switch (responseData.Command)
            {
                case P2Pcommand.GetBlockChain:

                    //todo: get chain from {MyNode} and return to response peer
                
                    SendMesage(new Connection(System.Net.IPAddress.Parse(responseData.ResponsePeerIp),responseData.ResponsePeerPort),new P2Pmessage()
                    {
                        Command = P2Pcommand.GetBlockChainResponse,
                        ResponseRequired = false,
                        Data = MyNode.GetBlockChain()
                    });

                    break;

                case P2Pcommand.GetBlockChainResponse:

                    MyNode.SetBlockChain((List<Block>)responseData.Data);

                    break;

                case P2Pcommand.PutBlock:

                    MyNode.PutBroadcastedBlock((Block)responseData.Data);

                    break;


            }
        }
        
        /// <summary>
        /// Initiation of the P2P connector will attempt to connect to at least MinConnection nodes and up to
        /// MaxConnection nodes. It does this by querying or "inspecting" known nodes for suggestions on other nodes
        /// that we may attempt to connect to.
        /// </summary>
        public void Initiate(int MinConnection = 3, int MaxConnections = 10)
        {
        
            Connection Node = GetNextUnAttmeptedNode();

            if (Node == null)
            {
                // This connector was created with no first node to connect to (which is fine
                // if this is to be an independent node or the first of a new network of nodes)
                // therefore we have nothing to do here...
                return;
            }

            while (Node != null && ConnectedNodeCount() < MaxConnections )
            {

                Node.Connect();

                Node = GetNextUnAttmeptedNode();

                if (Node == null && FindMoreNodes())
                {
                    Node = GetNextUnAttmeptedNode();
                }

            }

            if(ConnectedNodeCount() < MinConnection)
            {
                // we failed to find enough nodes to connect to, disconnect from
                // all that we managed to find and throw exception
                foreach (Connection _node in Connections.Values)
                {
                    _node.disconnect();
                }
                throw new Exception("Unable to establish connection to a sufficient number of nodes.");
            }

        }

        /// <summary>
        /// Searches for more nodes that may be available to connect to, if any are found they are 
        /// added to {connections} and this function will return True
        /// </summary>
        /// <returns></returns>
        private bool FindMoreNodes()
        {
            bool OtherNodesFound = false;
            foreach (Connection Node in Connections.Values)
            {
                if(Node.Inspected != true)
                {
                    List<Connection> OtherKnownNodes = Node.GetOtherKnownNodes();
                    if(OtherKnownNodes != null)
                    {
                        foreach (Connection OtherNode in OtherKnownNodes)
                        {
                            if (Connections.ContainsKey($"{OtherNode.RemoteIP.Address}:{OtherNode.RemotePort}") == false)
                            {
                                Connections.Add($"{OtherNode.RemoteIP.Address}:{OtherNode.RemotePort}", OtherNode);
                                OtherNodesFound = true;
                            }
                        }
                    }

                }
            }
            return OtherNodesFound;
        }

        /// <summary>
        /// returns a count of connected nodes
        /// </summary>
        /// <returns></returns>
        public int ConnectedNodeCount()
        {
            int ConnectedCount = 0;
            foreach (Connection item in Connections.Values)
            {
                if(item.ConnectionState == Connection.State.connected)
                {
                    ConnectedCount += 1;
                }
            }
            return ConnectedCount;
        }

        /// <summary>
        /// Returns a count of all nodes (regardless of connection state)
        /// </summary>
        /// <returns></returns>
        public int TotalNodeCount()
        {
            return Connections.Count;
        }

        /// <summary>
        /// Get the next node that we haven't checked for additional alternative node connections
        /// </summary>
        /// <returns></returns>
        private Connection GetNextUnInspectedNode()
        {
            foreach (Connection item in Connections.Values)
            {
                if (item.Inspected != true)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Get next known node that we haven't attempted a connetion to
        /// </summary>
        /// <returns></returns>
        private Connection GetNextUnAttmeptedNode()
        {
            foreach (Connection item in Connections.Values)
            {
                if (item.ConnectionState == Connection.State.unattempted)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Query a known Node for suggestions on other known nodes that we could try and connect to.
        /// </summary>
        /// <returns>Number of nodes found</returns>
        private int InspectNodeForOtherKnownNodes(Connection Node)
        {

            List<Connection> NewNodes = Node.GetOtherKnownNodes();
            int NewNodesFound = 0;

            foreach (Connection item in NewNodes)
            {
                if (Connections.ContainsKey($"{item.RemoteIP.Address}:{item.RemotePort}") == false)
                {
                    Connections.Add($"{item.RemoteIP.Address}:{item.RemotePort}", item);
                    NewNodesFound += 1;
                }
            }
            
            return NewNodesFound;
            
        }

        /// <summary>
        /// Broadcast the creation of a new block to all connected nodes.
        /// </summary>
        /// <param name="NewBlock"></param>
        public void BroadcastNewBlockAdd(Block NewBlock)
        {
            //todo: loop through all connected nodes and set a message to indicate a new block has been added, include it's sequence
            foreach (var c in Connections.Values)
            {
                if(c.ConnectionState== Connection.State.connected)
                {
                    SendMesage(c, new P2Pmessage()
                    {
                        Command = P2Pcommand.PutBlock,
                        Data = NewBlock,
                        ResponseRequired = false
                    });
                }
            }
        }

        /// <summary>
        /// Validates the given node against all other connected nodes, if 
        ///  provided chain matches at least 80% of connected chains then
        ///  the node is considered valid
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool ValidateNode(BlockChainNode Node)
        {
            //todo: iterate through blocks in chain, calculate each hash afresh and check that they match the actual held chain
            return true;
        }

        
        /// <summary>
        /// Connect with an available node and retrieve a copy of their chain.
        /// </summary>
        /// <returns></returns>
        public List<Block> RequestInitialBlockChain()
        {
            
            SendMesage(GetRandomConnectedNode(), new P2Pmessage()
            {
                Command = P2Pcommand.GetBlockChain,
                ResponsePeerIp = this.ListeningIP,
                ResponsePeerPort = this.ListeningPort,
                ResponseRequired = true

            });
            
            return new List<Block>() { new Block() }; 
        }

        private Connection GetRandomConnectedNode()
        {
            Dictionary<string, Connection> dict = this.getConnectedNodes();
            int rnd = new Random().Next(0,dict.Count);
            int index = 0;
            foreach (var kvp in dict)
            {
                
                if (index == rnd)
                {
                    return kvp.Value; 
                }
                index++;
            }
            return null;
        }

        private Dictionary<string, Connection> getConnectedNodes()
        {
            Dictionary<string, Connection> nodes = new Dictionary<string, Connection>();

            foreach (var item in Connections)
            {
                if(item.Value.ConnectionState == Connection.State.connected)
                {
                    nodes.Add(item.Key,item.Value);
                }
            }

            return nodes;
        }

        [Serializable]
       private class P2Pmessage
        {
            public bool ResponseRequired;
            public string ResponsePeerIp;
            public int ResponsePeerPort;
            public P2Pcommand Command;
            public object Data;
        }



        private void SendMesage(Connection peer, P2Pmessage message)
        {

      
            Byte[] data = ObjectToByteArray(message);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            if (peer.ConnectionState != Connection.State.connected)
                peer.Connect();

            NetworkStream stream = peer.RemoteSocket.GetStream();

            // Send the message to the connected TcpServer. 
            stream.Write(data, 0, data.Length);
        }

        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }

        public void RegisterNode(BlockChainNode blockChainNode)
        {
            this.MyNode = blockChainNode;
        }

        public void ReceiveNewBlockAdd(Block block)
        {
            throw new NotImplementedException();
        }

         public bool ConnectToPeer(string ip, int port)
        {
            throw new NotImplementedException();
        }
    }


}

