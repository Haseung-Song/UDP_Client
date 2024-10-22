using System;
using System.Diagnostics;
using System.Net.Sockets;
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

                byte cmdCounter = 0x00;  // CMD Counter 초기값

                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 10; j++)  // 10번 루프 (테스트 반복)
                    {
                        byte[] message = new byte[31];  // 메시지의 바이트 크기 (5 Byte ~ 30 Byte)
                        message[0] = 0xAF;  // Header: Frame Sync (첫 번째 바이트)
                        message[1] = 0x01;  // Header: Destination Address (목적지 주소)
                        message[2] = 0x0A;  // Header: Source Address (출발지 주소)
                        message[3] = cmdCounter;  // CMD Counter: [0x00 ~ 0xFF]

                        // ModeOverride (7번째 비트 설정: 0x80 or 0x00)
                        if (j % 2 == 0)  // 짝수일 때 (ModeOverride = ON)
                        {
                            message[4] = (byte)(message[4] | 0x80);  // 7번째 비트를 1로 설정
                        }
                        else  // 홀수일 때 (ModeOverride = OFF)
                        {
                            message[4] = (byte)(message[4] & 0x7F);  // 7번째 비트를 0으로 설정
                        }

                        // FlightMode (6~5번째 비트 설정)
                        int flightMode = j % 4; // FlightMode는 0, 1, 2, 3 순환
                        message[4] = (byte)(message[4] | (flightMode << 5));  // 6~5번째 비트에 FlightMode 값 저장

                        // ModeEngage (4~1번째 비트 설정)
                        int modeEngage = j % 9; // ModeEngage는 0, 1, 2, 3, 4, 5, 6, 7, 8 순환
                        message[4] = (byte)(message[4] | (modeEngage << 1));

                        // 메시지 전송
                        await udpClient.SendAsync(message, message.Length);

                        // CMD Counter 증가 및 0xFF 이후 0으로 돌아감
                        cmdCounter = (byte)((cmdCounter + 1) % 256);

                        // 메시지 전송 후 500ms 지연
                        await Task.Delay(500);
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
