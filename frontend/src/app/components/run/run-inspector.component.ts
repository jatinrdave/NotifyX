import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Subject, takeUntil, combineLatest } from 'rxjs';
import { SignalRService, WorkflowRunUpdate, NodeExecutionUpdate, ExecutionProgressUpdate } from '../../services/signalr.service';
import { WorkflowRun, NodeExecutionResult } from '../../models/node.model';

@Component({
  selector: 'app-run-inspector',
  templateUrl: './run-inspector.component.html',
  styleUrls: ['./run-inspector.component.css']
})
export class RunInspectorComponent implements OnInit, OnDestroy {
  @Input() runId: string = '';
  @Input() workflow: any = null;

  run: WorkflowRun | null = null;
  nodeResults: NodeExecutionResult[] = [];
  executionProgress: ExecutionProgressUpdate | null = null;
  isConnected = false;
  isLoading = false;

  private destroy$ = new Subject<void>();

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    if (this.runId) {
      this.setupRealTimeUpdates();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.runId) {
      this.signalRService.unsubscribeFromRun(this.runId);
    }
  }

  private setupRealTimeUpdates(): void {
    // Check connection state
    this.signalRService.connectionState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.isConnected = state === 1; // HubConnectionState.Connected
      });

    // Subscribe to run updates
    this.signalRService.subscribeToRun(this.runId);

    // Get run updates
    this.signalRService.getRunUpdates(this.runId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        this.handleRunUpdate(update);
      });

    // Get node execution updates
    this.signalRService.getNodeExecutions(this.runId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        this.handleNodeExecution(update);
      });

    // Get execution progress
    this.signalRService.getExecutionProgress(this.runId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.executionProgress = progress;
      });
  }

  private handleRunUpdate(update: WorkflowRunUpdate): void {
    switch (update.type) {
      case 'run_status_change':
        this.updateRunStatus(update);
        break;
      case 'execution_completed':
        this.handleExecutionCompleted(update);
        break;
      case 'execution_error':
        this.handleExecutionError(update);
        break;
    }
  }

  private updateRunStatus(update: WorkflowRunUpdate): void {
    if (this.run) {
      this.run = {
        ...this.run,
        status: update.status as any,
        errorMessage: update.errorMessage
      };
    }
  }

  private handleExecutionCompleted(update: WorkflowRunUpdate): void {
    if (this.run) {
      this.run = {
        ...this.run,
        status: update.status as any,
        output: update.output,
        endTime: new Date(),
        durationMs: update.durationMs
      };
    }
  }

  private handleExecutionError(update: WorkflowRunUpdate): void {
    if (this.run) {
      this.run = {
        ...this.run,
        status: 'failed' as any,
        errorMessage: update.errorMessage,
        endTime: new Date()
      };
    }
  }

  private handleNodeExecution(update: NodeExecutionUpdate): void {
    const existingIndex = this.nodeResults.findIndex(r => r.nodeId === update.nodeId);
    const nodeResult: NodeExecutionResult = {
      runId: update.runId,
      nodeId: update.nodeId,
      status: update.status as any,
      input: update.input,
      output: update.output,
      errorMessage: update.errorMessage,
      startTime: new Date(),
      endTime: new Date(),
      durationMs: update.durationMs || 0,
      attempt: 1,
      metadata: {}
    };

    if (existingIndex >= 0) {
      this.nodeResults[existingIndex] = nodeResult;
    } else {
      this.nodeResults.push(nodeResult);
    }

    // Sort by execution order
    this.nodeResults.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());
  }

  getNodeName(nodeId: string): string {
    if (this.workflow?.nodes) {
      const node = this.workflow.nodes.find((n: any) => n.id === nodeId);
      return node?.label || node?.type || nodeId;
    }
    return nodeId;
  }

  getNodeType(nodeId: string): string {
    if (this.workflow?.nodes) {
      const node = this.workflow.nodes.find((n: any) => n.id === nodeId);
      return node?.type || 'unknown';
    }
    return 'unknown';
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'success':
      case 'completed':
        return 'check_circle';
      case 'failed':
        return 'error';
      case 'running':
        return 'play_circle';
      case 'pending':
        return 'schedule';
      case 'cancelled':
        return 'cancel';
      default:
        return 'help';
    }
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'success':
      case 'completed':
        return 'green';
      case 'failed':
        return 'red';
      case 'running':
        return 'blue';
      case 'pending':
        return 'orange';
      case 'cancelled':
        return 'gray';
      default:
        return 'gray';
    }
  }

  getProgressPercentage(): number {
    if (this.executionProgress) {
      return this.executionProgress.progressPercentage;
    }
    
    if (this.nodeResults.length === 0) {
      return 0;
    }

    const totalNodes = this.workflow?.nodes?.length || this.nodeResults.length;
    const completedNodes = this.nodeResults.filter(r => 
      r.status === 'success' || r.status === 'failed'
    ).length;

    return Math.round((completedNodes / totalNodes) * 100);
  }

  formatDuration(durationMs: number): string {
    if (durationMs < 1000) {
      return `${durationMs}ms`;
    } else if (durationMs < 60000) {
      return `${(durationMs / 1000).toFixed(1)}s`;
    } else {
      const minutes = Math.floor(durationMs / 60000);
      const seconds = Math.floor((durationMs % 60000) / 1000);
      return `${minutes}m ${seconds}s`;
    }
  }

  formatTimestamp(timestamp: Date): string {
    return timestamp.toLocaleTimeString();
  }

  isRunComplete(): boolean {
    return this.run?.status === 'completed' || this.run?.status === 'failed' || this.run?.status === 'cancelled';
  }

  isRunRunning(): boolean {
    return this.run?.status === 'running';
  }

  canReplay(): boolean {
    return this.run?.status === 'failed';
  }

  onReplay(): void {
    // Emit replay event to parent component
    // This would typically call a service to replay the run
    console.log('Replay requested for run:', this.runId);
  }

  onCancel(): void {
    // Emit cancel event to parent component
    // This would typically call a service to cancel the run
    console.log('Cancel requested for run:', this.runId);
  }
}