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
    public class HiveSectionServiceTests
    {
        public HiveSectionServiceTests()
        {
            Mapper.Reset();
            Mapper.Initialize(config =>
            {
                config.AddProfile<HiveManagementMappingProfile>();
            });
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSections_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listSectionEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listSectionEntity);

            var hiveSections = await hiveSectionService.GetHiveSectionsAsync();

            Assert.Equal(13, hiveSections.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_Found_Entity_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listSectionEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listSectionEntity);

            var hives = await hiveSectionService.GetHiveSectionAsync(listSectionEntity[0].Id);

            Assert.Equal(listSectionEntity[0].Id, hives.Id);
            Assert.Equal(listSectionEntity[0].Code, hives.Code);
            Assert.Equal(listSectionEntity[0].Name, hives.Name);
            Assert.Equal(listSectionEntity[0].IsDeleted, hives.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_Not_Found_Entity_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionId)
        {
            var listSectionEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listSectionEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveSectionService.GetHiveSectionAsync(hiveSectionId));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveSectionService_WithNull_FirstParameter_Test([Frozen] IMock<IUserContext> userContext)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveSectionService(null, userContext.Object));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveSectionService_WithNull_SecondParameter_Test([Frozen] IMock<IProductStoreHiveContext> context)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveSectionService(context.Object, null));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task Setstatus_NotFound_Entity_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionId, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(0).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() =>
                hiveSectionService.SetStatusAsync(hiveSectionId, deletedStatus));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatus_ValidData_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listEntity);

            await hiveSectionService.SetStatusAsync(listEntity[0].Id, deletedStatus);
            var hiveSectionAfter = await hiveSectionService.GetHiveSectionAsync(listEntity[0].Id);

            Assert.Equal(hiveSectionAfter.IsDeleted, deletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_NotFound_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionHiveId)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(0).ToList();
            context.Setup(c => c.Sections).ReturnsEntitySet(listEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveSectionService.GetHiveSectionAsync(hiveSectionHiveId));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_ValidData_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(c => c.Sections)
                .ReturnsEntitySet(listEntity);

            var foundHive = await hiveSectionService.GetHiveSectionAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, foundHive.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSection_AddedSuccessfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            var listHiveEntity = fixture.CreateMany<StoreHive>(13).ToList();
            context.Setup(x => x.Hives).ReturnsEntitySet(listHiveEntity);
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            createRequest.StoreHiveId = listHiveEntity[0].Id;
            var addedHiveSection = await hiveSectionService.CreateHiveSectionAsync(createRequest);

            var hiveSection = await hiveSectionService.GetHiveSectionAsync(addedHiveSection.Id);

            Assert.Equal(hiveSection.Name, createRequest.Name);
            Assert.Equal(hiveSection.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSection_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, string code)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            listEntity[0].Code = code;
            var listHiveEntity = fixture.CreateMany<StoreHive>(0).ToList();
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            createRequest.Code = code;
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            context.Setup(x => x.Hives).ReturnsEntitySet(listHiveEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveSectionService.CreateHiveSectionAsync(createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_UpdateSuccessfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            listEntity[0].Name = "tests";
            listEntity[0].Code = "tests";
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            var addedHive = await hiveSectionService.UpdeteHiveSectionAsync(listEntity[0].Id, createRequest);

            var hiveSection = await hiveSectionService.GetHiveSectionAsync(addedHive.Id);

            Assert.Equal(hiveSection.Id, listEntity[0].Id);
            Assert.Equal(hiveSection.Name, createRequest.Name);
            Assert.Equal(hiveSection.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionId, string code)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            listEntity[0].Code = code;
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            createRequest.Code = code;
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveSectionService.UpdeteHiveSectionAsync(hiveSectionId, createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_NotFoundException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionId)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveSectionService.UpdeteHiveSectionAsync(hiveSectionId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSection_Successfuly_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            var listHiveEntity = new List<StoreHive>();
            context.Setup(x => x.Hives).ReturnsEntitySet(listHiveEntity);
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            await hiveSectionService.SetStatusAsync(listEntity[0].Id, true);
            await hiveSectionService.DeleteHiveSectionAsync(listEntity[0].Id);

            var hiveSections = await hiveSectionService.GetHiveSectionsAsync();

            Assert.Equal(12, hiveSections.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSection_NotFoundException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService, int hiveSectionId)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            var createRequest = fixture.Create<UpdateHiveSectionRequest>();
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveSectionService.UpdeteHiveSectionAsync(hiveSectionId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSection_ConflictException_Test([Frozen] Mock<IProductStoreHiveContext> context, IFixture fixture, HiveSectionService hiveSectionService)
        {
            var listEntity = fixture.CreateMany<StoreHiveSection>(13).ToList();
            context.Setup(x => x.Sections).ReturnsEntitySet(listEntity);
            await hiveSectionService.SetStatusAsync(listEntity[0].Id, false);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => hiveSectionService.DeleteHiveSectionAsync(listEntity[0].Id));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }
    }
}