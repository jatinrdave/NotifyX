/// <reference types="cypress" />

declare global {
  namespace Cypress {
    interface Chainable {
      login(email: string, password: string): Chainable<void>;
      createWorkflow(name: string, description: string): Chainable<void>;
      addNodeToCanvas(nodeType: string): Chainable<void>;
      configureNode(nodeType: string, config: Record<string, any>): Chainable<void>;
      connectNodes(fromIndex: number, toIndex: number): Chainable<void>;
      waitForExecution(): Chainable<void>;
    }
  }
}

Cypress.Commands.add('login', (email: string, password: string) => {
  // Mock login for testing
  cy.window().then((win) => {
    win.localStorage.setItem('auth_token', 'mock-jwt-token');
    win.localStorage.setItem('user_email', email);
    win.localStorage.setItem('tenant_id', 'test-tenant');
  });
});

Cypress.Commands.add('createWorkflow', (name: string, description: string) => {
  cy.get('[data-cy="create-workflow-btn"]').click();
  cy.get('[data-cy="workflow-name"]').type(name);
  cy.get('[data-cy="workflow-description"]').type(description);
  cy.get('[data-cy="save-workflow-btn"]').click();
  cy.get('[data-cy="workflow-title"]').should('contain', name);
});

Cypress.Commands.add('addNodeToCanvas', (nodeType: string) => {
  cy.get(`[data-cy="connector-${nodeType}"]`)
    .trigger('dragstart');
  
  cy.get('[data-cy="workflow-canvas"]')
    .trigger('dragover')
    .trigger('drop');
  
  cy.get('[data-cy="workflow-node"]').should('have.length.at.least', 1);
});

Cypress.Commands.add('configureNode', (nodeType: string, config: Record<string, any>) => {
  cy.get('[data-cy="workflow-node"]').first().dblclick();
  cy.get('[data-cy="node-config-modal"]').should('be.visible');
  
  Object.entries(config).forEach(([key, value]) => {
    cy.get(`[data-cy="config-${key}"]`).type(value);
  });
  
  cy.get('[data-cy="save-config-btn"]').click();
  cy.get('[data-cy="node-config-modal"]').should('not.exist');
});

Cypress.Commands.add('connectNodes', (fromIndex: number, toIndex: number) => {
  cy.get('[data-cy="workflow-node"]').eq(fromIndex).find('[data-cy="output-port"]').click();
  cy.get('[data-cy="workflow-node"]').eq(toIndex).find('[data-cy="input-port"]').click();
  cy.get('[data-cy="workflow-edge"]').should('have.length.at.least', 1);
});

Cypress.Commands.add('waitForExecution', () => {
  cy.get('[data-cy="run-status"]', { timeout: 30000 })
    .should('satisfy', (status) => {
      return ['completed', 'failed', 'cancelled'].includes(status.text());
    });
});

export {};