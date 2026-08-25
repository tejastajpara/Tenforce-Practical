using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Text;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            var allPlanetsWithTheirMoons = new Collection<Planet>();

            Console.WriteLine("Started Loading GetAllPlanets...");

            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

            Console.WriteLine("GetAllPlanets Are Loaded...");

            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn(
                    $"{LoggerMessage.GetRequestFailed}{response.StatusCode}");

                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            if (results == null)
            {
                return allPlanetsWithTheirMoons;
            }

            foreach (var planet in results.Bodies)
            {
                Console.WriteLine($"Processing planet: {planet.Id}");

                if (planet.Moons != null)
                {
                    var newMoonsCollection = new Collection<MoonDto>();

                    foreach (var moon in planet.Moons)
                    {
                        Console.WriteLine($"Requesting moon: {moon.URLId}");

                        var moonResponse = _httpClientService.Client
                            .GetAsync(
                                UriPath.GetMoonByIdQueryParameters + moon.URLId)
                            .Result;

                        Console.WriteLine(
                            $"Moon response: {moonResponse.StatusCode}");

                        if (!moonResponse.IsSuccessStatusCode)
                        {
                            Logger.Instance.Warn(
                                $"{LoggerMessage.GetRequestFailed}{moonResponse.StatusCode}");

                            continue;
                        }

                        var moonContent =
                            moonResponse.Content.ReadAsStringAsync().Result;

                        var moonDto =
                            JsonConvert.DeserializeObject<MoonDto>(moonContent);

                        if (moonDto != null)
                        {
                            newMoonsCollection.Add(moonDto);
                        }
                    }

                    planet.Moons = newMoonsCollection;
                }

                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}
