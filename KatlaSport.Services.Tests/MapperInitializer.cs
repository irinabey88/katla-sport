using System;
using AutoMapper;
using KatlaSport.Services.HiveManagement;
using KatlaSport.Services.ProductManagement;

namespace KatlaSport.Services.Tests
{
    public sealed class MapperInitializer
    {
        private static readonly  Lazy<MapperInitializer> _mapper = new Lazy<MapperInitializer>(() => new MapperInitializer());

        private MapperInitializer()
        {
            Mapper.Reset();
            Mapper.Initialize(config =>
            {
                config.AddProfile<HiveManagementMappingProfile>();
                config.AddProfile<ProductManagementMappingProfile>();
            });
        }

        public static MapperInitializer Instance => _mapper.Value;
    }
}