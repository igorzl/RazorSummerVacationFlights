using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorSummerVacationFlights.Models
{
    public class Place
    {
        public string PlaceId { get; set; }
        public string PlaceName { get; set; }
        public string CountryId { get; set; }
        public string RegionId { get; set; }
        public string CityId { get; set; }
        public string CountryName { get; set; }
    }

    public class PlacesList
    {
        public List<Place> Places { get; set; }
    }
}
