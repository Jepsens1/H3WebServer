using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
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
                ReadBodyData(request);
                SendResponse(context, $"You made a {request.HttpMethod} request");
                Receive();
            }
        }
        private void SendResponse(HttpListenerContext context, string text)
        {
            //Gets httplistenerresponse object from a client's request
            HttpListenerResponse response = context.Response;
            //Checks to see if status code is 200
            if(response.StatusCode == (int) HttpStatusCode.OK)
            {

                response.ContentType = "text/plain";
                SetCookie(response);
                
                response.OutputStream.Write(Encoding.UTF8.GetBytes(text), 0, 0);
                response.OutputStream.Close();
            }
        }
        private void SetCookie(HttpListenerResponse response)
        {
            int count = 0;
            for (int i = 0; i < response.Cookies.Count; i++)
            {
                if (response.Cookies[i].Name == "jwttoken")
                {
                    if (response.Cookies[i].Expired)
                    {
                        response.Cookies.Add(new Cookie("jwttoken", GenerateJWT()));
                    }
                }
                else
                {
                    count++;
                }
            }
            if(count == response.Cookies.Count)
            {
                response.Cookies.Add(new Cookie("jwttoken", GenerateJWT()));
            }
        }
        private void ReadBodyData(HttpListenerRequest request)
        {
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
        }

        private string GenerateJWT()
        {
            Claim[] claims = new Claim[] { new Claim(JwtRegisteredClaimNames.Sub, "testuser") }; 
            SymmetricSecurityKey secretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisismySecretKey"));
            SigningCredentials credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new JwtSecurityToken("testissuer", "testaudience"
                , claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: credentials

                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
