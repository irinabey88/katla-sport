using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using KatlaSport.DataAccess.ProductStoreHive;
using KatlaSport.Services.HiveManagement;
using Moq;
using Xunit;

namespace KatlaSport.Services.Tests.HiveManagement
{
    public class HiveServiceTests
    {
        public HiveServiceTests()
        {
            Mapper.Reset();
            Mapper.Initialize(config =>
            {
                config.CreateMap<StoreHive, HiveListItem>();
                config.CreateMap<StoreHive, Hive>();
                config.CreateMap<StoreHiveSection, HiveSectionListItem>();
                config.CreateMap<StoreHiveSection, HiveSection>();
                config.CreateMap<StoreHive, Hive>();
                config.CreateMap<UpdateHiveRequest, StoreHive>();
            });
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveService_WithNull_FirstParameter_Test([Frozen] IMock<IUserContext> userContext)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveService(null, userContext.Object));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveService_WithNull_SecondParameter_Test([Frozen] IMock<IProductStoreHiveContext> context)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveService(context.Object, null));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task Setstatus_NotFound_Entity_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, int hiveId, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<StoreHive>(0).ToList();
            context.Setup(c => c.Hives).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() =>
                hiveService.SetStatusAsync(hiveId, deletedStatus));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatus_ValidData_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            context.Setup(c => c.Hives).ReturnsEntitySet(listEntity);

            await hiveService.SetStatusAsync(listEntity[0].Id, deletedStatus);
            var hiveAfter = await hiveService.GetHiveAsync(listEntity[0].Id);

            Assert.Equal(hiveAfter.IsDeleted, deletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_NotFound_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, int hiveId)
        {
            var listEntity = fixture.CreateMany<StoreHive>(0).ToList();
            context.Setup(c => c.Hives).ReturnsEntitySet(listEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveService.GetHiveAsync(hiveId));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_ValidData_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            context.Setup(c => c.Hives)
                .ReturnsEntitySet(listEntity);

            var foundHive = await hiveService.GetHiveAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, foundHive.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHive_AddedSuccessfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            var listSectionEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            context.Setup(x => x.Sections).ReturnsEntitySet(listSectionEntity);
            var createRequest = fixture.Create<UpdateHiveRequest>();
            var addedHive = await hiveService.CreateHiveAsync(createRequest);

            var hive = await hiveService.GetHiveAsync(addedHive.Id);

            Assert.Equal(hive.Name, createRequest.Name);
            Assert.Equal(hive.Address, createRequest.Address);
            Assert.Equal(hive.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHive_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveService.CreateHiveAsync(createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_UpdateSuccessfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateHiveRequest>();
            var addedHive = await hiveService.UpdateHiveAsync(listEntity[0].Id, createRequest);

            var hive = await hiveService.GetHiveAsync(addedHive.Id);

            Assert.Equal(hive.Id, listEntity[0].Id);
            Assert.Equal(hive.Name, createRequest.Name);
            Assert.Equal(hive.Address, createRequest.Address);
            Assert.Equal(hive.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, int hiveId)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveService.UpdateHiveAsync(hiveId, createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_NotFoundException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, int hiveId)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveRequest>();
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveService.UpdateHiveAsync(hiveId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_Successfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            context.Setup(x => x.Sections).ReturnsEntitySet(new List<StoreHiveSection>());
            await hiveService.SetStatusAsync(listEntity[0].Id, true);
            await hiveService.DeleteHiveAsync(listEntity[0].Id);

            var hives = await hiveService.GetHivesAsync();

            Assert.Equal(12, hives.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_NotFoundException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService, int hiveId)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveRequest>();
            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveService.UpdateHiveAsync(hiveId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveService hiveService)
        {
            var listEntity = fixture.CreateMany<StoreHive>(13).ToList();

            context.Setup(x => x.Hives).ReturnsEntitySet(listEntity);
            await hiveService.SetStatusAsync(listEntity[0].Id, false);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveService.DeleteHiveAsync(listEntity[0].Id));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }
    }
}