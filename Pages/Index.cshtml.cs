using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HSNXT.Unirest;
using HSNXT.Unirest.Net;
using HSNXT.Unirest.Net.Entities;
using HSNXT.Unirest.Net.Entities.Exceptions;
using RazorSummerVacationFlights.Helpers;
using System.Xml;
using RazorSummerVacationFlights.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace RazorSummerVacationFlights.Pages
{
    public class IndexModel : PageModel
    {
        private string skyScannerApi = "skyscanner-skyscanner-flight-search-v1.p.rapidapi.com";
        const string covidApi = "covid-193.p.rapidapi.com";
        private string x_rapid_api_key = "here-put-your-personal-RapidAPI-key";

        private string getPlacesUrl = "https://skyscanner-skyscanner-flight-search-v1.p.rapidapi.com/apiservices/autosuggest/v1.0/IL/USD/en-US/?query={0}";
        const string getCovidStatisticsUrl = "https://covid-193.p.rapidapi.com/statistics?country={0}";
        const string getFlightQuotesUrl = "https://skyscanner-skyscanner-flight-search-v1.p.rapidapi.com/apiservices/browsequotes/v1.0/IL/USD/en-US/TLV-sky/{0}/anytime/anytime";

        private List<string> countriesToVisit = new List<string>()
            {
                "France",
                "USA",
                "UK",
                "Australia"
            };

        DateTime startSeasonDate = new DateTime(2021, 6, 1);
        DateTime endSeasonDate = new DateTime(2021, 8, 31);

        private async Task<string> ApiGetQueryResult(string apiUrl, string apiRequestUrl, string country)
        {
            HttpResponse<string> response = await Unirest.Get(string.Format(apiRequestUrl, country))
                .Header("X-RapidAPI-Host", apiUrl)
                .Header("X-RapidAPI-Key", x_rapid_api_key)
                .AsJsonAsync<string>();

            return response.Body;
        }

        private async Task Run(List<FlightToVacationPlace> flightResults)
        {
            List<Task<string>> getCountryTasks = new List<Task<string>>(countriesToVisit.Count);
            List<Task<string>> getCovidTasks = new List<Task<string>>(countriesToVisit.Count);
            List<Task<string>> getFlightsTasks = new List<Task<string>>(countriesToVisit.Count);

            List<string> getCountryResponses = new List<string>(countriesToVisit.Count);
            List<string> getCovidResponses = new List<string>(countriesToVisit.Count);
            List<string> getFlightsResponses = new List<string>(countriesToVisit.Count);

            List<PlacesList> getPlaces = new List<PlacesList>(countriesToVisit.Count);

            for (int i = 0; i < countriesToVisit.Count; i++)
            {
                getCountryTasks.Add(ApiGetQueryResult(skyScannerApi, getPlacesUrl, countriesToVisit[i]));
                getCovidTasks.Add(ApiGetQueryResult(covidApi, getCovidStatisticsUrl, countriesToVisit[i]));
            }

            ApiErrorMessage errorMessage = null;

            for (int i = 0; i < countriesToVisit.Count; i++)
            {
                getCountryResponses.Add(await getCountryTasks[i]);
                errorMessage = JsonHelper.DeserializeJSon<ApiErrorMessage>(getCountryResponses[i]);
                if(errorMessage != null && !string.IsNullOrEmpty(errorMessage.message))
                {
                    throw new Exception(errorMessage.message);
                }
                getCovidResponses.Add(await getCovidTasks[i]);
                getPlaces.Add(JsonHelper.DeserializeJSon<PlacesList>(getCountryResponses[i]));
                getFlightsTasks.Add(ApiGetQueryResult(skyScannerApi, getFlightQuotesUrl, getPlaces[i].Places[0].PlaceId));
            }

            for (int i = 0; i < countriesToVisit.Count; i++)
            {
                dynamic covidCases = JsonConvert.DeserializeObject(getCovidResponses[i]);

                getFlightsResponses.Add(await getFlightsTasks[i]);

                dynamic flights = JsonConvert.DeserializeObject(getFlightsResponses[i]);
                try
                {
                    dynamic dataItem = ((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)flights).First).First);
                    var count = dataItem.Count;
                    dataItem = ((Newtonsoft.Json.Linq.JToken)dataItem).First;
                    DateTime testDate = new DateTime();

                    while (count > 0)
                    {
                        testDate = (DateTime)dataItem.OutboundLeg.DepartureDate;

                        if (testDate >= startSeasonDate && testDate <= endSeasonDate)
                        {
                            flightResults.Add(new FlightToVacationPlace
                            {
                                DestinationCountry = countriesToVisit[i],
                                Price = (decimal)dataItem.MinPrice,
                                DepartureDateOutbound = (DateTime)dataItem.OutboundLeg.DepartureDate,
                                DepartureDateInbound = (DateTime)dataItem.InboundLeg.DepartureDate,
                                ActiveCovidCases = (int)covidCases.response[0].cases.active

                            });
                        }
                        dataItem = ((Newtonsoft.Json.Linq.JToken)dataItem).Next;
                        count--;
                    }

                }
                catch (Exception ex)
                {

                    throw new Exception($"Error in flight quotes data structure: {ex}");
                }
            }
        }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            List<FlightToVacationPlace> flightResults = new List<FlightToVacationPlace>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await Run(flightResults);
            }
            catch (Exception ex)
            {
                throw new Exception($"Please check your Web API connection settings, especially your personal RapidAPI key value!: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
            }

            TimeSpan ts = stopwatch.Elapsed;

            ViewData["timespan"] = string.Format("Elapsed Time is {0:00}:{1:00}:{2:00}.{3}",
                                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

            flightResults.Sort((d1, d2) =>
            {
                int res = d1.ActiveCovidCases.CompareTo(d2.ActiveCovidCases);
                if (res == 0)
                    res = d1.DestinationCountry.CompareTo(d2.DestinationCountry);
                if (res == 0)
                    res = d1.Price.CompareTo(d2.Price);
                return res;
            });

            var flightsData = flightResults;
            ViewData["flights"] = flightResults;

            ViewData["startSeasonDate"] = startSeasonDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            ViewData["endSeasonDate"] = endSeasonDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            return Page();
        }
    }
}
