export interface ConnectorRegistryEntry {
  id: string;
  name: string;
  description: string;
  category: string;
  version: string;
  icon?: string;
  inputs: ConnectorInput[];
  outputs: ConnectorOutput[];
  metadata: ConnectorMetadata;
}

export interface ConnectorInput {
  id: string;
  name: string;
  type: string;
  required: boolean;
  description?: string;
  defaultValue?: any;
}

export interface ConnectorOutput {
  id: string;
  name: string;
  type: string;
  description?: string;
}

export interface ConnectorConfiguration {
  properties: ConnectorProperty[];
  validation?: ConnectorValidation;
}

export interface ConnectorProperty {
  id: string;
  name: string;
  type: 'string' | 'number' | 'boolean' | 'object' | 'array';
  required: boolean;
  description?: string;
  defaultValue?: any;
  options?: any[];
}

export interface ConnectorValidation {
  required?: string[];
  patterns?: { [key: string]: string };
  min?: { [key: string]: number };
  max?: { [key: string]: number };
}

export interface ConnectorMetadata {
  author: string;
  license: string;
  repository?: string;
  documentation?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}