namespace OrganizacijaDogadjajaApp.Patterns
{
    public enum CircuitBreakerState
    {
        Closed,    // Svi zahtevi prolaze
        Open,      // Nakon threshold-a svi zahtevi se odbijaju
        HalfOpen   // Nakon 10 sekundi pušta jedan probni zahtev
    }

    public class CircuitBreaker
    {
        private readonly object _lock = new object();

        private readonly int _failureThreshold;
        private readonly TimeSpan _openDuration;

        private DateTime _lastFailureTime = DateTime.MinValue;

        private int _failureCount;

        private CircuitBreakerState _state = CircuitBreakerState.Closed;

        public CircuitBreaker(int failureThreshold, TimeSpan openDuration)
        {
            _failureThreshold = failureThreshold;
            _openDuration = openDuration;
        }

        public CircuitBreakerState State
        {
            get
            {
                lock (_lock)
                {
                    // OPEN -> HALF-OPEN
                    if (_state == CircuitBreakerState.Open &&
                        (DateTime.UtcNow - _lastFailureTime) > _openDuration)
                    {
                        _state = CircuitBreakerState.HalfOpen;

                        Console.WriteLine("CIRCUIT BREAKER -> HALF-OPEN");
                    }

                    return _state;
                }
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            // FAIL FAST
            if (State == CircuitBreakerState.Open)
            {
                Console.WriteLine("CIRCUIT BREAKER -> OPEN (FAIL FAST)");

                throw new CircuitBreakerOpenException(
                    "Circuit Breaker je OPEN - zahtev blokiran.");
            }

            try
            {
                var result = await action();

                lock (_lock)
                {
                    // Uspešan zahtev -> reset
                    _failureCount = 0;

                    if (_state != CircuitBreakerState.Closed)
                    {
                        Console.WriteLine("CIRCUIT BREAKER -> CLOSED");
                    }

                    _state = CircuitBreakerState.Closed;
                }

                return result;
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    _failureCount++;

                    _lastFailureTime = DateTime.UtcNow;

                    Console.WriteLine($"CIRCUIT BREAKER -> FAILURE COUNT: {_failureCount}");

                    // HALF-OPEN fail -> OPEN
                    if (_state == CircuitBreakerState.HalfOpen)
                    {
                        _state = CircuitBreakerState.Open;

                        Console.WriteLine("CIRCUIT BREAKER -> OPEN");
                    }

                    // Threshold dostignut -> OPEN
                    if (_failureCount >= _failureThreshold)
                    {
                        _state = CircuitBreakerState.Open;

                        Console.WriteLine("CIRCUIT BREAKER -> OPEN");
                    }
                }

                throw;
            }
        }
    }

    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException(string? message)
            : base(message)
        {
        }
    }
}