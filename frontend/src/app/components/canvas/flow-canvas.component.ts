import { Component, ElementRef, OnInit, ViewChild, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { WorkflowService } from '../../services/workflow.service';
import { ConnectorService } from '../../services/connector.service';
import { NodeModel, EdgeModel } from '../../models/node.model';
import { Subject, takeUntil } from 'rxjs';
import { DecimalPipe } from '@angular/common';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-flow-canvas',
  templateUrl: './flow-canvas.component.html',
  styleUrls: ['./flow-canvas.component.css'],
  standalone: true,
  imports: [DecimalPipe, CommonModule]
})
export class FlowCanvasComponent implements OnInit, OnDestroy {
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLElement>;
  @Input() workflow: any;
  @Output() workflowChange = new EventEmitter<any>();
  @Output() nodeSelected = new EventEmitter<NodeModel | null>();
  @Output() nodeDoubleClick = new EventEmitter<NodeModel>();

  nodes: NodeModel[] = [];
  edges: EdgeModel[] = [];
  selectedNode: NodeModel | null = null;
  isDragging = false;
  dragOffset = { x: 0, y: 0 };
  canvasOffset = { x: 0, y: 0 };
  zoom = 1;
  panOffset = { x: 0, y: 0 };

  private destroy$ = new Subject<void>();

  constructor(
    private workflowService: WorkflowService,
    private connectorService: ConnectorService
  ) {}

