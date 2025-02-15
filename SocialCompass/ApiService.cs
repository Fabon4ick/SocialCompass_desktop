using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace SocialCompass
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        // Метод для авторизации пользователя
        public async Task<UserResponse> AuthenticateUserAsync(string phoneNumber, string password)
        {
            // URL для авторизации (измените на ваш реальный URL)
            var url = $"http://localhost:10000/user/{phoneNumber}/{password}";

            // Отправка GET-запроса на сервер для авторизации
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Получение данных о пользователе
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserResponse>(jsonResponse);
                return user;
            }
            else
            {
                throw new Exception("Неверный номер телефона или пароль.");
            }
        }

        public async Task<List<ApplicationResponse>> GetApplicationsAsync()
        {
            var url = "http://localhost:10000/applications"; // URL API
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var applications = JsonConvert.DeserializeObject<List<ApplicationResponse>>(jsonResponse);

                // Если поля уже являются списком заболеваний, то никакого разделения не требуется
                return applications;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при загрузке заявок: {response.StatusCode} - {error}");
            }
        }

        public async Task<List<ApplicationResponse>> GetActiveApplicationsAsync()
        {
            var url = "http://localhost:10000/active-applications"; // URL API
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var activeApplications = JsonConvert.DeserializeObject<List<ApplicationResponse>>(jsonResponse);
                return activeApplications;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при загрузке активных заявок: {response.StatusCode} - {error}");
            }
        }

        public async Task DeleteApplicationAsync(int applicationId)
        {
            var url = $"http://localhost:10000/applications/{applicationId}"; // URL API для удаления заявки
            HttpResponseMessage response = await _httpClient.DeleteAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Если заявка удалена успешно, возвращаем успешный результат
                Console.WriteLine($"Заявка с ID {applicationId} успешно удалена.");
            }
            else
            {
                // Если возникла ошибка, выбрасываем исключение с сообщением об ошибке
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при удалении заявки: {response.StatusCode} - {error}");
            }
        }

        public async Task UpdateApplicationAsync(int applicationId, string newStartDate, string newEndDate)
        {
            var url = $"http://localhost:10000/applications/{applicationId}";

            var jsonData = new
            {
                dateStart = newStartDate,
                dateEnd = newEndDate
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(jsonData),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _httpClient.PutAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Заявка успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при обновлении заявки: {response.StatusCode} - {error}");
            }
        }

        public async Task<ObservableCollection<StaffResponse>> GetStaffsAsync()
        {
            var url = "http://localhost:10000/staffs"; // URL API для получения списка сотрудников
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var staffs = JsonConvert.DeserializeObject<List<StaffResponse>>(jsonResponse);

                return new ObservableCollection<StaffResponse>(staffs);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при загрузке списка сотрудников: {response.StatusCode} - {error}");
            }
        }

        public async Task DeleteStaffAsync(int staffId)
        {
            var url = $"http://localhost:10000/staffs/{staffId}"; // URL API для удаления сотрудника
            HttpResponseMessage response = await _httpClient.DeleteAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Сотрудник с ID {staffId} успешно удален.");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при удалении сотрудника: {response.StatusCode} - {error}");
            }
        }

        public async Task UpdateStaffAsync(int staffId, StaffUpdate staffData)
        {
            var url = $"http://localhost:10000/staffs/{staffId}";

            // Указываем явно `System.Text.Json.JsonSerializer`
            var json = System.Text.Json.JsonSerializer.Serialize(staffData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Сотрудник с ID {staffId} успешно обновлен.");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при обновлении сотрудника: {response.StatusCode} - {error}");
            }
        }

        public async Task<bool> AddStaffAsync(StaffRequest staffData)
        {
            var url = "http://localhost:10000/staffs/";

            // Сериализация в JSON с camelCase
            var json = System.Text.Json.JsonSerializer.Serialize(staffData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Сотрудник успешно добавлен.");
                return true; // Возвращаем true, если добавление прошло успешно
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при добавлении сотрудника: {response.StatusCode} - {error}");
            }
            return false; // Возвращаем false, если добавление не удалось
        }

        public async Task<List<T>> GetItemsAsync<T>(string tableName)
        {
            var url = $"http://localhost:10000/get_items/{tableName}"; // Ваш API маршрут
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

                // Если данные есть в таблице
                if (responseData.ContainsKey("data"))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(responseData["data"].ToString());
                    return data;
                }
                else
                {
                    // Если данных нет
                    MessageBox.Show(responseData["message"].ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<T>();
                }
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при загрузке данных: {response.StatusCode} - {error}");
            }
        }

        public async Task DeleteItemAsync(string table, int id)
        {
            var url = $"http://localhost:10000/{table}/{id}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> AddItemAsync(string tableName, string name)
        {
            var url = $"http://localhost:10000/add_item/{tableName}"; // URL для добавления элемента в таблицу

            var requestData = new
            {
                name = name
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"); // Используем JSON для тела запроса

            HttpResponseMessage response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

            // Возвращаем true, если запрос успешен
            return response.IsSuccessStatusCode;
        }


    }
}
