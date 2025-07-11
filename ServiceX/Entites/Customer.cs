﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class Customer
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int Balanace { get; set; } = 0;

    public ICollection<Order> Orders { get; set; }
    public ICollection<Review> Reviews { get; set; }
}
