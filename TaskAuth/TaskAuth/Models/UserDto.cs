﻿namespace TaskAuth.Models
{
    public class UserDto
    {
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        
    }
}
