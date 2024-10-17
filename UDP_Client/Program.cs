using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Client
{
    class UDP_Client
    {
        public static async Task StartClient()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect("127.0.0.1", 2000);
            try
            {
                Console.WriteLine("UDP Client Started...");
                if (udpClient == null)
                {
                    udpClient = new UdpClient();
                }

                for (int i = 0; i < 1000; i++)
                {
                    // 1. 클라이언트 메시지 [송신] 부분
                    byte[] message = Encoding.UTF8.GetBytes("Hello from UDP_Client");
                    await udpClient.SendAsync(message, message.Length);

                    await Task.Delay(500);

                    // 2. 서버 메시지 [수신] 부분
                    //UdpReceiveResult result = await udpClient.ReceiveAsync();
                    //string response = Encoding.UTF8.GetString(result.Buffer);
                    //Console.WriteLine($"Server Response: {response}");

                    //await Task.Delay(500);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                udpClient.Close();
                udpClient.Dispose();
            }

        }

        static async Task Main(string[] args)
        {
            await StartClient();
        }

    }

}
