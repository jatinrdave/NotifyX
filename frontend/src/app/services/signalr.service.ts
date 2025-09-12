import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { filter, map } from 'rxjs/operators';

export interface WorkflowRunUpdate {
  type: 'run_status_change' | 'node_execution' | 'execution_progress' | 'execution_completed' | 'execution_error';
  runId: string;
  timestamp: Date;
  [key: string]: any;
}

export interface NodeExecutionUpdate {
  type: 'node_execution';
  runId: string;
  nodeId: string;
  status: string;
  input: any;
  output?: any;
  errorMessage?: string;
  durationMs?: number;
  timestamp: Date;
}

export interface ExecutionProgressUpdate {
  type: 'execution_progress';
  runId: string;
  completedNodes: number;
  totalNodes: number;
  progressPercentage: number;
  currentNodeId?: string;
  currentNodeType?: string;
  timestamp: Date;
}

export interface WorkflowChangeUpdate {
  type: 'workflow_changed';
  workflowId: string;
  changeType: string;
  timestamp: Date;
}

export interface SystemEventUpdate {
  type: 'system_event';
  eventId: string;
  eventType: string;
  message: string;
  severity: string;
  data: any;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private connectionStateSubject = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);
  private workflowUpdatesSubject = new Subject<WorkflowRunUpdate>();
  private nodeExecutionSubject = new Subject<NodeExecutionUpdate>();
  private executionProgressSubject = new Subject<ExecutionProgressUpdate>();
  private workflowChangeSubject = new Subject<WorkflowChangeUpdate>();
  private systemEventSubject = new Subject<SystemEventUpdate>();

  public connectionState$ = this.connectionStateSubject.asObservable();
  public workflowUpdates$ = this.workflowUpdatesSubject.asObservable();
  public nodeExecutions$ = this.nodeExecutionSubject.asObservable();
  public executionProgress$ = this.executionProgressSubject.asObservable();
  public workflowChanges$ = this.workflowChangeSubject.asObservable();
  public systemEvents$ = this.systemEventSubject.asObservable();

  constructor() {}

  /**
   * Starts the SignalR connection
   */
  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/workflowHub', {
        accessTokenFactory: () => {
          // Get JWT token from localStorage or auth service
          return localStorage.getItem('auth_token') || '';
        }
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    this.setupEventHandlers();

    try {
      await this.hubConnection.start();
      this.connectionStateSubject.next(this.hubConnection.state);
      console.log('SignalR connection started');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionStateSubject.next(HubConnectionState.Disconnected);
    }
  }

  /**
   * Stops the SignalR connection
   */
  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionStateSubject.next(HubConnectionState.Disconnected);
      console.log('SignalR connection stopped');
    }
  }

  /**
   * Subscribes to updates for a specific workflow run
   */
  async subscribeToRun(runId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('SubscribeToRun', runId);
      console.log(`Subscribed to run: ${runId}`);
    }
  }

  /**
   * Unsubscribes from updates for a specific workflow run
   */
  async unsubscribeFromRun(runId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('UnsubscribeFromRun', runId);
      console.log(`Unsubscribed from run: ${runId}`);
    }
  }

  /**
   * Subscribes to updates for a specific workflow
   */
  async subscribeToWorkflow(workflowId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('SubscribeToWorkflow', workflowId);
      console.log(`Subscribed to workflow: ${workflowId}`);
    }
  }

  /**
   * Unsubscribes from updates for a specific workflow
   */
  async unsubscribeFromWorkflow(workflowId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('UnsubscribeFromWorkflow', workflowId);
      console.log(`Unsubscribed from workflow: ${workflowId}`);
    }
  }

  /**
   * Gets updates for a specific run
   */
  getRunUpdates(runId: string): Observable<WorkflowRunUpdate> {
    return this.workflowUpdates$.pipe(
      filter(update => update.runId === runId)
    );
  }

  /**
   * Gets node execution updates for a specific run
   */
  getNodeExecutions(runId: string): Observable<NodeExecutionUpdate> {
    return this.nodeExecutions$.pipe(
      filter(update => update.runId === runId)
    );
  }

  /**
   * Gets execution progress for a specific run
   */
  getExecutionProgress(runId: string): Observable<ExecutionProgressUpdate> {
    return this.executionProgress$.pipe(
      filter(update => update.runId === runId)
    );
  }

  /**
   * Gets workflow changes for a specific workflow
   */
  getWorkflowChanges(workflowId: string): Observable<WorkflowChangeUpdate> {
    return this.workflowChanges$.pipe(
      filter(update => update.workflowId === workflowId)
    );
  }

  /**
   * Checks if the connection is established
   */
  isConnected(): boolean {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }

  /**
   * Gets the current connection state
   */
  getConnectionState(): HubConnectionState {
    return this.hubConnection?.state || HubConnectionState.Disconnected;
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Run status changes
    this.hubConnection.on('RunStatusChanged', (data: any) => {
      this.workflowUpdatesSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Node executions
    this.hubConnection.on('NodeExecuted', (data: any) => {
      this.nodeExecutionSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Execution progress
    this.hubConnection.on('ExecutionProgress', (data: any) => {
      this.executionProgressSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Execution completed
    this.hubConnection.on('ExecutionCompleted', (data: any) => {
      this.workflowUpdatesSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Execution errors
    this.hubConnection.on('ExecutionError', (data: any) => {
      this.workflowUpdatesSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Workflow changes
    this.hubConnection.on('WorkflowChanged', (data: any) => {
      this.workflowChangeSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // System events
    this.hubConnection.on('SystemEvent', (data: any) => {
      this.systemEventSubject.next({
        ...data,
        timestamp: new Date(data.timestamp)
      });
    });

    // Connection state changes
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.connectionStateSubject.next(HubConnectionState.Disconnected);
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting', error);
      this.connectionStateSubject.next(HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected', connectionId);
      this.connectionStateSubject.next(HubConnectionState.Connected);
    });
  }
}