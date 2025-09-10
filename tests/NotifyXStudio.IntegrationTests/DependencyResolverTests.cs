using FluentAssertions;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Resolvers;
using Xunit;

namespace NotifyXStudio.IntegrationTests
{
    /// <summary>
    /// Integration tests for the dependency resolver.
    /// </summary>
    public class DependencyResolverTests : BaseIntegrationTest
    {
        [Fact]
        public void Resolve_WithSimpleDependencies_ShouldSucceed()
        {
            // Arrange
            var registry = CreateTestRegistry();
            var resolver = new DependencyResolver(registry, ResolutionStrategy.HighestCompatible);
            var requested = new List<DependencySpec>
            {
                new DependencySpec { ConnectorId = "notifyx.sendNotification", VersionRange = "^1.0.0" }
            };

            // Act
            var result = resolver.Resolve(requested);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResolvedVersions.Should().ContainKey("notifyx.sendNotification");
            result.ResolvedVersions["notifyx.sendNotification"].Should().Be("1.0.0");
        }

        [Fact]
        public void Resolve_WithPeerDependencies_ShouldSucceed()
        {
            // Arrange
            var registry = CreateTestRegistryWithDependencies();
            var resolver = new DependencyResolver(registry, ResolutionStrategy.HighestCompatible);
            var requested = new List<DependencySpec>
            {
                new DependencySpec { ConnectorId = "notifyx.slackIntegration", VersionRange = "^1.0.0" }
            };

            // Act
            var result = resolver.Resolve(requested);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResolvedVersions.Should().ContainKey("notifyx.slackIntegration");
            result.ResolvedVersions.Should().ContainKey("notifyx.sendNotification");
            result.ResolvedVersions["notifyx.slackIntegration"].Should().Be("1.0.0");
            result.ResolvedVersions["notifyx.sendNotification"].Should().Be("1.2.3");
        }

