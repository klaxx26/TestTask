using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WSCS
{
    public class SSession
    {
        private string login;
        private TcpClient client;
        private NetworkStream stream;

        public SSession(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();
            Thread receiver = new Thread(new ThreadStart(Receive));
            receiver.Start();
        }

        public string Login
        {
            get
            {
                return this.login;
            }
            set
            {
                this.login = value;
            }
        }

        public TcpClient Client
        {
            get
            {
                return this.client;
            }
            set
            {
                this.client = value;
            }
        }

        public void Receive()
        {
            while (true)
            {
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ; 
                byte[] bytes = new byte[client.Available];

                lock (stream)
                {
                    stream.Read(bytes, 0, client.Available);
                }

                string s = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("=====Handshaking from client=====\n{0}", s);

                    string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                    string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    bool fin = (bytes[0] & 0b10000000) != 0,
                        mask = (bytes[1] & 0b10000000) != 0;

                    int opcode = bytes[0] & 0b00001111, 
                        msglen = bytes[1] - 128, // & 0111 1111
                        offset = 2;


                    if (msglen == 126)
                    {
                        msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                        offset = 4;
                    }


                    if (msglen == 0)
                        Console.WriteLine("msglen == 0");
                    else if (mask)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                        offset += 4;

                        for (int i = 0; i < msglen; ++i) {
                            decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);
                        }

                        string text = Encoding.UTF8.GetString(decoded);
                        Console.WriteLine("{0}", text);

                        this.login = text;

                    }
                    else
                        Console.WriteLine("mask bit not set");

                    Console.WriteLine();
                }
            }
        }

        public void Send(string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            byte[] newResponse = new byte[2 + response.Length];
            (new byte[] { 0b10000001 }).CopyTo(newResponse, 0);
            (new byte[] { Convert.ToByte(response.Length) }).CopyTo(newResponse, 1);
            response.CopyTo(newResponse, 2);

            lock (stream)
            {
                stream.Write(newResponse, 0, newResponse.Length);
            }
        }

    }
}