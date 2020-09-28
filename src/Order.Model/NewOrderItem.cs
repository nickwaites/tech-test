using System;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class NewOrderItem
    {
        [Required]
        public Guid? ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Quantity")]
        public int? Quantity { get; set; }
    }
}
