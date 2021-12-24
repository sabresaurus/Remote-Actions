using System;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    public class HTTPServer
    {
        private static HttpListener listener;
        private static Thread listenerThread;

        // This example requires the System and System.Net namespaces.
        public static void SimpleListenerExample(string[] prefixes)
        {
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();

            listenerThread = new Thread(ListenForRequests);
            listenerThread.Start();
        }

        private static void ListenForRequests()
        {
            while (true)
            {
                var result = listener.BeginGetContext(ListenerCallback, listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private static void ListenerCallback(IAsyncResult result)
        {
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            Debug.Log("Method: " + request.HttpMethod);
            Debug.Log("LocalUrl: " + request.Url.LocalPath);

            foreach (var key in request.QueryString.AllKeys)
            {
                Debug.Log("Key: " + key + ", Value: " + request.QueryString.GetValues(key)[0]);
            }

            // if (request.HttpMethod == "POST")
            // {
            //     var data_text = new StreamReader(request.InputStream,
            //         request.ContentEncoding).ReadToEnd();
            //     Debug.Log(data_text);
            // }

            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!" + request.Url.LocalPath + "</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();

            context.Response.Close();
        }
    }
}