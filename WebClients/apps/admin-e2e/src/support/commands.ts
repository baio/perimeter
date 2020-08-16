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
    return cy
        .formSelect(value)
        .click()
        .get('nz-option-item .ant-select-item-option-content')
        .eq(index)
        .click()
        .formSelect(value)
        .find('nz-select-top-control')
        .click(100, 0, { force: true });
});

Cypress.Commands.add('rows', (index?: number, cellIndex?: number) => {
    const res = cy.get('tbody tr.ant-table-row');
    const res1 = index !== undefined ? res.eq(index) : res;
    return cellIndex !== undefined ? res1.find('td').eq(cellIndex) : res1;
});

Cypress.Commands.add(
    'rowCommand',
    (rowIndex: number, commandIndex: number) => {
        return cy
            .get('tbody tr.ant-table-row')
            .eq(rowIndex)
            .find('td .table-actions a')
            .eq(commandIndex);
    }
);

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

Cypress.Commands.add('reinitDb', () => {
    const baseUrl = Cypress.env('apiBaseUrl');

    const refreshDbUrl = Cypress.env('reinitDbUrl');

    const url = resolve(baseUrl, refreshDbUrl);

    return cy.request('POST', url).then((resp) =>
        cy.window().then((win) => {
            // TODO
            win.sessionStorage.setItem('access_token', resp.body.accessToken);
        })
    );
});

/*
Cypress.Commands.add('signUp', () => {
    const baseUrl = Cypress.env('apiBaseUrl');
    const signUpData = {
        email: EMAIL,
        password: PASSWORD,
        firsName: 'test',
        lastName: 'user',
    };
    const signUpUrl = resolve(baseUrl, 'auth/sign-up');

    const signUpConfirmData = {
        token: Cypress.env('confirmSignupToken'),
    };
    const signUpConfirmUrl = resolve(baseUrl, 'auth/sign-up/confirm');

    cy.request('POST', signUpUrl, signUpData).request(
        'POST',
        signUpConfirmUrl,
        signUpConfirmData
    );
});

Cypress.Commands.add('reset', () => {
    cy.refreshDb();
});
*/

Cypress.Commands.add('stickyVariable', (value) => {
    if (value) {
        return cy.writeFile('stickyVariable.txt', value);
    } else {
        return cy.readFile('stickyVariable.txt');
    }
});
