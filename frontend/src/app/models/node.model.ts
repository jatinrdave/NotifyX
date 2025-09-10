export interface NodeModel {
  id: string;
  type: string;
  label?: string;
  position: { x: number; y: number; };
  config?: any;
  inputs?: NodeInput[];
  outputs?: NodeOutput[];
  category?: string;
  color?: string;
  iconShape?: 'circle' | 'square' | 'hexagon';
  isEnabled?: boolean;
  metadata?: Record<string, any>;
}

export interface NodeInput {
  name: string;
  type: string;
  required: boolean;
  defaultValue?: any;
  description?: string;
  validation?: Record<string, any>;
}

export interface NodeOutput {
  name: string;
  type: string;
  description?: string;
}

export interface EdgeModel {
  id: string;
  from: string;
  to: string;
  fromOutput?: string;
  toInput?: string;
  condition?: string;
  label?: string;
  metadata?: Record<string, any>;
}

export interface WorkflowModel {
  id: string;
  name: string;
  description?: string;
  nodes: NodeModel[];
  edges: EdgeModel[];
  triggers?: WorkflowTrigger[];
  globalVariables?: Record<string, any>;
  tags?: string[];
  version: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy: string;
}

export interface WorkflowTrigger {
  type: 'webhook' | 'schedule' | 'notifyxEvent' | 'kafka' | 'manual';
  config: any;
  isActive: boolean;
  metadata?: Record<string, any>;
}

export interface ConnectorManifest {
  id: string;
  name: string;
  version: string;
  type: 'trigger' | 'action' | 'transform';
  category: string;
  description: string;
  icon: string;
  inputs: ConnectorInput[];
  outputs: ConnectorOutput[];
  auth: ConnectorAuth;
  ui: ConnectorUI;
  metadata: ConnectorMetadata;
}

export interface ConnectorInput {
  name: string;
  type: string;
  required: boolean;
  defaultValue?: any;
  description: string;
  validation?: Record<string, any>;
}

export interface ConnectorOutput {
  name: string;
  type: string;
  description: string;
}

export interface ConnectorAuth {
  type: 'none' | 'apiKey' | 'oauth2' | 'jwt';
  fields: Record<string, string>;
}

export interface ConnectorUI {
  color: string;
  iconShape: 'circle' | 'square' | 'hexagon';
  group: string;
}

export interface ConnectorMetadata {
  tags: string[];
  documentationUrl: string;
  createdBy: string;
}

export interface WorkflowRun {
  id: string;
  workflowId: string;
  status: 'pending' | 'running' | 'completed' | 'failed' | 'cancelled' | 'timeout';
  mode: 'test' | 'scheduled' | 'triggered' | 'manual';
  input: any;
  output?: any;
  errorMessage?: string;
  startTime: Date;
  endTime?: Date;
  durationMs?: number;
  triggeredBy?: string;
  nodeResults: NodeExecutionResult[];
  metadata?: Record<string, any>;
}

export interface NodeExecutionResult {
  runId: string;
  nodeId: string;
  status: 'pending' | 'running' | 'success' | 'failed' | 'skipped' | 'timeout';
  input: any;
  output?: any;
  errorMessage?: string;
  startTime: Date;
  endTime?: Date;
  durationMs?: number;
  attempt: number;
  metadata?: Record<string, any>;
}