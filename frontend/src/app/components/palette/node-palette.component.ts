import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ConnectorService } from '../../services/connector.service';
import { ConnectorManifest } from '../../models/node.model';

@Component({
  selector: 'app-node-palette',
  templateUrl: './node-palette.component.html',
  styleUrls: ['./node-palette.component.css']
})
export class NodePaletteComponent implements OnInit, OnDestroy {
  connectors: ConnectorManifest[] = [];
  filteredConnectors: ConnectorManifest[] = [];
  categories: string[] = [];
  selectedCategory: string | null = null;
  searchTerm = '';
  isLoading = false;

  private destroy$ = new Subject<void>();

  constructor(private connectorService: ConnectorService) {}

  ngOnInit(): void {
    this.loadConnectors();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Loads all available connectors
   */
  private loadConnectors(): void {
    this.isLoading = true;
    
    this.connectorService.getConnectors()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (connectors) => {
          this.connectors = connectors;
          this.filteredConnectors = connectors;
          this.categories = this.extractCategories(connectors);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load connectors:', error);
          this.isLoading = false;
        }
      });
  }

  /**
   * Extracts unique categories from connectors
   */
  private extractCategories(connectors: ConnectorManifest[]): string[] {
    const categories = new Set(connectors.map(c => c.category));
    return Array.from(categories).sort();
  }

  /**
   * Filters connectors based on search term and category
   */
  filterConnectors(): void {
    let filtered = this.connectors;

    // Filter by category
    if (this.selectedCategory) {
      filtered = filtered.filter(c => c.category === this.selectedCategory);
    }

    // Filter by search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(c => 
        c.name.toLowerCase().includes(term) ||
        c.description.toLowerCase().includes(term) ||
        c.metadata.tags.some(tag => tag.toLowerCase().includes(term))
      );
    }

    this.filteredConnectors = filtered;
  }

  /**
   * Handles search input changes
   */
  onSearchChange(): void {
    this.filterConnectors();
  }

  /**
   * Handles category selection
   */
  onCategorySelect(category: string | null): void {
    this.selectedCategory = category;
    this.filterConnectors();
  }

  /**
   * Handles connector drag start
   */
  onConnectorDragStart(connector: ConnectorManifest, event: DragEvent): void {
    if (event.dataTransfer) {
      event.dataTransfer.setData('text/plain', connector.id);
      event.dataTransfer.effectAllowed = 'copy';
    }
  }

  /**
   * Gets the icon for a connector type
   */
  getConnectorIcon(connector: ConnectorManifest): string {
    const typeIcons: Record<string, string> = {
      'trigger': 'play_arrow',
      'action': 'play_circle',
      'transform': 'transform',
      'notifyx': 'notifications',
      'http': 'http',
      'slack': 'chat',
      'email': 'email',
      'sms': 'sms',
      'database': 'storage',
      'webhook': 'webhook'
    };

    // Check for specific connector types first
    if (connector.id.includes('notifyx')) return 'notifications';
    if (connector.id.includes('http')) return 'http';
    if (connector.id.includes('slack')) return 'chat';
    if (connector.id.includes('email')) return 'email';
    if (connector.id.includes('sms')) return 'sms';
    if (connector.id.includes('database')) return 'storage';
    if (connector.id.includes('webhook')) return 'webhook';

    // Fall back to type-based icons
    return typeIcons[connector.type] || 'extension';
  }

  /**
   * Gets the color for a connector
   */
  getConnectorColor(connector: ConnectorManifest): string {
    return connector.ui.color || '#4F46E5';
  }

  /**
   * Gets connectors grouped by category
   */
  getConnectorsByCategory(): { category: string; connectors: ConnectorManifest[] }[] {
    const grouped = new Map<string, ConnectorManifest[]>();
    
    this.filteredConnectors.forEach(connector => {
      if (!grouped.has(connector.category)) {
        grouped.set(connector.category, []);
      }
      grouped.get(connector.category)!.push(connector);
    });

    return Array.from(grouped.entries())
      .map(([category, connectors]) => ({ category, connectors }))
      .sort((a, b) => a.category.localeCompare(b.category));
  }

  /**
   * Clears the search and filters
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = null;
    this.filterConnectors();
  }

  /**
   * Gets the total number of connectors
   */
  getTotalConnectors(): number {
    return this.connectors.length;
  }

  /**
   * Gets the number of filtered connectors
   */
  getFilteredCount(): number {
    return this.filteredConnectors.length;
  }
}