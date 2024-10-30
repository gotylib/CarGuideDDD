using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CarGuideDDD.TelegramBot.ProcessingMethods
{
    public static class Methods
    {
        private const string ClientId = "52506965";
        private const string ClientSecret = "Qsbj6ZrB7cRpU8UHy0SS";
        private const string RedirectUri = "https://t.me/BycarQPDbot";

        private static async Task<List<string>> GetAccessTokenAsync(string code)
        {
            using var httpClient = new HttpClient();
            var respons = new List<string>();
            try
            {
                var response = await httpClient.GetStringAsync($"https://oauth.vk.com/access_token?client_id={ClientId}&client_secret={ClientSecret}&redirect_uri={RedirectUri}&scope=email&code={code}");
                dynamic jsonResponse = JsonConvert.DeserializeObject(response);

                respons.Add((string)jsonResponse.access_token);
                respons.Add((string)jsonResponse.email);
                return respons;
            }
            catch (Exception ex)
            {
                respons.Add("Error");
                Console.WriteLine(ex.Message);
                return respons;
            }
        }

        private static async Task<dynamic?> GetUserInfoAsync(string accessToken)
        {
            if (accessToken == "Error")
            {
                return null;
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"https://api.vk.com/method/users.get?access_token={accessToken}&fields=email&v=5.131");
            dynamic userInfo = JsonConvert.DeserializeObject(response);

            // Проверка на наличие ошибок в ответе 
            if (userInfo?.error == null) return userInfo.response[0];
            Console.WriteLine($"Error: {userInfo.error.message} (code: {userInfo.error.error_code})");
            return null;

        }

        public static async Task<List<string>> HandleRedirect(string code)
        {
            var accessToken = await GetAccessTokenAsync(code);
            var userInfo = await GetUserInfoAsync(accessToken[0]);
            var result = new List<string>();

            if (userInfo != null)
            {
                // Теперь можно получить email, но он может быть null
                string email = userInfo.email ?? "Email не предоставлен"; // обрабатываем случай, если email отсутствует
                Console.WriteLine($"User ID: {userInfo.id}, Name: {userInfo.first_name}, Email: {accessToken[1]}");
                result.Add((string)userInfo.first_name + (string)userInfo.last_nameme);
                result.Add(accessToken[1]);
                return result;
            }

            result.Add("Error");
            Console.WriteLine("Не удалось получить информацию о пользователе.");
            return result;
            
        }


        public static string Translit(string input)
        {
            var translitDict = new Dictionary<char, string?>
            {
            {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
            {'Е', "E"}, {'Ё', "Yo"}, {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"},
            {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
            {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
            {'У', "U"}, {'Ф', "F"}, {'Х', "Kh"}, {'Ц', "Ts"}, {'Ч', "Ch"},
            {'Ш', "Sh"}, {'Щ', "Sch"}, {'Ъ', ""}, {'Ы', "Y"}, {'Ь', ""},
            {'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"},
            // Добавьте строчные буквы
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
            {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
            {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
            {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
            {'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"},
            {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

            var result = new StringBuilder();

            foreach (var c in input)
            {
                if (translitDict.TryGetValue(c, out var transliteratedChar))
                {
                    result.Append(transliteratedChar);
                }
                else
                {
                    result.Append(c); // Если символ не найден, добавляем его как есть
                }
            }

            return result.ToString();
        }

        public static async Task<string> BuyOrInformate(string token, int carId, bool state)
        {
            var url = state ? "https://localhost:7162/api/Cars/BuyCar" : "https://localhost:7162/api/Cars/InformateCar";

            var data = new
            {
                id = carId,
                status = state
            };

            using var httpClient = new HttpClient();
            // Установка заголовков
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Сериализация данных в JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Выполнение POST-запроса
            var response = await httpClient.PostAsync(url, content);

            // Проверка ответа
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + responseData);
                return responseData;
            }

            Console.WriteLine("Error: " + response.StatusCode);
            return "Error";
        }
    }
}
