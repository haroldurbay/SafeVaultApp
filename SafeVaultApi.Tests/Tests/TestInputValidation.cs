using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using SafeVaultApi.Contracts;

namespace SafeVaultApi.Tests.Tests;

[TestFixture]
public class TestInputValidation
{
    [TestCase("admin' OR '1'='1", "admin@example.com")]
    [TestCase("admin; DROP TABLE Users;--", "admin@example.com")]
    [TestCase("' UNION SELECT * FROM Users --", "admin@example.com")]
    [TestCase("safe.user", "user@example.com' OR '1'='1")]
    public void TestForSQLInjection(string username, string email)
    {
        var request = new RegisterUserRequest
        {
            Username = username,
            Email = email,
            Password = "Str0ng!Password"
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults, Is.Not.Empty);
        });
    }

    [TestCase("<script>alert('xss')</script>", "safe@example.com")]
    [TestCase("<img src=x onerror=alert(1)>", "safe@example.com")]
    [TestCase("safe.user", "<script>alert(1)</script>@example.com")]
    [TestCase("safe.user", "user@example.com\r\n<script>alert(1)</script>")]
    public void TestForXSS(string username, string email)
    {
        var request = new RegisterUserRequest
        {
            Username = username,
            Email = email,
            Password = "Str0ng!Password"
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults, Is.Not.Empty);
        });
    }
}