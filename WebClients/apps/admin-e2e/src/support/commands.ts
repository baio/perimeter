// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
// eslint-disable-next-line @typescript-eslint/no-namespace
import { resolve } from 'url';
import { writeFileSync, readFileSync } from 'fs';

declare namespace Cypress {
    interface Chainable<Subject> {
        login(email: string, password: string): void;
    }
}
//
// -- This is a parent command --
Cypress.Commands.add('login', (email, password) => {
    console.log('Custom command example: Login', email, password);
});
//
// -- This is a child command --
// Cypress.Commands.add("drag", { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add("dismiss", { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })
Cypress.Commands.add('dataCy', (value, selector = '') => {
    return cy.get(`[data-cy=${value}]${selector}`);
});

Cypress.Commands.add('refreshDb', () => {
    const baseUrl = Cypress.env('apiBaseUrl');

    const refreshDbUrl = Cypress.env('resetDbUrl');

    const url = resolve(baseUrl, refreshDbUrl);

    return cy.request('POST', url);
});


Cypress.Commands.add('stickyVariable', (value) => {
    if (value) {
        return cy.writeFile('stickyVariable.txt', value);
    } else {
        return cy.readFile('stickyVariable.txt');
    }
});
