using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Order.Model
{
    public class NewOrder
    {
        [Required]
        public Guid? CustomerId { get; set; }

        [Required]
        public Guid? ResellerId { get; set; }

        [Required]
        public IEnumerable<NewOrderItem> Items { get; set; }
    }
}
