using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieApp.Data.Context;
using MovieApp.Services;
using MovieApp.Services.APIModels.Users;
using MovieApp.Services.Interfaces;

namespace MovieApp.Tests.LoginTests;

[TestClass]
public class LoginTests
{
    private readonly Mock<IUserService> _userService = new Mock<IUserService>();
    private readonly UserService userService;
    private readonly Mock<IMovieDbContext> _movieDbContext = new Mock<IMovieDbContext>();

    public LoginTests()
    {
        userService = new UserService(_movieDbContext.Object);
    }

    [TestMethod]
    public void TestMethod1()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        CreateUser user = new CreateUser
        {
            Username = "testing",
            Email = "testing@gmail.com",
            Password = "testing"
        };
    }
}
