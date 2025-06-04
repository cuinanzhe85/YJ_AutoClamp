using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YJ_AutoClamp.Models
{
    public class TcpServer_Model
    {
        private TcpListener _listener;
        private bool _isRunning = false;

        // 클라이언트 목록 (스레드 안전한 ConcurrentDictionary 사용)
        private ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();

        // 포트 번호 설정
        private int _port;

        public TcpServer_Model(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        // 서버 시작
        public void Start()
        {
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"서버가 포트 {_port}에서 시작되었습니다.");

            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    string clientKey = ((IPEndPoint)client.Client.RemoteEndPoint).ToString();

                    if (_clients.TryAdd(clientKey, client))
                    {
                        Console.WriteLine($"클라이언트 접속: {clientKey}");
                        _ = HandleClientAsync(clientKey, client);
                    }
                }
            });
        }

        // 서버 중지
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            foreach (var client in _clients.Values)
            {
                client.Close();
            }

            _clients.Clear();
            Console.WriteLine("서버가 중지되었습니다.");
        }

        // 클라이언트 처리
        private async Task HandleClientAsync(string clientKey, TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (_isRunning && client.Connected)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                        break;

                    string received = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine($"[{clientKey}] 받은 메시지: {received}");

                    // 받은 데이터 처리
                    string response = ProcessClientData(received);

                    // 처리 결과를 클라이언트에 응답
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{clientKey}] 오류: {ex.Message}");
            }
            finally
            {
                client.Close();
                _clients.TryRemove(clientKey, out _);
                Console.WriteLine($"클라이언트 연결 해제됨: {clientKey}");
            }
        }
        // 클라이언트 데이터 처리 메서드
        private string ProcessClientData(string data)
        {
            // 간단한 프로토콜 예시

            if (string.IsNullOrWhiteSpace(data))
                return "Empty message received";

            // PING 명령에 대한 응답
            if (data.Equals("PING", StringComparison.OrdinalIgnoreCase))
                return "PONG";

            // ECHO 명령: "ECHO:문자열"
            if (data.StartsWith("ECHO:", StringComparison.OrdinalIgnoreCase))
            {
                string echoMsg = data.Substring(5).Trim();
                return echoMsg;
            }

            // 기타 처리 - 받은 그대로 다시 돌려줌
            return $"서버가 처리한 메시지: {data}";
        }
        // 클라이언트 연결 상태 체크
        public void CheckClientConnections()
        {
            Console.WriteLine("현재 연결된 클라이언트 목록:");
            foreach (var kv in _clients)
            {
                string key = kv.Key;
                TcpClient client = kv.Value;

                bool connected = IsClientConnected(client);
                Console.WriteLine($"[{key}] 연결 상태: {(connected ? "연결됨" : "끊김")}");
            }
        }

        // 실제 연결 여부 판단
        private bool IsClientConnected(TcpClient client)
        {
            try
            {
                if (client == null || !client.Connected)
                    return false;

                if (client.Client.Poll(0, SelectMode.SelectRead) && client.Available == 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
