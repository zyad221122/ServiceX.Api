﻿namespace ServiceX.Contracts.UserContract;

public class VerifyOtpAndResetPasswordRequest
{
    public string Email { get; set; }
    public string Otp { get; set; }
    public string NewPassword { get; set; }
}