        [Fact]
        public void Resolve_WithConflictingDependencies_ShouldFail()
        {
            // Arrange
            var registry = CreateTestRegistryWithConflicts();
            var resolver = new DependencyResolver(registry, ResolutionStrategy.FailFast);
            var requested = new List<DependencySpec>
            {
                new DependencySpec { ConnectorId = "notifyx.sendNotification", VersionRange = "^1.0.0" },
                new DependencySpec { ConnectorId = "notifyx.legacyEmailConnector", VersionRange = "^0.9.0" }
            };

            // Act
            var result = resolver.Resolve(requested);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Resolve_WithLockfile_ShouldUseLockedVersions()
        {
            // Arrange
            var registry = CreateTestRegistry();
            var resolver = new DependencyResolver(registry, ResolutionStrategy.HighestCompatible);
            var requested = new List<DependencySpec>
            {
                new DependencySpec { ConnectorId = "notifyx.sendNotification", VersionRange = "^1.0.0" }
            };
            var lockfile = new Dictionary<string, string>
            {
                ["notifyx.sendNotification"] = "1.0.0"
            };

            // Act
            var result = resolver.Resolve(requested, lockfile);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResolvedVersions["notifyx.sendNotification"].Should().Be("1.0.0");
        }

        [Fact]
        public void Resolve_WithPreferStableStrategy_ShouldPreferStableVersions()
        {
            // Arrange
            var registry = CreateTestRegistryWithPreReleases();
            var resolver = new DependencyResolver(registry, ResolutionStrategy.PreferStable);
            var requested = new List<DependencySpec>
            {
                new DependencySpec { ConnectorId = "notifyx.sendNotification", VersionRange = "^1.0.0" }
            };

            // Act
            var result = resolver.Resolve(requested);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ResolvedVersions["notifyx.sendNotification"].Should().Be("1.0.0"); // Should prefer stable over pre-release
        }

        private ConnectorRegistry CreateTestRegistry()
        {
            return new ConnectorRegistry
            {
                RegistryVersion = "1.2.0",
                LastUpdated = DateTime.UtcNow,
                Connectors = new List<ConnectorRegistryEntry>
                {
                    new ConnectorRegistryEntry
                    {
                        Id = "notifyx.sendNotification",
                        Name = "Send Notification",
                        Version = "1.0.0",
                        Category = "Notification",
                        ManifestUrl = "https://cdn.notifyx.dev/manifests/sendNotification.json",
                        Icon = "https://cdn.notifyx.dev/icons/send.svg",
                        Tags = new List<string> { "notifyx", "action", "communication" },
                        Author = new ConnectorAuthor { Name = "NotifyX Team", Url = "https://notifyx.dev" },
                        Compatibility = new ConnectorCompatibility
                        {
                            Frontend = new List<string> { "angular-18+" },
                            Backend = new List<string> { ".net-9+" },
                            NotifyXCore = new List<string> { "1.0.0+" }
                        },
                        Dependencies = new ConnectorDependencies
                        {
                            Runtime = new ConnectorRuntimeDependencies
                            {
                                Nuget = new NuGetDependency { PackageName = "NotifyX.SDK", Version = "^1.0.0" }
                            },
                            Peer = new List<string>(),
                            Apis = new List<ApiDependency>()
                        },
                        ConflictRules = new ConnectorConflictRules
                        {
                            IncompatibleWith = new List<string>(),
                            PreferVersion = new Dictionary<string, string>(),
                            ResolutionStrategy = ResolutionStrategy.HighestCompatible
                        }
                    }
                }
            };
        }

        private ConnectorRegistry CreateTestRegistryWithDependencies()
        {
            var registry = CreateTestRegistry();
            
            // Add a newer version of sendNotification
            registry.Connectors.Add(new ConnectorRegistryEntry
            {
                Id = "notifyx.sendNotification",
                Name = "Send Notification",
                Version = "1.2.3",
                Category = "Notification",
                ManifestUrl = "https://cdn.notifyx.dev/manifests/sendNotification.json",
                Icon = "https://cdn.notifyx.dev/icons/send.svg",
                Tags = new List<string> { "notifyx", "action", "communication" },
                Author = new ConnectorAuthor { Name = "NotifyX Team", Url = "https://notifyx.dev" },
                Compatibility = new ConnectorCompatibility
                {
                    Frontend = new List<string> { "angular-18+" },
                    Backend = new List<string> { ".net-9+" },
                    NotifyXCore = new List<string> { "1.2.0+" }
                },
                Dependencies = new ConnectorDependencies
                {
                    Runtime = new ConnectorRuntimeDependencies
                    {
                        Nuget = new NuGetDependency { PackageName = "NotifyX.SDK", Version = "^1.2.0" }
                    },
                    Peer = new List<string>(),
                    Apis = new List<ApiDependency>()
                },
                ConflictRules = new ConnectorConflictRules
                {
                    IncompatibleWith = new List<string>(),
                    PreferVersion = new Dictionary<string, string>(),
                    ResolutionStrategy = ResolutionStrategy.HighestCompatible
                }
            });

            // Add slack integration that depends on sendNotification
            registry.Connectors.Add(new ConnectorRegistryEntry
            {
                Id = "notifyx.slackIntegration",
                Name = "Slack Integration",
                Version = "1.0.0",
                Category = "Integration",
                ManifestUrl = "https://cdn.notifyx.dev/manifests/slackIntegration.json",
                Icon = "https://cdn.notifyx.dev/icons/slack.svg",
                Tags = new List<string> { "notifyx", "integration", "slack" },
                Author = new ConnectorAuthor { Name = "NotifyX Team", Url = "https://notifyx.dev" },
                Compatibility = new ConnectorCompatibility
                {
                    Frontend = new List<string> { "angular-18+" },
                    Backend = new List<string> { ".net-9+" },
                    NotifyXCore = new List<string> { "1.0.0+" }
                },
                Dependencies = new ConnectorDependencies
                {
                    Runtime = new ConnectorRuntimeDependencies
                    {
                        Npm = new NpmDependency { PackageName = "@slack/web-api", Version = "^7.0.0" }
                    },
                    Peer = new List<string> { "notifyx.sendNotification@^1.2.0" },
                    Apis = new List<ApiDependency>
                    {
                        new ApiDependency { Name = "Slack API", Url = "https://api.slack.com", AuthType = "oauth2" }
                    }
                },
                ConflictRules = new ConnectorConflictRules
                {
                    IncompatibleWith = new List<string>(),
                    PreferVersion = new Dictionary<string, string>(),
                    ResolutionStrategy = ResolutionStrategy.PreferStable
                }
            });

            return registry;
        }

        private ConnectorRegistry CreateTestRegistryWithConflicts()
        {
            var registry = CreateTestRegistry();
            
            // Add legacy email connector that conflicts with sendNotification
            registry.Connectors.Add(new ConnectorRegistryEntry
            {
                Id = "notifyx.legacyEmailConnector",
                Name = "Legacy Email Connector",
                Version = "0.9.0",
                Category = "Notification",
                ManifestUrl = "https://cdn.notifyx.dev/manifests/legacyEmailConnector.json",
                Icon = "https://cdn.notifyx.dev/icons/email.svg",
                Tags = new List<string> { "notifyx", "legacy", "email" },
                Author = new ConnectorAuthor { Name = "NotifyX Team", Url = "https://notifyx.dev" },
                Compatibility = new ConnectorCompatibility
                {
                    Frontend = new List<string> { "angular-18+" },
                    Backend = new List<string> { ".net-9+" },
                    NotifyXCore = new List<string> { "0.9.0+" }
                },
                Dependencies = new ConnectorDependencies
                {
                    Runtime = new ConnectorRuntimeDependencies
                    {
                        Nuget = new NuGetDependency { PackageName = "NotifyX.SDK", Version = "^0.9.0" }
                    },
                    Peer = new List<string>(),
                    Apis = new List<ApiDependency>()
                },
                ConflictRules = new ConnectorConflictRules
                {
                    IncompatibleWith = new List<string> { "notifyx.sendNotification@>=1.0.0" },
                    PreferVersion = new Dictionary<string, string>(),
                    ResolutionStrategy = ResolutionStrategy.FailFast
                }
            });

            return registry;
        }

        private ConnectorRegistry CreateTestRegistryWithPreReleases()
        {
            var registry = CreateTestRegistry();
            
            // Add pre-release version
            registry.Connectors.Add(new ConnectorRegistryEntry
            {
                Id = "notifyx.sendNotification",
                Name = "Send Notification",
                Version = "1.1.0-beta.1",
                Category = "Notification",
                ManifestUrl = "https://cdn.notifyx.dev/manifests/sendNotification.json",
                Icon = "https://cdn.notifyx.dev/icons/send.svg",
                Tags = new List<string> { "notifyx", "action", "communication" },
                Author = new ConnectorAuthor { Name = "NotifyX Team", Url = "https://notifyx.dev" },
                Compatibility = new ConnectorCompatibility
                {
                    Frontend = new List<string> { "angular-18+" },
                    Backend = new List<string> { ".net-9+" },
                    NotifyXCore = new List<string> { "1.1.0+" }
                },
                Dependencies = new ConnectorDependencies
                {
                    Runtime = new ConnectorRuntimeDependencies
                    {
                        Nuget = new NuGetDependency { PackageName = "NotifyX.SDK", Version = "^1.1.0" }
                    },
                    Peer = new List<string>(),
                    Apis = new List<ApiDependency>()
                },
                ConflictRules = new ConnectorConflictRules
                {
                    IncompatibleWith = new List<string>(),
                    PreferVersion = new Dictionary<string, string>(),
                    ResolutionStrategy = ResolutionStrategy.PreferStable
                }
            });

            return registry;
        }
    }
}