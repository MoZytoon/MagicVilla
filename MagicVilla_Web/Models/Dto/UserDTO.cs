﻿namespace MagicVilla_Web.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        //get from token
        //public string Role { get; set; }
    }
}
