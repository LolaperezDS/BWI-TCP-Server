using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Players
{
    /*
      0  Red,
      1  Blue,
      2  Green,
      3  Yellow,
    */

    public class Player
    {

        const int BUFFER_STANDART_SIZE = 1024;

        private TcpClient tcpClient;
        public TcpClient TcpClient => tcpClient;

        private NetworkStream networkStream;
        public NetworkStream NetworkStream => networkStream;

        public Task<string> ThreadingTask;

        private int playerNumber;
        public bool isInitiazlizated = false;
        public string NickName;

        public Player(TcpClient tcpClient, NetworkStream networkStream, int playerNumber)
        {
            this.tcpClient = tcpClient;
            this.networkStream = networkStream;
            if (playerNumber > 3 || playerNumber < 0) throw new Exception("Wrong player order");
            this.playerNumber = playerNumber;
        }

        public async Task SendMessageAsync(string message, int buffSize = BUFFER_STANDART_SIZE)
        {
            await Task.Run(() => SendHandler(message, buffSize));
        }
        public void SendHandler(string message, int buffSize)
        {
            byte[] msg = new byte[message.Length + 2];
            msg[0] = 0x00;
            Encoding.Default.GetBytes(message).CopyTo(msg, 1);  // конвертируем строку в массив байт
            msg[message.Length + 2 - 1] = 0x01;
            networkStream.Write(msg, 0, msg.Length);     // отправляем сообщение
        }
        public async void RecieveMessageAsync(int buffSize = BUFFER_STANDART_SIZE)
        {
            ThreadingTask = Task<String>.Run(() => RecieveHandler(buffSize));
            await ThreadingTask;
        }
        private string RecieveHandler(int buffSize)
        {
            // byte[] msg = new byte[buffSize];     // готовим место для принятия сообщения
            // int count = networkStream.Read(msg, 0, msg.Length);   // читаем сообщение от клиента
            // return Encoding.Default.GetString(msg, 0, count); // выводим на экран полученное сообщение в виде строк

            GetZeroByte();
            
            StringBuilder stringBuilder = new StringBuilder();
            byte[] msg = new byte[1];
            
            while (true)
            {
                int count = networkStream.Read(msg, 0, 1);
                if (count == 0)
                {
                    continue;
                    // throw new Exception("Cannot read from tcp connection");
                }

                if (msg[0] == 0x01)
                {
                    break;
                }

                stringBuilder.Append(Encoding.Default.GetString(msg, 0, 1));
            }

            return stringBuilder.ToString();
        }

        private void GetZeroByte()
        {
            byte[] bytes = new byte[1]; 
            while (true)
            {
                int count = networkStream.Read(bytes, 0, 1);
                if (count == 0)
                {
                    continue;
                    // throw new Exception("Cannot read from tcp connection");
                }

                if (bytes[0] == 0x00)
                {
                    return;
                }
            }
        }

    }
}