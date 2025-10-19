using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/Oglasi")]
    public class OglasiController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _oglasiCollection;

        public OglasiController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("avtonet");
            _oglasiCollection = database.GetCollection<BsonDocument>("oglasi");
        }

        // 🔎 GET: Pridobi vse oglase s filtri
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OglasDto filter)
        {
            Console.WriteLine(filter);
            Console.WriteLine($"Filter: MinPrice={filter.MinPrice}, MaxPrice={filter.MaxPrice}");

            // Korak 1: Pridobimo vse oglase iz MongoDB brez filtra
            var allOglasi = await _oglasiCollection.Find(new BsonDocument()).ToListAsync();

            // Korak 2: Filtriramo oglase glede na parametre filtra
            var filteredOglasi = allOglasi.AsQueryable();

            // Dodajamo filtre glede na vrednosti, ki jih uporabnik poda v query string (filter parametri)
            if (!string.IsNullOrEmpty(filter.Brand))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("brand").ToString() == filter.Brand);

            if (!string.IsNullOrEmpty(filter.Model))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("model").ToString() == filter.Model);

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
            {
                // Filtriramo ceno med MinPrice in MaxPrice
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("price").ToInt32() >= filter.MinPrice.Value &&
                                                               oglas.GetValue("price").ToInt32() <= filter.MaxPrice.Value);
            }
            else if (filter.MinPrice.HasValue)
            {
                // Filtriramo le z minimalno ceno
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("price").ToInt32() >= filter.MinPrice.Value);
            }
            else if (filter.MaxPrice.HasValue)
            {
                // Filtriramo le z maksimalno ceno
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("price").ToInt32() <= filter.MaxPrice.Value);
            }

            if (filter.MinYear.HasValue)
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("year").ToInt32() >= filter.MinYear.Value);

            if (filter.MaxYear.HasValue)
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("year").ToInt32() <= filter.MaxYear.Value);

            if (!string.IsNullOrEmpty(filter.Color))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("color").ToString() == filter.Color);

            if (!string.IsNullOrEmpty(filter.BodyStyle))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("bodyStyle").ToString() == filter.BodyStyle);

            if (!string.IsNullOrEmpty(filter.Fuel))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("fuel").ToString() == filter.Fuel);

            if (!string.IsNullOrEmpty(filter.Transmission))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("transmission").ToString() == filter.Transmission);

            if (!string.IsNullOrEmpty(filter.Regija))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("regija").ToString() == filter.Regija);

            if (!string.IsNullOrEmpty(filter.StanjeVozila))
                filteredOglasi = filteredOglasi.Where(oglas => oglas.GetValue("stanjeVozila").ToString() == filter.StanjeVozila);

            // Korak 3: Sortiramo rezultate, če je potrebno
            // Tukaj lahko dodate polja, po katerih želite, da se oglasi sortirajo (npr. po ceni ali letu)
            filteredOglasi = filteredOglasi.OrderBy(oglas => oglas.GetValue("price").ToInt32()); // Sortiranje po ceni

            // Korak 4: Pretvorimo rezultate v DTO objekt za pošiljanje nazaj
            var oglasiDto = filteredOglasi.Select(oglas => new OglasDto
            {
                Brand = oglas.GetValue("brand", "").ToString(),
                Model = oglas.GetValue("model", "").ToString(),
                Price = oglas.GetValue("price", 0).ToInt32(),
                Year = oglas.GetValue("year", 0).ToInt32(),
                Color = oglas.GetValue("color", "").ToString(),
                BodyStyle = oglas.GetValue("bodyStyle", "").ToString(),
                Fuel = oglas.GetValue("fuel", "").ToString(),
                Transmission = oglas.GetValue("transmission", "").ToString(),
                Doors = oglas.GetValue("doors", "").ToString(),
                Seats = oglas.GetValue("seats", "").ToString(),
                StanjeVozila = oglas.GetValue("stanjeVozila", "").ToString(),
                TipPonudbe = oglas.GetValue("tipPonudbe", "").ToString(),
                Lastnik = oglas.GetValue("lastnik", "").ToString(),
                Kraj = oglas.GetValue("kraj", "").ToString(),
                Regija = oglas.GetValue("regija", "").ToString(),
                Telefon = oglas.GetValue("telefon", "").ToString()
            }).ToList();

            // Korak 5: Pošljemo rezultate kot odgovor
            return Ok(oglasiDto);
        }

        // ✨ POST: Dodaj oglas
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OglasDto oglasDto)
        {
            var oglasDocument = new BsonDocument
            {
                { "brand", oglasDto.Brand ?? "" },
                { "model", oglasDto.Model ?? "" },
                { "price", oglasDto.Price ?? 0 },
                { "year", oglasDto.Year ?? 0 },
                { "color", oglasDto.Color ?? "" },
                { "bodyStyle", oglasDto.BodyStyle ?? "" },
                { "fuel", oglasDto.Fuel ?? "" },
                { "transmission", oglasDto.Transmission ?? "" },
                { "doors", oglasDto.Doors ?? "" },
                { "seats", oglasDto.Seats ?? "" },
                { "stanjeVozila", oglasDto.StanjeVozila ?? "" },
                { "tipPonudbe", oglasDto.TipPonudbe ?? "" },
                { "lastnik", oglasDto.Lastnik ?? "" },
                { "kraj", oglasDto.Kraj ?? "" },
                { "regija", oglasDto.Regija ?? "" },
                { "telefon", oglasDto.Telefon ?? "" }
            };

            await _oglasiCollection.InsertOneAsync(oglasDocument);
            return Ok(new { Message = "Oglas uspešno objavljen!" });
        }

        // ❌ DELETE: Briši oglas
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] OglasDto oglasDto)
        {
            var filterBuilder = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("brand", oglasDto.Brand),
                Builders<BsonDocument>.Filter.Eq("model", oglasDto.Model),
                Builders<BsonDocument>.Filter.Eq("price", oglasDto.Price),
                Builders<BsonDocument>.Filter.Eq("year", oglasDto.Year),
                Builders<BsonDocument>.Filter.Eq("lastnik", oglasDto.Lastnik)
            );

            var result = await _oglasiCollection.DeleteOneAsync(filterBuilder);

            if (result.DeletedCount == 0)
            {
                return NotFound("Oglas ni bil najden.");
            }

            return Ok(new { Message = "Oglas uspešno izbrisan!" });
        }

        // 📌 DTO Model za validacijo
        public class OglasDto
        {
            public string? Brand { get; set; }
            public string? Model { get; set; }
            public int? Price { get; set; }
            public int? Year { get; set; }
            public int? MinPrice { get; set; }
            public int? MaxPrice { get; set; }
            public int? MinYear { get; set; }
            public int? MaxYear { get; set; }
            public string? Color { get; set; }
            public string? BodyStyle { get; set; }
            public string? Fuel { get; set; }
            public string? Transmission { get; set; }
            public string? Doors { get; set; }
            public string? Seats { get; set; }
            public string? StanjeVozila { get; set; }
            public string? TipPonudbe { get; set; }
            public string? Lastnik { get; set; }
            public string? Kraj { get; set; }
            public string? Regija { get; set; }
            public string? Telefon { get; set; }
        }
    }
}
