using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripsApi.Models;
using TripsApi.Models.DTO_s;
using TripsApi.Services;

namespace TripsApi.Controllers
{
    [Route("api/trips")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MyContext _context;
        private IDbService _dbservice;

        public ClientsController(MyContext context, IDbService dbservice)
        {
            _context = context;
            _dbservice = dbservice;
        }
        [HttpGet]
        public IActionResult GetTrips()
        {
           
            return Ok(_dbservice.GetTripList());
        }
        [HttpDelete("{idClient}")]
        public IActionResult DeleteTrips(int idClient)
        {
            int returnVal = _dbservice.DeleteClient(idClient);
            if (returnVal == 0)
                return NotFound("There is no record to delte or client is connected to trip.");
            return Ok("Client is deleted");
        }
        [HttpPost("{idTrip}/clients")]
        public IActionResult PostTrips(int idTrip,GetTripResponse client)
        {
            int returnedValue = _dbservice.PostClient(idTrip, client);
            if (returnedValue == 0)
                return NotFound("Klient jest już zapisany na wycieczke");
            else if (returnedValue == -1)
                return NotFound("Wycieczka nie istnieje");

            return Ok("Klient zostal pomyslnie przypisany do wycieczki.");
        }

    }

}
