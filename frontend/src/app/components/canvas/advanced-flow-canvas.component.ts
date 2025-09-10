import { Component, ElementRef, OnInit, ViewChild, HostListener, Input, Output, EventEmitter } from '@angular/core';
import { WorkflowService } from '../../services/workflow.service';
import { NodeModel, EdgeModel } from '../../models/node.model';
import { ConnectorService } from '../../services/connector.service';
import { ConnectorRegistryEntry } from '../../models/connector.model';

interface CanvasState {
  pan: { x: number; y: number };
  scale: number;
  selectedNodes: string[];
  selectedEdges: string[];
  clipboard: { nodes: NodeModel[]; edges: EdgeModel[] };
  history: CanvasAction[];
  historyIndex: number;
}

interface CanvasAction {
  type: 'add' | 'remove' | 'move' | 'connect' | 'disconnect' | 'config';
  data: any;
  timestamp: Date;
}

interface DragState {
  isDragging: boolean;
  startPos: { x: number; y: number };
  currentPos: { x: number; y: number };
  draggedNodes: string[];
}

interface SelectionBox {
  isActive: boolean;
  start: { x: number; y: number };
  end: { x: number; y: number };
}

@Component({
  selector: 'app-advanced-flow-canvas',
  templateUrl: './advanced-flow-canvas.component.html',
  styleUrls: ['./advanced-flow-canvas.component.css']
})
export class AdvancedFlowCanvasComponent implements OnInit {
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLElement>;
  @ViewChild('selectionBox') selectionBoxRef!: ElementRef<HTMLElement>;

  @Input() workflow: any = null;
  @Output() workflowChange = new EventEmitter<any>();
  @Output() nodeSelect = new EventEmitter<NodeModel | null>();
  @Output() nodeDoubleClick = new EventEmitter<NodeModel>();

  // Canvas state
  state: CanvasState = {
    pan: { x: 0, y: 0 },
    scale: 1,
    selectedNodes: [],
    selectedEdges: [],
    clipboard: { nodes: [], edges: [] },
    history: [],
    historyIndex: -1
  };

  // Drag state
  dragState: DragState = {
    isDragging: false,
    startPos: { x: 0, y: 0 },
    currentPos: { x: 0, y: 0 },
    draggedNodes: []
  };

  // Selection box
  selectionBox: SelectionBox = {
    isActive: false,
    start: { x: 0, y: 0 },
    end: { x: 0, y: 0 }
  };

  // Node and edge data
  nodes: NodeModel[] = [];
  edges: EdgeModel[] = [];
  connectors: ConnectorRegistryEntry[] = [];

  // UI state
  isPanning = false;
  isConnecting = false;
  connectionStart: NodeModel | null = null;
  showGrid = true;
  snapToGrid = true;
  gridSize = 20;

  // Keyboard shortcuts
  private keyBindings = new Map<string, () => void>();

  constructor(
    private workflowService: WorkflowService,
    private connectorService: ConnectorService
  ) {
    this.setupKeyboardShortcuts();
  }

  ngOnInit(): void {
    this.loadConnectors();
    this.loadWorkflow();
  }

  private setupKeyboardShortcuts(): void {
    this.keyBindings.set('ctrl+z', () => this.undo());
    this.keyBindings.set('ctrl+y', () => this.redo());
    this.keyBindings.set('ctrl+c', () => this.copy());
    this.keyBindings.set('ctrl+v', () => this.paste());
    this.keyBindings.set('ctrl+a', () => this.selectAll());
    this.keyBindings.set('delete', () => this.deleteSelected());
    this.keyBindings.set('escape', () => this.clearSelection());
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const key = this.getKeyString(event);
    const handler = this.keyBindings.get(key);
    if (handler) {
      event.preventDefault();
      handler();
    }
  }

