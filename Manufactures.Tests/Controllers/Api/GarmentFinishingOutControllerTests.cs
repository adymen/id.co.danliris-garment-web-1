﻿using Barebone.Tests;
using Manufactures.Controllers.Api;
using Manufactures.Domain.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GarmentFinishingOuts;
using Manufactures.Domain.GarmentFinishingOuts.Commands;
using Manufactures.Domain.GarmentFinishingOuts.ReadModels;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Manufactures.Domain.Shared.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Manufactures.Tests.Controllers.Api
{
    public class GarmentFinishingOutControllerTests : BaseControllerUnitTest
    {
        private readonly Mock<IGarmentFinishingOutRepository> _mockGarmentFinishingOutRepository;
        private readonly Mock<IGarmentFinishingOutItemRepository> _mockGarmentFinishingOutItemRepository;
        private readonly Mock<IGarmentFinishingOutDetailRepository> _mockGarmentFinishingOutDetailRepository;
        private readonly Mock<IGarmentFinishingInItemRepository> _mockFinishingInItemRepository;

        public GarmentFinishingOutControllerTests() : base()
        {
            _mockGarmentFinishingOutRepository = CreateMock<IGarmentFinishingOutRepository>();
            _mockGarmentFinishingOutItemRepository = CreateMock<IGarmentFinishingOutItemRepository>();
            _mockGarmentFinishingOutDetailRepository = CreateMock<IGarmentFinishingOutDetailRepository>();
            _mockFinishingInItemRepository = CreateMock<IGarmentFinishingInItemRepository>();

            _MockStorage.SetupStorage(_mockGarmentFinishingOutRepository);
            _MockStorage.SetupStorage(_mockGarmentFinishingOutItemRepository);
            _MockStorage.SetupStorage(_mockGarmentFinishingOutDetailRepository);
            _MockStorage.SetupStorage(_mockFinishingInItemRepository);
        }

        private GarmentFinishingOutController CreateGarmentFinishingOutController()
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            GarmentFinishingOutController controller = (GarmentFinishingOutController)Activator.CreateInstance(typeof(GarmentFinishingOutController), _MockServiceProvider.Object);
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
            var unitUnderTest = CreateGarmentFinishingOutController();

            _mockGarmentFinishingOutRepository
                .Setup(s => s.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<GarmentFinishingOutReadModel>().AsQueryable());

            Guid finishingOutGuid = Guid.NewGuid();
            _mockGarmentFinishingOutRepository
                .Setup(s => s.Find(It.IsAny<IQueryable<GarmentFinishingOutReadModel>>()))
                .Returns(new List<GarmentFinishingOut>()
                {
                    new GarmentFinishingOut(finishingOutGuid, null,new UnitDepartmentId(1),null,null,"Finishing",DateTimeOffset.Now, "RONo", null, new UnitDepartmentId(1), null, null,new GarmentComodityId(1),null,null,true)
                });

            Guid finishingInItemGuid = Guid.NewGuid();
            Guid finishingInGuid = Guid.NewGuid();
            Guid finishingOutItemGuid = Guid.NewGuid();
            GarmentFinishingOutItem garmentFinishingOutItem = new GarmentFinishingOutItem(finishingOutItemGuid, finishingOutGuid, finishingInGuid, finishingInItemGuid, new ProductId(1), null, null, null, new SizeId(1), null, 1, new UomId(1), null, null, 1, 1, 1);
            _mockGarmentFinishingOutItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingOutItemReadModel>()
                {
                    garmentFinishingOutItem.GetReadModel()
                }.AsQueryable());

            GarmentFinishingOutDetail garmentFinishingOutDetail = new GarmentFinishingOutDetail(Guid.NewGuid(), finishingOutItemGuid, new SizeId(1), null, 1, new UomId(1), null);
            _mockGarmentFinishingOutDetailRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingOutDetailReadModel>()
                {
                    garmentFinishingOutDetail.GetReadModel()
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
            var unitUnderTest = CreateGarmentFinishingOutController();
            Guid finishingOutGuid = Guid.NewGuid();
            _mockGarmentFinishingOutRepository
                .Setup(s => s.Find(It.IsAny<Expression<Func<GarmentFinishingOutReadModel, bool>>>()))
                .Returns(new List<GarmentFinishingOut>()
                {
                    new GarmentFinishingOut(finishingOutGuid, null,new UnitDepartmentId(1),null,null,"Finishing",DateTimeOffset.Now, "RONo", null, new UnitDepartmentId(1), null, null,new GarmentComodityId(1),null,null,true)
                });

            Guid finishingInItemGuid = Guid.NewGuid();
            Guid finishingInGuid = Guid.NewGuid();
            Guid finishingOutItemGuid = Guid.NewGuid();
            _mockGarmentFinishingOutItemRepository
                .Setup(s => s.Find(It.IsAny<Expression<Func<GarmentFinishingOutItemReadModel, bool>>>()))
                .Returns(new List<GarmentFinishingOutItem>()
                {
                    new GarmentFinishingOutItem(finishingOutItemGuid, finishingOutGuid, finishingInGuid, finishingInItemGuid, new ProductId(1), null, null, null, new SizeId(1), null, 1, new UomId(1), null, null, 1,1,1)
                });

            _mockGarmentFinishingOutDetailRepository
                .Setup(s => s.Find(It.IsAny<Expression<Func<GarmentFinishingOutDetailReadModel, bool>>>()))
                .Returns(new List<GarmentFinishingOutDetail>()
                {
                    new GarmentFinishingOutDetail(Guid.NewGuid(), finishingOutItemGuid, new SizeId(1), null, 1, new UomId(1), null)
                });

            //_mockFinishingInItemRepository
            //    .Setup(s => s.Query)
            //    .Returns(new List<GarmentFinishingInItemReadModel>().AsQueryable());

            // Act
            var result = await unitUnderTest.Get(Guid.NewGuid().ToString());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Post_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentFinishingOutController();
            Guid finishingOutGuid = Guid.NewGuid();
            _MockMediator
                .Setup(s => s.Send(It.IsAny<PlaceGarmentFinishingOutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentFinishingOut(finishingOutGuid, null,  new UnitDepartmentId(1), null, null, "Finishing", DateTimeOffset.Now, "RONo", null, new UnitDepartmentId(1), null, null, new GarmentComodityId(1), null, null, true));

            // Act
            var result = await unitUnderTest.Post(It.IsAny<PlaceGarmentFinishingOutCommand>());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Put_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentFinishingOutController();
            Guid finishingOutGuid = Guid.NewGuid();
            _MockMediator
                .Setup(s => s.Send(It.IsAny<UpdateGarmentFinishingOutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentFinishingOut(finishingOutGuid, null,  new UnitDepartmentId(1), null, null, "Finishing", DateTimeOffset.Now, "RONo", null, new UnitDepartmentId(1), null, null, new GarmentComodityId(1), null, null, true));

            // Act
            var result = await unitUnderTest.Put(Guid.NewGuid().ToString(), new UpdateGarmentFinishingOutCommand());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }

        [Fact]
        public async Task Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateGarmentFinishingOutController();
            Guid finishingOutGuid = Guid.NewGuid();
            _MockMediator
                .Setup(s => s.Send(It.IsAny<RemoveGarmentFinishingOutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GarmentFinishingOut(finishingOutGuid, null, new UnitDepartmentId(1), null, null, "Finishing", DateTimeOffset.Now, "RONo", null, new UnitDepartmentId(1), null, null, new GarmentComodityId(1), null, null, true));

            // Act
            var result = await unitUnderTest.Delete(Guid.NewGuid().ToString());

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        }
        
    }
}