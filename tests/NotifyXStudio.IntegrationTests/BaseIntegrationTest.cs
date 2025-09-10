using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace NotifyXStudio.IntegrationTests
{
    /// <summary>
    /// Base class for integration tests with containerized dependencies.
    /// </summary>
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly PostgreSqlContainer _postgresContainer;
        protected readonly KafkaContainer _kafkaContainer;
        protected readonly RedisContainer _redisContainer;
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;

        protected BaseIntegrationTest()
        {
            // Initialize test containers
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15")
                .WithDatabase("notifyxstudio_test")
                .WithUsername("test")
                .WithPassword("test")
                .WithPortBinding(5432, true)
                .Build();

            _kafkaContainer = new KafkaBuilder()
                .WithImage("confluentinc/cp-kafka:latest")
                .WithPortBinding(9092, true)
                .WithPortBinding(9093, true)
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "zookeeper:2181")
                .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9092")
                .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
                .Build();

            _redisContainer = new RedisBuilder()
                .WithImage("redis:7")
                .WithPortBinding(6379, true)
                .Build();

            // Create web application factory
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Configure test services
                        ConfigureTestServices(services);
                    });
                });

            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            // Start containers
            await _postgresContainer.StartAsync();
            await _kafkaContainer.StartAsync();
            await _redisContainer.StartAsync();

            // Wait for containers to be ready
            await WaitForContainersToBeReady();

            // Configure application with container connection strings
            ConfigureApplication();
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();

            await _postgresContainer.StopAsync();
            await _kafkaContainer.StopAsync();
            await _redisContainer.StopAsync();
        }

        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
            // Override services for testing
            // This can be overridden in derived test classes
        }

        protected virtual void ConfigureApplication()
        {
            // Configure application with test container connection strings
            // This can be overridden in derived test classes
        }

        private async Task WaitForContainersToBeReady()
        {
            // Wait for PostgreSQL
            var maxRetries = 30;
            var retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    using var connection = new Npgsql.NpgsqlConnection(_postgresContainer.GetConnectionString());
                    await connection.OpenAsync();
                    break;
                }
                catch
                {
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            if (retryCount >= maxRetries)
            {
                throw new InvalidOperationException("PostgreSQL container failed to start");
            }

            // Wait for Redis
            retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    using var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
                    if (connection.IsConnected)
                    {
                        break;
                    }
                }
                catch
                {
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            if (retryCount >= maxRetries)
            {
                throw new InvalidOperationException("Redis container failed to start");
            }

            // Wait for Kafka
            retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    // Simple Kafka readiness check
                    await Task.Delay(5000); // Kafka needs time to start
                    break;
                }
                catch
                {
                    retryCount++;
                    await Task.Delay(1000);
                }
            }
        }

        protected string GetPostgresConnectionString()
        {
            return _postgresContainer.GetConnectionString();
        }

        protected string GetKafkaBootstrapServers()
        {
            return $"{_kafkaContainer.Hostname}:{_kafkaContainer.GetMappedPublicPort(9092)}";
        }

        protected string GetRedisConnectionString()
        {
            return _redisContainer.GetConnectionString();
        }

        protected T GetService<T>() where T : notnull
        {
            return _factory.Services.GetRequiredService<T>();
        }

        protected IServiceScope CreateScope()
        {
            return _factory.Services.CreateScope();
        }
    }
}