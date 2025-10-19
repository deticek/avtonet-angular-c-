using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApi.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("ime")]
        public string Ime { get; set; }

        [BsonElement("priimek")]
        public string Priimek { get; set; }

        [BsonElement("drzava")]
        public string Drzava { get; set; }

        [BsonElement("PostnaSt")]
        public int postnast { get; set; }

        [BsonElement("ulica")]
        public string Ulica { get; set; }

        [BsonElement("hisnastevilka")]
        public int Hisnastevilka { get; set; }

        [BsonElement("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [BsonElement("naziv")]
        public string Naziv { get; set; }

        [BsonElement("Telefonska")]
        public string telefonska { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }
    }
}
