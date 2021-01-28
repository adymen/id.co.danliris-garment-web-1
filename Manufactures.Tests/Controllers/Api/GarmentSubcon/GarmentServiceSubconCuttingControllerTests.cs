﻿using Barebone.Tests;
using Manufactures.Controllers.Api.GarmentSubcon;
using Manufactures.Domain.GarmentSubcon.ServiceSubconCuttings;
using Manufactures.Domain.GarmentSubcon.ServiceSubconCuttings.ReadModels;
using Manufactures.Domain.GarmentSubcon.ServiceSubconCuttings.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Manufactures.Domain.Shared.ValueObjects;
using System.Net;
using System.Linq.Expressions;
using Manufactures.Domain.GarmentSubcon.ServiceSubconCuttings.Commands;
using System.Threading;
using Newtonsoft.Json;

namespace Manufactures.Tests.Controllers.Api.GarmentSubcon
{
    public class GarmentServiceSubconCuttingControllerTests : BaseControllerUnitTest
    {
        private Mock<IGarmentServiceSubconCuttingRepository> _mockGarmentServiceSubconCuttingRepository;
        private Mock<IGarmentServiceSubconCuttingItemRepository> _mockGarmentServiceSubconCuttingItemRepository;
        
        public GarmentServiceSubconCuttingControllerTests() : base()
        {
            _mockGarmentServiceSubconCuttingRepository = CreateMock<IGarmentServiceSubconCuttingRepository>();
            _mockGarmentServiceSubconCuttingItemRepository = CreateMock<IGarmentServiceSubconCuttingItemRepository>();
           
            _MockStorage.SetupStorage(_mockGarmentServiceSubconCuttingRepository);
            _MockStorage.SetupStorage(_mockGarmentServiceSubconCuttingItemRepository);
            
        }

        private GarmentServiceSubconCuttingController CreateGarmentServiceSubconCuttingController()
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            GarmentServiceSubconCuttingController controller = (GarmentServiceSubconCuttingController)Activator.CreateInstance(typeof(GarmentServiceSubconCuttingController), _MockServiceProvider.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            return controller;
        }

        [Fact]
        public async Task Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _mockGarmentServiceSubconCuttingRepository
                .Setup(s => s.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<GarmentServiceSubconCuttingReadModel>().AsQueryable());

            Guid ServiceSubconCuttingGuid = Guid.NewGuid();
            _mockGarmentServiceSubconCuttingRepository
                .Setup(s => s.Find(It.IsAny<IQueryable<GarmentServiceSubconCuttingReadModel>>()))
                .Returns(new List<GarmentServiceSubconCutting>()
                {
                    new GarmentServiceSubconCutting(ServiceSubconCuttingGuid, null, null, new UnitDepartmentId(1), null, null,  DateTimeOffset.Now, false)
                });

            Guid ServiceSubconCuttingItemGuid = Guid.NewGuid();
            GarmentServiceSubconCuttingItem garmentServiceSubconCuttingItem = new GarmentServiceSubconCuttingItem(Guid.Empty, ServiceSubconCuttingGuid, Guid.Empty, null, null, new GarmentComodityId(1), null, null);
            _mockGarmentServiceSubconCuttingItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentServiceSubconCuttingItemReadModel>()
                {
                    garmentServiceSubconCuttingItem.GetReadModel()
                }.AsQueryable());

            // Act
            var result = await unitUnderTest.Get();

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task GetSingle_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _mockGarmentServiceSubconCuttingRepository
                .Setup(s => s.Find(It.IsAny<Expression<Func<GarmentServiceSubconCuttingReadModel, bool>>>()))
                .Returns(new List<GarmentServiceSubconCutting>()
                {
                    new GarmentServiceSubconCutting(Guid.NewGuid(), null, null, new UnitDepartmentId(1), null, null, DateTimeOffset.Now, false)
                });

            _mockGarmentServiceSubconCuttingItemRepository
                .Setup(s => s.Find(It.IsAny<Expression<Func<GarmentServiceSubconCuttingItemReadModel, bool>>>()))
                .Returns(new List<GarmentServiceSubconCuttingItem>()
                {
                    new GarmentServiceSubconCuttingItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),null,null,new GarmentComodityId(1),null,null)
                });

           
            // Act
            var result = await unitUnderTest.Get(Guid.NewGuid().ToString());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Post_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _MockMediator
                .Setup(s => s.Send(It.IsAny<PlaceGarmentServiceSubconCuttingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentServiceSubconCutting(Guid.NewGuid(), null, null, new UnitDepartmentId(1), null, null, DateTimeOffset.Now, false));

            // Act
            var result = await unitUnderTest.Post(It.IsAny<PlaceGarmentServiceSubconCuttingCommand>());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Post_Throws_Exception()
        {
            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _MockMediator
                .Setup(s => s.Send(It.IsAny<PlaceGarmentServiceSubconCuttingCommand>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => unitUnderTest.Post(It.IsAny<PlaceGarmentServiceSubconCuttingCommand>()));
        }

        [Fact]
        public async Task Put_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _MockMediator
                .Setup(s => s.Send(It.IsAny<UpdateGarmentServiceSubconCuttingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentServiceSubconCutting(Guid.NewGuid(), null, null, new UnitDepartmentId(1), null, null, DateTimeOffset.Now, false));

            // Act
            var result = await unitUnderTest.Put(Guid.NewGuid().ToString(), new UpdateGarmentServiceSubconCuttingCommand());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Delete_StateUnderTest_ExpectedBehavior()
        {

            // Arrange
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _MockMediator
                .Setup(s => s.Send(It.IsAny<RemoveGarmentServiceSubconCuttingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentServiceSubconCutting(Guid.NewGuid(), null, null, new UnitDepartmentId(1), null, null, DateTimeOffset.Now, false));

            // Act
            var result = await unitUnderTest.Delete(Guid.NewGuid().ToString());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        

        [Fact]
        public async Task GetComplete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var id = Guid.NewGuid();
            var unitUnderTest = CreateGarmentServiceSubconCuttingController();

            _mockGarmentServiceSubconCuttingRepository
                .Setup(s => s.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<GarmentServiceSubconCuttingReadModel>() { new GarmentServiceSubconCuttingReadModel(id) }
                .AsQueryable());

            _mockGarmentServiceSubconCuttingRepository
                .Setup(s => s.Find(It.IsAny<IQueryable<GarmentServiceSubconCuttingReadModel>>()))
                .Returns(new List<GarmentServiceSubconCutting>()
                {
                    new GarmentServiceSubconCutting(Guid.NewGuid(), null, null, new UnitDepartmentId(1), null, null, DateTimeOffset.Now, false)
                });

            _mockGarmentServiceSubconCuttingItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentServiceSubconCuttingItemReadModel>()
                {
                    new GarmentServiceSubconCuttingItemReadModel(Guid.NewGuid())
                }.AsQueryable());

            _mockGarmentServiceSubconCuttingItemRepository
                .Setup(s => s.Find(It.IsAny<IQueryable<GarmentServiceSubconCuttingItemReadModel>>()))
                .Returns(new List<GarmentServiceSubconCuttingItem>()
                {
                    new GarmentServiceSubconCuttingItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),null,null,new GarmentComodityId(1),null,null)
                });

            // Act
            var orderData = new
            {
                Article = "desc",
            };

            string oder = JsonConvert.SerializeObject(orderData);
            var result = await unitUnderTest.GetComplete(1, 25, oder, new List<string>(), "", "{}");

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        


    }
}