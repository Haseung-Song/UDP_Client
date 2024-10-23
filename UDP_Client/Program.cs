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
                Console.WriteLine("UDP Client Started!");
                byte cmdCounter = 0x00; // CMD Counter 초기값
                if (udpClient == null)
                {
                    udpClient = new UdpClient();
                }
                int angleFlap = -40;
                int angleTilt = 0;
                int knobSpeed = 0;
                int knobAltitude = 0;
                int knobHeading = 0;
                int stickThrottle = 0;
                int stickRoll = -100;
                int stickPitch = -100;
                int stickYaw = -100;
                while (true)
                {
                    for (int j = 0; j <= 80; j++) // 80번 루프 (테스트 반복)
                    {
                        byte[] message = new byte[31]; // 메시지의 바이트 크기 (5 Byte ~ 30 Byte)
                        message[0] = 0xAF; // Header: Frame Sync (첫 번째 바이트)
                        message[1] = 0x01; // Header: Destination Address (목적지 주소)
                        message[2] = 0x0A; // Header: Source Address (출발지 주소)
                        message[3] = cmdCounter; // CMD Counter: [0x00 ~ 0xFF]

                        // ModeOverride (7번째 비트 설정: 0x80 or 0x00)
                        if (j % 2 == 0)  // 짝수일 때 (ModeOverride = ON)
                        {
                            message[4] = (byte)(message[4] | 0x80); // 7번째 비트를 1로 설정 (Byte #4.)
                            message[5] = (byte)(message[5] | 0x80); // 7번째 비트를 1로 설정 (Byte #5.)
                            message[6] = (byte)(message[6] | 0x80); // 7번째 비트를 1로 설정 (Byte #6.)
                        }
                        else  // 홀수일 때 (ModeOverride = OFF)
                        {
                            message[4] = (byte)(message[4] & 0x7F); // 7번째 비트를 0으로 설정 (Byte #4.)
                            message[5] = (byte)(message[5] & 0x7F); // 7번째 비트를 0으로 설정 (Byte #5.)
                            message[6] = (byte)(message[6] & 0x7F); // 7번째 비트를 0으로 설정 (Byte #6.)
                        }

                        // FlightMode (6~5번째 비트 설정)
                        int flightMode = j % 4; // FlightMode는 0, 1, 2, 3 순환
                        message[4] = (byte)(message[4] | (flightMode << 5)); // 6~5번째 비트에 FlightMode 값 저장

                        // ModeEngage (4~1번째 비트 설정)
                        int modeEngage = j % 9; // ModeEngage는 0, 1, 2, 3, 4, 5, 6, 7, 8 순환
                        message[4] = (byte)(message[4] | (modeEngage << 1));

                        // FlapAngle  (6~1번째 비트 설정)
                        int flapAngle = (angleFlap + 40) / 2;
                        message[5] = (byte)(message[5] | (flapAngle << 1));

                        // TiltAngle  (6~0번째 비트 설정)
                        message[6] = (byte)(message[6] | angleTilt);

                        message[7] = (byte)(knobSpeed);          // KnobSpeed    (8 바이트 설정)
                        message[8] = (byte)(knobAltitude / 15);  // knobAltitude (9 바이트 설정)
                        message[9] = (byte)(knobHeading / 2);    // knobHeading (10 바이트 설정)

                        message[10] = (byte)(stickThrottle);     // StickThrottle (11 바이트 설정)

                        // 정수 값을 0.01 단위로 변환 ([-1 ~ 1] 범위 변환) (StickRoll)
                        double stickRollToDouble = stickRoll * 0.01;
                        // [-1 ~ 1] => [0X00 ~ 0XC8] 변환
                        int rollStick = (int)((stickRollToDouble + 1) * 100);
                        message[11] = (byte)(rollStick);         // StickRoll     (12 바이트 설정)

                        // 정수 값을 0.01 단위로 변환 ([-1 ~ 1] 범위 변환) (StickPitch)
                        double stickPitchToDouble = stickPitch * 0.01;
                        // [-1 ~ 1] => [0X00 ~ 0XC8] 변환
                        int pitchStick = (int)((stickPitchToDouble + 1) * 100);
                        message[12] = (byte)(pitchStick);

                        // 정수 값을 0.01 단위로 변환 ([-1 ~ 1] 범위 변환) (StickYaw)
                        double stickYawToDouble = stickYaw * 0.01;
                        // [-1 ~ 1] => [0X00 ~ 0XC8] 변환
                        int yawStick = (int)((stickYawToDouble + 1) * 100);
                        message[13] = (byte)(yawStick);

                        await udpClient.SendAsync(message, message.Length); // 메시지 전송
                        await Task.Delay(100); // 메시지 전송 후 0.1초 지연

                        // CMD Counter 증가 및 0xFF ,후 0으로 돌아감
                        cmdCounter = (byte)((cmdCounter + 1) % 256);

                        // 각도 2만큼 증가
                        angleFlap += 2;
                        // [-40도 ~ 40도] 순환
                        if (angleFlap > 40) angleFlap = -40;

                        // 각도 1만큼 증가
                        angleTilt += 1;
                        // [0도 ~ 90도] 순환
                        if (angleTilt > 90) angleTilt = 0;

                        // 속도(km/h) 1만큼 증가
                        knobSpeed += 1;
                        // [0km/h ~ 250km/h] 순환
                        if (knobSpeed > 250) knobSpeed = 0;

                        // 고도(m) 15만큼 증가
                        knobAltitude += 15;
                        // [0m ~ 3000m] 순환
                        if (knobAltitude > 3000) knobAltitude = 0;

                        // 각도 2만큼 증가
                        knobHeading += 2;
                        // [0도 ~ 358도] 순환
                        if (knobHeading > 358) knobHeading = 0;

                        // 고도 1만큼 증가
                        stickThrottle += 1;
                        double stickThrottleToDouble = stickThrottle * 0.005;
                        // [0 ~ 1] 순환
                        if (stickThrottleToDouble > 1) stickThrottle = 0;

                        // 속도 1만큼 증가
                        stickRoll += 1;
                        // [-100 ~ 100] 순환
                        if (stickRoll > 100) stickRoll = -100;

                        // 속도 1만큼 증가
                        stickPitch += 1;
                        // [-100 ~ 100] 순환
                        if (stickPitch > 100) stickPitch = -100;

                        // 속도 1만큼 증가
                        stickYaw += 1;
                        // [-100 ~ 100] 순환
                        if (stickYaw > 100) stickYaw = -100;
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
