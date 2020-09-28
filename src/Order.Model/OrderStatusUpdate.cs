using System;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class OrderStatusUpdate
    {
        [Required]
        public Guid? StatusId { get; set; }
    }
}
