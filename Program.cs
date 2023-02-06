using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Players;
using System.Text.Json;


namespace BlockWarsServerTcp
{
    class Program
    {
        const int BUFFER_STANDART_SIZE = 1024;
        const int LARGE_BUFFER_SIZE = 2 * 65536;
        const string DATA_PATH = "/saves/";
        static void Main(string[] args)
        {
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
                NetworkStream stream = client.GetStream();
                players[i] = new Player(client, stream, i);

                // Ожидание приветственного сообщения PlayerData
                players[i].RecieveMessageAsync();
                players[i].ThreadingTask.Wait();

                // Принятие и присваивание никнейма игрока
                PlayerData pd = JsonSerializer.Deserialize<PlayerData>(players[i].ThreadingTask.Result);
                players[i].NickName = pd.NickName;
                Console.WriteLine("Подключился " + pd.NickName + " пользователь " + players[i].TcpClient.Client.RemoteEndPoint.ToString());

                // Отсылаем номер игрока и информацию о столе
                PlayerInitialization pi = new PlayerInitialization(i, levelData);
                string message = JsonSerializer.Serialize<PlayerInitialization>(pi);
                players[i].SendMessageAsync(message, LARGE_BUFFER_SIZE);


                // TODO если пакет не десереализуемый, абортить соединение и ожидать новый
            }

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
                        Console.WriteLine("Message recieved from " + players[i].TcpClient.Client.RemoteEndPoint.ToString() + " NICK=" + players[i].NickName + ": " + players[i].ThreadingTask.Result);

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

        private static string LoadLevel(string filename)
        {
            string loadedData = "";
            using (var reader = new StreamReader(DATA_PATH + filename))
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
                Console.WriteLine(i.ToString() + " - " + levelNames[i]);
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