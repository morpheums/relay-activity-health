namespace Relay.Core.Calendar;

public sealed class NotAWeekStartException : ArgumentException
{
    public NotAWeekStartException()
    {
    }

    public NotAWeekStartException(string message)
        : base(message)
    {
    }

    public NotAWeekStartException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotAWeekStartException(DateOnly localDate, string parameterName)
        : base($"{localDate:yyyy-MM-dd} is not a Monday.", parameterName)
    {
        LocalDate = localDate;
    }

    public DateOnly? LocalDate { get; }
}
