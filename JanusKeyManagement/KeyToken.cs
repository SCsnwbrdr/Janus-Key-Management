﻿namespace JanusKeyManagement
{
    public class KeyToken
    {
        public string Token { get; set; }
        public bool IsPrimary { get; set; }
        public string Identifier { get; set; }
        public bool DeadToken { get; set; }

        public KeyToken() { }
    }
}