  ngOnInit(): void {
    if (this.workflow) {
      this.nodes = this.workflow.nodes || [];
      this.edges = this.workflow.edges || [];
    }

    // Subscribe to workflow changes
    this.workflowService.currentWorkflow$
      .pipe(takeUntil(this.destroy$))
      .subscribe(workflow => {
        if (workflow) {
          this.nodes = workflow.nodes || [];
          this.edges = workflow.edges || [];
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Handles dropping a node from the palette onto the canvas
   */
  onDropNode(event: DragEvent): void {
    event.preventDefault();
    
    const nodeType = event.dataTransfer?.getData('text/plain');
    if (!nodeType) return;

    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = (event.clientX - rect.left - this.panOffset.x) / this.zoom;
    const y = (event.clientY - rect.top - this.panOffset.y) / this.zoom;

    const node = this.workflowService.createNode(nodeType, { x, y });
    this.addNode(node);
  }

  /**
   * Handles drag over events for the canvas
   */
  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  /**
   * Adds a node to the workflow
   */
  addNode(node: NodeModel): void {
    this.nodes.push(node);
    this.emitWorkflowChange();
  }

  /**
   * Updates a node in the workflow
   */
  updateNode(nodeId: string, updates: Partial<NodeModel>): void {
    const nodeIndex = this.nodes.findIndex(n => n.id === nodeId);
    if (nodeIndex !== -1) {
      this.nodes[nodeIndex] = { ...this.nodes[nodeIndex], ...updates };
      this.emitWorkflowChange();
    }
  }

  /**
   * Removes a node from the workflow
   */
  removeNode(nodeId: string): void {
    this.nodes = this.nodes.filter(n => n.id !== nodeId);
    this.edges = this.edges.filter(e => e.from !== nodeId && e.to !== nodeId);
    this.emitWorkflowChange();
    
    if (this.selectedNode?.id === nodeId) {
      this.selectedNode = null;
      this.nodeSelected.emit(null);
    }
  }

  /**
   * Handles node click events
   */
  onNodeClick(node: NodeModel, event: MouseEvent): void {
    event.stopPropagation();
    this.selectedNode = node;
    this.nodeSelected.emit(node);
  }

  /**
   * Handles node double-click events
   */
  onNodeDoubleClick(node: NodeModel, event: MouseEvent): void {
    event.stopPropagation();
    this.nodeDoubleClick.emit(node);
  }

  /**
   * Handles canvas click events (deselect nodes)
   */
  onCanvasClick(): void {
    this.selectedNode = null;
    this.nodeSelected.emit(null);
  }

  /**
   * Handles node drag start
   */
  onNodeDragStart(node: NodeModel, event: MouseEvent): void {
    this.isDragging = true;
    this.selectedNode = node;
    this.nodeSelected.emit(node);
    
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.dragOffset = {
      x: event.clientX - rect.left - node.position.x * this.zoom,
      y: event.clientY - rect.top - node.position.y * this.zoom
    };
  }

  /**
   * Handles node drag
   */
  onNodeDrag(node: NodeModel, event: MouseEvent): void {
    if (!this.isDragging) return;

    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const newX = (event.clientX - rect.left - this.dragOffset.x - this.panOffset.x) / this.zoom;
    const newY = (event.clientY - rect.top - this.dragOffset.y - this.panOffset.y) / this.zoom;

    this.updateNode(node.id, { position: { x: newX, y: newY } });
  }

  /**
   * Handles node drag end
   */
  onNodeDragEnd(): void {
    this.isDragging = false;
  }

  /**
   * Handles canvas pan start
   */
  onPanStart(event: MouseEvent): void {
    if (this.isDragging) return;
    
    this.canvasOffset = { x: event.clientX, y: event.clientY };
  }

  /**
   * Handles canvas pan
   */
  onPan(event: MouseEvent): void {
    if (this.isDragging) return;

    const deltaX = event.clientX - this.canvasOffset.x;
    const deltaY = event.clientY - this.canvasOffset.y;

    this.panOffset.x += deltaX;
    this.panOffset.y += deltaY;

    this.canvasOffset = { x: event.clientX, y: event.clientY };
  }

  /**
   * Handles canvas zoom
   */
  onZoom(event: WheelEvent): void {
    event.preventDefault();
    
    const delta = event.deltaY > 0 ? 0.9 : 1.1;
    this.zoom = Math.max(0.1, Math.min(3, this.zoom * delta));
  }

  /**
   * Centers the canvas view
   */
  centerView(): void {
    this.panOffset = { x: 0, y: 0 };
    this.zoom = 1;
  }

  /**
   * Fits all nodes in view
   */
  fitToView(): void {
    if (this.nodes.length === 0) return;

    const bounds = this.getNodesBounds();
    const canvasRect = this.canvasRef.nativeElement.getBoundingClientRect();
    
    const scaleX = canvasRect.width / (bounds.width + 100);
    const scaleY = canvasRect.height / (bounds.height + 100);
    this.zoom = Math.min(scaleX, scaleY, 1);

    this.panOffset = {
      x: (canvasRect.width - bounds.width * this.zoom) / 2 - bounds.x * this.zoom,
      y: (canvasRect.height - bounds.height * this.zoom) / 2 - bounds.y * this.zoom
    };
  }

  /**
   * Gets the bounding box of all nodes
   */
  private getNodesBounds(): { x: number; y: number; width: number; height: number } {
    if (this.nodes.length === 0) {
      return { x: 0, y: 0, width: 0, height: 0 };
    }

    const xs = this.nodes.map(n => n.position.x);
    const ys = this.nodes.map(n => n.position.y);
    
    const minX = Math.min(...xs);
    const minY = Math.min(...ys);
    const maxX = Math.max(...xs);
    const maxY = Math.max(...ys);

    return {
      x: minX,
      y: minY,
      width: maxX - minX + 200, // Add node width
      height: maxY - minY + 100  // Add node height
    };
  }

  /**
   * Emits workflow change event
   */
  private emitWorkflowChange(): void {
    const workflow = {
      ...this.workflow,
      nodes: this.nodes,
      edges: this.edges
    };
    this.workflowChange.emit(workflow);
  }

  /**
   * Gets the transform style for the canvas
   */
  getCanvasTransform(): string {
    return `translate(${this.panOffset.x}px, ${this.panOffset.y}px) scale(${this.zoom})`;
  }

  /**
   * Gets the transform style for a node
   */
  getNodeTransform(node: NodeModel): string {
    return `translate(${node.position.x}px, ${node.position.y}px)`;
  }

  /**
   * Gets the path for an edge
   */
  getEdgePath(edge: EdgeModel): string {
    const fromNode = this.nodes.find(n => n.id === edge.from);
    const toNode = this.nodes.find(n => n.id === edge.to);
    
    if (!fromNode || !toNode) return '';
    
    const fromX = fromNode.position.x + 100; // Node width / 2
    const fromY = fromNode.position.y + 50;  // Node height / 2
    const toX = toNode.position.x + 100;
    const toY = toNode.position.y + 50;
    
    return `M ${fromX} ${fromY} L ${toX} ${toY}`;
  }

  /**
   * Gets the color for a node
   */
  getNodeColor(node: NodeModel): string {
    const typeColors: Record<string, string> = {
      'trigger': '#4CAF50',
      'action': '#2196F3',
      'transform': '#FF9800',
      'condition': '#9C27B0',
      'notifyx': '#E91E63',
      'http': '#607D8B',
      'slack': '#4A148C',
      'email': '#795548',
      'sms': '#3F51B5',
      'database': '#009688',
      'webhook': '#FF5722'
    };
    
    return typeColors[node.type] || '#757575';
  }

  /**
   * Gets the icon for a node
   */
  getNodeIcon(node: NodeModel): string {
    const typeIcons: Record<string, string> = {
      'trigger': 'play_arrow',
      'action': 'play_circle',
      'transform': 'transform',
      'condition': 'help_outline',
      'notifyx': 'notifications',
      'http': 'http',
      'slack': 'chat',
      'email': 'email',
      'sms': 'sms',
      'database': 'storage',
      'webhook': 'webhook'
    };
    
    return typeIcons[node.type] || 'extension';
  }

  /**
   * Gets the description for a node
   */
  getNodeDescription(node: NodeModel): string {
    return (node as any).description || `${node.type} node`;
  }
}