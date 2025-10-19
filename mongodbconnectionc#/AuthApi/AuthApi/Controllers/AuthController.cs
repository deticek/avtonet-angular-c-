using System.IO;
using System.Text;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly UserService _userService;

        public RegisterController(UserService userService)
        {
            _userService = userService;
        }

        // Registracija uporabnika
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (userDto.Password != userDto.ConfirmPassword)
                return BadRequest("Passwords do not match!");

            var hashedPassword = HashPassword(userDto.Password);
            // Pretvorimo podatke uporabnika v BSON dokument
            var userDocument = new BsonDocument
            {
                { "username", userDto.Username },
                { "email", userDto.Email },
                { "firstName", userDto.FirstName },
                { "lastName", userDto.LastName },
                { "regija", userDto.Regija },
                { "country", userDto.Country },
                { "postalCode", userDto.PostalCode },
                { "street", userDto.Street },
                { "houseNumber", userDto.HouseNumber },
                { "dateOfBirth", userDto.DateOfBirth },
                { "sellerTitle", userDto.SellerTitle ?? string.Empty },
                { "phoneNumber", userDto.PhoneNumber },
                { "password", hashedPassword } // Poskrbite za hashiranje gesel!
            };

            await _userService.CreateUserAsync(userDocument);
            return Ok(new { Message = "User registered successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {

            Console.WriteLine(loginDto.Email + "Pass" + loginDto.Password);
            var filter = Builders<BsonDocument>.Filter.Eq("email", loginDto.Email);
            var userDocument = await _userService.GetUserAsync(filter);

            if (userDocument == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordHash = userDocument["password"].AsString;
            if (!VerifyPassword(loginDto.Password, passwordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = GenerateToken(userDocument["email"].AsString);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    Id = userDocument["_id"].ToString(),
                    Username = userDocument["username"].AsString,
                    Email = userDocument["email"].AsString,
                    FirstName = userDocument["firstName"].AsString,
                    LastName = userDocument["lastName"].AsString,
                    Regija = userDocument["regija"].AsString,
                    Country = userDocument["country"].AsString,
                    PostalCode = userDocument["postalCode"].AsString,
                    Street = userDocument["street"].AsString,
                    HouseNumber = userDocument["houseNumber"].AsString,
                    DateOfBird = userDocument["dateOfBirth"].AsString,
                    PhoneNumber = userDocument["phoneNumber"].AsString
                }
            });
        }

        public class UpdateUserRequest
        {
            public UserDto OldUser { get; set; } // Old user data for finding the user
            public UserDto UpdatedUser { get; set; } // New user data for updating the user
        }

        // Posodobitev uporabniških podatkov
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            if (request == null || request.OldUser == null || request.UpdatedUser == null)
            {
                return BadRequest("Invalid request payload.");
            }

            // Log the old user data for debugging
            Console.WriteLine("Old User Data:");
            Console.WriteLine($"Email: {request.OldUser.Email}");
            Console.WriteLine($"FirstName: {request.OldUser.FirstName}");
            Console.WriteLine($"LastName: {request.OldUser.LastName}");

            Console.WriteLine("Updated User Data:");
            Console.WriteLine($"Email: {request.UpdatedUser.Email}");
            Console.WriteLine($"FirstName: {request.UpdatedUser.FirstName}");
            Console.WriteLine($"LastName: {request.UpdatedUser.LastName}");

            // Build the filter using the old user data
            var filter = Builders<BsonDocument>.Filter.Eq("email", request.OldUser.Email)
                          & Builders<BsonDocument>.Filter.Eq("firstName", request.OldUser.FirstName)
                          & Builders<BsonDocument>.Filter.Eq("lastName", request.OldUser.LastName);

            // Find the user in the database using the old user data
            var existingUser = await _userService.GetUserAsync(filter);

            if (existingUser == null)
            {
                return NotFound("Uporabnik ni bil najden! Filter: " + filter.ToString());
            }

            // Log the new user data for debugging
            Console.WriteLine("New User Data:");
            Console.WriteLine($"Email: {request.UpdatedUser.Email}");
            Console.WriteLine($"FirstName: {request.UpdatedUser.FirstName}");
            Console.WriteLine($"LastName: {request.UpdatedUser.LastName}");

            // Update the user data using the new user data
            var update = Builders<BsonDocument>.Update
                .Set("email", request.UpdatedUser.Email)
                .Set("firstName", request.UpdatedUser.FirstName)
                .Set("lastName", request.UpdatedUser.LastName)
                .Set("regija", request.UpdatedUser.Regija)
                .Set("country", request.UpdatedUser.Country)
                .Set("postalCode", request.UpdatedUser.PostalCode)
                .Set("street", request.UpdatedUser.Street)
                .Set("houseNumber", request.UpdatedUser.HouseNumber)
                .Set("dateOfBirth", request.UpdatedUser.DateOfBirth)
                .Set("phoneNumber", request.UpdatedUser.PhoneNumber);

            // Update the user in the database
            await _userService.UpdateUserAsync(filter, update);

            return Ok(new { Message = "Podatki so bili uspešno posodobljeni!" });
        }

        // Pomembne pomožne metode za preverjanje gesla in generiranje žetona
        private static string GenerateToken(string email)
        {
            // Simplified token generation (replace with JWT in production)
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            var hashedInput = HashPassword(inputPassword);
            return hashedInput == storedPasswordHash;
        }

        // DTO za prijavo uporabnika
        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // DTO za registracijo in posodobitev uporabnika
        public class UserDto
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Regija { get; set; }
            public string? Country { get; set; }
            public string? PostalCode { get; set; }
            public string? Street { get; set; }
            public string? HouseNumber { get; set; }
            public string? DateOfBirth { get; set; }
            public string? SellerTitle { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Password { get; set; }
            public string? ConfirmPassword { get; set; }
        }
    }
}
