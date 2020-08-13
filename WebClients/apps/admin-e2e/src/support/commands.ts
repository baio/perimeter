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
    return cy.get(`.hlc-form-input-${value} input`);
});

Cypress.Commands.add('rows', () => {
    return cy.get('tbody').find('tr');
});

Cypress.Commands.add('submitButton', () => {
    return cy.dataCy('drawer-submit');
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

    return cy
        .request('POST', url)
        .then((resp) =>
            cy
                .window()
                .then((win) => {
                    win.sessionStorage.setItem('access_token', resp.body.accessToken)
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
