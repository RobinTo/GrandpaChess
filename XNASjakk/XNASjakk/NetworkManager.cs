using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace XNASjakk
{
    class NetworkManager
    {

        // Server Related
        TcpListener listener;
        Socket s;
        NetworkStream serverStream;
        // ---

        // Client
        TcpClient client;
        NetworkStream clientStream;

        // ---

        string ip;
        BinaryWriter bw;
        BinaryReader br;

        public bool StartServer(string ip)
        {
            this.ip = ip;
            try
            {
                IPAddress local = IPAddress.Parse(ip);
                listener = new TcpListener(local, 8001);
                listener.Start();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool TryAcceptClient()
        {
            try
            {
                if(listener.Pending())
                    s = listener.AcceptSocket();
                
                serverStream = new NetworkStream(s);

                bw = new BinaryWriter(serverStream);
                br = new BinaryReader(serverStream);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool StartClient(string ip)
        {
            try
            {
                client = new TcpClient();

                this.ip = ip;

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

        public bool isDataAvailableServer()
        {
            if (s.Available > 0)
                return true;
            else
                return false;
        }
        public bool isDataAvailableClient()
        {
            return clientStream.DataAvailable;
        }

        public void CloseDown()
        {
            if (s != null)
            {
                s.Close();
                listener.Stop();
            }
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
