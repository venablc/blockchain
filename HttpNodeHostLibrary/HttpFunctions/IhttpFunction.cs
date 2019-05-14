using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChainNodeHttpHost.HttpFunctions
{
    interface IHttpFunction
    {
        string HttpAction(SimpleBlockChain.BlockChainNode Node, String data);
    }
}