  @HostListener('keyup', ['$event'])
  onKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.cancelConnection();
      this.clearSelection();
    }
  }

  private getKeyString(event: KeyboardEvent): string {
    const modifiers = [];
    if (event.ctrlKey) modifiers.push('ctrl');
    if (event.shiftKey) modifiers.push('shift');
    if (event.altKey) modifiers.push('alt');
    if (event.metaKey) modifiers.push('meta');
    
    modifiers.push(event.key.toLowerCase());
    return modifiers.join('+');
  }

  // Canvas interaction handlers
  @HostListener('mousedown', ['$event'])
  onMouseDown(event: MouseEvent): void {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const canvasPos = {
      x: (event.clientX - rect.left - this.state.pan.x) / this.state.scale,
      y: (event.clientY - rect.top - this.state.pan.y) / this.state.scale
    };

    if (event.button === 0) { // Left click
      if (event.ctrlKey || event.metaKey) {
        // Start selection box
        this.startSelectionBox(event);
      } else {
        // Start panning or node dragging
        this.startPanning(event);
      }
    } else if (event.button === 2) { // Right click
      this.showContextMenu(event, canvasPos);
    }
  }

  @HostListener('mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    if (this.selectionBox.isActive) {
      this.updateSelectionBox(event);
    } else if (this.isPanning) {
      this.updatePanning(event);
    } else if (this.dragState.isDragging) {
      this.updateNodeDragging(event);
    }
  }

  @HostListener('mouseup', ['$event'])
  onMouseUp(event: MouseEvent): void {
    if (this.selectionBox.isActive) {
      this.finishSelectionBox();
    } else if (this.isPanning) {
      this.finishPanning();
    } else if (this.dragState.isDragging) {
      this.finishNodeDragging();
    }
  }

  @HostListener('wheel', ['$event'])
  onWheel(event: WheelEvent): void {
    event.preventDefault();
    this.zoom(event.deltaY < 0 ? 1.1 : 0.9, event);
  }

  // Node interaction handlers
  onNodeMouseDown(node: NodeModel, event: MouseEvent): void {
    event.stopPropagation();
    
    if (event.button === 0) { // Left click
      if (!event.ctrlKey && !event.metaKey) {
        this.clearSelection();
      }
      
      this.selectNode(node.id);
      this.startNodeDragging(node, event);
    }
  }

  onNodeDoubleClick(node: NodeModel): void {
    this.nodeDoubleClick.emit(node);
  }

  onNodePortClick(node: NodeModel, portType: 'input' | 'output', event: MouseEvent): void {
    event.stopPropagation();
    
    if (this.isConnecting) {
      if (portType === 'input' && this.connectionStart) {
        this.createConnection(this.connectionStart, node);
        this.cancelConnection();
      }
    } else {
      if (portType === 'output') {
        this.startConnection(node);
      }
    }
  }

  // Drag and drop handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    
    const nodeType = event.dataTransfer?.getData('nodeType');
    const nodeLabel = event.dataTransfer?.getData('nodeLabel');
    
    if (nodeType) {
      const rect = this.canvasRef.nativeElement.getBoundingClientRect();
      const position = {
        x: (event.clientX - rect.left - this.state.pan.x) / this.state.scale,
        y: (event.clientY - rect.top - this.state.pan.y) / this.state.scale
      };
      
      this.addNode(nodeType, position, nodeLabel);
    }
  }

  // Canvas operations
  private startPanning(event: MouseEvent): void {
    this.isPanning = true;
    this.dragState.startPos = { x: event.clientX, y: event.clientY };
  }

  private updatePanning(event: MouseEvent): void {
    if (!this.isPanning) return;
    
    const dx = event.clientX - this.dragState.startPos.x;
    const dy = event.clientY - this.dragState.startPos.y;
    
    this.state.pan.x += dx;
    this.state.pan.y += dy;
    
    this.dragState.startPos = { x: event.clientX, y: event.clientY };
  }

  private finishPanning(): void {
    this.isPanning = false;
  }

  private startNodeDragging(node: NodeModel, event: MouseEvent): void {
    this.dragState.isDragging = true;
    this.dragState.draggedNodes = this.state.selectedNodes;
    this.dragState.startPos = { x: event.clientX, y: event.clientY };
  }

  private updateNodeDragging(event: MouseEvent): void {
    if (!this.dragState.isDragging) return;
    
    const dx = (event.clientX - this.dragState.startPos.x) / this.state.scale;
    const dy = (event.clientY - this.dragState.startPos.y) / this.state.scale;
    
    this.dragState.draggedNodes.forEach(nodeId => {
      const node = this.nodes.find(n => n.id === nodeId);
      if (node) {
        node.position.x += dx;
        node.position.y += dy;
        
        if (this.snapToGrid) {
          node.position.x = Math.round(node.position.x / this.gridSize) * this.gridSize;
          node.position.y = Math.round(node.position.y / this.gridSize) * this.gridSize;
        }
      }
    });
    
    this.dragState.startPos = { x: event.clientX, y: event.clientY };
  }

  private finishNodeDragging(): void {
    if (this.dragState.isDragging) {
      this.saveState('move');
      this.dragState.isDragging = false;
      this.dragState.draggedNodes = [];
    }
  }

  // Selection box operations
  private startSelectionBox(event: MouseEvent): void {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.selectionBox = {
      isActive: true,
      start: {
        x: (event.clientX - rect.left - this.state.pan.x) / this.state.scale,
        y: (event.clientY - rect.top - this.state.pan.y) / this.state.scale
      },
      end: {
        x: (event.clientX - rect.left - this.state.pan.x) / this.state.scale,
        y: (event.clientY - rect.top - this.state.pan.y) / this.state.scale
      }
    };
  }

  private updateSelectionBox(event: MouseEvent): void {
    if (!this.selectionBox.isActive) return;
    
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.selectionBox.end = {
      x: (event.clientX - rect.left - this.state.pan.x) / this.state.scale,
      y: (event.clientY - rect.top - this.state.pan.y) / this.state.scale
    };
  }

  private finishSelectionBox(): void {
    if (!this.selectionBox.isActive) return;
    
    const selectedNodes = this.getNodesInSelectionBox();
    this.state.selectedNodes = selectedNodes.map(n => n.id);
    this.selectionBox.isActive = false;
  }

  private getNodesInSelectionBox(): NodeModel[] {
    const minX = Math.min(this.selectionBox.start.x, this.selectionBox.end.x);
    const maxX = Math.max(this.selectionBox.start.x, this.selectionBox.end.x);
    const minY = Math.min(this.selectionBox.start.y, this.selectionBox.end.y);
    const maxY = Math.max(this.selectionBox.start.y, this.selectionBox.end.y);
    
    return this.nodes.filter(node => 
      node.position.x >= minX && 
      node.position.x <= maxX && 
      node.position.y >= minY && 
      node.position.y <= maxY
    );
  }

  // Connection operations
  private startConnection(node: NodeModel): void {
    this.isConnecting = true;
    this.connectionStart = node;
  }

  private cancelConnection(): void {
    this.isConnecting = false;
    this.connectionStart = null;
  }

  private createConnection(fromNode: NodeModel, toNode: NodeModel): void {
    const edge: EdgeModel = {
      id: `edge-${Date.now()}`,
      from: fromNode.id,
      to: toNode.id,
      condition: ''
    };
    
    this.edges.push(edge);
    this.saveState('connect');
    this.emitWorkflowChange();
  }

  // Node operations
  private addNode(type: string, position: { x: number; y: number }, label?: string): void {
    const node: NodeModel = {
      id: `node-${Date.now()}`,
      type,
      label: label || type,
      position: this.snapToGrid ? {
        x: Math.round(position.x / this.gridSize) * this.gridSize,
        y: Math.round(position.y / this.gridSize) * this.gridSize
      } : position,
      config: {}
    };
    
    this.nodes.push(node);
    this.selectNode(node.id);
    this.saveState('add');
    this.emitWorkflowChange();
  }

  private selectNode(nodeId: string): void {
    if (!this.state.selectedNodes.includes(nodeId)) {
      this.state.selectedNodes.push(nodeId);
    }
    
    const node = this.nodes.find(n => n.id === nodeId);
    this.nodeSelect.emit(node || null);
  }

  private clearSelection(): void {
    this.state.selectedNodes = [];
    this.state.selectedEdges = [];
    this.nodeSelect.emit(null);
  }

  private selectAll(): void {
    this.state.selectedNodes = this.nodes.map(n => n.id);
  }

  private deleteSelected(): void {
    // Remove selected nodes
    this.nodes = this.nodes.filter(n => !this.state.selectedNodes.includes(n.id));
    
    // Remove edges connected to deleted nodes
    this.edges = this.edges.filter(e => 
      !this.state.selectedNodes.includes(e.from) && 
      !this.state.selectedNodes.includes(e.to)
    );
    
    this.clearSelection();
    this.saveState('remove');
    this.emitWorkflowChange();
  }

  // Clipboard operations
  private copy(): void {
    const selectedNodes = this.nodes.filter(n => this.state.selectedNodes.includes(n.id));
    const selectedEdges = this.edges.filter(e => 
      this.state.selectedNodes.includes(e.from) && 
      this.state.selectedNodes.includes(e.to)
    );
    
    this.state.clipboard = {
      nodes: JSON.parse(JSON.stringify(selectedNodes)),
      edges: JSON.parse(JSON.stringify(selectedEdges))
    };
  }

  private paste(): void {
    if (this.state.clipboard.nodes.length === 0) return;
    
    const offset = { x: 20, y: 20 };
    const pastedNodes: NodeModel[] = [];
    
    this.state.clipboard.nodes.forEach(node => {
      const newNode: NodeModel = {
        ...node,
        id: `node-${Date.now()}-${Math.random()}`,
        position: {
          x: node.position.x + offset.x,
          y: node.position.y + offset.y
        }
      };
      
      this.nodes.push(newNode);
      pastedNodes.push(newNode);
    });
    
    this.state.selectedNodes = pastedNodes.map(n => n.id);
    this.saveState('add');
    this.emitWorkflowChange();
  }

  // History operations
  private saveState(action: string): void {
    const stateData = {
      nodes: JSON.parse(JSON.stringify(this.nodes)),
      edges: JSON.parse(JSON.stringify(this.edges))
    };
    
    const actionData: CanvasAction = {
      type: action as any,
      data: stateData,
      timestamp: new Date()
    };
    
    // Remove any history after current index
    this.state.history = this.state.history.slice(0, this.state.historyIndex + 1);
    
    // Add new action
    this.state.history.push(actionData);
    this.state.historyIndex = this.state.history.length - 1;
    
    // Limit history size
    if (this.state.history.length > 50) {
      this.state.history.shift();
      this.state.historyIndex--;
    }
  }

  private undo(): void {
    if (this.state.historyIndex > 0) {
      this.state.historyIndex--;
      this.restoreState(this.state.history[this.state.historyIndex]);
    }
  }

  private redo(): void {
    if (this.state.historyIndex < this.state.history.length - 1) {
      this.state.historyIndex++;
      this.restoreState(this.state.history[this.state.historyIndex]);
    }
  }

  private restoreState(action: CanvasAction): void {
    this.nodes = JSON.parse(JSON.stringify(action.data.nodes));
    this.edges = JSON.parse(JSON.stringify(action.data.edges));
    this.emitWorkflowChange();
  }

  // Zoom operations
  private zoom(factor: number, event: MouseEvent): void {
    const oldScale = this.state.scale;
    this.state.scale = Math.max(0.1, Math.min(5, this.state.scale * factor));
    
    // Adjust pan to zoom towards mouse cursor
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const mouseX = event.clientX - rect.left;
    const mouseY = event.clientY - rect.top;
    
    this.state.pan.x = mouseX - ((mouseX - this.state.pan.x) / oldScale) * this.state.scale;
    this.state.pan.y = mouseY - ((mouseY - this.state.pan.y) / oldScale) * this.state.scale;
  }

  // Utility methods
  private loadConnectors(): void {
    this.connectorService.getConnectors().subscribe(
      connectors => this.connectors = connectors,
      error => console.error('Error loading connectors:', error)
    );
  }

  private loadWorkflow(): void {
    if (this.workflow) {
      this.nodes = this.workflow.nodes || [];
      this.edges = this.workflow.edges || [];
    }
  }

  private emitWorkflowChange(): void {
    const workflow = {
      ...this.workflow,
      nodes: this.nodes,
      edges: this.edges
    };
    this.workflowChange.emit(workflow);
  }

  private showContextMenu(event: MouseEvent, position: { x: number; y: number }): void {
    // Implementation for context menu
    console.log('Context menu at:', position);
  }

  // Public methods for external components
  public fitToScreen(): void {
    if (this.nodes.length === 0) return;
    
    const bounds = this.getNodesBounds();
    const canvasRect = this.canvasRef.nativeElement.getBoundingClientRect();
    
    const scaleX = canvasRect.width / (bounds.width + 100);
    const scaleY = canvasRect.height / (bounds.height + 100);
    this.state.scale = Math.min(scaleX, scaleY, 1);
    
    this.state.pan.x = (canvasRect.width - bounds.width * this.state.scale) / 2 - bounds.x * this.state.scale;
    this.state.pan.y = (canvasRect.height - bounds.height * this.state.scale) / 2 - bounds.y * this.state.scale;
  }

  public centerView(): void {
    this.state.pan = { x: 0, y: 0 };
    this.state.scale = 1;
  }

  private getNodesBounds(): { x: number; y: number; width: number; height: number } {
    if (this.nodes.length === 0) {
      return { x: 0, y: 0, width: 0, height: 0 };
    }
    
    const xs = this.nodes.map(n => n.position.x);
    const ys = this.nodes.map(n => n.position.y);
    
    const minX = Math.min(...xs);
    const maxX = Math.max(...xs);
    const minY = Math.min(...ys);
    const maxY = Math.max(...ys);
    
    return {
      x: minX,
      y: minY,
      width: maxX - minX + 200, // Add node width
      height: maxY - minY + 100  // Add node height
    };
  }

  public getTransform(): string {
    return `translate(${this.state.pan.x}px, ${this.state.pan.y}px) scale(${this.state.scale})`;
  }

  public getSelectionBoxStyle(): any {
    if (!this.selectionBox.isActive) return { display: 'none' };
    
    const left = Math.min(this.selectionBox.start.x, this.selectionBox.end.x);
    const top = Math.min(this.selectionBox.start.y, this.selectionBox.end.y);
    const width = Math.abs(this.selectionBox.end.x - this.selectionBox.start.x);
    const height = Math.abs(this.selectionBox.end.y - this.selectionBox.start.y);
    
    return {
      position: 'absolute',
      left: `${left}px`,
      top: `${top}px`,
      width: `${width}px`,
      height: `${height}px`,
      border: '1px dashed #007acc',
      backgroundColor: 'rgba(0, 122, 204, 0.1)',
      pointerEvents: 'none'
    };
  }
}