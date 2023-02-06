using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.IO;


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

            TcpClient[] clients = new TcpClient[countOfplayers];
            NetworkStream[] streams = new NetworkStream[countOfplayers];
            for (int i = 0; i < countOfplayers; i++)
            {
                clients[i] = server.AcceptTcpClient();
                streams[i] = clients[i].GetStream();
                Console.WriteLine("Подключился " + (i + 1).ToString() + " пользователь " + clients[i].Client.RemoteEndPoint.ToString());

                // Отсылаем номер игрока и информацию о столе
                string message = "order;" + i.ToString() + ";" + levelData;
                SendMessageAsync(message, streams[i], LARGE_BUFFER_SIZE);
            }


            // Начинаем прослушивать клиентов
            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 0; i < countOfplayers; i++)
            {
                tasks.Add(RecieveMessageAsync(streams[i]));
            }

            while (true)
            {
                for (int i = 0; i < countOfplayers; i++)
                {
                    if (tasks[i].Status == TaskStatus.RanToCompletion)
                    {
                        // Выводим сообщение, которое нам прислали
                        Console.WriteLine("Message recieved from " + clients[i].Client.RemoteEndPoint.ToString() + ": " + tasks[i].Result);

                        // Отсылаем пакет всем, кроме того, кто его прислал
                        for (int j = 0; j < countOfplayers; j++)
                        {
                            if (i == j) continue;

                            SendMessageAsync(tasks[i].Result, streams[j]);
                        }

                        // Опять начинаем прослушивать клиента
                        tasks[i] = RecieveMessageAsync(streams[i]);
                    }
                }
            }
        }


        private static async Task SendMessageAsync(string message, NetworkStream ns, int buffSize = BUFFER_STANDART_SIZE)
        {
            await Task.Run(() => SendHandler(message, ns, buffSize));
        }

        private static void SendHandler(string message, NetworkStream ns, int buffSize)
        {
            byte[] msg = new byte[buffSize];
            msg = Encoding.Default.GetBytes(message);  // конвертируем строку в массив байт

            ns.Write(msg, 0, msg.Length);     // отправляем сообщение
        }


        private static async Task<String> RecieveMessageAsync(NetworkStream ns, int buffSize = BUFFER_STANDART_SIZE)
        {
            return await Task<String>.Run(() => RecieveHandler(ns, buffSize));
        }


        private static string RecieveHandler(NetworkStream ns, int buffSize)
        {
            byte[] msg = new byte[buffSize];     // готовим место для принятия сообщения
            int count = ns.Read(msg, 0, msg.Length);   // читаем сообщение от клиента
            return Encoding.Default.GetString(msg, 0, count); // выводим на экран полученное сообщение в виде строк
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