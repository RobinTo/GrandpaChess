using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace XNASjakk
{
    class NetworkManager
    {

        // Client
        TcpClient client;
        NetworkStream clientStream;
        // ---

        string ip;
        BinaryWriter bw;
        BinaryReader br;

        StringBuilder sb = new StringBuilder();
        byte[] buf = new byte[8192];

        public string receiveIP()
        {
            HttpWebRequest request = (HttpWebRequest)
        WebRequest.Create("http://www.robint.net/chessip.txt");

            // execute the request
            HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            string tempString = null;
            int count = 0;

            do
            {
                // fill the buffer with data
                count = resStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0); // any more data to read?

            // print out page source
            return(sb.ToString());
        }

        public bool StartClient(string ip)
        {
            try
            {
                client = new TcpClient();

                this.ip = ip;
                string onlineIP = receiveIP();
                onlineIP = "192.168.0.105";
                if (this.ip != onlineIP)
                {
                    this.ip = onlineIP;
                    using (StreamWriter writer =  new StreamWriter("ipAddresse.txt"))
                    {
                        writer.Write(onlineIP);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool TryConnect()
        {
            try
            {
                client.Connect(ip, 8001);

                clientStream = client.GetStream();

                bw = new BinaryWriter(clientStream);
                br = new BinaryReader(clientStream);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendMove(int id, int x, int y)
        {
            try
            {
                byte b = 99;
                bw.Write(b);
                bw.Write(id);
                bw.Write(x);
                bw.Write(y);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendUndo()
        {
            try
            {
                byte b = 98;
                bw.Write(b);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool isDataAvailableClient()
        {
            return clientStream.DataAvailable;
        }

        public void CloseDown()
        {
            if (client != null)
                client.Close();
        }

        public bool AreWeStillConnected()
        {
            byte b = 0;
            try
            {
                bw.Write(b);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public byte ReturnByte()
        {
            return br.ReadByte();
        }

        public receiveMove ReceiveMove()
        {   
            return new receiveMove(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        }
    }
}
