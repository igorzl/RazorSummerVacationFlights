using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RazorSummerVacationFlights.Models
{
    public class FlightToVacationPlace
    {
        [Display(Name = "Country")]
        public string DestinationCountry { get; set; }

        [Display(Name = "Departure date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DepartureDateOutbound { get; set; }

        [Display(Name = "Arrival date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DepartureDateInbound { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,0}")]
        [Display(Name = "COVID cases")]
        public int ActiveCovidCases { get; set; }
    }
}
