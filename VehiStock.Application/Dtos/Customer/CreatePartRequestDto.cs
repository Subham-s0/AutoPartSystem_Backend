using System.ComponentModel.DataAnnotations;

namespace VehiStock.Application.DTOs.Customer
{
    /// <summary>
    /// DTO used when a client creates a request for a specific part.
    /// The controller will set PartId from the route, so only the
    /// request‑specific fields are needed here.
    /// </summary>
    public class CreatePartRequestDto
    {
        /// <summary>
        /// Quantity of the part being requested. Must be at least 1.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Optional free‑form note from the requester (e.g., reason for request).
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// PartId is populated by the controller from the route parameter.
        /// It is kept here for clarity and for the service signature.
        /// </summary>
        public int PartId { get; set; }
    }
}
