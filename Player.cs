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
            byte[] msg = new byte[buffSize];
            msg = Encoding.Default.GetBytes(message);  // конвертируем строку в массив байт
            networkStream.Write(msg, 0, msg.Length);     // отправляем сообщение
        }
        public async void RecieveMessageAsync(int buffSize = BUFFER_STANDART_SIZE)
        {
            ThreadingTask = Task<String>.Run(() => RecieveHandler(buffSize));
            await ThreadingTask;
        }
        private string RecieveHandler(int buffSize)
        {
            byte[] msg = new byte[buffSize];     // готовим место для принятия сообщения
            int count = networkStream.Read(msg, 0, msg.Length);   // читаем сообщение от клиента
            return Encoding.Default.GetString(msg, 0, count); // выводим на экран полученное сообщение в виде строк
        }

    }
}