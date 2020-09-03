import { EMAIL, PASSWORD } from './_setup';

// when visit page with different host, cypress will be reloaded every time
// and clear page state completely !

// The tests is not fully work due to cypress local storage issue, after signup / confirm
// login should be performed manually, by open localhost:4201/home and then login !
// The localState(auth_state) is lost during cypress tests
// https://github.com/cypress-io/cypress/issues/461

// sso
// After 1st login try login 2nd time in the row, it should nota ask login / password again

describe('signup flow', () => {
    // `before` will be invoked every time for specs in same describe, why ???

    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
        return cy.resetDb();
    });

    it('signup / login', () => {
        cy.visit('/home');
        cy.url().should('include', '/home');
        cy.dataCy('signup-button').click();
        cy.url().should('include', '/auth/register');

        cy.server();

        cy.route({ method: 'POST', url: '**/auth/sign-up' }).as('signup');

        cy.dataCy('email')
            .type(EMAIL)
            .dataCy('password')
            .type(PASSWORD)
            .dataCy('confirm-password')
            .type(PASSWORD)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.wait('@signup');

        cy.get('@signup').should((req: any) => {
            assert.isTrue(!!req.request.body.queryString);

            const qs = req.request.body.queryString;

            cy.url().should('include', '/auth/register-sent');

            const url = `/auth/register-confirm${qs}&token=${Cypress.env(
                'confirmSignupToken'
            )}`;

            cy.visit(url);

            cy.url().should('include', '/auth/login');

            cy.dataCy('email')
                .type(EMAIL)
                .dataCy('password')
                .type(PASSWORD)
                .dataCy('submit')
                .click();

            cy.url().should('include', '/login-cb');

            cy.url().should('not.include', '/login-cb');

            cy.window().then((win) => {
                assert.isTrue(!!win.localStorage.getItem('id_token'));
                assert.isTrue(!!win.localStorage.getItem('access_token'));
                assert.isTrue(!!win.localStorage.getItem('refresh_token'));
            });

            cy.url().should('include', '/profile/home');
        });
    });

    it('create tenant could be closed', () => {
        cy.dataCy('create-tenant').click().get('.ant-drawer-close').click();
        cy.url().should('not.include', 'create-tenant');
    });

    it('create tenant', () => {
        cy.dataCy('create-tenant')
            .click()
            .formField('name')
            .type('test')
            .submitButton()
            .click();

        cy.url().should('include', 'auth/login');
    });

    it('login', () => {
        cy.dataCy('email')
            .type(EMAIL)
            .dataCy('password')
            .type(PASSWORD)
            .dataCy('submit')
            .click();

        cy.url().should('match', /\/tenants\/\d+\/domains/);

        cy.rows(0, 1).dataCy('env-btn').eq(0).click();

        cy.url().should('match', /\/domains\/\d+\/apps/);
    });
});
