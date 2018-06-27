using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using KatlaSport.DataAccess.ProductCatalogue;
using KatlaSport.Services.ProductManagement;
using Moq;
using Xunit;
using ProductCategory = KatlaSport.DataAccess.ProductCatalogue.ProductCategory;

namespace KatlaSport.Services.Tests.ProductManagement
{
    public class ProductCategoryServiceTests
    {

        public ProductCategoryServiceTests()
        {
            var mapper = MapperInitializer.Instance;
        }

        [Theory]
        [AutoMoqData]
        public void Create_ProductCategoryService_WithNull_FirstParameter_Test([Frozen] IMock<IUserContext> userContext)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ProductCategoryService(null, userContext.Object));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_ProductCategoryService_WithNull_SecondParameter_Test([Frozen] IMock<IProductCatalogueContext> context)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ProductCategoryService(context.Object, null));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProductCategory_Found_Entity_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            context.Setup(c => c.Categories).ReturnsEntitySet(listEntity);

            var category = await productCategoryService.GetCategoryAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, category.Id);
            Assert.Equal(listEntity[0].Code, category.Code);
            Assert.Equal(listEntity[0].Name, category.Name);
            Assert.Equal(listEntity[0].IsDeleted, category.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task Setstatus_NotFoundException_Entity_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, int productCategoeyId, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(0).ToList();
            context.Setup(c => c.Categories).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() =>
                productCategoryService.SetStatusAsync(productCategoeyId, deletedStatus));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatus_ValidData_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            context.Setup(c => c.Categories).ReturnsEntitySet(listEntity);

            await productCategoryService.SetStatusAsync(listEntity[0].Id, deletedStatus);
            var categoryAfter = await productCategoryService.GetCategoryAsync(listEntity[0].Id);

            Assert.Equal(categoryAfter.IsDeleted, deletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProductCategory_NotFound_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, int productCategoeyId)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(0).ToList();
            context.Setup(c => c.Categories).ReturnsEntitySet(listEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCategoryService.GetCategoryAsync(productCategoeyId));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProductCategory_ValidData_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            context.Setup(c => c.Categories)
                .ReturnsEntitySet(listEntity);

            var foundHive = await productCategoryService.GetCategoryAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, foundHive.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateProductCategory_AddedSuccessfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            var addedCategory = await productCategoryService.CreateCategoryAsync(createRequest);

            var category = await productCategoryService.GetCategoryAsync(addedCategory.Id);

            Assert.Equal(category.Name, createRequest.Name);
            Assert.Equal(category.Description, createRequest.Description);
            Assert.Equal(category.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateProductCategory_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCategoryService.CreateCategoryAsync(createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProductCategory_UpdateSuccessfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            var addedCategory = await productCategoryService.UpdateCategoryAsync(listEntity[0].Id, createRequest);

            var category = await productCategoryService.GetCategoryAsync(addedCategory.Id);

            Assert.Equal(category.Id, listEntity[0].Id);
            Assert.Equal(category.Name, createRequest.Name);
            Assert.Equal(category.Description, createRequest.Description);
            Assert.Equal(category.Code, createRequest.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProductCategory_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, int productCategoeyId)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCategoryService.UpdateCategoryAsync(productCategoeyId, createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProductCategory_NotFoundException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, int productCategoeyId)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCategoryService.UpdateCategoryAsync(productCategoeyId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProductCategory_Successfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            var idDeleted = listEntity[0].Id;
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);

            await productCategoryService.SetStatusAsync(listEntity[0].Id, true);
            await productCategoryService.DeleteCategoryAsync(listEntity[0].Id);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCategoryService.GetCategoryAsync(idDeleted));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProductCategory_NotFoundException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService, int productCategoeyId)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();
            var createRequest = fixture.Create<UpdateProductCategoryRequest>();
            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCategoryService.UpdateCategoryAsync(productCategoeyId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProductCategory_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCategoryService productCategoryService)
        {
            var listEntity = fixture.CreateMany<ProductCategory>(13).ToList();

            context.Setup(x => x.Categories).ReturnsEntitySet(listEntity);
            await productCategoryService.SetStatusAsync(listEntity[0].Id, false);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCategoryService.DeleteCategoryAsync(listEntity[0].Id));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }
    }
}