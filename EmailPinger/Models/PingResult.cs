using System;

namespace EmailPinger.Models
{
    public class PingResult
    {
        public DateTime PingTime { get; private set; }
        public bool Success { get; private set; }
        public long RoundTripTime { get; private set; } 

        public PingResult() {
            PingTime = DateTime.Now;
        }

        public PingResult(bool success, long roundTripTime) : this()
        {
            Success = success;
            RoundTripTime = roundTripTime;
        }

        public override string ToString()
        {
            return $@"{{ ""PingTime"": {PingTime}, ""RoundTripTime"": {RoundTripTime}, ""Success"": {Success} }},";
        }
    }
}
