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
            udpClient.Connect("127.0.0.1", 20000);

            try
            {
                Console.WriteLine("UDP Client Started!");
                byte cmdCounter = 0x00;
                if (udpClient == null)
                {
                    udpClient = new UdpClient();
                }
                int flapAngle = 0;
                int tiltAngle = 0;
                uint knobSpeed = 0;
                uint knobAltitude = 0;
                uint knobHeading = 0;
                uint stickThrottle = 0;
                uint stickRoll = 0;
                uint stickPitch = 0;
                uint stickYaw = 0;
                uint lonOfLP = 0;
                uint latOfLP = 0;
                ushort altOfLP = 0;
                while (true)
                {
                    for (int j = 0; j <= 10; j++) // 총 10번 루프 (테스트 반복)
                    {
                        byte[] message = new byte[32]; // message 바이트 수: (0 Byte ~ 32 Byte)
                        message[0] = 0xAF;             // Header: Frame Sync (첫 번째 바이트)
                        message[1] = 0x01;             // Header: Destination Address (목적지 주소)
                        message[2] = 0x0A;             // Header: Source Address (출발지 주소)
                        message[3] = cmdCounter;       // CMD Counter: [0x00 ~ 0xFF]

                        // 짝수일 때  (= ON)
                        if (j % 2 == 0)
                        {
                            message[4] = (byte)(message[4] | 0x80);   // 7번째 비트를 1로 설정  (Byte #5.)
                            message[5] = (byte)(message[5] | 0x80);   // 7번째 비트를 1로 설정  (Byte #6.)
                            message[6] = (byte)(message[6] | 0x80);   // 7번째 비트를 1로 설정  (Byte #7.)
                            message[24] = (byte)(message[24] | 0x80); // 7번째 비트를 1로 설정 (Byte #25.)
                            message[24] = (byte)(message[24] | 0x01); // 0번째 비트를 1로 설정 (Byte #25.)
                        }
                        // 홀수일 때 (= OFF)
                        else
                        {
                            message[4] = (byte)(message[4] & 0x7F);    // 7번째 비트를 0으로 설정  (Byte #5.)
                            message[5] = (byte)(message[5] & 0x7F);    // 7번째 비트를 0으로 설정  (Byte #6.)
                            message[6] = (byte)(message[6] & 0x7F);    // 7번째 비트를 0으로 설정  (Byte #7.)
                            message[24] = (byte)(message[24] & ~0x80); // 7번째 비트를 0으로 설정 (Byte #25.)
                            message[24] = (byte)(message[24] & ~0x01); // 0번째 비트를 0으로 설정 (Byte #25.)
                        }

                        // [FlightMode]는 [0, 1, 2, 3]까지 순환
                        int flightMode = j % 4;
                        message[4] = (byte)(message[4] | (flightMode << 5));       // FlightMode (6 ~ 5번째 비트 설정) (Byte #5.)

                        // [ModeEngage]는 [0, 1, 2, 3, 4, 5, 6, 7, 8]까지 순환
                        int modeEngage = j % 9;
                        message[4] = (byte)(message[4] | (modeEngage << 1));       // ModeEngage (4 ~ 1번째 비트 설정) (Byte #5.)
                        message[5] = (byte)(message[5] | (flapAngle << 1));        // FlapAngle  (6 ~ 1번째 비트 설정) (Byte #6.)
                        message[6] = (byte)(message[6] | (tiltAngle));             // TiltAngle  (6 ~ 0번째 비트 설정) (Byte #7.)

                        message[7] = (byte)(knobSpeed / 1);      // KnobSpeed      (Byte #8.)
                        message[8] = (byte)(knobAltitude / 15);  // knobAltitude   (Byte #9.)
                        message[9] = (byte)(knobHeading / 2);    // knobHeading    (Byte #10.)

                        message[10] = (byte)(stickThrottle);     // StickThrottle  (Byte #11.)
                        message[11] = (byte)(stickRoll);         // StickRoll      (Byte #12.)
                        message[12] = (byte)(stickPitch);        // StickPitch     (Byte #13.)
                        message[13] = (byte)(stickYaw);          // StickYaw       (Byte #14.)

                        byte[] lonOfLPBytes = BitConverter.GetBytes(lonOfLP); // 경도값을 4 바이트로 변환
                        Array.Copy(lonOfLPBytes, 0, message, 14, 4);  // message[14] ~ message[17]에 복사 [Byte #15. ~ Byte #18.]  = [4 Byte]

                        byte[] latOfLPBytes = BitConverter.GetBytes(latOfLP); // 위도값을 4 바이트로 변환
                        Array.Copy(latOfLPBytes, 0, message, 18, 4);  // message[18] ~ message[22]에 복사 [Byte #19. ~ Byte #22.]  = [4 Byte]

                        byte[] altOfLPBytes = BitConverter.GetBytes(altOfLP); // 고도값을 2 바이트로 변환
                        Array.Copy(altOfLPBytes, 0, message, 23, 2);  // message[23] ~ message[24]에 복사 [Byte #23. ~ Byte #24.]  = [2 Byte]

                        await udpClient.SendAsync(message, message.Length); // 메시지 송신 (Client => Server)

                        await Task.Delay(200); // 메시지 송신 후, 0.2초 지연

                        cmdCounter = (byte)((cmdCounter + 1) % 256); // CMD Counter 값 [0 ~ 255] 순환

                        // [플랩각°(도)] 2만큼 증가
                        flapAngle += 2;
                        // [0 ~ 40] 순환
                        if (flapAngle > 40) flapAngle = 0;

                        // [틸트각°(도)] 1만큼 증가
                        tiltAngle += 1;
                        // [0도 ~ 90도] 순환
                        if (tiltAngle > 90) tiltAngle = 0;

                        // [노브 속도(km/h)] 1만큼 증가
                        knobSpeed += 1;
                        // [0 ~ 250] 순환
                        if (knobSpeed > 250) knobSpeed = 0;

                        // [노브 고도(m)] 15만큼 증가
                        knobAltitude += 15;
                        // [0 ~ 3000] 순환
                        if (knobAltitude > 3000) knobAltitude = 0;

                        // [노브 방위°(도)] 2만큼 증가
                        knobHeading += 2;
                        // [0 ~ 358] 순환
                        if (knobHeading > 358) knobHeading = 0;

                        // [스틱 고도] 1만큼 증가
                        stickThrottle += 1;
                        // [0 ~ 200] 순환
                        if (stickThrottle > 200) stickThrottle = 0;

                        // [스틱 횡방향 속도] 1만큼 증가
                        stickRoll += 1;
                        // [0 ~ 200] 순환
                        if (stickRoll > 200) stickRoll = 0;

                        // [스틱 종방향 속도] 1만큼 증가
                        stickPitch += 1;
                        // [0 ~ 200] 순환
                        if (stickPitch > 200) stickPitch = 0;

                        // [스틱 방위] 1만큼 증가
                        stickYaw += 1;
                        // [0 ~ 200] 순환
                        if (stickYaw > 200) stickYaw = 0;

                        // 경도 1만큼 증가
                        lonOfLP += 1;
                        // [0 ~ 3600000000] 순환
                        if (lonOfLP > 3600000000) lonOfLP = 0;
                        //Console.WriteLine($"LonOfLP: {lonOfLP}, 고도(m): {(lonOfLP * 0.0000001) - 180.0}°(도)");

                        // 위도 1만큼 증가
                        latOfLP += 1;
                        // [0 ~ 1800000000] 순환
                        if (latOfLP > 1800000000) latOfLP = 0;
                        //Console.WriteLine($"LatOfLP: {latOfLP}, 고도(m): {(latOfLP * 0.0000001) - 90.0}°(도)");

                        // 고도 1만큼 증가
                        altOfLP += 1;
                        // [0 ~ 60000] 순환
                        if (altOfLP > 60000) altOfLP = 0;
                        //Console.WriteLine($"AltOfLP: {altOfLP}, 고도(m): {(altOfLP * 0.025) - 500.0} m");
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
