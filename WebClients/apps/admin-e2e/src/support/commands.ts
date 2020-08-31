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
import { EMAIL, PASSWORD } from '../integration/_setup';

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

Cypress.Commands.add('formField', (value) => {
    return cy.get(
        `.hlc-form-input-${value} input, .hlc-form-input-${value} textarea`
    );
});

Cypress.Commands.add('formSelect', (value) => {
    return cy.get(`.hlc-form-input-${value} nz-select`);
});

Cypress.Commands.add('formSelectChoose', (value, index) => {
    cy.formSelect(value)
        .click()
        .get('nz-option-item .ant-select-item-option-content')
        .eq(index)
        .click()
        .formSelect(value)
        .find('nz-select-top-control')
        .click(100, 0, { force: true });
});

Cypress.Commands.add('formSelectSingle', (value, index) => {
    cy.formSelect(value)
        .click()
        .get('nz-option-item .ant-select-item-option-content')
        .eq(index)
        .click()
        .formSelect(value);
});

Cypress.Commands.add('rows', (index?: number, cellIndex?: number) => {
    const res = cy.get('tbody tr.ant-table-row');
    const res1 = index !== undefined ? res.eq(index) : res;
    return cellIndex !== undefined ? res1.find('td').eq(cellIndex) : res1;
});

Cypress.Commands.add('rowCommand', (rowIndex: number, commandIndex: number) => {
    return cy
        .get('tbody tr.ant-table-row')
        .eq(rowIndex)
        .find('td .table-actions a')
        .eq(commandIndex);
});

Cypress.Commands.add('submitButton', () => {
    return cy.dataCy('drawer-submit');
});

Cypress.Commands.add('cancelButton', () => {
    return cy.dataCy('drawer-close');
});

Cypress.Commands.add('confirmNoButton', () => {
    return cy.get('.ant-modal-confirm-body-wrapper button.ant-btn').eq(0);
});

Cypress.Commands.add('confirmYesButton', () => {
    return cy.get('.ant-modal-confirm-body-wrapper button.ant-btn').eq(1);
});

Cypress.Commands.add('resetDb', () => {
    const baseUrl = Cypress.env('apiBaseUrl');

    const refreshDbUrl = Cypress.env('resetDbUrl');

    const url = resolve(baseUrl, refreshDbUrl);

    return cy.request('POST', url);
});

Cypress.Commands.add('reinitDb', (loginAsDomain) => {
    const baseUrl = Cypress.env('apiBaseUrl');

    const refreshDbUrl = Cypress.env('reinitDbUrl');

    const url = resolve(baseUrl, refreshDbUrl);

    return cy.request('POST', url, { loginAsDomain }).then((resp) =>
        cy.window().then((win) => {
            win.localStorage.setItem('access_token', resp.body.access_token);
            win.localStorage.setItem('id_token', resp.body.id_token);
        })
    );
});

Cypress.Commands.add('login', () => {
    cy.visit('/home');
    cy.dataCy('login-button')
        .click()
        .dataCy('email')
        .type(EMAIL)
        .dataCy('password')
        .type(PASSWORD)
        .dataCy('submit')
        .click();
});
