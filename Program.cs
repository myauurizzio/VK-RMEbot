using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using VK_RMEbot.Models;
using System.Web;

namespace VK_RMEbot
{
    class Program
    {
        
        /// <summary>
        /// Точка входа в программу бота для сообщества VK
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //пишем диагностические сообщения в лог и на экран
            //каждый день новый лог
            string logFileBot = $@"logs\{DateTime.Now.ToString("yyyyMMdd")}_vkbot.log ";
            string logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";

            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Bot started ");
            logStringBot += $@"[Bot started]";
            File.AppendAllText(logFileBot, logStringBot);


            //для самовосстановления при сетевых отказах используем счётчик ошибок и метку времени
            int ExceptionCounter = 0;
            DateTime LastExceptionTime = DateTime.Now;

            //загружаем сценарии работы
            LoadScenarios();

            //включаем самого бота
            while (true)
            {
                try
                {
                    //вызываем модуль работы с VK 
                    SolverApi();
                }
                // перехватываем исключения
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.Now.ToString()} Exception {e.Message} / {e.Source}");
                    logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
                    logStringBot += $@"[Exception msg: {e.Message}][Exception src: {e.Source}]";
                    File.AppendAllText(logFileBot, logStringBot);

                    //если с момента предыдущего исключения прошло меньше 30 минут увеличиваем счётчик ошибок
                    //если насчитали больше 10 ошибок, то завершаем программу и выходим
                    if (DateTime.Now < LastExceptionTime.AddMinutes(30))
                    {
                        ExceptionCounter++;
                        if (ExceptionCounter > 10)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString()} too much exceptions ");
                            logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
                            logStringBot += $@"[too much exceptions]";
                            File.AppendAllText(logFileBot, logStringBot);

