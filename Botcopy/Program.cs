using System;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK;
using ApiAiSDK.Model;


namespace Bot
{
    class Program
    {
        static TelegramBotClient Bot;
        static ApiAi apiAi;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("962747114:AAGbOL1r_o0HgOiZZ1ELqpkuZMX5FBtbZtg");

            AIConfiguration config = new AIConfiguration("863ba3f639334b6aa5d703544554a840", SupportedLanguage.Russian);
            apiAi = new ApiAi(config);

            Bot.OnMessage += BotOnMessageRecieved;

            Bot.OnCallbackQuery += BotOnCallbackQueryRecieved;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            Bot.StartReceiving();

            Console.ReadLine();

            Bot.StartReceiving();
        }

        private static async void BotOnCallbackQueryRecieved(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} нажал кнопку {buttonText}");

            if (buttonText == "Изображение") 
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://pixabay.com/ru/photos/");
            }
            else if (buttonText == "Видео")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.youtube.com/");
            }

            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Вы нажали кнопку {buttonText}");
        }

        private static async void BotOnMessageRecieved(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text)
                return;

            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"{name} send messege: {message.Text}");

            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start - запуск бота
/inline - вывод меню
/keyboard - вывод клавивтуры";

                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;
                case "/inline":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    new []
                    {
                        InlineKeyboardButton.WithUrl("VK", "https://vk.com/id104796612"),
                        InlineKeyboardButton.WithUrl("Telegramm", "https://t.me/ВикторДацков")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Изображение"),
                        InlineKeyboardButton.WithCallbackData("Видео")
                    }

                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню: ", replyMarkup: inlineKeyboard);
                    break;
                case "/keyboard":
                    var replykeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new []
                        {
                            new KeyboardButton("Привет!"),
                            new KeyboardButton("Как дела?")
                        },
                        new[]
                        {
                            new KeyboardButton("Контакт") {RequestContact = true},
                            new KeyboardButton("Геолокация") { RequestLocation = true}
                        }
                    }
                    );
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Сообщение", replyMarkup: replykeyboard);
                    break;

                default:
                    var response = apiAi.TextRequest(message.Text);
                    string answer = response.Result.Fulfillment.Speech;
                    if (answer == "")
                        answer = "Ивините, я Вас не понял.";
                    await Bot.SendTextMessageAsync(message.From.Id, answer);
                    break;


            }
        }
    }
}
