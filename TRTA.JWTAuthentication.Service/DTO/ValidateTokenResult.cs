using System;
using System.Collections.Generic;

namespace TRTA.OSP.Authentication.Service.DTO
{
    public class ValidateTokenResult
    {
        public bool Succeeded { get; set; }
        public string Subject { get; set; }
        public IDictionary<string, string> Attributes { get; set; }
        public Exception Exception { get; set; }
        public ValidateTokenResult()
        {
        }
        public ValidateTokenResult(bool succeeded, IDictionary<string, string> attributes, string subject)
        {
            Succeeded = succeeded;
            Attributes = attributes;
            Subject = subject;
        }
    }
}