                            break;
                        }
                    }
                    //если с момента предыдущего исключения прошло больше 30 минут, 
                    //то сбрасываем счётчик ошибок 
                    //и выставляем метку времени предыдущего исключения в текущий момент
                    else
                    {
                        ExceptionCounter = 0;
                        LastExceptionTime = DateTime.Now;
                    }
                    //первые семь раз приостанавливаем программу на минуту в надежде,
                    //что источник исключения исчезнет (восстановится канал связи, например)
                    if (ExceptionCounter < 7)
                    {
                        Thread.Sleep(60000);
                    }
                    //следующие три раза делаем паузы по пять минут
                    else
                    {
                        Thread.Sleep(60000 * 5);
                    }
                    //при неустранимой проблеме программа последовательно делает десять пауз
                    //общая длительность последовательных попыток восстановления - 27 минут
                    //если между исключениями функциональность программы 
                    //восстановится на 4 минуты (в сумме между между любыми попытками) счётчик ошибок сбросится
                    //а метка времени будет обновлена 
                    //так программа пытается работать на плохих каналах связи


                }
                finally
                {
                    //Console.WriteLine($"{DateTime.Now.ToString()} Exception counted ");
                    
                }

            }

            //если ничего не помогло, то выходим совсем
            logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
            logStringBot += $@"[Bot stopped]";
            File.AppendAllText(logFileBot, logStringBot);

            Console.WriteLine("\n\nPress any key to exit... ");
            Console.ReadLine();
        }


        /// <summary>
        /// Метод, загружающий сценарии из JSON файлов
        /// </summary>
        public static void LoadScenarios ()
        {
            //Список сценариев находится в файле с фиксированным именем
            //В нём заходятся служебные параметры и имена остальных файлов
            string json = File.ReadAllText("SC-list.json");

            //ищем в списке ID стартового сценария и парсим его в static класс Scenarios
            Scenarios.default_id = JObject.Parse(json)["default_id"].ToString();

            //Делаем объект List<Scenario> в который грузим список сценариев
            Scenarios.scenarios = new List<Scenario>();

            var _scenarios = JObject.Parse(json)["scenarios"].ToList();

            //Проходим по списку, загружаем указанные в нём файлы
            //парсим их в объекты индивидуальных сценариев в общем дереве
            //в результате, получаем возможность перехода между ветками из любого узла в любой
            //формат файлов и детали реализации приведены в документации "VK-RMEbot script algo.ppt"
            foreach (var _scenario in _scenarios)
            {
                string json2 = File.ReadAllText(_scenario["filename"].ToString());

                Scenario scenario = new Scenario();
                scenario.scenario_id = JObject.Parse(_scenario.ToString())["scenario_id"].ToString(); // _scenario.scenario_id  JObject.Parse(json2)["scenario_id"].ToString();
                scenario.scenario_name = JObject.Parse(json2)["scenario_name"].ToString();
                scenario.start_id = JObject.Parse(json2)["start_id"].ToString();

                scenario.steps = new List<Step>();

                var _steps = JObject.Parse(json2)["steps"].ToList();

                foreach(var _step in _steps)
                {
                    Step step = new Step();

                    step.id = JObject.Parse(_step.ToString())["id"].ToString();
                    step.text = JObject.Parse(_step.ToString())["text"].ToString();

                    step.buttons = new List<BotButton>();

                    var _buttons = JObject.Parse(_step.ToString())["buttons"].ToList();

                    foreach(var _button in _buttons)
                    {
                        BotButton button = new BotButton();

                        button.label = JObject.Parse(_button.ToString())["label"].ToString();
                        button.color = JObject.Parse(_button.ToString())["color"].ToString();
                        button.next_id = JObject.Parse(_button.ToString())["next_id"].ToString();

                        if (JObject.Parse(_button.ToString()).ContainsKey("scenario_id"))
                        {
                            button.scenario_id = JObject.Parse(_button.ToString())["scenario_id"].ToString();
                        }


                        step.buttons.Add(button);
                    }
                    scenario.steps.Add(step);
                }
                Scenarios.scenarios.Add(scenario);
            }

            string logFileBot = $@"logs\{DateTime.Now.ToString("yyyyMMdd")}_vkbot.log ";
            string logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";

            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Scenario Loaded ");
            logStringBot += $@"[Scenario: Loaded]";
            File.AppendAllText(logFileBot, logStringBot);

            return;
        }

        /// <summary>
        /// Метод разбирающий сообщение пользователя и возвращающий в ответ следующий шаг сценария
        /// </summary>
        /// <param name="inMsg">входящее сообщение</param>
        /// <param name="inPayload">входящий Payload (нажатая кнопка)</param>
        /// <returns></returns>
        static string GetMessage(string inMsg, string inPayload)
        {
            //будущий текст ответа
            string outMsg = "&message=";
            //будущие кнопки в ответе
            string keyboard = "&keyboard=";

            Scenario scenario;
            Step step;

            // проверяем, была ли пользователем нажата кнопка 
            if (!String.IsNullOrEmpty(inPayload) 
                && (inPayload.IndexOf(@"{""command"":""start""}") == -1)
                && (inPayload.IndexOf(@"""scenario"":") > -1)
                && (inPayload.IndexOf(@"""next_id"":") > -1)
                )
            {
                // получаем из кнопки id сценария и следующего шага
                string scen_id = JObject.Parse(inPayload)["scenario"].ToString();
                string next_id = JObject.Parse(inPayload)["next_id"].ToString();

                //по полученным id делаем выборку из дерева сценариев
                scenario = Scenarios.scenarios.FirstOrDefault(_sce => _sce.scenario_id == scen_id);
                step = scenario.steps
                    .FirstOrDefault(_st => _st.id == next_id)
                    ;

            }
            else
            {
                //если кнопка не нажата (первый запуск или пользователь отправил текст, а не нажал кнопку)
                //находим сценарий по умолчанию и его первый шаг
                scenario = Scenarios.scenarios.FirstOrDefault(_sce => _sce.scenario_id == Scenarios.default_id);
                step = scenario.steps
                    .FirstOrDefault(_st => _st.id == scenario.start_id)
                    ;


            }

            //включаем функционал кнопок в чате с пользователем и формируем их массив
            //формат описан в бот-API VK
            keyboard += @"{""inline"": true,""buttons"": [";

            foreach (BotButton _btn in step.buttons)
            {
                string scenario_id = String.Empty;
                if (_btn.scenario_id is null)
                {
                    scenario_id = scenario.scenario_id;
                }
                else
                {
                    scenario_id = _btn.scenario_id;
                }

                string btn = @"[{""action"": {""type"": ""text"",""payload"": """
                            + @"{\""scenario\"":\""" + scenario_id + @"\"","
                            + @"\""next_id\"":\""" + _btn.next_id + @"\""}"
                            + @""",""label"": """
                            + _btn.label
                            + @"""},""color"": """
                            + _btn.color
                            + @"""}],";
                keyboard += btn;
            }

            keyboard = keyboard.TrimEnd(',') + "]}";

            // формируем текст-описание следующего шага
            // кодируем его для исключения управляющих символов
            outMsg += HttpUtility.UrlEncode(step.text);

            // возвращаем вызывающему методу текст+кнопки
            return outMsg + keyboard;
        }


        /// <summary>
        /// Метод работающий с API VK
        /// </summary>
        static void SolverApi()
        {
            string logFileBot = $@"logs\{DateTime.Now.ToString("yyyyMMdd")}_vkbot.log ";
            string logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";

            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} initialization ");
            logStringBot += "[initialization]";
            File.AppendAllText(logFileBot, logStringBot);

            //параметры подключения
            //для безопасности и универсальности вынесены за пределы компилируемого кода в отдельные файлы
            //процесс настройки и получения значений описан в документации "VK-RMEbot server settings.ppt"
            string apikey = File.ReadAllText("_apikey");
            ulong appid = Convert.ToUInt64(File.ReadAllText("_appid"));
            string login = File.ReadAllText("_login");
            string password = File.ReadAllText("_password");
            string groupid = File.ReadAllText("_groupid");

            // цикл создания сеансов обращений к серверу VK 
            // первый запуск, перезапуск после обработанной ошибки, перезапуск по таймеру
            while (true)
            {

                //используем функционал VkNet
                VkApi vkClient = new VkApi();
                WebClient webClient = new WebClient();

                vkClient.Authorize(new ApiAuthParams
                {
                    ApplicationId = appid,
                    Login = login,
                    Password = password,
                    Settings = Settings.All
                });

                var param = new VkParameters(new Dictionary<string, string>() { { "group_id", groupid } });
                //получить id группы https://vk.com/dev/utils.resolveScreenName

                dynamic longPoll = JObject.Parse(vkClient.Call("groups.getLongPollServer", param).RawJson);
                // https://vk.com/dev/groups.getLongPollServer 
                // https://vk.com/dev/using_longpoll 


                // key - секретный ключ сессии
                // server - адрес сервера
                // ts - номер последнего события, начиная с которого надо получать данные
                // https://{$server}?act=a_check&key={$key}&ts={$ts}&wait=25&mode=2&version=1
                string json = String.Empty;

                string url = String.Empty;

                // специальная метка времени для принудительного перезапуска сеанса работы с VK
                // иначе сеанс зависает и перестаёт принимать и отправлять сообщения
                // по умолчанию перезапуск сеанса идёт каждые 30 минут
                DateTime watchDog = DateTime.Now;
                Console.WriteLine($"WatchDog set to {watchDog.ToString("dd.MM.yyyy HH:mm:ss")}");
                logStringBot = @$"[WatchDog set to {watchDog.ToString("dd.MM.yyyy HH:mm:ss")}]";
                File.AppendAllText(logFileBot, logStringBot);

                //обращение к longPoll сервису VK
                while (true)
                {
                    //строка запроса
                    url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=10",
                        longPoll.response.server.ToString(),
                        longPoll.response.key.ToString(),
                        json != String.Empty ? JObject.Parse(json)["ts"].ToString() : longPoll.response.ts.ToString()
                        );
                    // https://lp.vk.com/wh193815240?act=a_check&key=9.......2&wait=25&mode=2&ts=1



                    //ответ сервера
                    json = webClient.DownloadString(url);

                    //Console.WriteLine(url);
                    //Console.WriteLine(json);


                    //пустой ответ
                    #region Empty Json
                    //{ 
                    //    ts: "500",
                    //    updates: [ ]
                    //}
                    #endregion

                    // проверяем ответ на пустоту, а так же некоторые сообщения об ошибках
                    var jsonMsg = json.IndexOf(":[]}") > -1 ? "" : $"{json} \n";

                    // обрабатываем ошибки и перезапускаем сеанс работы с VK
                    if (json.IndexOf("failed\":2}") > -1)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} json failed 2 ");
                        logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
                        logStringBot += $"[Error: json failed 2]";
                        File.AppendAllText(logFileBot, logStringBot);

                        break;
                    }

                    if (json.IndexOf("failed\":3}") > -1)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString()} json failed 3 ");
                        logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
                        logStringBot += $"[Error: json failed 3]";
                        File.AppendAllText(logFileBot, logStringBot);

                        break;
                    }

                    // Парсим ответ сервера в список-коллекцию (List)
                    var col = JObject.Parse(json)["updates"].ToList();

                    // Проходим по списку
                    foreach (var item in col)
                    {
                        if (item["type"].ToString() == "message_new")
                        {
                            string key = apikey;
                            // ВАЖНО - функциональность API VK зависит от запрошенной версии
                            // например "v=5.69" даёт кнопки и возвращает их payload
                            // а "v=5.41" - нет
                            string urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.69&access_token={key}&user_id=";
                            //string urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.41&access_token={key}&user_id=";

                            // парсим ответ сервера в переменные, часть сохраняем в файл для отладки
                            string usr = item["object"]["message"]["from_id"].ToString();


                            string logFile = $@"logs\{DateTime.Now.ToString("yyyyMMdd")}_vkbot_msg.log ";
                            string logString = $@"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";

                            logString += $"[From ID: {usr}]";
                            Console.WriteLine($"From ID: {usr}");

                            string msg = item["object"]["message"]["text"].ToString();

                            logString += $"[Inbound msg: { msg}]";
                            Console.WriteLine($"Inbound msg: {msg}");

                            //парсим payload нажатой пользователем кнопки (если есть)
                            string payload = String.Empty;

                            if (!String.IsNullOrEmpty(item["object"]["message"]["payload"]?.ToString()))
                            {
                                payload = item["object"]["message"]["payload"].ToString();
                                logString += $"[Payload: {payload}]";
                                Console.WriteLine($"Payload: {payload}");
                            }
                            //если нет - пльзователь не нажал кнопку, а написал боту текст вручную
                            else
                            {
                                logString += $"[Payload: ]";
                                Console.WriteLine("No payload");
                            }

                            File.AppendAllText(logFile, logString);

                            //Console.WriteLine(GetMessage(msg, payload));

                            // формируем строку запроса к longPoll API VK
                            // в том числе добавляем кнопки вариантов 
                            // из сценария, вызывая метод GetMessage(msg, payload) 
                            string post = string.Format(urlBotMsg + "{0}{1}",
                                                                item["object"]["message"]["from_id"].ToString(),
                                                                $"{GetMessage(msg, payload)}"
                                                                );

                            //Console.WriteLine(post);

                            //отправляем наш ответ пользователю
                            webClient.DownloadString(post);

                            logString = $"\n+\n";
                            File.AppendAllText(logFile, logString);

                            logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}]";
                            logStringBot += $"[+]";
                            File.AppendAllText(logFileBot, logStringBot);


                            Console.WriteLine("+");

                            // ждём одну секунду, чтобы не спамить VK частыми запросами (могут забанить за DDOS) 
                            Thread.Sleep(1000);
                        }


                    }


                    // проверяем метку времени принудительного перезапуска
                    // каждые полчаса выходим из цикла работы с longPoll API
                    // для перезапуска сеанса работы с VK
                    if (DateTime.Now > watchDog.AddMinutes(30))
                    {
                        Console.WriteLine($"{DateTime.Now.ToString()} WatchDog timeout break ");
                        logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}][Status: WatchDog timeout break]";
                        File.AppendAllText(logFileBot, logStringBot);

                        break;
                    }

                }

                // регистрируем в логах штатный выход из внутреннего цикла, ждём 0.1 сек и перезапускаем сеанс
                Console.WriteLine($"{DateTime.Now.ToString()} inner cycle exited. try to restart ");
                logStringBot = $"\n[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}][Status: inner cycle exited. try to restart]";
                File.AppendAllText(logFileBot, logStringBot);

                Thread.Sleep(100);
            }
        }

    

    }
}


