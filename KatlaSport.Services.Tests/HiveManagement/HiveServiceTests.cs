using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
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
                config.CreateMap<StoreHive, Hive>();
            });
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveService_WithNull_FirstParameter_Test([Frozen]IMock<IUserContext> userContext)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveService(null, userContext.Object));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_HiveService_WithNull_SecondParameter_Test([Frozen]IMock<IProductStoreHiveContext> context)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HiveService(context.Object, null));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task Setstatus_NotFound_Entity_Test(int hiveId, bool deletedStatus)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var context = fixture.Freeze<Mock<IProductStoreHiveContext>>();
            context.Setup(c => c.Hives).ReturnsEntitySet( new StoreHive[0] );
            var service = fixture.Create<HiveService>();
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => service.SetStatusAsync(hiveId, deletedStatus));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatus_ValidData_Test(int hiveId, bool deletedStatus)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var context = fixture.Freeze<Mock<IProductStoreHiveContext>>();
            context.Setup(c => c.Hives).ReturnsEntitySet(new List<StoreHive> { new StoreHive { Id = hiveId } });
            var service = fixture.Create<HiveService>();

            await service.SetStatusAsync(hiveId, deletedStatus);
            var hiveAfter = await service.GetHiveAsync(hiveId);

            Assert.Equal(hiveAfter.IsDeleted, deletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_NotFound_Test(int hiveId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var context = fixture.Freeze<Mock<IProductStoreHiveContext>>();
            context.Setup(c => c.Hives).ReturnsEntitySet(new StoreHive[0]);
            var service = fixture.Create<HiveService>();

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => service.GetHiveAsync(hiveId));
            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_ValidData_Test(int hiveId)
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var context = fixture.Freeze<Mock<IProductStoreHiveContext>>();
            context.Setup(c => c.Hives).ReturnsEntitySet(new List<StoreHive> { new StoreHive { Id = hiveId }, new StoreHive() { Id = hiveId + 1 } });
            var service = fixture.Create<HiveService>();

            var foundHive = await service.GetHiveAsync(hiveId);

            Assert.Equal(hiveId, foundHive.Id);
        }
    }
}