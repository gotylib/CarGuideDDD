using CarGuideDDD.Infrastructure.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;


namespace CarGuideDDD.Infrastructure.Services
{
    public class KeycloakAdminClientService : IKeycloakAdminClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _username;
        private readonly string _password;
        private readonly string _realm;
        private string _accessToken;

        public KeycloakAdminClientService(string tokenEndpoint, string clientId, string clientSecret, string username, string password, string realm)
        {
            _httpClient = new HttpClient();
            _tokenEndpoint = tokenEndpoint;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
            _realm = realm;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_accessToken != null)
            {
                return _accessToken;
            }

            var client = new HttpClient();

            // Устанавливаем URL для запроса
            var url = "http://keycloak:8080/realms/dev/protocol/openid-connect/token";

            // Создаем содержимое запроса
            var requestBody = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", "bak"),
            new KeyValuePair<string, string>("client_secret", "SUzNRpiGKxEbpZgdPwNOc272dAk1iHKB"),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

            // Устанавливаем заголовок Content-Type
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            try
            {
                // Отправляем POST-запрос
                var response = await client.PostAsync(url, requestBody);

                // Проверяем успешность ответа
                response.EnsureSuccessStatusCode();

                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                _accessToken = tokenResponse.access_token;

                return _accessToken;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Error: " + e.Message);
                return String.Empty;    
            }

           
        }

        public async Task<string> GetUserIdByUsernameAsync(string username)
        {

            // Получаем access token
            var accessToken = await GetAccessTokenAsync();

            // Устанавливаем URL для запроса информации о пользователе
            var url = $"http://keycloak:8080/admin/realms/{_realm}/protocol/openid-connect/users?username={username}";

            // Настраиваем запрос
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                // Отправляем GET-запрос
                var response = await _httpClient.SendAsync(request);

                // Проверяем успешность ответа
                response.EnsureSuccessStatusCode();

                var users = JsonConvert.DeserializeObject<List<dynamic>>(await response.Content.ReadAsStringAsync());

                // Находим пользователя с данным именем
                var user = users.FirstOrDefault();

                // Если пользователь найден, возвращаем его идентификатор
                return user != null ? user.id.ToString() : string.Empty;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Error: " + e.Message);
                return string.Empty;
            }
        }

        public async Task AddRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Failed to obtain access token");
                }

                var rolesEndpoint = $"http://keycloak:8080/admin/realms/{_realm}/users/{userId}/role-mappings/clients/bak";

                var roleRequest = new HttpRequestMessage(HttpMethod.Post, rolesEndpoint);
                roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                roleRequest.Content = new StringContent(
                    JsonConvert.SerializeObject(new[] { new { name = roleName } }),
                    Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(roleRequest);
                response.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
