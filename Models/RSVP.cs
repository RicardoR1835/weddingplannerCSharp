using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace weddingPlanner.Models
{
    public class RSVP
    {
        [Key]
        public int RsvpId {get;set;}

        public int UserId {get;set;}

        public int WeddingId {get;set;}

        public Wedding UpcomingWedding {get;set;}

        public User Guest {get;set;}
    }
}