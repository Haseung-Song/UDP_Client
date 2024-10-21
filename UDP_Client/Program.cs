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

                for (int j = 0; j < 100; j++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        // 1. 클라이언트 메시지 [송신] 부분
                        // 5번째 바이트를 포함한 데이터를 준비 (다른 바이트는 0으로 초기화)
                        byte[] message = new byte[31];  // 적어도 26바이트 크기여야 함 (5 Byte ~ 30 Byte)
                        message[0] = 0xAF; // Header: Frame Sync (첫 바이트)
                        message[1] = 0x01; // Header: Destination Address (목적지 주소)
                        message[2] = 0x0A; // Header: Source Address (출발지 주소)
                        byte cmdCounter = 0x00;
                        message[3] = cmdCounter; // CMD Counter: [0x00 ~ 0xFF]
                                                 // (CMD Counter + 1)시키고, 0xFF 이후에는 다시 0으로 돌아감
                        cmdCounter = (byte)((cmdCounter + 1) % 256);  // 0~255 순환
                        if (i >= 0 && i < 5)
                        {
                            message[4] = (byte)(cmdCounter | 0x80); // 7번째 비트를 1로 설정
                        }
                        if (i >= 5 && i < 10)
                        {
                            message[4] = (byte)(cmdCounter | 0x7F); // 7번째 비트를 0으로 설정
                        }
                        await udpClient.SendAsync(message, message.Length);
                        await Task.Delay(500);
                        // 2. 서버 메시지 [수신] 부분
                        //UdpReceiveResult result = await udpClient.ReceiveAsync();
                        //string response = Encoding.UTF8.GetString(result.Buffer);
                        //Console.WriteLine($"Server Response: {response}");

                        //await Task.Delay(500);
                    }

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
