import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { v4 as uuid } from 'uuid';
import { WorkflowModel, NodeModel, WorkflowRun, NodeExecutionResult } from '../models/node.model';

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private readonly apiUrl = '/api/workflows';
  private currentWorkflowSubject = new BehaviorSubject<WorkflowModel | null>(null);
  public currentWorkflow$ = this.currentWorkflowSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Creates a new node for the workflow canvas
   */
  createNode(type: string, position: { x: number; y: number }, config?: any): NodeModel {
    return {
      id: uuid(),
      type,
      position,
      config: config || {},
      isEnabled: true,
      metadata: {}
    };
  }

  /**
   * Creates a new workflow
   */
  createWorkflow(name: string, description?: string): Observable<WorkflowModel> {
    const workflow: Partial<WorkflowModel> = {
      name,
      description,
      nodes: [],
      edges: [],
      triggers: [],
      globalVariables: {},
      tags: [],
      version: 1,
      isActive: true,
      createdAt: new Date(),
      updatedAt: new Date(),
      createdBy: 'current-user', // TODO: Get from auth service
      updatedBy: 'current-user'
    };

    return this.http.post<WorkflowModel>(this.apiUrl, workflow).pipe(
      tap(workflow => this.currentWorkflowSubject.next(workflow))
    );
  }

  /**
   * Gets a workflow by ID
   */
  getWorkflow(id: string): Observable<WorkflowModel> {
    return this.http.get<WorkflowModel>(`${this.apiUrl}/${id}`).pipe(
      tap(workflow => this.currentWorkflowSubject.next(workflow))
    );
  }

  /**
   * Updates a workflow
   */
  updateWorkflow(workflow: WorkflowModel): Observable<WorkflowModel> {
    return this.http.put<WorkflowModel>(`${this.apiUrl}/${workflow.id}`, workflow).pipe(
      tap(updatedWorkflow => this.currentWorkflowSubject.next(updatedWorkflow))
    );
  }

  /**
   * Saves the current workflow
   */
  saveWorkflow(workflow: WorkflowModel): Observable<WorkflowModel> {
    return this.updateWorkflow(workflow);
  }

  /**
   * Lists workflows with pagination and filtering
   */
  listWorkflows(
    page: number = 1,
    pageSize: number = 20,
    search?: string,
    tags?: string[]
  ): Observable<WorkflowModel[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (tags && tags.length > 0) {
      tags.forEach(tag => {
        params = params.append('tags', tag);
      });
    }

    return this.http.get<WorkflowModel[]>(this.apiUrl, { params });
  }

  /**
   * Deletes a workflow
   */
  deleteWorkflow(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Exports a workflow as JSON
   */
  exportWorkflow(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/export`, {});
  }

  /**
   * Imports a workflow from JSON
   */
  importWorkflow(workflowData: any): Observable<WorkflowModel> {
    return this.http.post<WorkflowModel>(`${this.apiUrl}/import`, { workflow: workflowData });
  }

  /**
   * Starts a workflow run
   */
  startRun(workflowId: string, payload: any, mode: 'test' | 'scheduled' | 'triggered' | 'manual' = 'manual'): Observable<{ runId: string }> {
    return this.http.post<{ runId: string }>(`${this.apiUrl}/${workflowId}/runs`, {
      payload,
      mode
    });
  }

  /**
   * Gets the current workflow
   */
  getCurrentWorkflow(): WorkflowModel | null {
    return this.currentWorkflowSubject.value;
  }

  /**
   * Sets the current workflow
   */
  setCurrentWorkflow(workflow: WorkflowModel | null): void {
    this.currentWorkflowSubject.next(workflow);
  }

  /**
   * Adds a node to the current workflow
   */
  addNode(node: NodeModel): void {
    const currentWorkflow = this.getCurrentWorkflow();
    if (currentWorkflow) {
      const updatedWorkflow = {
        ...currentWorkflow,
        nodes: [...currentWorkflow.nodes, node],
        updatedAt: new Date()
      };
      this.setCurrentWorkflow(updatedWorkflow);
    }
  }

  /**
   * Updates a node in the current workflow
   */
  updateNode(nodeId: string, updates: Partial<NodeModel>): void {
    const currentWorkflow = this.getCurrentWorkflow();
    if (currentWorkflow) {
      const updatedWorkflow = {
        ...currentWorkflow,
        nodes: currentWorkflow.nodes.map(node =>
          node.id === nodeId ? { ...node, ...updates } : node
        ),
        updatedAt: new Date()
      };
      this.setCurrentWorkflow(updatedWorkflow);
    }
  }

  /**
   * Removes a node from the current workflow
   */
  removeNode(nodeId: string): void {
    const currentWorkflow = this.getCurrentWorkflow();
    if (currentWorkflow) {
      const updatedWorkflow = {
        ...currentWorkflow,
        nodes: currentWorkflow.nodes.filter(node => node.id !== nodeId),
        edges: currentWorkflow.edges.filter(edge => edge.from !== nodeId && edge.to !== nodeId),
        updatedAt: new Date()
      };
      this.setCurrentWorkflow(updatedWorkflow);
    }
  }

  /**
   * Adds an edge to the current workflow
   */
  addEdge(edge: any): void {
    const currentWorkflow = this.getCurrentWorkflow();
    if (currentWorkflow) {
      const updatedWorkflow = {
        ...currentWorkflow,
        edges: [...currentWorkflow.edges, { ...edge, id: uuid() }],
        updatedAt: new Date()
      };
      this.setCurrentWorkflow(updatedWorkflow);
    }
  }

  /**
   * Removes an edge from the current workflow
   */
  removeEdge(edgeId: string): void {
    const currentWorkflow = this.getCurrentWorkflow();
    if (currentWorkflow) {
      const updatedWorkflow = {
        ...currentWorkflow,
        edges: currentWorkflow.edges.filter(edge => edge.id !== edgeId),
        updatedAt: new Date()
      };
      this.setCurrentWorkflow(updatedWorkflow);
    }
  }

  /**
   * Validates the current workflow
   */
  validateWorkflow(): { isValid: boolean; errors: string[] } {
    const currentWorkflow = this.getCurrentWorkflow();
    if (!currentWorkflow) {
      return { isValid: false, errors: ['No workflow loaded'] };
    }

    const errors: string[] = [];

    // Check for at least one node
    if (currentWorkflow.nodes.length === 0) {
      errors.push('Workflow must have at least one node');
    }

    // Check for trigger nodes
    const triggerNodes = currentWorkflow.nodes.filter(node => node.type.includes('trigger'));
    if (triggerNodes.length === 0) {
      errors.push('Workflow must have at least one trigger node');
    }

    // Check for orphaned nodes
    const connectedNodeIds = new Set<string>();
    currentWorkflow.edges.forEach(edge => {
      connectedNodeIds.add(edge.from);
      connectedNodeIds.add(edge.to);
    });

    const orphanedNodes = currentWorkflow.nodes.filter(node => 
      !node.type.includes('trigger') && !connectedNodeIds.has(node.id)
    );

    if (orphanedNodes.length > 0) {
      errors.push(`Found ${orphanedNodes.length} orphaned nodes that are not connected`);
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }
}