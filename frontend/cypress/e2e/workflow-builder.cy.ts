describe('Workflow Builder', () => {
  beforeEach(() => {
    cy.visit('/');
    cy.login('test@example.com', 'password');
  });

  it('should create a new workflow', () => {
    // Click create workflow button
    cy.get('[data-cy="create-workflow-btn"]').click();
    
    // Fill workflow details
    cy.get('[data-cy="workflow-name"]').type('Test Workflow');
    cy.get('[data-cy="workflow-description"]').type('A test workflow for automation');
    
    // Save workflow
    cy.get('[data-cy="save-workflow-btn"]').click();
    
    // Verify workflow was created
    cy.get('[data-cy="workflow-title"]').should('contain', 'Test Workflow');
    cy.get('[data-cy="workflow-description"]').should('contain', 'A test workflow for automation');
  });

  it('should add nodes to workflow canvas', () => {
    // Create a new workflow
    cy.createWorkflow('Test Workflow', 'Test description');
    
    // Drag a node from palette to canvas
    cy.get('[data-cy="node-palette"]').should('be.visible');
    cy.get('[data-cy="connector-notifyx-sendNotification"]').should('be.visible');
    
    // Drag and drop the NotifyX Send Notification node
    cy.get('[data-cy="connector-notifyx-sendNotification"]')
      .trigger('dragstart');
    
    cy.get('[data-cy="workflow-canvas"]')
      .trigger('dragover')
      .trigger('drop');
    
    // Verify node was added to canvas
    cy.get('[data-cy="workflow-canvas"]')
      .find('[data-cy="workflow-node"]')
      .should('have.length', 1);
    
    // Verify node properties
    cy.get('[data-cy="workflow-node"]')
      .should('contain', 'Send Notification');
  });

  it('should configure node properties', () => {
    // Create workflow with a node
    cy.createWorkflow('Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    
    // Double-click node to open configuration
    cy.get('[data-cy="workflow-node"]').dblclick();
    
    // Verify configuration modal opens
    cy.get('[data-cy="node-config-modal"]').should('be.visible');
    
    // Configure node properties
    cy.get('[data-cy="config-channel"]').select('email');
    cy.get('[data-cy="config-recipient"]').type('test@example.com');
    cy.get('[data-cy="config-message"]').type('Hello from NotifyX Studio!');
    
    // Save configuration
    cy.get('[data-cy="save-config-btn"]').click();
    
    // Verify configuration was saved
    cy.get('[data-cy="node-config-modal"]').should('not.exist');
    cy.get('[data-cy="workflow-node"]').should('contain', 'test@example.com');
  });

  it('should connect nodes with edges', () => {
    // Create workflow with two nodes
    cy.createWorkflow('Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    cy.addNodeToCanvas('http.request');
    
    // Connect the nodes
    cy.get('[data-cy="workflow-node"]').first().find('[data-cy="output-port"]').click();
    cy.get('[data-cy="workflow-node"]').last().find('[data-cy="input-port"]').click();
    
    // Verify edge was created
    cy.get('[data-cy="workflow-edge"]').should('have.length', 1);
  });

  it('should run workflow and show execution results', () => {
    // Create and configure a simple workflow
    cy.createWorkflow('Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    cy.configureNode('notifyx.sendNotification', {
      channel: 'email',
      recipient: 'test@example.com',
      message: 'Test message'
    });
    
    // Run the workflow
    cy.get('[data-cy="run-workflow-btn"]').click();
    
    // Verify run inspector opens
    cy.get('[data-cy="run-inspector"]').should('be.visible');
    
    // Wait for execution to complete
    cy.get('[data-cy="run-status"]', { timeout: 30000 })
      .should('contain', 'completed');
    
    // Verify node execution results
    cy.get('[data-cy="node-result"]')
      .should('have.length', 1)
      .should('contain', 'success');
  });

  it('should show real-time execution updates', () => {
    // Create workflow with multiple nodes
    cy.createWorkflow('Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    cy.addNodeToCanvas('http.request');
    cy.connectNodes(0, 1);
    
    // Run workflow
    cy.get('[data-cy="run-workflow-btn"]').click();
    
    // Verify real-time updates
    cy.get('[data-cy="execution-progress"]').should('be.visible');
    cy.get('[data-cy="live-indicator"]').should('be.visible');
    
    // Wait for progress updates
    cy.get('[data-cy="progress-percentage"]', { timeout: 10000 })
      .should('not.equal', '0%');
    
    // Verify node execution updates
    cy.get('[data-cy="node-result"]', { timeout: 30000 })
      .should('have.length', 2);
  });

  it('should handle workflow execution errors', () => {
    // Create workflow with invalid configuration
    cy.createWorkflow('Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    cy.configureNode('notifyx.sendNotification', {
      channel: 'email'
      // Missing required fields
    });
    
    // Run workflow
    cy.get('[data-cy="run-workflow-btn"]').click();
    
    // Verify error handling
    cy.get('[data-cy="run-status"]', { timeout: 10000 })
      .should('contain', 'failed');
    
    cy.get('[data-cy="error-message"]')
      .should('be.visible')
      .should('contain', 'required');
  });

  it('should export and import workflows', () => {
    // Create a workflow
    cy.createWorkflow('Export Test Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    cy.configureNode('notifyx.sendNotification', {
      channel: 'email',
      recipient: 'test@example.com',
      message: 'Test message'
    });
    
    // Export workflow
    cy.get('[data-cy="export-workflow-btn"]').click();
    
    // Verify export dialog
    cy.get('[data-cy="export-dialog"]').should('be.visible');
    cy.get('[data-cy="export-json"]').should('contain', 'Export Test Workflow');
    
    // Download export
    cy.get('[data-cy="download-export-btn"]').click();
    
    // Import workflow
    cy.get('[data-cy="import-workflow-btn"]').click();
    
    // Upload exported file
    cy.get('[data-cy="import-file-input"]').selectFile('cypress/downloads/workflow-export.json');
    
    // Verify import
    cy.get('[data-cy="import-success"]').should('be.visible');
    cy.get('[data-cy="workflow-title"]').should('contain', 'Export Test Workflow');
  });

  it('should validate workflow before execution', () => {
    // Create invalid workflow (no trigger)
    cy.createWorkflow('Invalid Workflow', 'Test description');
    cy.addNodeToCanvas('notifyx.sendNotification');
    
    // Try to run workflow
    cy.get('[data-cy="run-workflow-btn"]').click();
    
    // Verify validation error
    cy.get('[data-cy="validation-error"]')
      .should('be.visible')
      .should('contain', 'trigger');
    
    // Add trigger node
    cy.addNodeToCanvas('notifyx.onDeliveryStatus');
    
    // Try to run again
    cy.get('[data-cy="run-workflow-btn"]').click();
    
    // Should now run successfully
    cy.get('[data-cy="run-inspector"]').should('be.visible');
  });
});