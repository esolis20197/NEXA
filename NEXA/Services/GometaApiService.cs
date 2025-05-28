using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NEXA.Models;

namespace NEXA.Services
{
    public class GometaApiService
    {
        private readonly HttpClient _httpClient;

        public GometaApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GometaResponse> BuscarCedula(string cedula)
        {
            var url = $"https://apis.gometa.org/cedulas/{cedula}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var resultado = JsonSerializer.Deserialize<GometaResponse>(json, opciones);
            return resultado;
        }
    }
}
