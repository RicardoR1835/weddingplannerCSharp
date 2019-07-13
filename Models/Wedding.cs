using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace weddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId {get;set;}

        [Required]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name {get;set;}

        [Required]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        public string Name2 {get;set;}

        [Required(ErrorMessage = "Please enter your address")]
        public string Address {get;set;}

        [Required]
        public DateTime Date {get;set;}

        public int UserId {get;set;}

        public User Creator {get;set;}

        public List<RSVP> GuestList {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;

        public DateTime UpdatedAt {get;set;} = DateTime.Now;

    }
}