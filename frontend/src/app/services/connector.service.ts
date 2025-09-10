import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ConnectorManifest } from '../models/node.model';

@Injectable({
  providedIn: 'root'
})
export class ConnectorService {
  private readonly apiUrl = '/api/connectors';
  private connectorsSubject = new BehaviorSubject<ConnectorManifest[]>([]);
  public connectors$ = this.connectorsSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Gets all available connectors
   */
  getConnectors(
    category?: string,
    search?: string,
    tags?: string[]
  ): Observable<ConnectorManifest[]> {
    let params = new HttpParams();

    if (category) {
      params = params.set('category', category);
    }

    if (search) {
      params = params.set('search', search);
    }

    if (tags && tags.length > 0) {
      tags.forEach(tag => {
        params = params.append('tags', tag);
      });
    }

    return this.http.get<ConnectorManifest[]>(this.apiUrl, { params }).pipe(
      tap(connectors => this.connectorsSubject.next(connectors))
    );
  }

  /**
   * Gets a specific connector by ID
   */
  getConnector(connectorId: string): Observable<ConnectorManifest> {
    return this.http.get<ConnectorManifest>(`${this.apiUrl}/${connectorId}`);
  }

  /**
   * Gets connector categories
   */
  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  /**
   * Gets available versions for a connector
   */
  getConnectorVersions(connectorId: string): Observable<ConnectorVersion[]> {
    return this.http.get<ConnectorVersion[]>(`${this.apiUrl}/${connectorId}/versions`);
  }

  /**
   * Validates a connector manifest
   */
  validateManifest(manifest: ConnectorManifest): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, manifest);
  }

  /**
   * Tests a connector configuration
   */
  testConnector(connectorId: string, config: any, credentials?: any): Observable<ConnectorTestResult> {
    return this.http.post<ConnectorTestResult>(`${this.apiUrl}/${connectorId}/test`, {
      config,
      credentials
    });
  }

  /**
   * Resolves connector dependencies
   */
  resolveDependencies(
    requestedConnectors: DependencySpec[],
    strategy: ResolutionStrategy = ResolutionStrategy.HighestCompatible,
    lockfile?: Record<string, string>
  ): Observable<ResolutionResult> {
    return this.http.post<ResolutionResult>(`${this.apiUrl}/resolve`, {
      requestedConnectors,
      resolutionStrategy: strategy,
      lockfile
    });
  }

  /**
   * Gets connectors by category
   */
  getConnectorsByCategory(category: string): Observable<ConnectorManifest[]> {
    return this.getConnectors(category);
  }

  /**
   * Searches connectors
   */
  searchConnectors(searchTerm: string): Observable<ConnectorManifest[]> {
    return this.getConnectors(undefined, searchTerm);
  }

  /**
   * Gets connectors by tags
   */
  getConnectorsByTags(tags: string[]): Observable<ConnectorManifest[]> {
    return this.getConnectors(undefined, undefined, tags);
  }

  /**
   * Gets the current connectors list
   */
  getCurrentConnectors(): ConnectorManifest[] {
    return this.connectorsSubject.value;
  }

  /**
   * Refreshes the connectors list
   */
  refreshConnectors(): Observable<ConnectorManifest[]> {
    return this.getConnectors();
  }

  /**
   * Gets connectors grouped by category
   */
  getConnectorsGroupedByCategory(): Observable<Record<string, ConnectorManifest[]>> {
    return this.getConnectors().pipe(
      map(connectors => {
        const grouped: Record<string, ConnectorManifest[]> = {};
        connectors.forEach(connector => {
          if (!grouped[connector.category]) {
            grouped[connector.category] = [];
          }
          grouped[connector.category].push(connector);
        });
        return grouped;
      })
    );
  }

  /**
   * Gets popular connectors (most used)
   */
  getPopularConnectors(limit: number = 10): Observable<ConnectorManifest[]> {
    // This would typically come from analytics data
    // For now, return a subset of connectors
    return this.getConnectors().pipe(
      map(connectors => connectors.slice(0, limit))
    );
  }

  /**
   * Gets recently added connectors
   */
  getRecentConnectors(limit: number = 10): Observable<ConnectorManifest[]> {
    // This would typically come from a timestamp field
    // For now, return a subset of connectors
    return this.getConnectors().pipe(
      map(connectors => connectors.slice(-limit).reverse())
    );
  }

  /**
   * Gets connectors by type
   */
  getConnectorsByType(type: 'trigger' | 'action' | 'transform'): Observable<ConnectorManifest[]> {
    return this.getConnectors().pipe(
      map(connectors => connectors.filter(c => c.type === type))
    );
  }

  /**
   * Gets NotifyX-specific connectors
   */
  getNotifyXConnectors(): Observable<ConnectorManifest[]> {
    return this.getConnectors().pipe(
      map(connectors => connectors.filter(c => c.id.includes('notifyx')))
    );
  }

  /**
   * Gets third-party connectors
   */
  getThirdPartyConnectors(): Observable<ConnectorManifest[]> {
    return this.getConnectors().pipe(
      map(connectors => connectors.filter(c => !c.id.includes('notifyx')))
    );
  }
}

// Supporting interfaces
export interface ConnectorVersion {
  version: string;
  publishedAt: Date;
  description: string;
  changes: string[];
  isLatest: boolean;
  isStable: boolean;
}

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
}

export interface ConnectorTestResult {
  success: boolean;
  errorMessage?: string;
  output?: any;
  durationMs: number;
}

export interface DependencySpec {
  connectorId: string;
  versionRange: string;
}

export enum ResolutionStrategy {
  HighestCompatible = 'highestCompatible',
  PreferStable = 'preferStable',
  FailFast = 'failFast'
}

export interface ResolutionResult {
  success: boolean;
  resolvedVersions: Record<string, string>;
  errorMessage?: string;
}