using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartSolarMicrogrid.API.Components.Identity.Models
{

    public class RoleTabPermissions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; }

        public List<string>? VisibleTabs { get; set; } = null;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}