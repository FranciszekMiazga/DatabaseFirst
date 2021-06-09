using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripsApi.Models;
using TripsApi.Models.DTO_s;

namespace TripsApi.Services
{
    public interface IDbService
    {
        IEnumerable GetTripList();
        int DeleteClient(int clientId);
        int PostClient(int tripId,GetTripResponse client);
    }
    public class DatabaseService: IDbService
    {
        private readonly MyContext _context;

        public DatabaseService(MyContext context)
        {
            _context = context;
        }


        public IEnumerable GetTripList()
        {
            var clientTripList = _context.Trips.Include(c => c.ClientTrips)
                .ThenInclude(c => c.IdClientNavigation)
                .Include(e => e.CountryTrips)
                .ThenInclude(e => e.IdCountryNavigation)
                .Select(c => new
                {
                    Name = c.Name,
                    Description = c.Description,
                    DateFrom = c.DateFrom,
                    DateTo = c.DateTo,
                    MaxPeople = c.MaxPeople,
                    Clients = c.ClientTrips.Select(c => new
                    {
                        c.IdClientNavigation.FirstName,
                        c.IdClientNavigation.LastName
                    }),
                    Countries = c.CountryTrips.Select(e => new
                    {
                        e.IdCountryNavigation.Name
                    })

                }).OrderByDescending(t=>t.DateFrom).ToList();

            return clientTripList;
        }

        public int DeleteClient(int clientId)
        {
            int answerValue = CheckIfIdIsCorrect(clientId);
            if (answerValue == 0)
                return answerValue;

            var client = _context.Clients.First(c => c.IdClient == clientId);
            _context.Clients.Remove(client);
            _context.SaveChanges();

            return 1;
        }

        public int PostClient(int tripId, GetTripResponse tripsRespose)
        {
            var result = _context.Clients.Where(e=>e.Pesel==tripsRespose.Pesel).Count();
            
            Client client = null;

            if (result == 0)
                client = AddClientToDb(tripsRespose);
            else
                client = _context.Clients.Where(e => e.Pesel == tripsRespose.Pesel).First();

            if (!IsClientRegisteredToTrip(tripsRespose))
                return 0;
            if (!IfTripExist(tripsRespose))
                return -1;

            
            AssignClientToTrip(tripsRespose);

            return 1;
        }
        private int CheckIfIdIsCorrect(int clientId)
        {
            var obj = _context.ClientTrips.Where(e=>e.IdClient==clientId).ToList();
            var obj2 = _context.Clients.Where(c=>c.IdClient==clientId).ToList();

            if (obj.Count !=0 || obj2.Count ==0)
                return 0;
            return 1;
        }
        private Client AddClientToDb(GetTripResponse tripsResponse)
        {
            var client = new Client
            {
                IdClient = _context.Clients.Max(c => c.IdClient) + 1,
                FirstName=tripsResponse.FirstName,
                LastName=tripsResponse.LastName,
                Email=tripsResponse.Email,
                Pesel=tripsResponse.Pesel,
                Telephone=tripsResponse.Telephone
            };
            _context.Clients.Add(client);
            _context.SaveChanges();
            return client;
        }
        private bool IsClientRegisteredToTrip(GetTripResponse tripsRespose)
        {
            int clientId = _context.Clients.Where(e => e.Pesel == tripsRespose.Pesel).Select(e=>e.IdClient).First();
            var result = _context.ClientTrips
                .Where(e => e.IdClient == clientId && e.IdTrip==tripsRespose.IdTrip)
                .Count();

            if (result != 0)
                return false;

            return true;
        }
        private bool IfTripExist(GetTripResponse tripsRespose)
        {
            var result = _context.Trips.Where(e => e.IdTrip == tripsRespose.IdTrip).Count();
            if (result == 0)
                return false;

            return true;
        }
        
        private void AssignClientToTrip(GetTripResponse tripsRespose)
        {
            int clientId = _context.Clients.Where(e => e.Pesel == tripsRespose.Pesel).Select(e => e.IdClient).First();

            var clientTrip = new ClientTrip
            {
                IdClient=clientId,
                IdTrip=tripsRespose.IdTrip,
                RegisteredAt=DateTime.Now,
                PaymentDate=tripsRespose.PaymentDate
            };
            _context.ClientTrips.Add(clientTrip);
            _context.SaveChanges();
          
        }
    }
}
