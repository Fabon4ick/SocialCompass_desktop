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
using SocialCompass.Models;

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

        public async Task<List<FeedbackResponse>> GetFeedbacksAsync()
        {
            var url = "http://localhost:10000/feedbacks"; // URL API FastAPI
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var feedbacks = JsonConvert.DeserializeObject<List<FeedbackResponse>>(jsonResponse);
                return feedbacks;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при загрузке отзывов: {response.StatusCode} - {error}");
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

        public async Task DeleteFeedbackAsync(int feedbackId)
        {
            var url = $"http://localhost:10000/feedbacks/{feedbackId}"; // URL API для удаления комментария
            HttpResponseMessage response = await _httpClient.DeleteAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Если комментарий удалён успешно, выводим сообщение
                Console.WriteLine($"Комментарий с ID {feedbackId} успешно удалён.");
            }
            else
            {
                // Если возникла ошибка, читаем и выбрасываем её
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при удалении комментария: {response.StatusCode} - {error}");
            }
        }

        public async Task UpdateFeedbackVisibilityAsync(int feedbackId, bool isVisible)
        {
            var url = $"http://localhost:10000/feedbacks/{feedbackId}";

            var jsonData = new
            {
                isVisible = isVisible
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(jsonData),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _httpClient.PutAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Комментарий с ID {feedbackId} успешно обновлён. Видимость: {isVisible}");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при обновлении комментария: {response.StatusCode} - {error}");
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

        public async Task UpdateApplicationAsync(int applicationId, string newStartDate, string newEndDate, int? newStaffId = null)
        {
            var url = $"http://localhost:10000/applications/{applicationId}";

            var jsonData = new
            {
                dateStart = newStartDate,
                dateEnd = newEndDate,
                staffId = newStaffId  // Передаём ID работника, если есть
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

        public async Task<bool> ReplaceAndDeleteStaffAsync(int oldStaffId, int newStaffId)
        {
            var url = "http://localhost:10000/replace_and_delete_staff/"; // URL API

            var requestData = new
            {
                old_id = oldStaffId,
                new_id = newStaffId
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(url, jsonContent).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Сотрудник с ID {oldStaffId} заменён на {newStaffId} и удалён. {result}");
                    return true; // Возвращаем true, если операция успешна
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new Exception($"Ошибка при замене и удалении сотрудника: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                return false; // Возвращаем false при ошибке
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

            var json = System.Text.Json.JsonSerializer.Serialize(staffData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Сотрудник успешно добавлен.");
                return true;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Ошибка при добавлении сотрудника: {response.StatusCode} - {error}");
            }
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

        public async Task<bool> ReplaceItemAsync(string tableName, int oldId, int newId)
        {
            var url = $"http://localhost:10000/replace_item/{tableName}";

            var requestData = new
            {
                old_id = oldId,
                new_id = newId
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                MessageBox.Show($"Ошибка при замене:\n{error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<ApplicationResponse>> SearchApplicationsAsync(string searchQuery)
        {
            var url = $"http://localhost:10000/applications/search?search={Uri.EscapeDataString(searchQuery)}";

            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<List<ApplicationResponse>>(jsonResponse);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                MessageBox.Show($"Ошибка при поиске заявок:\n{error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<ApplicationResponse>();
            }
        }



    }
}
