using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace H3WebServer
{
    public class Server
    {
        private int Port = 8888;
        private HttpListener _listener;
        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + Port.ToString() + "/");
            _listener.Start();
            Receive();
        }
        public void Stop()
        {
            _listener.Stop();
        }
        private void Receive()
        {
            //Recieves incoming request async instead of creating threads
            //This method is called once for each async request
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }
        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                //Gets HttpListenerContext object based on a client request
                HttpListenerContext context = _listener.EndGetContext(result);
                //Gets client request
                HttpListenerRequest request = context.Request;

                Console.WriteLine($"{request.HttpMethod} {request.Url}");
                //Checks to see if request has any body data
                if (request.HasEntityBody)
                {
                    Stream body = request.InputStream;
                    Encoding encoding = request.ContentEncoding;
                    StreamReader reader = new StreamReader(body, encoding);
                    string data = reader.ReadToEnd();
                    Console.WriteLine($"data: {data}");
                    reader.Close();
                    body.Close();
                }
                SendResponse(context);
                Receive();
            }
        }
        private void SendResponse(HttpListenerContext context)
        {
            //Gets httplistenerresponse object from a client's request
            HttpListenerResponse response = context.Response;
            //Checks to see if status code is 200
            if(response.StatusCode == (int) HttpStatusCode.OK)
            {
                response.ContentType = "text/plain";
                //Writes hello as response
                response.OutputStream.Write(Encoding.UTF8.GetBytes("hello"), 0, 0);
                response.OutputStream.Close();
            }
        }
    }
}
