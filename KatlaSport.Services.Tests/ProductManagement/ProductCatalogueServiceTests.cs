using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using KatlaSport.DataAccess.ProductCatalogue;
using KatlaSport.Services.ProductManagement;
using Moq;
using Xunit;

namespace KatlaSport.Services.Tests.ProductManagement
{
    public class ProductCatalogueServiceTests
    {
        public ProductCatalogueServiceTests()
        {
            var mapper = MapperInitializer.Instance;
        }

        [Theory]
        [AutoMoqData]
        public void Create_ProductCatalog_WithNull_FirstParameter_Test([Frozen] IMock<IUserContext> userContext)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ProductCatalogueService(null, userContext.Object));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public void Create_ProductCatalog_WithNull_SecondParameter_Test([Frozen] IMock<IProductCatalogueContext> context)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ProductCatalogueService(context.Object, null));

            Assert.Equal(typeof(ArgumentNullException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProducts_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            int start = 3;
            int amount = 5;
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products).ReturnsEntitySet(listEntity);

            var products = await productCatalogueService.GetProductsAsync(start, amount);

            Assert.Equal(amount, products.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProduct_Found_Entity_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products).ReturnsEntitySet(listEntity);

            var products = await productCatalogueService.GetProductAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, products.Id);
            Assert.Equal(listEntity[0].Code, products.Code);
            Assert.Equal(listEntity[0].Name, products.Name);
            Assert.Equal(listEntity[0].IsDeleted, products.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProduct_NotFound_Entity_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, int productCatalogId)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products).ReturnsEntitySet(listEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCatalogueService.GetProductAsync(productCatalogId));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task Setstatus_NotFound_Entity_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, int productCatalogId, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products).ReturnsEntitySet(listEntity);

            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() =>
                productCatalogueService.SetStatusAsync(productCatalogId, deletedStatus));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatus_ValidData_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, bool deletedStatus)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products).ReturnsEntitySet(listEntity);

            await productCatalogueService.SetStatusAsync(listEntity[0].Id, deletedStatus);
            var productAfter = await productCatalogueService.GetProductAsync(listEntity[0].Id);

            Assert.Equal(productAfter.IsDeleted, deletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetProduct_ValidData_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(c => c.Products)
                .ReturnsEntitySet(listEntity);

            var foundProduct = await productCatalogueService.GetProductAsync(listEntity[0].Id);

            Assert.Equal(listEntity[0].Id, foundProduct.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateProduct_AddedSuccessfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateProductRequest>();
            var addedProduct = await productCatalogueService.CreateProductAsync(createRequest);

            var product = await productCatalogueService.GetProductAsync(addedProduct.Id);

            Assert.Equal(product.Name, createRequest.Name);
            Assert.Equal(product.ManufacturerCode, createRequest.ManufacturerCode);
            Assert.Equal(product.Code, createRequest.Code);
            Assert.Equal(product.Description, createRequest.Description);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateProduct_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            var createRequest = fixture.Create<UpdateProductRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCatalogueService.CreateProductAsync(createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProduct_UpdateSuccessfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var createRequest = fixture.Create<UpdateProductRequest>();
            var addedProduct = await productCatalogueService.UpdateProductAsync(listEntity[0].Id, createRequest);

            var product = await productCatalogueService.GetProductAsync(addedProduct.Id);

            Assert.Equal(product.Name, createRequest.Name);
            Assert.Equal(product.ManufacturerCode, createRequest.ManufacturerCode);
            Assert.Equal(product.Code, createRequest.Code);
            Assert.Equal(product.Description, createRequest.Description);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProduct_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, int productCatalogId)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            var createRequest = fixture.Create<UpdateProductRequest>();
            createRequest.Code = listEntity[0].Code;
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCatalogueService.UpdateProductAsync(productCatalogId, createRequest));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateProduct_NotFoundException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, int productCatalogId)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            var createRequest = fixture.Create<UpdateProductRequest>();
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCatalogueService.UpdateProductAsync(productCatalogId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProduct_Successfuly_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
           ;
            await productCatalogueService.SetStatusAsync(listEntity[0].Id, true);
            await productCatalogueService.DeleteProductAsync(listEntity[0].Id);

            var products = await productCatalogueService.GetProductsAsync(0, 12);

            Assert.Equal(12, products.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProduct_NotFoundException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService, int productCatalogId)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();
            var createRequest = fixture.Create<UpdateProductRequest>();
            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            var ex = await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => productCatalogueService.UpdateProductAsync(productCatalogId, createRequest));

            Assert.Equal(typeof(RequestedResourceNotFoundException), ex.GetType());
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteProduct_ConflictException_Test([Frozen] Mock<IProductCatalogueContext> context, IFixture fixture, ProductCatalogueService productCatalogueService)
        {
            var listEntity = fixture.CreateMany<CatalogueProduct>(13).ToList();

            context.Setup(x => x.Products).ReturnsEntitySet(listEntity);
            await productCatalogueService.SetStatusAsync(listEntity[0].Id, false);
            var ex = await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => productCatalogueService.DeleteProductAsync(listEntity[0].Id));

            Assert.Equal(typeof(RequestedResourceHasConflictException), ex.GetType());
        }
    }
}