using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleBlockChain
{
    public class NodeP2PConnector : INodeConnector
    {
        
        private class Connection
        {
            public IPAddress RemoteIP;
            public int? RemotePort;

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
            }

            public bool Connect()
            {
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
        

        public string ListeningIP { get; }
        public int ListeningPort { get; }



        public NodeP2PConnector(IPAddress RemoteIP, int? RemotePort, string ListeningIP, int ListeningPort)
        {
            this.ListeningIP = ListeningIP;
            this.ListeningPort = ListeningPort;

            StartListening();

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

        private bool FindMoreNodes()
        {
            bool OtherNodesFound = false;
            foreach (Connection Node in Connections.Values)
            {
                if(Node.Inspected != true)
                {
                    List<Connection> OtherKnownNodes = Node.GetOtherKnownNodes();
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
            return OtherNodesFound;
        }

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

        public int TotalNodeCount()
        {
            return Connections.Count;
        }

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
            //todo: implement
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
            //todo: implement
            return true;
        }

        public List<Block> RetrieveInitialBlockChain()
        {
            //todo: implement
            return null;
        }


    }


}

