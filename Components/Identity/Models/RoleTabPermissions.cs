using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SmartSolarMicrogrid.API.Components.Identity.Models
{
    /// <summary>
    /// Stores tab visibility configuration for each user role.
    /// When VisibleTabs is null, users of this role see all tabs by default.
    /// </summary>
    public class RoleTabPermissions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// The user role this configuration applies to.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; }

        /// <summary>
        /// List of tab keys that are visible for this role.
        /// When null, all tabs are visible (default behavior).
        /// </summary>
        public List<string>? VisibleTabs { get; set; } = null;

        /// <summary>
        /// Timestamp when this configuration was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}