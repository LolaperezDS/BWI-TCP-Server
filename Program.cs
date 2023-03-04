using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Players;
using System.Text.Json;


namespace BlockWarsServerTcp
{
    class Program
    {
        const int LARGE_BUFFER_SIZE = 65536 * 2;
        static string DATA_PATH = AppDomain.CurrentDomain.BaseDirectory + "\\saves\\";
        static void Main(string[] args)
        {
            // DebugFunction();
            // initiaization
            Console.WriteLine("Введите количество игроков: ");
            int countOfplayers = Convert.ToInt32(Console.ReadLine());
            if (countOfplayers > 4 || countOfplayers <= 0) throw new Exception();

            Console.WriteLine("Введите порт для сервера: ");
            int port = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Выбор уровня");
            string levelData = LoadLevel(ChoseSave());

            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            

            Player[] players = new Player[countOfplayers];

            for (int i = 0; i < countOfplayers; i++)
            {
                // Создание объекта игрок
                TcpClient client = server.AcceptTcpClient();
                client.SendBufferSize = LARGE_BUFFER_SIZE;
                NetworkStream stream = client.GetStream();
                players[i] = new Player(client, stream, i);

                // Ожидание приветственного сообщения PlayerData
                players[i].RecieveMessageAsync();
                players[i].ThreadingTask.Wait();

                // Принятие и присваивание никнейма игрока
                string PlayerNick = players[i].ThreadingTask.Result;
                players[i].NickName = PlayerNick;
                Logger.LogInfo("Подключился " + PlayerNick + " пользователь " + players[i].TcpClient.Client.RemoteEndPoint.ToString());

                // Отсылаем номер игрока и информацию о столе
                string message = i.ToString() + ";" + levelData;
                players[i].SendMessageAsync(message, LARGE_BUFFER_SIZE);

                client.SendBufferSize = 1024 * 8;
            }

            Console.Clear();
            Console.WriteLine("Сервер запущен для " + countOfplayers + " игроков...");

            foreach (Player player in players)
            {
                player.RecieveMessageAsync();
            }

            while (true)
            {
                for (int i = 0; i < countOfplayers; i++)
                {
                    if (players[i].ThreadingTask.Status == TaskStatus.RanToCompletion)
                    {
                        // Выводим сообщение, которое нам прислали
                        Logger.LogInfo("Message recieved from " + players[i].TcpClient.Client.RemoteEndPoint.ToString() + " Nickname = " + players[i].NickName + " \tMessage: " + players[i].ThreadingTask.Result);
                        CheckPlayers(players);
                        // Отсылаем пакет всем, кроме того, кто его прислал
                        for (int j = 0; j < countOfplayers; j++)
                        {
                            if (i == j) continue;
                            players[j].SendMessageAsync(players[i].ThreadingTask.Result);
                        }

                        // Опять начинаем прослушивать клиента
                        players[i].RecieveMessageAsync();
                    }
                }
            }
        }

        private static void CheckPlayers(Player[] players)
        {
            foreach (Player player in players)
            {
                Logger.LogInfo(player.NickName + " Connected: " + player.TcpClient.Connected.ToString());
            }
        }

        private static string LoadLevel(string filename)
        {
            string loadedData = "";
            using (var reader = new StreamReader(filename))
            {
                loadedData = reader.ReadToEnd();
            }
            return loadedData;
        }
        private static string ChoseSave()
        {
            string searchFilter = "*.json";
            string[] levelNames = Directory.GetFiles(DATA_PATH, searchFilter);
            if (levelNames.Length == 0)
            {
                Console.WriteLine("Найдено 0 сохранений");
                throw new Exception();
            }
            Console.WriteLine("Выберите номер:");
            for (int i = 0; i < levelNames.Length; i++)
            {
                Console.WriteLine(i.ToString() + " - " + levelNames[i].Split('\\')[levelNames[i].Split('\\').Length - 1].Split('.')[0]);
            }
            int index = Convert.ToInt32(Console.ReadLine());
            if (index < 0 || index >= levelNames.Length)
            {
                Console.WriteLine("Ты чё в глаза ебёшься?");
                throw new Exception();
            }
            return levelNames[index];
        }
    }
}