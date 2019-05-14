using System;
using System.Net;
using System.Threading.Tasks;
using SimpleBlockChain;
using System.Collections.Generic;
using BlockChainNodeHttpHost.HttpFunctions;
using System.IO;
using System.Text;

namespace BlockChainNodeHttpHost
{
    public class HttpHost
    {
        private static BlockChainNode _BlockChainNode;
        private HttpListener _HttpListener;
        private Task _HttpListenerThread;
        const string HTTP_LISTENER_URL = "http://*:8080/";
        private static Dictionary<string,IHttpFunction> _HttpFunctions;

        public HttpHost(SimpleBlockChain.BlockChainNode Node)
        {
            _BlockChainNode = Node;

            AddHttpFunctions();
            
        }

        private void AddHttpFunctions()
        {
            _HttpFunctions = new Dictionary<string, IHttpFunction>
            {
                { "/add/", new AddBlock() },
                { "/count/", new BlockCount() },
                { "/get/", new GetBlock() }
            };




        }

        public void Start()
        {
            try
            {
                _HttpListenerThread = Task.Run(() => InitHttpListener());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void InitHttpListener()
        {
            if (HttpListener.IsSupported != true)
            {
                throw new Exception("Http Listener not supported, this node will cannot take on role as payment gateway");
            }

            _HttpListener = new HttpListener();


            _HttpListener.Prefixes.Add(HTTP_LISTENER_URL);



            _HttpListener.Start();
            IAsyncResult result;

            while (1 == 1)
            {
                result = _HttpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _HttpListener);
                Console.WriteLine("Waiting for request to be processed asyncronously.");
                result.AsyncWaitHandle.WaitOne();
                Console.WriteLine("Request processed asyncronously.");
            }



        }

        protected static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new System.IO.StreamReader(body, encoding);
            string data = reader.ReadToEnd();

            if (!_HttpFunctions.ContainsKey(request.Url.AbsolutePath))
            {
                Console.WriteLine($"Request to non-existant function received: {request.Url.AbsolutePath}");
                return;
            }


            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = _HttpFunctions[request.Url.AbsolutePath].HttpAction(_BlockChainNode,data);

            Console.WriteLine($"Http response was: {responseString}");
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}
