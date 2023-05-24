using System;

public class Utilities
{
    public static string UserID_GuidBase64Shortened() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                                .Replace("/", "_")
                                                .Replace("+", "-")
                                                .Substring(0, 15);
}
