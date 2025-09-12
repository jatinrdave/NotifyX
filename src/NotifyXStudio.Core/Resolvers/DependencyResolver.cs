using System;
using System.Collections.Generic;
using System.Linq;
using Semver;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Resolvers
{
    /// <summary>
    /// Resolves connector dependencies using semantic versioning and conflict resolution rules.
    /// </summary>
    public class DependencyResolver
    {
        private readonly ConnectorRegistry _registry;
        private readonly ResolutionStrategy _strategy;

        public DependencyResolver(ConnectorRegistry registry, ResolutionStrategy strategy = ResolutionStrategy.HighestCompatible)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _strategy = strategy;
        }

        /// <summary>
        /// Resolves dependencies for the requested connectors.
        /// </summary>
        public ResolutionResult Resolve(IEnumerable<DependencySpec> requested, Dictionary<string, string>? lockfile = null)
        {
            if (requested == null) throw new ArgumentNullException(nameof(requested));

            try
            {
                // Build initial constraints map
                var constraints = new Dictionary<string, List<SemVersionRange>>();
                
                void AddConstraint(string id, string rangeStr)
                {
                    if (!constraints.TryGetValue(id, out var list))
                    {
                        list = new List<SemVersionRange>();
                        constraints[id] = list;
                    }
                    list.Add(ParseRange(rangeStr));
                }

                // Add requested constraints
                foreach (var req in requested)
                {
                    AddConstraint(req.ConnectorId, req.VersionRange);
                }

                // Add lockfile constraints if provided
                if (lockfile != null)
                {
                    foreach (var kv in lockfile)
                    {
                        AddConstraint(kv.Key, $"={kv.Value}");
                    }
                }

                // Build candidates map
                var candidates = BuildCandidatesMap();

                // Attempt resolution
                var initialSelection = new Dictionary<string, string>();
                var result = TryResolve(initialSelection, constraints, candidates, out var finalSelection);

                if (!result)
                {
                    return new ResolutionResult
                    {
                        Success = false,
                        ErrorMessage = "Unable to resolve dependencies. See diagnostics for details."
                    };
                }

                return new ResolutionResult
                {
                    Success = true,
                    ResolvedVersions = finalSelection
                };
            }
            catch (Exception ex)
            {
                return new ResolutionResult
                {
                    Success = false,
                    ErrorMessage = $"Resolution failed: {ex.Message}"
                };
            }
        }

        private Dictionary<string, List<ConnectorDescriptor>> BuildCandidatesMap()
        {
            var candidates = new Dictionary<string, List<ConnectorDescriptor>>();

            foreach (var entry in _registry.Connectors)
            {
                if (!candidates.TryGetValue(entry.Id, out var list))
                {
                    list = new List<ConnectorDescriptor>();
                    candidates[entry.Id] = list;
                }

                var descriptor = new ConnectorDescriptor
                {
                    Id = entry.Id,
                    Version = entry.Version,
                    PeerConnectors = entry.Dependencies.Peer.Select(p => new DependencySpec
                    {
                        ConnectorId = p.Split('@')[0],
                        VersionRange = p.Contains('@') ? p.Split('@')[1] : "*"
                    }).ToList(),
                    Dependencies = new DependencyCollection
                    {
                        Nuget = entry.Dependencies.Runtime.Nuget,
                        Npm = entry.Dependencies.Runtime.Npm,
                        Apis = entry.Dependencies.Apis,
                        Connectors = new List<DependencySpec>() // Will be populated from manifest
                    },
                    ConflictRules = new ConflictRules
                    {
                        IncompatibleWith = entry.ConflictRules.IncompatibleWith,
                        PreferVersion = entry.ConflictRules.PreferVersion,
                        ResolutionStrategy = entry.ConflictRules.ResolutionStrategy.ToString()
                    }
                };

                list.Add(descriptor);
            }

            // Sort candidates by strategy
            foreach (var kv in candidates)
            {
                var sorted = kv.Value
                    .OrderByDescending(d => SemVersion.Parse(d.Version, SemVersionStyles.Strict))
                    .ToList();

                if (_strategy == ResolutionStrategy.PreferStable)
                {
                    sorted = sorted
                        .OrderBy(d => IsPreRelease(d.Version) ? 1 : 0)
                        .ThenByDescending(d => SemVersion.Parse(d.Version))
                        .ToList();
                }

                candidates[kv.Key] = sorted;
            }

            return candidates;
        }

        private bool TryResolve(
            Dictionary<string, string> selection,
            Dictionary<string, List<SemVersionRange>> constraints,
            Dictionary<string, List<ConnectorDescriptor>> candidates,
            out Dictionary<string, string> finalSelection)
        {
            finalSelection = null!;

            // Find unresolved connectors with constraints
            var toResolve = constraints.Keys.Where(k => !selection.ContainsKey(k)).ToList();
            
            if (!toResolve.Any())
            {
                finalSelection = new Dictionary<string, string>(selection);
                return true;
            }

            // Heuristic: pick connector with fewest candidate versions satisfying current constraints
            var pick = toResolve
                .OrderBy(k => CountSatisfyingCandidates(k, constraints, candidates))
                .ThenBy(k => k)
                .First();

            var allowed = candidates.ContainsKey(pick) ? candidates[pick] : new List<ConnectorDescriptor>();
            var satisfying = allowed.Where(d => SatisfiesAllConstraints(d, constraints)).ToList();

            foreach (var cand in satisfying)
            {
                // Check conflict rules against already selected connectors
                if (ConflictsWithSelection(cand, selection)) continue;

                // Prepare new selection and propagate new constraints
                selection[pick] = cand.Version;
                var savedConstraints = CloneConstraints(constraints);

                // Add peer connector constraints and direct connector dependencies
                foreach (var peer in cand.PeerConnectors ?? Enumerable.Empty<DependencySpec>())
                {
                    AddSemverConstraint(constraints, peer.ConnectorId, peer.VersionRange);
                }

                foreach (var dep in cand.Dependencies?.Connectors ?? Enumerable.Empty<DependencySpec>())
                {
                    AddSemverConstraint(constraints, dep.ConnectorId, dep.VersionRange);
                }

                // Quick unsatisfiable check
                if (AnyUnsatisfiable(constraints, candidates))
                {
                    // Rollback and continue
                    constraints = savedConstraints;
                    selection.Remove(pick);
                    continue;
                }

                if (TryResolve(selection, constraints, candidates, out finalSelection))
                {
                    return true;
                }

                // Rollback
                constraints = savedConstraints;
                selection.Remove(pick);
            }

            return false;
        }

        private static SemVersionRange ParseRange(string rangeStr)
        {
            // Normalize common forms, accept exact "=" as specific
            if (rangeStr.StartsWith("="))
            {
                return SemVersionRange.Parse(rangeStr.Substring(1));
            }
            return SemVersionRange.Parse(rangeStr);
        }

        private static bool IsPreRelease(string version)
        {
            return SemVersion.Parse(version).IsPrerelease;
        }

        private static bool SatisfiesAllConstraints(ConnectorDescriptor d, Dictionary<string, List<SemVersionRange>> constraints)
        {
            if (!constraints.TryGetValue(d.Id, out var ranges)) return true;
            
            var version = SemVersion.Parse(d.Version);
            return ranges.All(r => r.Contains(version));
        }

        private static void AddSemverConstraint(Dictionary<string, List<SemVersionRange>> constraints, string id, string range)
        {
            var r = ParseRange(range);
            if (!constraints.TryGetValue(id, out var list))
            {
                list = new List<SemVersionRange>();
                constraints[id] = list;
            }
            list.Add(r);
        }

        private bool ConflictsWithSelection(ConnectorDescriptor cand, Dictionary<string, string> selection)
        {
            // Check conflict rules against already selected connectors
            foreach (var incomp in cand.ConflictRules?.IncompatibleWith ?? Enumerable.Empty<string>())
            {
                // Parse pattern like "notifyx.legacy@<1.0.0"
                var parts = incomp.Split('@', 2);
                var id = parts[0];
                var rng = parts.Length > 1 ? parts[1] : null;
                
                if (selection.TryGetValue(id, out var selectedVer) && rng != null)
                {
                    var range = ParseRange(rng);
                    if (range.Contains(SemVersion.Parse(selectedVer)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool AnyUnsatisfiable(Dictionary<string, List<SemVersionRange>> constraints, Dictionary<string, List<ConnectorDescriptor>> candidates)
        {
            foreach (var kv in constraints)
            {
                var id = kv.Key;
                var ranges = kv.Value;
                
                if (!candidates.ContainsKey(id)) return true; // No versions available
                
                var any = candidates[id].Any(desc => ranges.All(r => r.Contains(SemVersion.Parse(desc.Version))));
                if (!any) return true;
            }
            return false;
        }

        private static Dictionary<string, List<SemVersionRange>> CloneConstraints(Dictionary<string, List<SemVersionRange>> src)
        {
            return src.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        }

        private int CountSatisfyingCandidates(string connectorId, Dictionary<string, List<SemVersionRange>> constraints, Dictionary<string, List<ConnectorDescriptor>> candidates)
        {
            if (!candidates.ContainsKey(connectorId)) return 0;
            
            var cand = candidates[connectorId];
            if (!constraints.TryGetValue(connectorId, out var ranges)) return cand.Count;
            
            return cand.Count(d => ranges.All(r => r.Contains(SemVersion.Parse(d.Version))));
        }
    }

    /// <summary>
    /// Result of dependency resolution.
    /// </summary>
    public class ResolutionResult
    {
        /// <summary>Whether resolution was successful.</summary>
        public bool Success { get; init; }

        /// <summary>Resolved versions for each connector.</summary>
        public Dictionary<string, string> ResolvedVersions { get; init; } = new();

        /// <summary>Error message if resolution failed.</summary>
        public string ErrorMessage { get; init; } = string.Empty;
    }

    /// <summary>
    /// Dependency specification for resolution.
    /// </summary>
    public class DependencySpec
    {
        /// <summary>Connector identifier.</summary>
        public string ConnectorId { get; init; } = string.Empty;

        /// <summary>Version range constraint.</summary>
        public string VersionRange { get; init; } = string.Empty;
    }

    /// <summary>
    /// Connector descriptor used during resolution.
    /// </summary>
    public class ConnectorDescriptor
    {
        /// <summary>Connector identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Connector version.</summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>Peer connector dependencies.</summary>
        public List<DependencySpec> PeerConnectors { get; init; } = new();

        /// <summary>Runtime dependencies.</summary>
        public DependencyCollection Dependencies { get; init; } = new();

        /// <summary>Conflict resolution rules.</summary>
        public ConflictRules ConflictRules { get; init; } = new();
    }

    /// <summary>
    /// Dependency collection for a connector.
    /// </summary>
    public class DependencyCollection
    {
        /// <summary>NuGet package dependency.</summary>
        public NuGetDependency? Nuget { get; init; }

        /// <summary>NPM package dependency.</summary>
        public NpmDependency? Npm { get; init; }

        /// <summary>External API dependencies.</summary>
        public List<ApiDependency> Apis { get; init; } = new();

        /// <summary>Direct connector dependencies.</summary>
        public List<DependencySpec> Connectors { get; init; } = new();
    }

    /// <summary>
    /// Conflict resolution rules.
    /// </summary>
    public class ConflictRules
    {
        /// <summary>Incompatible connector versions.</summary>
        public List<string> IncompatibleWith { get; init; } = new();

        /// <summary>Preferred versions for dependencies.</summary>
        public Dictionary<string, string> PreferVersion { get; init; } = new();

        /// <summary>Resolution strategy to use.</summary>
        public string ResolutionStrategy { get; init; } = "highestCompatible";
    }
}